using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using MirrorSharp.Advanced;
using MirrorSharp.Internal;
using MirrorSharp.Testing.Internal;
using MirrorSharp.Testing.Results;
using Newtonsoft.Json;

// ReSharper disable HeapView.ClosureAllocation
// ReSharper disable HeapView.DelegateAllocation
// ReSharper disable HeapView.ObjectAllocation

namespace MirrorSharp.Testing {
    public class MirrorSharpTestDriver {
        private static readonly LanguageManager LanguageManager = new LanguageManager(new[] {
            LanguageNames.CSharp,
            LanguageNames.VisualBasic,
            "F#"
        });

        private MirrorSharpTestDriver([CanBeNull] MirrorSharpOptions options = null, [CanBeNull] string languageName = LanguageNames.CSharp) {
            var language = LanguageManager.GetLanguage(languageName);

            Middleware = new TestMiddleware(options);
            Session = new WorkSession(language, options);
        }

        internal MiddlewareBase Middleware { get; }
        internal WorkSession Session { get; }

        [NotNull]
        public static MirrorSharpTestDriver New([CanBeNull] MirrorSharpOptions options = null, [CanBeNull] string languageName = LanguageNames.CSharp) {
            return new MirrorSharpTestDriver(options, languageName);
        }

        public MirrorSharpTestDriver SetText(string text) {
            Session.ReplaceText(text);
            return this;
        }

        public MirrorSharpTestDriver SetTextWithCursor(string textWithCursor) {
            var cursorPosition = textWithCursor.LastIndexOf('|');
            var text = textWithCursor.Remove(cursorPosition, 1);

            Session.ReplaceText(text);
            Session.CursorPosition = cursorPosition;
            return this;
        }

        [PublicAPI]
        public async Task SendTypeCharsAsync(string value) {
            foreach (var @char in value) {
                await SendAsync(CommandIds.TypeChar, @char);
            }
        }

        [PublicAPI]
        public Task<SlowUpdateResult<object>> SendSlowUpdateAsync() => SendSlowUpdateAsync<object>();

        [PublicAPI]
        public Task<SlowUpdateResult<TExtensionResult>> SendSlowUpdateAsync<TExtensionResult>()
            where TExtensionResult : class
        {
            return SendAsync<SlowUpdateResult<TExtensionResult>>(CommandIds.SlowUpdate);
        }

        [PublicAPI]
        public Task<OptionsEchoResult> SendSetOptionsAsync(IDictionary<string, string> options) {
            return SendAsync<OptionsEchoResult>(CommandIds.SetOptions, string.Join(",", options.Select(o => $"{o.Key}={o.Value}")));
        }

        internal async Task<TResult> SendAsync<TResult>(char commandId, HandlerTestArgument argument = null)
            where TResult : class
        {
            var sender = new StubCommandResultSender();
            await Middleware.GetHandler(commandId).ExecuteAsync(argument?.ToAsyncData(commandId) ?? AsyncData.Empty, Session, sender, CancellationToken.None);
            return sender.LastMessageJson != null ? JsonConvert.DeserializeObject<TResult>(sender.LastMessageJson) : null;
        }

        internal Task SendAsync(char commandId, HandlerTestArgument argument = default(HandlerTestArgument)) {
            return Middleware.GetHandler(commandId).ExecuteAsync(argument?.ToAsyncData(commandId) ?? AsyncData.Empty, Session, new StubCommandResultSender(), CancellationToken.None);
        }

        private class TestMiddleware : MiddlewareBase {
            public TestMiddleware([CanBeNull] MirrorSharpOptions options) : base(LanguageManager, options) {
            }
        }
    }
}
