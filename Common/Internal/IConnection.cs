using System;
using System.Threading;
using System.Threading.Tasks;

namespace MirrorSharp.Internal {
    internal interface IConnection : IDisposable {
        bool IsConnected { get; }

        Task ReceiveAndProcessAsync(CancellationToken cancellationToken);
    }
}
