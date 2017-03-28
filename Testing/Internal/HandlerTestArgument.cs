using System;
using System.Linq;
using System.Text;
using MirrorSharp.Internal;

// ReSharper disable HeapView.ClosureAllocation
// ReSharper disable HeapView.DelegateAllocation
// ReSharper disable HeapView.ObjectAllocation

namespace MirrorSharp.Testing.Internal {
    internal class HandlerTestArgument {
        private readonly byte[][] _data;

        private HandlerTestArgument(params byte[][] data) {
            _data = data;
        }

        public static implicit operator HandlerTestArgument(string value) {
            return Encoding.UTF8.GetBytes(value);
        }

        public static implicit operator HandlerTestArgument(string[] values) {
            return values.Select(Encoding.UTF8.GetBytes).ToArray();
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

        public static implicit operator HandlerTestArgument(byte[][] data) {
            return new HandlerTestArgument(data);
        }

        public AsyncData ToAsyncData() {
            var nextIndex = 1;
            return new AsyncData(
                new ArraySegment<byte>(_data.ElementAtOrDefault(0) ?? new byte[0]),
                _data.Length > 1,
                #pragma warning disable 1998
                async () => {
                #pragma warning restore 1998
                    var next = _data.ElementAtOrDefault(nextIndex);
                    if (next == null)
                        return null;
                    nextIndex += 1;
                    return new ArraySegment<byte>(next);
                }
            );
        }
    }
}
