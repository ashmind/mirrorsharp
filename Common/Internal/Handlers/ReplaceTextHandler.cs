using System;
using System.Buffers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal.Handlers.Shared;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    internal class ReplaceTextHandler : ICommandHandler {
        public char CommandId => CommandIds.ReplaceText;
        private readonly ISignatureHelpSupport _signatureHelp;
        private readonly ICompletionSupport _completion;
        private readonly ITypedCharEffects _typedCharEffects;
        private readonly ArrayPool<char> _charArrayPool;

        public ReplaceTextHandler(
            ISignatureHelpSupport signatureHelp,
            ICompletionSupport completion,
            ITypedCharEffects typedCharEffects,
            ArrayPool<char> charArrayPool
        ) {
            _signatureHelp = signatureHelp;
            _completion = completion;
            _typedCharEffects = typedCharEffects;
            _charArrayPool = charArrayPool;
        }

        public async Task ExecuteAsync(AsyncData data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var first = data.GetFirst();
            var partStart = 0;

            int? start = null;
            int? length = null;
            int? cursorPosition = null;
            string? reason = null;

            for (var i = 0; i < first.Length; i++) {
                if (first.Span[i] != (byte)':')
                    continue;

                var part = first.Slice(partStart, i - partStart);
                if (start == null) {
                    start = FastConvert.Utf8BytesToInt32(part.Span);
                    partStart = i + 1;
                    continue;
                }

                if (length == null) {
                    length = FastConvert.Utf8BytesToInt32(part.Span);
                    partStart = i + 1;
                    continue;
                }

                if (cursorPosition == null) {
                    cursorPosition = FastConvert.Utf8BytesToInt32(part.Span);
                    partStart = i + 1;
                    continue;
                }

                reason = part.Length > 0 ? Encoding.UTF8.GetString(part.Span) : string.Empty;
                partStart = i + 1;
                break;
            }
            if (start == null || length == null || cursorPosition == null || reason == null)
                throw new FormatException("Command arguments must be 'start:length:cursor:reason:text'.");

            var text = await AsyncDataConvert.ToUtf8StringAsync(data, partStart, _charArrayPool).ConfigureAwait(false);

            session.ReplaceText(text, start.Value, length.Value);
            session.CursorPosition = cursorPosition.Value;
            await _signatureHelp.ApplyCursorPositionChangeAsync(session, sender, cancellationToken).ConfigureAwait(false);
            await _completion.ApplyReplacedTextAsync(reason, _typedCharEffects, session, sender, cancellationToken).ConfigureAwait(false);
        }
    }
}
