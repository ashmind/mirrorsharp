using System;
using System.Threading.Tasks;

namespace MirrorSharp.Internal {
    internal struct AsyncData {
        public static readonly AsyncData Empty = new AsyncData(new ArraySegment<byte>(new byte[0]), false, () => null);

        private readonly ArraySegment<byte> _first;
        private readonly Func<Task<ArraySegment<byte>?>> _getNextAsync;
        private readonly bool _getNextCalled;

        public AsyncData(ArraySegment<byte> first, bool mightHaveNext, Func<Task<ArraySegment<byte>?>> getNextAsync) {
            _first = first;
            MightHaveNext = mightHaveNext;
            _getNextAsync = Argument.NotNull(nameof(getNextAsync), getNextAsync);
            _getNextCalled = false;
        }

        public ArraySegment<byte> GetFirst() {
            if (_getNextCalled)
                throw new InvalidOperationException();
            return _first;
        }

        public bool MightHaveNext { get; }

        public Task<ArraySegment<byte>?> GetNextAsync() => _getNextAsync();
    }
}
