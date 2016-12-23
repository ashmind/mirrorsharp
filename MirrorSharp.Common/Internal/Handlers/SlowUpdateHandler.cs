using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using MirrorSharp.Advanced;
using MirrorSharp.Internal.Reflection;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    public class SlowUpdateHandler : ICommandHandler {
        private static readonly IReadOnlyCollection<CodeAction> NoCodeActions = new CodeAction[0];
        [CanBeNull] private readonly ISlowUpdateExtension _extension;

        public SlowUpdateHandler([CanBeNull] ISlowUpdateExtension extension) {
            _extension = extension;
        }

        public IImmutableList<char> CommandIds { get; } = ImmutableList.Create('U');

        public async Task ExecuteAsync(ArraySegment<byte> data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var compilation = await session.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
            var diagnostics = await compilation.WithAnalyzers(session.Analyzers).GetAllDiagnosticsAsync(cancellationToken).ConfigureAwait(false);
            var extensionResult = _extension != null ? await _extension.PrepareAsync(session, cancellationToken).ConfigureAwait(false) : null;

            await SendSlowUpdateAsync(diagnostics, session, extensionResult, sender, cancellationToken).ConfigureAwait(false);
        }

        private async Task SendSlowUpdateAsync(ImmutableArray<Diagnostic> diagnostics, WorkSession session, object extensionResult, ICommandResultSender sender, CancellationToken cancellationToken) {
            session.CurrentCodeActions.Clear();
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
                var actions = await GetCodeActionsAsync(diagnostic, session, cancellationToken).ConfigureAwait(false);
                if (actions.Count > 0) {
                    writer.WritePropertyStartArray("actions");
                    WriteActions(writer, actions, session);
                    writer.WriteEndArray();
                }
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            if (_extension != null) {
                writer.WritePropertyStartObject("x");
                _extension.Write(writer, extensionResult, cancellationToken);
                writer.WriteEndObject();
            }
            await sender.SendJsonMessageAsync(cancellationToken).ConfigureAwait(false);
        }

        private static void WriteActions(FastUtf8JsonWriter writer, IReadOnlyCollection<CodeAction> actions, WorkSession session) {
            foreach (var action in actions) {
                if (action is CodeActionWithOptions)
                    continue;

                if (!RoslynInternalCalls.GetIsInvokable(action)) {
                    WriteActions(writer, RoslynInternalCalls.GetCodeActions(action), session);
                    continue;
                }
                var id = session.CurrentCodeActions.Count;
                session.CurrentCodeActions.Add(action);
                writer.WriteStartObject();
                writer.WriteProperty("id", id);
                writer.WriteProperty("title", action.Title);
                writer.WriteEndObject();
            }
        }

        private async Task<IReadOnlyCollection<CodeAction>> GetCodeActionsAsync(Diagnostic diagnostic, WorkSession session, CancellationToken cancellationToken) {
            List<CodeAction> actions = null;
            Action<CodeAction, ImmutableArray<Diagnostic>> registerCodeFix = (action, _) => {
                if (actions == null)
                    actions = new List<CodeAction>();
                actions.Add(action);
            };
            var fixContext = new CodeFixContext(session.Document, diagnostic, registerCodeFix, cancellationToken);
            var providers = ImmutableDictionary.GetValueOrDefault(session.CodeFixProviders, diagnostic.Id);
            if (providers == null)
                return NoCodeActions;

            foreach (var provider in providers) {
                await provider.RegisterCodeFixesAsync(fixContext).ConfigureAwait(false);
            }
            return actions ?? NoCodeActions;
        }

        public bool CanChangeSession => false;
    }
}
