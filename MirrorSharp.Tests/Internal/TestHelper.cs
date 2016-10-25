using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Handlers;
using Newtonsoft.Json;

namespace MirrorSharp.Tests.Internal {
    public static class TestHelper {
        public static Task ExecuteHandlerAsync<TCommandHandler>(WorkSession session, HandlerTestArgument argument = default(HandlerTestArgument))
            where TCommandHandler: ICommandHandler, new()
        {
            return new TCommandHandler().ExecuteAsync(argument.ToArraySegment(), session, new StubCommandResultSender(), CancellationToken.None);
        }

        public static async Task<TResult> ExecuteHandlerAsync<TCommandHandler, TResult>(WorkSession session, HandlerTestArgument argument = default(HandlerTestArgument))
            where TCommandHandler : ICommandHandler, new()
            where TResult : class
        {
            var sender = new StubCommandResultSender();
            await new TCommandHandler().ExecuteAsync(argument.ToArraySegment(), session, sender, CancellationToken.None);
            return sender.LastMessageJson != null ? JsonConvert.DeserializeObject<TResult>(sender.LastMessageJson) : null;
        }

        public static WorkSession SessionFromTextWithCursor(string textWithCursor) {
            var cursorPosition = textWithCursor.LastIndexOf('|');
            var text = textWithCursor.Remove(cursorPosition, 1);

            var session = new WorkSession {
                CursorPosition = cursorPosition,
                SourceText = SourceText.From(text)
            };
            return session;
        }
    }
}
