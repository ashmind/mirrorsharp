using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MirrorSharp.Internal.Reflection;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers.Shared {
    internal class SignatureHelpSupport : ISignatureHelpSupport {
        public Task ApplyCursorPositionChangeAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var currentHelp = session.CurrentSignatureHelp;
            if (currentHelp == null)
                return TaskEx.CompletedTask;

            if (!currentHelp.Value.Items.ApplicableSpan.Contains(session.CursorPosition)) {
                session.CurrentSignatureHelp = null;
                return SendSignatureHelpAsync(null, sender, cancellationToken);
            }

            // not sure if there is any better way to recalculate the selected parameter only,
            // but doesn't seem so at the moment
            return TryApplySignatureHelpAsync(currentHelp.Value.Provider, new SignatureHelpTriggerInfoData(SignatureHelpTriggerReason.RetriggerCommand), session, sender, cancellationToken, true);
        }

        public Task ApplyTypedCharAsync(char @char, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var trigger = new SignatureHelpTriggerInfoData(SignatureHelpTriggerReason.TypeCharCommand, @char);
            if (session.CurrentSignatureHelp != null) {
                var provider = session.CurrentSignatureHelp.Value.Provider;
                if (provider.IsRetriggerCharacter(@char)) {
                    session.CurrentSignatureHelp = null;
                    return SendSignatureHelpAsync(null, sender, cancellationToken);
                }

                return TryApplySignatureHelpAsync(provider, trigger, session, sender, cancellationToken, sendIfEmpty: true);
            }

            return TryApplySignatureHelpAsync(session, sender, cancellationToken, trigger);
        }

        public Task ForceSignatureHelpAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var trigger = new SignatureHelpTriggerInfoData(SignatureHelpTriggerReason.InvokeSignatureHelpCommand);
            return TryApplySignatureHelpAsync(session, sender, cancellationToken, trigger);
        }

        private async Task TryApplySignatureHelpAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken, SignatureHelpTriggerInfoData trigger) {
            foreach (var provider in session.SignatureHelpProviders) {
                if (await TryApplySignatureHelpAsync(provider, trigger, session, sender, cancellationToken).ConfigureAwait(false))
                    return;
            }
        }

        private async Task<bool> TryApplySignatureHelpAsync(ISignatureHelpProviderWrapper provider, SignatureHelpTriggerInfoData trigger, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken, bool sendIfEmpty = false) {
            // ReSharper disable once PossibleInvalidOperationException
            if (trigger.TriggerReason == SignatureHelpTriggerReason.TypeCharCommand && !provider.IsTriggerCharacter(trigger.TriggerCharacter.Value))
                return false;

            var help = await provider.GetItemsAsync(session.Document, session.CursorPosition, trigger, cancellationToken).ConfigureAwait(false);
            if (!sendIfEmpty && help == null)
                return false;

            session.CurrentSignatureHelp = help != null ? new CurrentSignatureHelp(provider, help) : (CurrentSignatureHelp?)null;
            await SendSignatureHelpAsync(help, sender, cancellationToken).ConfigureAwait(false);
            return true;
        }

        private Task SendSignatureHelpAsync([CanBeNull] SignatureHelpItemsData items, ICommandResultSender sender, CancellationToken cancellationToken) {
            var writer = sender.StartJsonMessage("signatures");
            if (items == null)
                return sender.SendJsonMessageAsync(cancellationToken);

            var selectedItemIndex = items.SelectedItemIndex;
            writer.WritePropertyName("span");
            writer.WriteSpan(items.ApplicableSpan);
            writer.WritePropertyStartArray("signatures");
            var itemIndex = 0;
            foreach (var item in items.Items) {
                writer.WriteStartObject();
                if (selectedItemIndex == null && items.ArgumentCount <= item.ParameterCount)
                    selectedItemIndex = itemIndex;
                if (itemIndex == selectedItemIndex)
                    writer.WriteProperty("selected", true);
                writer.WritePropertyStartArray("parts");
                writer.WriteTaggedTexts(item.PrefixDisplayParts);
                var parameterIndex = 0;
                foreach (var parameter in item.Parameters) {
                    if (parameterIndex > 0)
                        writer.WriteTaggedTexts(item.SeparatorDisplayParts);
                    var selected = items.ArgumentIndex == parameterIndex;
                    writer.WriteTaggedTexts(parameter.PrefixDisplayParts, selected);
                    writer.WriteTaggedTexts(parameter.DisplayParts, selected);
                    writer.WriteTaggedTexts(parameter.SuffixDisplayParts, selected);
                    parameterIndex += 1;
                }
                writer.WriteTaggedTexts(item.SuffixDisplayParts);
                writer.WriteEndArray();
                writer.WriteEndObject();
                itemIndex += 1;
            }
            writer.WriteEndArray();
            return sender.SendJsonMessageAsync(cancellationToken);
        }
    }
}
