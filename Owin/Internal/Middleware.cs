using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal;

namespace MirrorSharp.Owin.Internal {
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using WebSocketAccept = Action<IDictionary<string, object>?, Func<IDictionary<string, object>, Task>>;

    internal class Middleware : MiddlewareBase {
        private readonly AppFunc _next;

        public Middleware(AppFunc next, MirrorSharpOptions options) : base(options) {
            _next = Argument.NotNull(nameof(next), next);
        }

        public Task Invoke(IDictionary<string, object> environment) {
            object accept;
            if (!environment.TryGetValue("websocket.Accept", out accept))
                return _next(environment);

            ((WebSocketAccept) accept)(null, async e => {
                var contextKey = typeof(WebSocketContext).FullName;
                if (!e.TryGetValue(contextKey, out var contextAsObject) || contextAsObject == null) {
                    throw new NotSupportedException(
                         $"At the moment, MirrorSharp requires Owin host to provide '{contextKey}'.\r\n" +
                          "It's not in the specification, but it is provided by the IIS host at least. " +
                          "After spending some time on this, I don't feel that a WebSocket wrapper for Owin " +
                          "is worth the effort. However if you want to implement one, I will appreciate it.\r\n" +
                          "You can find my attempt at https://gist.github.com/ashmind/40563ead5b467a243308a02d27c707ed."
                    );
                }

                var context = (WebSocketContext)contextAsObject;
                var callCancelled = (CancellationToken)e["websocket.CallCancelled"];
                // there is a weird issue where a socket never gets closed (deadlock?)
                // if the loop is done in the standard ASP.NET thread
                await Task.Run(
                    () => WebSocketLoopAsync(context.WebSocket, callCancelled),
                    callCancelled
                );
            });
            return Task.CompletedTask;
        }
    }
}