using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MirrorSharp.Internal;
using MirrorSharp.Testing.Internal;
using MirrorSharp.Testing.Internal.Results;
using MirrorSharp.Testing.Results;
using Newtonsoft.Json;

// ReSharper disable HeapView.ClosureAllocation
// ReSharper disable HeapView.DelegateAllocation
// ReSharper disable HeapView.ObjectAllocation

namespace MirrorSharp.Testing {
    public class MirrorSharpTestDriver: IWorkSessionTracker {
        private static readonly MirrorSharpOptions DefaultOptions = new();
        private static readonly ConcurrentDictionary<MirrorSharpOptions, LanguageManager> LanguageManagerCache = new();

        private readonly TestMiddleware _middleware;
        private readonly MirrorSharpServices _services;

        private WorkSession? _session;

        private MirrorSharpTestDriver(MirrorSharpOptions? options = null, MirrorSharpServices? services = null, string languageName = LanguageNames.CSharp) {
            if (services?.SessionTracker != null)
                throw new ArgumentException("Custom session trackers are not supported.", nameof(services));

            options ??= DefaultOptions;
            services ??= new();

            services.SessionTracker = this;

            _services = services;
            _middleware = new TestMiddleware(GetLanguageManager(options), options, services.ToImmutable());
        }

        internal WorkSession Session => _session
            ?? throw new InvalidOperationException("Work session has not been captured yet.");

        // Obsolete: will be removed in the next major version. However no changes are required on caller side.
        public static MirrorSharpTestDriver New() {
            return new MirrorSharpTestDriver(options: null, services: null, languageName: LanguageNames.CSharp);
        }

        // Obsolete: will be removed in the next major version. However no changes are required on caller side.
        public static MirrorSharpTestDriver New(MirrorSharpOptions? options = null, string languageName = LanguageNames.CSharp) {
            return new MirrorSharpTestDriver(options, services: null, languageName);
        }

        public static MirrorSharpTestDriver New(MirrorSharpServices services) {
            return new MirrorSharpTestDriver(options: null, services: services, languageName: LanguageNames.CSharp);
        }

        public static MirrorSharpTestDriver New(MirrorSharpOptions options) {
            return new MirrorSharpTestDriver(options: options, services: null, languageName: LanguageNames.CSharp);
        }

        public static MirrorSharpTestDriver New(MirrorSharpOptions? options = null, MirrorSharpServices? services = null, string languageName = LanguageNames.CSharp) {
            return new MirrorSharpTestDriver(options, services, languageName);
        }

        public MirrorSharpTestDriver SetText(string text) {
            Session.ReplaceText(text);
            return this;
        }

        public MirrorSharpTestDriver SetTextWithCursor(string textWithCursor) {
            var parsed = TextWithCursor.Parse(textWithCursor);

            Session.ReplaceText(parsed.Text);
            Session.CursorPosition = parsed.CursorPosition;
            return this;
        }

        public async Task SendTypeCharsAsync(string value) {
            foreach (var @char in value) {
                await SendAsync(CommandIds.TypeChar, @char);
            }
        }

        public Task<SlowUpdateResult<object>> SendSlowUpdateAsync() => SendSlowUpdateAsync<object>();

        public Task<SlowUpdateResult<TExtensionResult>> SendSlowUpdateAsync<TExtensionResult>() {
            return SendWithRequiredResultAsync<SlowUpdateResult<TExtensionResult>>(CommandIds.SlowUpdate);
        }

        public Task<OptionsEchoResult> SendSetOptionAsync(string name, string value) {
            return SendWithRequiredResultAsync<OptionsEchoResult>(CommandIds.SetOptions, $"{name}={value}");
        }

        public Task<OptionsEchoResult> SendSetOptionsAsync(IDictionary<string, string> options) {
            return SendWithRequiredResultAsync<OptionsEchoResult>(CommandIds.SetOptions, string.Join(",", options.Select(o => $"{o.Key}={o.Value}")));
        }

        public Task<InfoTipResult?> SendRequestInfoTipAsync(int position) {
            return SendWithOptionalResultAsync<InfoTipResult>(CommandIds.RequestInfoTip, position);
        }

        internal Task SendReplaceTextAsync(string newText, int start = 0, int length = 0, int newCursorPosition = 0, string reason = "") {
            // ReSharper disable HeapView.BoxingAllocation
            return SendAsync(CommandIds.ReplaceText, $"{start}:{length}:{newCursorPosition}:{reason}:{newText}");
            // ReSharper restore HeapView.BoxingAllocation
        }

        internal Task<CompletionsResult?> SendTypeCharAsync(char @char) {
            return SendWithOptionalResultAsync<CompletionsResult>(CommandIds.TypeChar, @char);
        }

        internal Task SendBackspaceAsync() {
            return SendReplaceTextAsync("", Session.CursorPosition - 1, 1, Session.CursorPosition - 1);
        }

        internal async Task<TResult> SendWithRequiredResultAsync<TResult>(char commandId, HandlerTestArgument? argument = null)
            where TResult: class
        {
            return await SendWithOptionalResultAsync<TResult>(commandId, argument)
                ?? throw new Exception($"Expected {typeof(TResult).Name} for command {commandId} was not received.");
        }

        internal async Task<TResult?> SendWithOptionalResultAsync<TResult>(char commandId, HandlerTestArgument? argument = null)
            where TResult : class
        {
            var socket = new TestSocket();
            socket.SetupToReceive(argument?.ToAsyncData(commandId));

            await _middleware.GetHandler(commandId).ExecuteAsync(argument?.ToAsyncData(commandId) ?? AsyncData.Empty, Session, sender, CancellationToken.None);
            return sender.LastMessageJson != null
                ? JsonConvert.DeserializeObject<TResult>(sender.LastMessageJson)
                : null;
        }

        internal Task SendAsync(char commandId, HandlerTestArgument? argument = default(HandlerTestArgument)) {
            return _middleware.GetHandler(commandId).ExecuteAsync(
                argument?.ToAsyncData(commandId) ?? AsyncData.Empty,
                Session,
                new StubCommandResultSender(Session, _services.ConnectionSendViewer),
                CancellationToken.None
            );
        }

        private static LanguageManager GetLanguageManager(MirrorSharpOptions options) {
            return LanguageManagerCache.GetOrAdd(options, _ => new LanguageManager(options));
        }

        void IWorkSessionTracker.TrackNewWorkSession(WorkSession session) {
            if (_session != null)
                throw new InvalidOperationException("Attempted to capture work session twice for the same driver.");

            _session = session;
        }
    }
}
