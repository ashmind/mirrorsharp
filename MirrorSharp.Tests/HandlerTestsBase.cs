using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Commands;
using Moq;
using Newtonsoft.Json;

namespace MirrorSharp.Tests {
    public abstract class HandlerTestsBase<THandler>
        where THandler : ICommandHandler, new()
    {
        private readonly ICommandResultSender _commandResultSender;
        private string _lastJsonMessage;

        protected HandlerTestsBase() {
            // ReSharper disable once VirtualMemberCallInConstructor
            _commandResultSender = MockCommandResultSender();
        }

        protected virtual Task ExecuteAsync(ICommandHandler handler, WorkSession session, ArraySegment<byte> data) {
            return handler.ExecuteAsync(data, session, _commandResultSender, CancellationToken.None);
        }

        protected virtual Task ExecuteAsync(WorkSession session, ArraySegment<byte> data) {
            return ExecuteAsync(new THandler(), session, data);
        }

        protected virtual async Task<TResult> ExecuteAndCaptureResultAsync<TResult>(ICommandHandler handler, WorkSession session, ArraySegment<byte> data = default(ArraySegment<byte>)) {
            await ExecuteAsync(handler, session, data);
            return JsonConvert.DeserializeObject<TResult>(_lastJsonMessage);
        }

        protected virtual Task<TResult> ExecuteAndCaptureResultAsync<TResult>(WorkSession session, ArraySegment<byte> data = default(ArraySegment<byte>)) {
            return ExecuteAndCaptureResultAsync<TResult>(new THandler(), session, data);
        }

        protected virtual ICommandResultSender MockCommandResultSender() {
            var sender = new Mock<ICommandResultSender>();
            var writer = new StringWriter();
            sender.Setup(m => m.StartJsonMessage(It.IsAny<string>()))
                  .Returns(new JsonTextWriter(writer));
            sender.Setup(m => m.SendJsonMessageAsync(It.IsAny<CancellationToken>()))
                  .Callback(() => _lastJsonMessage = "{" + writer + "}")
                  .Returns(TaskEx.CompletedTask);
            sender.SetReturnsDefault(TaskEx.CompletedTask);

            return sender.Object;
        }
    }
}
