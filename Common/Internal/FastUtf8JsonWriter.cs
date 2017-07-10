using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Text;

namespace MirrorSharp.Internal {
    internal class FastUtf8JsonWriter : IFastJsonWriterInternal {
        private static readonly int[] PowersOfTen = {
            1000000000, 100000000, 10000000, 1000000, 100000, 10000, 1000, 100, 10, 1
        };

        private int _position = 0;
        private readonly char[] _oneCharBuffer = new char[1];
        private readonly ArrayPool<byte> _bufferPool;
        private byte[] _buffer;

        private readonly ArrayPool<State> _stateStackPool;
        private readonly State[] _stateStack;
        private int _stateStackIndex = 0;

        private FastUtf8JsonStringWriter _stringWriter;

        public FastUtf8JsonWriter(ArrayPool<byte> bufferPool) {
            _bufferPool = bufferPool;
            _buffer = bufferPool.Rent(4096);

            _stateStackPool = ArrayPool<State>.Shared;
            _stateStack = _stateStackPool.Rent(64);
        }

        public ArraySegment<byte> WrittenSegment => new ArraySegment<byte>(_buffer, 0, _position);

        public void WriteStartObject() {
            WriteStartValue();
            WriteRawByte(Utf8.CurlyOpen);
            PushState(State.ObjectStart);
        }

        public void WriteEndObject() {
            PopState();
            WriteRawByte(Utf8.CurlyClose);
            WriteEndValue();
        }

        public void WriteStartArray() {
            WriteStartValue();
            WriteRawByte(Utf8.SquareOpen);
            PushState(State.ArrayStart);
        }

        public void WriteEndArray() {
            PopState();
            WriteRawByte(Utf8.SquareClose);
            WriteEndValue();
        }

        public void WriteProperty(string name, string value) {
            WritePropertyName(name);
            WriteValue(value);
        }

        public void WriteProperty(string name, CharArrayString value) {
            WritePropertyName(name);
            WriteValue(value);
        }

        public void WriteProperty(string name, char value) {
            WritePropertyName(name);
            WriteValue(value);
        }

        public void WriteProperty(string name, int value) {
            WritePropertyName(name);
            WriteValue(value);
        }

        public void WriteProperty(string name, bool value) {
            WritePropertyName(name);
            WriteValue(value);
        }

        public void WritePropertyStartObject(string name) {
            WritePropertyName(name);
            WriteStartObject();
        }

        public void WritePropertyStartArray(string name) {
            WritePropertyName(name);
            WriteStartArray();
        }

        public void WritePropertyName(string name) {
            if (GetState() == State.ObjectAfterProperty)
                WriteRawByte(Utf8.Comma);
            WriteValue(name);
            WriteRawByte(Utf8.Colon);
            ReplaceState(State.ObjectPropertyValue);
        }

        public void WriteValue(string value) {
            WriteStartValue();
            if (value == null) {
                WriteRawBytes(Utf8.Null);
                WriteEndValue();
                return;
            }

            WriteRawByte(Utf8.Quote);
            WriteUnquotedString(value);
            WriteRawByte(Utf8.Quote);
            WriteEndValue();
        }

        private void WriteUnquotedString(string value) {
            foreach (var @char in value) {
                WriteUnquotedChar(@char);
            }
        }

        public void WriteValue(CharArrayString value) {
            WriteStartValue();
            WriteRawByte(Utf8.Quote);
            foreach (var @char in value.Chars) {
                WriteUnquotedChar(@char);
            }
            WriteRawByte(Utf8.Quote);
            WriteEndValue();
        }

        public void WriteValue(char value) {
            WriteStartValue();
            WriteRawByte(Utf8.Quote);
            WriteUnquotedChar(value);
            WriteRawByte(Utf8.Quote);
            WriteEndValue();
        }

        private void WriteUnquotedChar(char @char) {
            if (@char < 32) {
                WriteRawBytes(Utf8.Escaped[@char]);
                return;
            }

            if (@char == '\\') {
                WriteRawBytes(Utf8.EscapedSlash);
                return;
            }

            if (@char == '"') {
                WriteRawBytes(Utf8.EscapedQuote);
                return;
            }

            if (@char < 256) {
                WriteRawByte((byte)@char);
                return;
            }

            _oneCharBuffer[0] = @char;
            _position += Encoding.UTF8.GetBytes(_oneCharBuffer, 0, 1, _buffer, _position);
        }

        public void WriteValue(int value) {
            WriteStartValue();
            if (value < 0) {
                WriteRawByte(Utf8.Minus);
                value = -value;
            }

            if (value < 10) {
                WriteRawByte(Utf8.Digits[value]);
                WriteEndValue();
                return;
            }

            var remainder = value;
            foreach (var power in PowersOfTen) {
                if (value < power)
                    continue;
                WriteRawByte(Utf8.Digits[remainder / power]);
                remainder %= power;
            }
            WriteEndValue();
        }

