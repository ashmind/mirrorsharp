using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MirrorSharp.Internal.Commands {
    public class ApplyDiagnosticActionHandler : ICommandHandler {
        public IImmutableList<char> CommandIds { get; } = ImmutableList.Create('F');

        public async Task ExecuteAsync(ArraySegment<byte> data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var actionId = FastConvert.Utf8ByteArrayToInt32(data);
            var action = session.CurrentCodeActions[actionId];
            var operations = await action.GetOperationsAsync(cancellationToken).ConfigureAwait(false);
            foreach (var operation in operations) {
                operation.Apply(session.Workspace, cancellationToken);
            }
            var changes = await session.UpdateFromWorkspaceAsync().ConfigureAwait(false);

            var writer = sender.StartJsonMessage("changes");
            writer.WriteProperty("echo", false);
            writer.WritePropertyStartArray("changes");
            foreach (var change in changes) {
                writer.WriteChange(change);
            }
            writer.WriteEndArray();
            await sender.SendJsonMessageAsync(cancellationToken).ConfigureAwait(false);
        }

        public bool CanChangeSession => false;
    }
}
