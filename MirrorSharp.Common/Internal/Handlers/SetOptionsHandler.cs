using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AshMind.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using MirrorSharp.Advanced;
using MirrorSharp.Internal.Languages;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    public class SetOptionsHandler : ICommandHandler {
        private static readonly char[] Comma = { ',' };
        private static readonly char[] EqualsSign = { '=' };

        private readonly IReadOnlyDictionary<string, Action<WorkSession, string>> _optionSetters;
        private readonly IReadOnlyCollection<ILanguage> _languages;
        [CanBeNull] private readonly ISetOptionsFromClientExtension _extension;

        public IImmutableList<char> CommandIds { get; } = ImmutableList.Create('O');

        internal SetOptionsHandler([NotNull] IReadOnlyCollection<ILanguage> languages, [CanBeNull] ISetOptionsFromClientExtension extension) {
            _optionSetters = new Dictionary<string, Action<WorkSession, string>> {
                { "language", SetLanguage },
                { "optimize", SetOptimize }
            };
            _languages = languages;
            _extension = extension;
        }

        public Task ExecuteAsync(ArraySegment<byte> data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            // this doesn't happen too often, so microptimizations are not required
            var optionsString = Encoding.UTF8.GetString(data);
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

                var setOption = _optionSetters.GetValueOrDefault(name);
                if (setOption == null)
                    throw new FormatException($"Option '{name}' was not recognized (to use {nameof(ISetOptionsFromClientExtension)}, make sure your option name starts with 'x-').");
                setOption(session, value);
                session.RawOptionsFromClient[name] = value;
            }

            return SendOptionsEchoAsync(session, sender, cancellationToken);
        }

        private void SetLanguage(WorkSession session, string value) {
            var language = _languages.FirstOrDefault(l => l.Name == value);
            if (language == null)
                throw new NotSupportedException($"Language '{value}' is not supported.");

            session.ChangeLanguage(language);
        }

        private void SetOptimize(WorkSession session, string value) {
            var level = (OptimizationLevel)Enum.Parse(typeof(OptimizationLevel), value, true);
            session.ChangeCompilationOptions(nameof(CompilationOptions.OptimizationLevel), o => o.WithOptimizationLevel(level));
        }

        private async Task SendOptionsEchoAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            session.CurrentCodeActions.Clear();
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

