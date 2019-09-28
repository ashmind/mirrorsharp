using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers.Shared {
    internal class TypedCharEffects : ITypedCharEffects {
        private readonly ICompletionSupport _completion;
        private readonly ISignatureHelpSupport _signatureHelp;

        public TypedCharEffects(ICompletionSupport completion, ISignatureHelpSupport signatureHelp) {
            _completion = completion;
            _signatureHelp = signatureHelp;
        }

        public async Task ApplyTypedCharAsync(char @char, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            await _completion.ApplyTypedCharAsync(@char, session, sender, cancellationToken).ConfigureAwait(false);
            await _signatureHelp.ApplyTypedCharAsync(@char, session, sender, cancellationToken).ConfigureAwait(false);
        }
    }
}