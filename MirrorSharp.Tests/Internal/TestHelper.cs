using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Advanced;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Handlers;
using MirrorSharp.Internal.Languages;
using Newtonsoft.Json;

namespace MirrorSharp.Tests.Internal {
    internal static class TestHelper {
        public static readonly string[] ObjectMemberNames = {
            nameof(Equals),
            nameof(GetHashCode),
            nameof(GetType),
            nameof(ToString)
        };

        public static readonly CSharpLanguage CSharp = new CSharpLanguage();
        public static readonly VisualBasicLanguage VisualBasic = new VisualBasicLanguage();

        public static Task ExecuteHandlerAsync<TCommandHandler>(WorkSession session, HandlerTestArgument argument = default(HandlerTestArgument), MirrorSharpOptions options = null)
            where TCommandHandler: ICommandHandler
        {
            var handler = new CommandFactory(options).Create<TCommandHandler>();
            return handler.ExecuteAsync(argument.ToArraySegment(), session, new StubCommandResultSender(), CancellationToken.None);
        }

        public static async Task<TResult> ExecuteHandlerAsync<TCommandHandler, TResult>(WorkSession session, HandlerTestArgument argument = default(HandlerTestArgument), MirrorSharpOptions options = null)
            where TCommandHandler : ICommandHandler
            where TResult : class
        {
            var sender = new StubCommandResultSender();
            var handler = new CommandFactory(options).Create<TCommandHandler>();
            await handler.ExecuteAsync(argument.ToArraySegment(), session, sender, CancellationToken.None);
            return sender.LastMessageJson != null ? JsonConvert.DeserializeObject<TResult>(sender.LastMessageJson) : null;
        }

        public static WorkSession Session() => new WorkSession(CSharp);
        public static WorkSession SessionFromText(string text, IWorkSessionOptions options = null, ILanguage language = null) => new WorkSession(language ?? CSharp, options) {
            SourceText = SourceText.From(text)
        };

        public static WorkSession SessionFromTextWithCursor(string textWithCursor) {
            var cursorPosition = textWithCursor.LastIndexOf('|');
            var text = textWithCursor.Remove(cursorPosition, 1);

            var session = new WorkSession(CSharp) {
                CursorPosition = cursorPosition,
                SourceText = SourceText.From(text)
            };
            return session;
        }

        public static async Task TypeCharsAsync(WorkSession session, string value) {
            foreach (var @char in value) {
                await ExecuteHandlerAsync<TypeCharHandler>(session, @char);
            }
        }

        private class CommandFactory : MiddlewareBase {
            public CommandFactory(MirrorSharpOptions options = null) : base(CSharp, VisualBasic, options) {
            }

            public TCommandHandler Create<TCommandHandler>() {
                return CreateHandlers().OfType<TCommandHandler>().First();
            }
        }
    }
}
