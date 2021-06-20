using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.QuickInfo;
using MirrorSharp.Advanced;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    internal class RequestInfoTipHandler : ICommandHandler {
        public char CommandId => CommandIds.RequestInfoTip;

        public Task ExecuteAsync(AsyncData data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            if (!session.IsRoslyn)
                return Task.CompletedTask;

            var cursorPosition = FastConvert.Utf8BytesToInt32(data.GetFirst().Span);
            return ExecuteForRoslynAsync(cursorPosition, session, sender, cancellationToken);
        }

        private async Task ExecuteForRoslynAsync(
            int cursorPosition,
            WorkSession session,
            ICommandResultSender sender,
            CancellationToken cancellationToken
        ) {
            var info = await session.Roslyn.QuickInfoService
                .GetQuickInfoAsync(session.Roslyn.Document, cursorPosition, cancellationToken)
                .ConfigureAwait(false);

            if (IsNullOrEmpty(info))
                return;
            await SendInfoTipAsync(info, sender, cancellationToken).ConfigureAwait(false);
        }

        private Task SendInfoTipAsync(QuickInfoItem info, ICommandResultSender sender, CancellationToken cancellationToken) {
            var writer = sender.StartJsonMessage("infotip");
            if (IsNullOrEmpty(info))
                return sender.SendJsonMessageAsync(cancellationToken);

            writer.WriteTagsProperty("kinds", info.Tags);
            writer.WritePropertyStartArray("sections");
            foreach (var section in info.Sections) {
                writer.WriteStartObject();
                writer.WriteProperty("kind", FastConvert.StringToLowerInvariantString(section.Kind));
                writer.WritePropertyStartArray("parts");
                writer.WriteTaggedTexts(section.TaggedParts);
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteSpanProperty("span", info.Span);
            return sender.SendJsonMessageAsync(cancellationToken);
        }

        private static bool IsNullOrEmpty(QuickInfoItem info) {
            // Note that Sections.IsEmpty doesn't mean there is nothing
            // E.g. closing bracket `}` will have related open bracket
            // code in related spans. However this isn't supported yet.
            return info == null || info.Sections.IsEmpty;
        }
    }
}
