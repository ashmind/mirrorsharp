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
            var firstByte = first.Span[0];

            if (firstByte == (byte)'I') {
                var infoItemIndex = FastConvert.Utf8BytesToInt32(first.Span.Slice(1));
                return _completion.SendItemInfoAsync(infoItemIndex, session, sender, cancellationToken);
            }

            if (firstByte == (byte)'X')
                return _completion.CancelCompletionAsync(session, sender, cancellationToken);

            if (firstByte == (byte)'F')
                return _completion.ForceCompletionAsync(session, sender, cancellationToken);

            var itemIndex = FastConvert.Utf8BytesToInt32(first.Span);
            return _completion.SelectCompletionAsync(itemIndex, session, sender, cancellationToken);
        }
    }
}