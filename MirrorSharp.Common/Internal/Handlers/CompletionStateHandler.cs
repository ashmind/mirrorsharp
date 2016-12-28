using System;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal.Handlers.Shared;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    internal class CompletionStateHandler : ICommandHandler {
        public char CommandId => 'S';
        private readonly ICompletionSupport _completion;

        public CompletionStateHandler(ICompletionSupport completion) {
            _completion = completion;
        }

        public Task ExecuteAsync(ArraySegment<byte> data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var first = data.Array[data.Offset];
            if (first == (byte)'X')
                return _completion.ApplyCompletionCancellationAsync(session, sender, cancellationToken);

            if (first == (byte)'F')
                return _completion.ApplyCompletionForceAsync(session, sender, cancellationToken);

            var itemIndex = FastConvert.Utf8ByteArrayToInt32(data);
            return _completion.ApplyCompletionSelectionAsync(itemIndex, session, sender, cancellationToken);
        }
    }
}