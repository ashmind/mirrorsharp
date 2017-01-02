using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Advanced;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Languages;
using MirrorSharp.Testing.Internal;
using Newtonsoft.Json;

namespace MirrorSharp.Testing {
    public class MirrorSharpTestDriver {
        private static readonly CSharpLanguage CSharp = new CSharpLanguage();
        private static readonly VisualBasicLanguage VisualBasic = new VisualBasicLanguage();

        private MirrorSharpTestDriver([CanBeNull] MirrorSharpOptions options = null, [CanBeNull] string languageName = LanguageNames.CSharp) {
            var language = new ILanguage[] { CSharp, VisualBasic }.First(l => l.Name == languageName);

            Middleware = new TestMiddleware(options);
            Session = new WorkSession(language, options);
        }

        internal MiddlewareBase Middleware { get; }
        internal WorkSession Session { get; }

        [NotNull]
        public static MirrorSharpTestDriver New([CanBeNull] MirrorSharpOptions options = null, [CanBeNull] string languageName = LanguageNames.CSharp) {
            return new MirrorSharpTestDriver(options, languageName);
        }

        public MirrorSharpTestDriver SetTextWithCursor(string textWithCursor) {
            var cursorPosition = textWithCursor.LastIndexOf('|');
            var text = textWithCursor.Remove(cursorPosition, 1);

            Session.SourceText = SourceText.From(text);
            Session.CursorPosition = cursorPosition;
            return this;
        }

        public MirrorSharpTestDriver SetText(string text) {
            Session.SourceText = SourceText.From(text);
            return this;
        }

        public async Task TypeCharsAsync(string value) {
            foreach (var @char in value) {
                await SendAsync(CommandIds.TypeChar, @char);
            }
        }

        internal async Task<TResult> SendAsync<TResult>(char commandId, HandlerTestArgument argument = default(HandlerTestArgument))
            where TResult : class
        {
            var sender = new StubCommandResultSender();
            await Middleware.GetHandler(commandId).ExecuteAsync(argument.ToArraySegment(), Session, sender, CancellationToken.None);
            return sender.LastMessageJson != null ? JsonConvert.DeserializeObject<TResult>(sender.LastMessageJson) : null;
        }

        internal Task SendAsync(char commandId, HandlerTestArgument argument = default(HandlerTestArgument)) {
            return Middleware.GetHandler(commandId).ExecuteAsync(argument.ToArraySegment(), Session, new StubCommandResultSender(), CancellationToken.None);
        }

        private class TestMiddleware : MiddlewareBase {
            public TestMiddleware([CanBeNull] MirrorSharpOptions options) : base(CSharp, VisualBasic, options) {
            }
        }
    }
}
