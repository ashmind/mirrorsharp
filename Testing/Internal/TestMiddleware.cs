using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using MirrorSharp.Internal;

namespace MirrorSharp.Testing.Internal {
    internal class TestMiddleware : MiddlewareBase {
        public TestMiddleware(LanguageManager languageManager, IMiddlewareOptions options, ImmutableExtensionServices extensions)
            : base(languageManager, options, extensions) {
        }

        public Task WebSocketLoopAsync(TestWebSocket socket, CancellationToken cancellationToken) {
            return base.WebSocketLoopAsync(socket, cancellationToken);
        }
    }
}
