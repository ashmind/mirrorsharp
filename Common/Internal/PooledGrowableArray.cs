using System;
using System.Buffers;

namespace MirrorSharp.Internal {
    internal struct PooledGrowableArray<T> : IDisposable {
        private readonly ArrayPool<T> _pool;
        private T[] _array;

        public PooledGrowableArray(int initialLength, ArrayPool<T> pool) {
            _pool = pool;
            _array = pool.Rent(initialLength);
        }

        public T[] Array => _array;

        public void Grow(int newLength) {
            if (newLength <= _array.Length)
                return;

            var actualNewLength = _array.Length * (int)Math.Pow(2, Math.Log(Math.Ceiling((double)newLength / _array.Length), 2));
            var newArray = (T[])null;
            var oldArray = (T[])null;
            try {
                newArray = _pool.Rent(actualNewLength);
                System.Array.Copy(_array, 0, newArray, 0, _array.Length);
                oldArray = _array;
                _array = newArray;
            }
            catch (Exception) {
                if (_array != newArray)
                    _pool.Return(newArray);
                throw;
            }
            finally {
                if (_array != oldArray)
                    _pool.Return(oldArray);
            }
        }

        public void Dispose() {
            _pool.Return(_array);
        }
    }
}
