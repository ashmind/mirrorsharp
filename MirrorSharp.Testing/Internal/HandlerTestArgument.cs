using System;
using System.Text;

namespace MirrorSharp.Testing.Internal {
    internal struct HandlerTestArgument {
        private readonly byte[] _data;

        private HandlerTestArgument(byte[] data) {
            _data = data;
        }

        public static implicit operator HandlerTestArgument(string value) {
            return Encoding.UTF8.GetBytes(value);
        }

        public static implicit operator HandlerTestArgument(char value) {
            return Encoding.UTF8.GetBytes(new[] { value });
        }

        public static implicit operator HandlerTestArgument(int value) {
            return value.ToString();
        }

        public static implicit operator HandlerTestArgument(byte[] data) {
            return new HandlerTestArgument(data);
        }

        public ArraySegment<byte> ToArraySegment() => new ArraySegment<byte>(_data ?? new byte[0]);
    }
}
