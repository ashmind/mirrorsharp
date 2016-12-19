using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal.Handlers.Shared;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    public class MoveCursorHandler : ICommandHandler {
        public IImmutableList<char> CommandIds => ImmutableList.Create('M');
        private readonly ISignatureHelpSupport _signatureHelp;

        public MoveCursorHandler(ISignatureHelpSupport signatureHelp) {
            _signatureHelp = signatureHelp;
        }

        public Task ExecuteAsync(ArraySegment<byte> data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var cursorPosition = FastConvert.Utf8ByteArrayToInt32(data);
            session.CursorPosition = cursorPosition;
            return _signatureHelp.ApplyCursorPositionChangeAsync(session, sender, cancellationToken);
        }

        public bool CanChangeSession => true;
    }
}
