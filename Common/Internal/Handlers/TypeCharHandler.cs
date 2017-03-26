using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Internal.Handlers.Shared;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    internal class TypeCharHandler : ICommandHandler {
        public char CommandId => CommandIds.TypeChar;
        [NotNull] private readonly ITypedCharEffects _effects;

        public TypeCharHandler([NotNull] ITypedCharEffects effects) {
            _effects = effects;
        }

        public Task ExecuteAsync(AsyncData data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var @char = FastConvert.Utf8ByteArrayToChar(data.GetFirst());
            session.SourceText = session.SourceText.WithChanges(
                new TextChange(new TextSpan(session.CursorPosition, 0), FastConvert.CharToString(@char))
            );
            session.CursorPosition += 1;

            return _effects.ApplyTypedCharAsync(@char, session, sender, cancellationToken);
        }
    }
}