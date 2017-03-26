using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal.Handlers.Shared;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    internal class CompletionStateHandler : ICommandHandler {
        public char CommandId => CommandIds.CompletionState;
        private readonly ICompletionSupport _completion;

        public CompletionStateHandler(ICompletionSupport completion) {
            _completion = completion;
        }

        public Task ExecuteAsync(AsyncData data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var first = data.GetFirst();
            var firstByte = first.Array[first.Offset];
            if (firstByte == (byte)'X')
                return _completion.CancelCompletionAsync(session, sender, cancellationToken);

            if (firstByte == (byte)'F')
                return _completion.ForceCompletionAsync(session, sender, cancellationToken);

            var itemIndex = FastConvert.Utf8ByteArrayToInt32(first);
            return _completion.SelectCompletionAsync(itemIndex, session, sender, cancellationToken);
        }
    }
}