using System;
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
            var charSpan = data.GetFirst().Span;
            if (charSpan.IsEmpty) // Diagnosing https://github.com/ashmind/SharpLab/issues/682 -- once that's resolved this can be removed
                throw new ArgumentException($"Attempted to call TypeChar with empty char. MightHaveNext: {data.MightHaveNext}.", nameof(data));

            var @char = FastConvert.Utf8BytesToChar(charSpan);
            session.ReplaceText(FastConvert.CharToString(@char), session.CursorPosition, 0);
            session.CursorPosition += 1;

            return _effects.ApplyTypedCharAsync(@char, session, sender, cancellationToken);
        }
    }
}