using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    internal class ExtensionCommandHandler : ICommandHandler {
        private readonly IReadOnlyDictionary<string, ICommandExtension> _extensions;

        public char CommandId => CommandIds.ExtensionCommand;

        public ExtensionCommandHandler(IReadOnlyCollection<ICommandExtension> extensions) {
            _extensions = extensions.ToDictionary(e => e.Name);
        }

        public Task ExecuteAsync(AsyncData data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var first = data.GetFirst();
            var (name, offsetShift) = ReadExtensionName(first);
            var extension = _extensions[name];

            var newFirst = new ArraySegment<byte>(
                first.Array,
                first.Offset + offsetShift,
                first.Count - offsetShift
            );
            return extension.ExecuteAsync(data.WithNewFirst(newFirst), session, sender, cancellationToken);
        }

        private (string name, int offsetShift) ReadExtensionName(ArraySegment<byte> segment) {
            var endOffset = segment.Offset + segment.Count - 1;
            for (var i = segment.Offset; i <= endOffset; i++) {
                if (segment.Array[i] == (byte)':') {
                    var nameLength = i - segment.Offset;
                    var part = new ArraySegment<byte>(segment.Array, segment.Offset, nameLength);
                    return (Encoding.UTF8.GetString(part), nameLength + 1 /* 1=':' */);
                }
            }

            throw new Exception("Command arguments must be 'name:...'.");
        }
    }
}
