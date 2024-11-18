using System;
using System.Threading;
using System.Threading.Tasks;

namespace MirrorSharp.Advanced.EarlyAccess {
    internal interface IConnectionSendViewer {
        Task ViewDuringSendAsync(string messageTypeName, ReadOnlyMemory<byte> message, IWorkSession session, CancellationToken cancellationToken);
    }
}