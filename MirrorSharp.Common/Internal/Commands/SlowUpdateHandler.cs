using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MirrorSharp.Internal.Commands {
    public class SlowUpdateHandler : ICommandHandler {
        public IImmutableList<char> CommandIds { get; } = ImmutableList.Create('U');

        public async Task ExecuteAsync(ArraySegment<byte> data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var compilation = await session.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
            var diagnostics = await compilation.WithAnalyzers(session.Analyzers).GetAllDiagnosticsAsync(cancellationToken).ConfigureAwait(false);

            await SendSlowUpdateAsync(diagnostics, sender, cancellationToken).ConfigureAwait(false);
        }

        private Task SendSlowUpdateAsync(ImmutableArray<Diagnostic> diagnostics, ICommandResultSender sender, CancellationToken cancellationToken) {
            var writer = sender.StartJsonMessage("slowUpdate");
            writer.WritePropertyStartArray("diagnostics");
            foreach (var diagnostic in diagnostics) {
                writer.WriteStartObject();
                writer.WriteProperty("message", diagnostic.GetMessage());
                writer.WriteProperty("severity", diagnostic.Severity.ToString("G").ToLowerInvariant());
                writer.WritePropertyStartArray("tags");
                foreach (var tag in diagnostic.Descriptor.CustomTags) {
                    if (tag != WellKnownDiagnosticTags.Unnecessary)
                        continue;
                    writer.WriteValue(tag.ToLowerInvariant());
                }
                writer.WriteEndArray();
                writer.WritePropertyName("span");
                writer.WriteSpan(diagnostic.Location.SourceSpan);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            return sender.SendJsonMessageAsync(cancellationToken);
        }

        public bool CanChangeSession => false;
    }
}
