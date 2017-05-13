using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using MirrorSharp.Advanced;
using MirrorSharp.Internal.Results;

// ReSharper disable HeapView.DelegateAllocation
// ReSharper disable HeapView.ClosureAllocation

namespace MirrorSharp.Internal.Handlers {
    internal class SetOptionsHandler : ICommandHandler {
        private static readonly char[] Comma = { ',' };
        private static readonly char[] EqualsSign = { '=' };

        private readonly IReadOnlyDictionary<string, Action<WorkSession, string>> _optionSetters;
        [NotNull] private readonly LanguageManager _languageManager;
        [NotNull] private readonly ArrayPool<char> _charArrayPool;
        [CanBeNull] private readonly ISetOptionsFromClientExtension _extension;

        public char CommandId => CommandIds.SetOptions;

        internal SetOptionsHandler(
            [NotNull] LanguageManager languageManager,
            [NotNull] ArrayPool<char> charArrayPool,
            [CanBeNull] ISetOptionsFromClientExtension extension = null
        ) {
            _optionSetters = new Dictionary<string, Action<WorkSession, string>> {
                { "language", SetLanguage },
                { "optimize", SetOptimize }
            };
            _languageManager = languageManager;
            _charArrayPool = charArrayPool;
            _extension = extension;
        }

        public async Task ExecuteAsync(AsyncData data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            // this doesn't happen too often, so microptimizations are not required
            var optionsString = await AsyncDataConvert.ToUtf8StringAsync(data, 0, _charArrayPool).ConfigureAwait(false);
            var parts = optionsString.Split(Comma);
            foreach (var part in parts) {
                var nameAndValue = part.Split(EqualsSign);
                var name = nameAndValue[0];
                var value = nameAndValue[1];

                if (name.StartsWith("x-")) {
                    if (!(_extension?.TrySetOption(session, name, value) ?? false))
                        throw new FormatException($"Extension option '{name}' was not recognized.");
                    session.RawOptionsFromClient[name] = value;
                    continue;
                }

                if (!_optionSetters.TryGetValue(name, out var setOption))
                    throw new FormatException($"Option '{name}' was not recognized (to use {nameof(ISetOptionsFromClientExtension)}, make sure your option name starts with 'x-').");
                setOption(session, value);
                session.RawOptionsFromClient[name] = value;
            }

            await SendOptionsEchoAsync(session, sender, cancellationToken).ConfigureAwait(false);
        }

        private void SetLanguage(WorkSession session, string value) {
            var language = _languageManager.GetLanguage(value);
            session.ChangeLanguage(language);
        }

        private void SetOptimize(WorkSession session, string value) {
            var level = (OptimizationLevel)Enum.Parse(typeof(OptimizationLevel), value, true);
            session.ChangeCompilationOptions(nameof(CompilationOptions.OptimizationLevel), o => o.WithOptimizationLevel(level));
        }

        private async Task SendOptionsEchoAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            session.RoslynOrNull?.CurrentCodeActions.Clear();
            var writer = sender.StartJsonMessage("optionsEcho");
            writer.WritePropertyStartObject("options");
            foreach (var pair in session.RawOptionsFromClient) {
                writer.WriteProperty(pair.Key, pair.Value);
            }
            writer.WriteEndObject();
            await sender.SendJsonMessageAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

