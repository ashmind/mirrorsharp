using System;
using System.Buffers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MirrorSharp.Internal.Handlers.Shared;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    internal class ReplaceTextHandler : ICommandHandler {
        public char CommandId => CommandIds.ReplaceText;
        [NotNull] private readonly ISignatureHelpSupport _signatureHelp;
        [NotNull] private readonly ICompletionSupport _completion;
        [NotNull] private readonly ITypedCharEffects _typedCharEffects;
        [NotNull] private readonly ArrayPool<char> _charArrayPool;

        public ReplaceTextHandler(
            [NotNull] ISignatureHelpSupport signatureHelp,
            [NotNull] ICompletionSupport completion,
            [NotNull] ITypedCharEffects typedCharEffects,
            [NotNull] ArrayPool<char> charArrayPool
        ) {
            _signatureHelp = signatureHelp;
            _completion = completion;
            _typedCharEffects = typedCharEffects;
            _charArrayPool = charArrayPool;
        }

        public async Task ExecuteAsync(AsyncData data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var first = data.GetFirst();
            var endOffset = first.Offset + first.Count - 1;
            var partStart = first.Offset;

            int? start = null;
            int? length = null;
            int? cursorPosition = null;
            string reason = null;

            for (var i = first.Offset; i <= endOffset; i++) {
                if (first.Array[i] != (byte)':')
                    continue;

                var part = new ArraySegment<byte>(first.Array, partStart, i - partStart);
                if (start == null) {
                    start = FastConvert.Utf8ByteArrayToInt32(part);
                    partStart = i + 1;
                    continue;
                }

                if (length == null) {
                    length = FastConvert.Utf8ByteArrayToInt32(part);
                    partStart = i + 1;
                    continue;
                }

                if (cursorPosition == null) {
                    cursorPosition = FastConvert.Utf8ByteArrayToInt32(part);
                    partStart = i + 1;
                    continue;
                }

                reason = part.Count > 0 ? Encoding.UTF8.GetString(part) : string.Empty;
                partStart = i + 1;
                break;
            }
            if (start == null || length == null || cursorPosition == null || reason == null)
                throw new FormatException("Command arguments must be 'start:length:cursor:reason:text'.");

            var text = await AsyncDataConvert.ToUtf8StringAsync(data, partStart - first.Offset, _charArrayPool).ConfigureAwait(false);

            session.ReplaceText(text, start.Value, length.Value);
            session.CursorPosition = cursorPosition.Value;
            await _signatureHelp.ApplyCursorPositionChangeAsync(session, sender, cancellationToken).ConfigureAwait(false);
            await _completion.ApplyReplacedTextAsync(reason, _typedCharEffects, session, sender, cancellationToken).ConfigureAwait(false);
        }
    }
}
