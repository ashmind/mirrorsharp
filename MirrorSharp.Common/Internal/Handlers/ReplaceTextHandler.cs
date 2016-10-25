using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    public class ReplaceTextHandler : ICommandHandler {
        public IImmutableList<char> CommandIds { get; } = ImmutableList.Create('P', 'R');

        public Task ExecuteAsync(ArraySegment<byte> data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var endOffset = data.Offset + data.Count - 1;
            var partStart = data.Offset;

            int? start = null;
            int? length = null;
            int? cursorPosition = null;

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

                cursorPosition = FastConvert.Utf8ByteArrayToInt32(part);
                partStart = i + 1;
                break;
            }
            if (start == null || length == null || cursorPosition == null)
                throw new FormatException("Command arguments must be 'start:length:cursor:text'.");

            var text = Encoding.UTF8.GetString(data.Array, partStart, endOffset - partStart + 1);

            session.SourceText = session.SourceText.WithChanges(new TextChange(new TextSpan(start.Value, length.Value), text));
            session.CursorPosition = cursorPosition.Value;
            return TaskEx.CompletedTask;
        }

        public bool CanChangeSession => true;
    }
}
