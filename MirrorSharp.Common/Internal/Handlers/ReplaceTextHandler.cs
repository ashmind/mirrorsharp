using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Internal.Handlers.Shared;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    internal class ReplaceTextHandler : ICommandHandler {
        public char CommandId => 'R';
        [NotNull] private readonly ISignatureHelpSupport _signatureHelp;
        [NotNull] private readonly ICompletionSupport _completion;

        public ReplaceTextHandler([NotNull] ISignatureHelpSupport signatureHelp, [NotNull] ICompletionSupport completion) {
            _signatureHelp = signatureHelp;
            _completion = completion;
        }

        public async Task ExecuteAsync(ArraySegment<byte> data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var endOffset = data.Offset + data.Count - 1;
            var partStart = data.Offset;

            int? start = null;
            int? length = null;
            int? cursorPosition = null;
            string trigger = null;

            for (var i = data.Offset; i <= endOffset; i++) {
                if (data.Array[i] != (byte)':')
                    continue;

                var part = new ArraySegment<byte>(data.Array, partStart, i - partStart);
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

                trigger = part.Count > 0 ? Encoding.UTF8.GetString(part.Array, part.Offset, part.Count) : string.Empty;
                partStart = i + 1;
                break;
            }
            if (start == null || length == null || cursorPosition == null || trigger == null)
                throw new FormatException("Command arguments must be 'start:length:cursor:trigger:text'.");

            var text = Encoding.UTF8.GetString(data.Array, partStart, endOffset - partStart + 1);

            session.SourceText = session.SourceText.WithChanges(new TextChange(new TextSpan(start.Value, length.Value), text));
            session.CursorPosition = cursorPosition.Value;
            await _signatureHelp.ApplyCursorPositionChangeAsync(session, sender, cancellationToken).ConfigureAwait(false);
            await _completion.ApplyReplacedTextAsync(trigger, session, sender, cancellationToken).ConfigureAwait(false);
        }
    }
}
