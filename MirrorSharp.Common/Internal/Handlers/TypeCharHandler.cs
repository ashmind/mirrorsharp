using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Internal.Handlers.Shared;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    public class TypeCharHandler : ICommandHandler {
        public char CommandId => 'C';
        [NotNull] private readonly ICompletionSupport _completion;
        [NotNull] private readonly ISignatureHelpSupport _signatureHelp;

        public TypeCharHandler([NotNull] ICompletionSupport completion, [NotNull] ISignatureHelpSupport signatureHelp) {
            _completion = completion;
            _signatureHelp = signatureHelp;
        }

        public async Task ExecuteAsync(ArraySegment<byte> data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var @char = FastConvert.Utf8ByteArrayToChar(data);
            session.SourceText = session.SourceText.WithChanges(
                new TextChange(new TextSpan(session.CursorPosition, 0), FastConvert.CharToString(@char))
            );
            session.CursorPosition += 1;

            await _completion.ApplyTypedCharAsync(@char, session, sender, cancellationToken).ConfigureAwait(false);
            await _signatureHelp.ApplyTypedCharAsync(@char, session, sender, cancellationToken).ConfigureAwait(false);
        }
    }
}