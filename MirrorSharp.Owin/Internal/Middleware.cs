using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MirrorSharp.Advanced;

namespace MirrorSharp.Owin.Internal {
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using WebSocketAccept = Action<IDictionary<string, object>, Func<IDictionary<string, object>, Task>>;

    internal class Middleware : MiddlewareBase {
        private static readonly Task Done = Task.FromResult((object) null);

        private readonly AppFunc _next;

        public Middleware([NotNull] AppFunc next, [CanBeNull] MirrorSharpOptions options) : base(options) {
            _next = Argument.NotNull(nameof(next), next);
        }

        [UsedImplicitly]
        public Task Invoke(IDictionary<string, object> environment) {
            object accept;
            if (!environment.TryGetValue("websocket.Accept", out accept))
                return _next(environment);

            ((WebSocketAccept) accept)(null, e => {
                var socket = new OwinWebSocket(e);
                return Task.WhenAny(WebSocketLoopAsync(socket), socket.AbortedTask);
            });
            return Done;
        }


    }
}
