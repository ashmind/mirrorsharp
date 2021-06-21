using System;
using System.Buffers;
using System.Collections.Generic;
using Xunit;

namespace MirrorSharp.Tests.Internal {
    public class TrackingArrayPool<T> : ArrayPool<T> {
        private readonly ArrayPool<T> _inner;
        private IDictionary<T[], string>? _rented;

        public TrackingArrayPool(ArrayPool<T> inner) {
            _inner = inner;
        }

        public override T[] Rent(int minimumLength) {
            var array = _inner.Rent(minimumLength);
            _rented?.Add(array, Environment.StackTrace);
            return array;
        }

        public override void Return(T[] array, bool clearArray = false) {
            _inner.Return(array);
            _rented?.Remove(array);
        }

        public void StartTracking() {
            _rented = new Dictionary<T[], string>();
        }

        public void AssertAllReturned() {
            Assert.Empty(_rented!.Values);
        }
    }
}
