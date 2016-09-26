using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace MirrorSharp.Internal.Commands {
    public class MoveCursorHandler : ICommandHandler {
        public IImmutableList<char> CommandIds => ImmutableList.Create('M');

        public Task ExecuteAsync(ArraySegment<byte> data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            session.CursorPosition = FastConvert.Utf8ByteArrayToInt32(data);
            return TaskEx.CompletedTask;
        }

        public bool CanChangeSession => true;
    }
}
