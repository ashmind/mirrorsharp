using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MirrorSharp.Internal.Handlers.Shared;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    internal class SignatureHelpStateHandler : ICommandHandler {
        public char CommandId => CommandIds.SignatureHelpState;
        [NotNull] private readonly ISignatureHelpSupport _signatureHelp;

        public SignatureHelpStateHandler([NotNull] ISignatureHelpSupport signatureHelp) {
            _signatureHelp = signatureHelp;
        }

        public Task ExecuteAsync(AsyncData data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var @char = FastConvert.Utf8ByteArrayToChar(data.GetFirst());
            if (@char != 'F') {
                // ReSharper disable once HeapView.BoxingAllocation
                throw new FormatException($"Unknown SignatureHelp command '{@char}'.");
            }

            return _signatureHelp.ForceSignatureHelpAsync(session, sender, cancellationToken);
        }
    }
}
