using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using MirrorSharp.Advanced;
using MirrorSharp.Internal.Reflection;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    internal class SlowUpdateHandler : ICommandHandler {
        [CanBeNull] private readonly ISlowUpdateExtension _extension;

        public SlowUpdateHandler([CanBeNull] ISlowUpdateExtension extension) {
            _extension = extension;
        }

        public char CommandId => CommandIds.SlowUpdate;

        public async Task ExecuteAsync(AsyncData data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            // Temporary suppression, need to figure out the best approach here.
            // ReSharper disable once HeapView.BoxingAllocation
            var diagnostics = (IReadOnlyList<Diagnostic>)await session.LanguageSession.GetDiagnosticsAsync(cancellationToken).ConfigureAwait(false);
            object extensionResult = null;
            try {
                if (_extension != null) {
                    var mutableDiagnostics = diagnostics.ToList();
                    extensionResult = await _extension.ProcessAsync(session, mutableDiagnostics, cancellationToken).ConfigureAwait(false);
                    diagnostics = mutableDiagnostics;
                }
                await SendSlowUpdateAsync(diagnostics, session, extensionResult, sender, cancellationToken).ConfigureAwait(false);
            }
            finally {
                (extensionResult as IDisposable)?.Dispose();
            }
        }

        private async Task SendSlowUpdateAsync(IReadOnlyList<Diagnostic> diagnostics, WorkSession session, object extensionResult, ICommandResultSender sender, CancellationToken cancellationToken) {
            if (session.IsRoslyn)
                session.Roslyn.CurrentCodeActions.Clear();
            var writer = sender.StartJsonMessage("slowUpdate");
            writer.WritePropertyStartArray("diagnostics");
            foreach (var diagnostic in diagnostics) {
                writer.WriteStartObject();
                writer.WriteProperty("id", diagnostic.Id);
                writer.WriteProperty("message", diagnostic.GetMessage());
                writer.WriteProperty("severity", FastConvert.EnumToLowerInvariantString(diagnostic.Severity));
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
                if (actions.Length > 0) {
                    writer.WritePropertyStartArray("actions");
                    WriteActions(writer, actions, session);
                    writer.WriteEndArray();
                }
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            if (_extension != null) {
                writer.WritePropertyName("x");
                _extension.WriteResult(writer, extensionResult, session);
            }
            await sender.SendJsonMessageAsync(cancellationToken).ConfigureAwait(false);
        }

        private static void WriteActions(IFastJsonWriter writer, ImmutableArray<CodeAction> actions, WorkSession session) {
            var roslynSession = session.Roslyn;
            foreach (var action in actions) {
                if (action is CodeActionWithOptions)
                    continue;

                if (RoslynReflectionFast.IsInlinable(action)) {
                    WriteActions(writer, RoslynReflectionFast.GetNestedCodeActions(action), session);
                    continue;
                }
                var id = roslynSession.CurrentCodeActions.Count;
                roslynSession.CurrentCodeActions.Add(action);
                writer.WriteStartObject();
                writer.WriteProperty("id", id);
                writer.WriteProperty("title", action.Title);
                writer.WriteEndObject();
            }
        }

        private async ValueTask<ImmutableArray<CodeAction>> GetCodeActionsAsync(Diagnostic diagnostic, WorkSession session, CancellationToken cancellationToken) {
            if (!session.IsRoslyn)
                return ImmutableArray<CodeAction>.Empty;

            // I don't think this can be avoided.
            // ReSharper disable once HeapView.ClosureAllocation
            ImmutableArray<CodeAction>.Builder actionsBuilder = null;
            Action<CodeAction, ImmutableArray<Diagnostic>> registerCodeFix = (action, _) => {
                if (actionsBuilder == null)
                    actionsBuilder = ImmutableArray.CreateBuilder<CodeAction>();
                actionsBuilder.Add(action);
            };
            var fixContext = new CodeFixContext(session.Roslyn.Document, diagnostic, registerCodeFix, cancellationToken);
            var providers = session.Roslyn.CodeFixProviders.GetValueOrDefault(diagnostic.Id);
            if (providers == null)
                return ImmutableArray<CodeAction>.Empty;

            foreach (var provider in providers) {
                await provider.RegisterCodeFixesAsync(fixContext).ConfigureAwait(false);
            }
            return actionsBuilder?.ToImmutable() ?? ImmutableArray<CodeAction>.Empty;
        }
    }
}
