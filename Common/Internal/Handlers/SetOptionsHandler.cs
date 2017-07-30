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
        private const string LanguageOptionName = "language";
        private static readonly char[] Comma = { ',' };
        private static readonly char[] EqualsSign = { '=' };

        [NotNull] private readonly LanguageManager _languageManager;
        [NotNull] private readonly ArrayPool<char> _charArrayPool;
        [CanBeNull] private readonly ISetOptionsFromClientExtension _extension;

        public char CommandId => CommandIds.SetOptions;

        internal SetOptionsHandler(
            [NotNull] LanguageManager languageManager,
            [NotNull] ArrayPool<char> charArrayPool,
            [CanBeNull] ISetOptionsFromClientExtension extension = null
        ) {
            _languageManager = languageManager;
            _charArrayPool = charArrayPool;
            _extension = extension;
        }

        public async Task ExecuteAsync(AsyncData data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            // this doesn't happen too often, so microptimizations are not required
            var optionsString = await AsyncDataConvert.ToUtf8StringAsync(data, 0, _charArrayPool).ConfigureAwait(false);
            var options = optionsString
                .Split(Comma)
                .Select(p => p.Split(EqualsSign))
                .ToDictionary(p => p[0], p => p[1]);

            if (options.TryGetValue(LanguageOptionName, out var language)) {
                // this has to be done first, as it might reset other options
                SetLanguage(session, language);
                session.RawOptionsFromClient[LanguageOptionName] = language;
            }

            foreach (var option in options) {
                var (name, value) = (option.Key, option.Value);
                if (name == LanguageOptionName)
                    continue;

                if (name.StartsWith("x-")) {
                    if (!(_extension?.TrySetOption(session, name, value) ?? false))
                        throw new FormatException($"Extension option '{name}' was not recognized.");
                    session.RawOptionsFromClient[name] = value;
                    continue;
                }
                
                throw new FormatException($"Option '{name}' was not recognized (to use {nameof(ISetOptionsFromClientExtension)}, make sure your option name starts with 'x-').");
            }

            await SendOptionsEchoAsync(session, sender, cancellationToken).ConfigureAwait(false);
        }

        private void SetLanguage(WorkSession session, string value) {
            var language = _languageManager.GetLanguage(value);
            session.ChangeLanguage(language);
        }

        private async Task SendOptionsEchoAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            if (session.IsRoslyn)
                session.Roslyn.CurrentCodeActions.Clear();
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

