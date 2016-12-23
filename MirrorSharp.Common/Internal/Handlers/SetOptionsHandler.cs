using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AshMind.Extensions;
using MirrorSharp.Internal.Languages;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    public class SetOptionsHandler : ICommandHandler {
        private static readonly char[] Comma = { ',' };
        private static readonly char[] EqualsSign = { '=' };

        private readonly IReadOnlyDictionary<string, Action<WorkSession, string>> _optionSetters;
        private readonly IReadOnlyCollection<ILanguage> _languages;

        public IImmutableList<char> CommandIds { get; } = ImmutableList.Create('O');

        internal SetOptionsHandler(IReadOnlyCollection<ILanguage> languages) {
            _optionSetters = new Dictionary<string, Action<WorkSession, string>> {
                { "language", SetLanguage }
            };
            _languages = languages;
        }

        public Task ExecuteAsync(ArraySegment<byte> data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            // this doesn't happen too often, so microptimizations are not required
            var optionsString = Encoding.UTF8.GetString(data);
            var parts = optionsString.Split(Comma);
            foreach (var part in parts) {
                var nameAndValue = part.Split(EqualsSign);
                var setOption = _optionSetters.GetValueOrDefault(nameAndValue[0]);
                if (setOption == null)
                    throw new FormatException("Option '" + nameAndValue[0] + "' was not recognized.");
                setOption(session, nameAndValue[1]);
            }
            return TaskEx.CompletedTask;
        }

        private void SetLanguage(WorkSession session, string value) {
            var language = _languages.FirstOrDefault(l => l.Name == value);
            if (language == null)
                throw new NotSupportedException($"Language '{value}' is not supported.");

            session.ChangeLanguage(language);
        }

        public bool CanChangeSession => true;
    }
}

