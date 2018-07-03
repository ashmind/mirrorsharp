using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Advanced;
using MirrorSharp.Internal.Reflection;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    internal class RequestInfoTipHandler : ICommandHandler {
        public char CommandId => CommandIds.RequestInfoTip;

        public Task ExecuteAsync(AsyncData data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            #if QUICKINFO
            if (!session.IsRoslyn)
                return Task.CompletedTask;

            var cursorPosition = FastConvert.Utf8ByteArrayToInt32(data.GetFirst());
            return ExecuteForRoslynAsync(cursorPosition, session, sender, cancellationToken);
            #else
            return Task.CompletedTask;
            #endif
        }

        #if QUICKINFO
        private async Task ExecuteForRoslynAsync(int cursorPosition, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            QuickInfoItemData info = null;
            foreach (var provider in session.Roslyn.QuickInfoProviders) {
                info = await provider.GetItemAsync(session.Roslyn.Document, cursorPosition, cancellationToken).ConfigureAwait(false);
                if (info != null)
                    break;
            }
            if (info == null)
                return;

            await SendInfoTipAsync(info, sender, cancellationToken).ConfigureAwait(false);
        }

        private Task SendInfoTipAsync(QuickInfoItemData item, ICommandResultSender sender, CancellationToken cancellationToken) {
            var writer = sender.StartJsonMessage("infotip");
            writer.WriteProperty("info", "<tip>");
            writer.WritePropertyName("span");
            writer.WriteSpan(item.TextSpan);
            return sender.SendJsonMessageAsync(cancellationToken);
        }
        #endif
    }
}
