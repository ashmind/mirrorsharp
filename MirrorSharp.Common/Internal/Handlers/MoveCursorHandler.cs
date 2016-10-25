using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    public class MoveCursorHandler : ICommandHandler {
        public IImmutableList<char> CommandIds => ImmutableList.Create('M');

        public Task ExecuteAsync(ArraySegment<byte> data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var cursorPosition = FastConvert.Utf8ByteArrayToInt32(data);
            session.CursorPosition = cursorPosition;
            if (session.CurrentSignatureHelp != null) {
                if (!session.CurrentSignatureHelp.Value.Items.ApplicableSpan.Contains(cursorPosition)) {
                    session.CurrentSignatureHelp = null;
                    return SendSignatureHelpCancelAsync(sender, cancellationToken);
                }
            }

            return TaskEx.CompletedTask;
        }

        private Task SendSignatureHelpCancelAsync(ICommandResultSender sender, CancellationToken cancellationToken) {
            sender.StartJsonMessage("signatures");
            return sender.SendJsonMessageAsync(cancellationToken);
        }

        public bool CanChangeSession => true;
    }
}
