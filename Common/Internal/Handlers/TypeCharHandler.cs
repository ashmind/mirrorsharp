using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal.Handlers.Shared;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    internal class TypeCharHandler : ICommandHandler {
        public char CommandId => CommandIds.TypeChar;
        private readonly ITypedCharEffects _effects;

        public TypeCharHandler(ITypedCharEffects effects) {
            _effects = effects;
        }

        public Task ExecuteAsync(AsyncData data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var @char = FastConvert.Utf8ByteArrayToChar(data.GetFirst());
            session.ReplaceText(FastConvert.CharToString(@char), session.CursorPosition, 0);
            session.CursorPosition += 1;

            return _effects.ApplyTypedCharAsync(@char, session, sender, cancellationToken);
        }
    }
}