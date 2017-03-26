using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal.Handlers.Shared;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    internal class MoveCursorHandler : ICommandHandler {
        public char CommandId => CommandIds.MoveCursor;
        private readonly ISignatureHelpSupport _signatureHelp;

        public MoveCursorHandler(ISignatureHelpSupport signatureHelp) {
            _signatureHelp = signatureHelp;
        }

        public Task ExecuteAsync(AsyncData data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var cursorPosition = FastConvert.Utf8ByteArrayToInt32(data.GetFirst());
            session.CursorPosition = cursorPosition;
            return _signatureHelp.ApplyCursorPositionChangeAsync(session, sender, cancellationToken);
        }
    }
}