        public void WriteValue(bool value) {
            WriteStartValue();
            WriteRawBytes(value ? Utf8.True : Utf8.False);
            WriteEndValue();
        }
                
        private void WriteStartValue() {
            if (_stateStack[_stateStackIndex] == State.ArrayAfterItem)
                WriteRawByte(Utf8.Comma);
        }

        private void WriteEndValue() {
            var state = GetState();
            if (state == State.ArrayStart) {
                ReplaceState(State.ArrayAfterItem);
            }
            else if (state == State.ObjectPropertyValue) {
                ReplaceState(State.ObjectAfterProperty);
            }
        }

        public TextWriter OpenString() {
            if (_stringWriter == null)
                _stringWriter = new FastUtf8JsonStringWriter(this);
            WriteStartValue();
            WriteRawByte(Utf8.Quote);
            return _stringWriter;
        }

        private void CloseString() {
            WriteRawByte(Utf8.Quote);
            WriteEndValue();
        }

        private void WriteRawByte(byte @byte) {
            EnsureCanWrite(1);
            _buffer[_position] = @byte;
            _position += 1;
        }

        private void WriteRawBytes(byte[] bytes) {
            EnsureCanWrite(bytes.Length);
            Buffer.BlockCopy(bytes, 0, _buffer, _position, bytes.Length);
            _position += bytes.Length;
        }

        private void EnsureCanWrite(int requiredExtraBytes) {
            if (_position + requiredExtraBytes <= _buffer.Length)
                return;

            byte[] newBuffer = null;
            try {
                newBuffer = _bufferPool.Rent(_position + requiredExtraBytes);
                Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _buffer.Length);
            }
            catch {
                if (newBuffer != null)
                    _bufferPool.Return(newBuffer);
                throw;
            }
            try {
                _bufferPool.Return(_buffer);
            }
            finally {
                _buffer = newBuffer;
            }
        }

        public void Reset() {
            _position = 0;
        }

        private State GetState() {
            return _stateStack[_stateStackIndex];
        }

        private void ReplaceState(State state) {
            _stateStack[_stateStackIndex] = state;
        }

        private void PushState(State state) {
            _stateStackIndex += 1;
            _stateStack[_stateStackIndex] = state;
        }

        private void PopState() {
            _stateStackIndex -= 1;
        }

        private enum State {
            // ReSharper disable once UnusedMember.Local
            None,
            ArrayStart,
            ArrayAfterItem,
            ObjectStart,
            ObjectPropertyValue,
            ObjectAfterProperty
        }

        private static class Utf8 {
            public const byte CurlyOpen = (byte)'{';
            public const byte CurlyClose = (byte)'}';
            public const byte SquareOpen = (byte)'[';
            public const byte SquareClose = (byte)']';
            public const byte Colon = (byte)':';
            public const byte Quote = (byte)'"';
            public const byte Comma = (byte)',';

            public const byte Minus = (byte)'-';
            public static readonly byte[] Digits = Enumerable.Range(0, 10)
                .Select(i => (byte)i.ToString()[0])
                .ToArray();

            public static readonly byte[] True = Encoding.UTF8.GetBytes("true");
            public static readonly byte[] False = Encoding.UTF8.GetBytes("false");
            public static readonly byte[] Null = Encoding.UTF8.GetBytes("null");

            public static readonly byte[][] Escaped = Enumerable.Range(0, 32)
                .Select(i => {
                    switch (i) {
                        case '\b': return "\\b";
                        case '\f': return "\\f";
                        case '\r': return "\\r";
                        case '\n': return "\\n";
                        case '\t': return "\\t";
                        default: return "\\u" + i.ToString("X4");
                    }
                })
                .Select(Encoding.UTF8.GetBytes)
                .ToArray();

            public static readonly byte[] EscapedSlash = Encoding.UTF8.GetBytes("\\\\");
            public static readonly byte[] EscapedQuote = Encoding.UTF8.GetBytes("\\\"");
        }

        private class FastUtf8JsonStringWriter : TextWriter {
            private readonly FastUtf8JsonWriter _owner;

            public FastUtf8JsonStringWriter(FastUtf8JsonWriter owner) {
                _owner = owner;
            }

            public override Encoding Encoding => Encoding.UTF8;

            public override void Write(char value) => _owner.WriteUnquotedChar(value);
            public override void Write(int value) => _owner.WriteValue(value);
            public override void Write(string value) => _owner.WriteUnquotedString(value);

            protected override void Dispose(bool disposing) {
                _owner.CloseString();
            }
        }

        public void Dispose() {
            _bufferPool.Return(_buffer);
            _stateStackPool.Return(_stateStack);
            GC.SuppressFinalize(this);
        }

        ~FastUtf8JsonWriter() {
            _bufferPool.Return(_buffer);
            _stateStackPool.Return(_stateStack);
        }
    }
}
