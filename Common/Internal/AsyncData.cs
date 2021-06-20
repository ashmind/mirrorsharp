using System;
using System.Threading.Tasks;

namespace MirrorSharp.Internal {
    internal readonly struct AsyncData {
        private static readonly Task<ReadOnlyMemory<byte>?> NullSegmentTask = Task.FromResult<ReadOnlyMemory<byte>?>(null);
        public static readonly AsyncData Empty = new(ReadOnlyMemory<byte>.Empty, false, static () => NullSegmentTask);

        private readonly ReadOnlyMemory<byte> _first;
        private readonly Func<Task<ReadOnlyMemory<byte>?>> _getNextAsync;
        private readonly bool _getNextCalled;

        public AsyncData(ReadOnlyMemory<byte> first, bool mightHaveNext, Func<Task<ReadOnlyMemory<byte>?>> getNextAsync) {
            Argument.NotNull(nameof(getNextAsync), getNextAsync);

            _first = first;
            MightHaveNext = mightHaveNext;
            _getNextAsync = getNextAsync;
            _getNextCalled = false;
        }

        [Obsolete("Can be removed in v3, but needed before that for compatibility with Testing.", true)]
        public AsyncData(ArraySegment<byte> first, bool mightHaveNext, Func<Task<ArraySegment<byte>?>> getNextAsync) {
            Argument.NotNull(nameof(getNextAsync), getNextAsync);

            _first = first;
            MightHaveNext = mightHaveNext;
            _getNextAsync = async () => await getNextAsync().ConfigureAwait(false);
            _getNextCalled = false;
        }

        public ReadOnlyMemory<byte> GetFirst() {
            if (_getNextCalled)
                throw new InvalidOperationException();
            return _first;
        }

        public bool MightHaveNext { get; }

        public Task<ReadOnlyMemory<byte>?> GetNextAsync() => _getNextAsync();
    }
}
