using System;
using System.Linq;
using System.Text;

namespace MirrorSharp.Internal {
    public class FastJsonWriter {
        private int _position = 0;
        private readonly char[] _oneCharBuffer = new char[1];
        private readonly byte[] _buffer;

        private readonly State[] _stateStack = new State[32];
        private int _stateStackIndex = 0;

        public FastJsonWriter(byte[] buffer) {
            _buffer = buffer;
        }

        public ArraySegment<byte> WrittenSegment => new ArraySegment<byte>(_buffer, 0, _position);

        public void WriteStartObject() {
            WriteStartValue();
            WriteRawByte(Utf8.CurlyOpen);
            _stateStackIndex += 1;
            _stateStack[_stateStackIndex] = State.ObjectStart;
        }

        public void WriteEndObject() {
            _stateStackIndex -= 1;
            WriteRawByte(Utf8.CurlyClose);
            WriteEndValue();
        }

        public void WriteStartArray() {
            WriteStartValue();
            WriteRawByte(Utf8.SquareOpen);
            _stateStackIndex += 1;
            _stateStack[_stateStackIndex] = State.ArrayStart;
        }

        public void WriteEndArray() {
            _stateStackIndex -= 1;
            WriteRawByte(Utf8.SquareClose);
            WriteEndValue();
        }

        public void WriteProperty(string name, string value) {
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
            var state = _stateStack[_stateStackIndex];
            if (state == State.ObjectAfterProperty)
                WriteRawByte(Utf8.Comma);
            WriteValue(name);
            WriteRawByte(Utf8.Colon);
            _stateStack[_stateStackIndex] = State.ObjectPropertyValue;
        }

        public void WriteValue(string value) {
            WriteStartValue();
            WriteRawByte(Utf8.Quote);
            foreach (var @char in value) {
                if (@char < 32) {
                    WriteRawBytes(Utf8.Escaped[@char]);
                    continue;
                }

                if (@char == '\\') {
                    WriteRawBytes(Utf8.EscapedSlash);
                    continue;
                }

                if (@char == '"') {
                    WriteRawBytes(Utf8.EscapedQuote);
                    continue;
                }

                if (@char < 256) {
                    WriteRawByte((byte)@char);
                    continue;
                }

                _oneCharBuffer[0] = @char;
                _position += Encoding.UTF8.GetBytes(_oneCharBuffer, 0, 1, _buffer, _position);
            }
            WriteRawByte(Utf8.Quote);
            WriteEndValue();
        }

        public void WriteValue(int value) {
            if (value < 0 || value > 1000) {
                WriteValue(value.ToString());
                return;
            }

            WriteInt32Fast(value);
            WriteEndValue();
        }

        private void WriteInt32Fast(int value) {
            if (value > 100) {
                WriteRawByte(Utf8.Digits[value / 100]);
                value = value % 100;
            }

            if (value > 10)
                WriteRawByte(Utf8.Digits[value / 10]);

            WriteRawByte(Utf8.Digits[(value % 10)]);
        }

        public void WriteValue(bool value) {
            WriteRawBytes(value ? Utf8.True : Utf8.False);
            WriteEndValue();
        }

        private void WriteStartValue() {
            if (_stateStack[_stateStackIndex] == State.ArrayAfterItem)
                WriteRawByte(Utf8.Comma);
        }

        private void WriteEndValue() {
            var state = _stateStack[_stateStackIndex];
            if (state == State.ArrayStart) {
                _stateStack[_stateStackIndex] = State.ArrayAfterItem;
            }
            else if (state == State.ObjectPropertyValue) {
                _stateStack[_stateStackIndex] = State.ObjectAfterProperty;
            }
        }

        private void WriteRawByte(byte @byte) {
            _buffer[_position] = @byte;
            _position += 1;
        }

        private void WriteRawBytes(byte[] bytes) {
            bytes.CopyTo(_buffer, _position);
            _position += bytes.Length;
        }

        public void Reset() {
            _position = 0;
        }

        private enum State {
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
            public static readonly byte[] Digits = Enumerable.Range(0, 10)
                .Select(i => (byte)i.ToString()[0])
                .ToArray();

            public static readonly byte[] True = Encoding.UTF8.GetBytes("true");
            public static readonly byte[] False = Encoding.UTF8.GetBytes("false");

            public static readonly byte[][] Escaped = Enumerable.Range(0, 32)
                .Select(i => {
                    switch (i) {
                        case '\b': return "\\b";
                        case '\f': return "\\f";
                        case '\r': return "\\r";
                        case '\n': return "\\n";
                        case '\t': return "\\t";
                        default: return "\\" + i.ToString("X4");
                    }
                })
                .Select(Encoding.UTF8.GetBytes)
                .ToArray();

            public static readonly byte[] EscapedSlash = Encoding.UTF8.GetBytes("\\\\");
            public static readonly byte[] EscapedQuote = Encoding.UTF8.GetBytes("\\\"");
        }
    }
}
