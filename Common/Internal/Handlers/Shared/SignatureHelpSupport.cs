using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MirrorSharp.Advanced;
using MirrorSharp.Internal.Reflection;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers.Shared {
    internal class SignatureHelpSupport : ISignatureHelpSupport {
        public Task ApplyCursorPositionChangeAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            if (!session.IsRoslyn)
                return Task.CompletedTask;

            var currentHelp = session.Roslyn.CurrentSignatureHelp;
            if (currentHelp == null)
                return Task.CompletedTask;

            if (!currentHelp.Value.Items.ApplicableSpan.Contains(session.CursorPosition)) {
                session.Roslyn.CurrentSignatureHelp = null;
                return SendSignatureHelpAsync(null, sender, cancellationToken);
            }

            // not sure if there is any better way to recalculate the selected parameter only,
            // but doesn't seem so at the moment
            return TryApplySignatureHelpAsync(currentHelp.Value.Provider, new SignatureHelpTriggerInfoData(SignatureHelpTriggerReason.RetriggerCommand), session, sender, cancellationToken, true);
        }

        public Task ApplyTypedCharAsync(char @char, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            if (!session.IsRoslyn)
                return Task.CompletedTask;

            var trigger = new SignatureHelpTriggerInfoData(SignatureHelpTriggerReason.TypeCharCommand, @char);
            if (session.Roslyn.CurrentSignatureHelp != null) {
                var provider = session.Roslyn.CurrentSignatureHelp.Value.Provider;
                if (provider.IsRetriggerCharacter(@char)) {
                    session.Roslyn.CurrentSignatureHelp = null;
                    return SendSignatureHelpAsync(null, sender, cancellationToken);
                }

                return TryApplySignatureHelpAsync(provider, trigger, session, sender, cancellationToken, sendIfEmpty: true);
            }

            return TryApplySignatureHelpAsync(session, sender, cancellationToken, trigger);
        }

        public Task ForceSignatureHelpAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            if (!session.IsRoslyn)
                return Task.CompletedTask;

            var trigger = new SignatureHelpTriggerInfoData(SignatureHelpTriggerReason.InvokeSignatureHelpCommand);
            return TryApplySignatureHelpAsync(session, sender, cancellationToken, trigger);
        }

        private async Task TryApplySignatureHelpAsync(WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken, SignatureHelpTriggerInfoData trigger) {
            foreach (var provider in session.Roslyn.SignatureHelpProviders) {
                if (await TryApplySignatureHelpAsync(provider, trigger, session, sender, cancellationToken).ConfigureAwait(false))
                    return;
            }
        }

        private async Task<bool> TryApplySignatureHelpAsync(ISignatureHelpProviderWrapper provider, SignatureHelpTriggerInfoData trigger, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken, bool sendIfEmpty = false) {
            // ReSharper disable once PossibleInvalidOperationException
            if (trigger.TriggerReason == SignatureHelpTriggerReason.TypeCharCommand && !provider.IsTriggerCharacter(trigger.TriggerCharacter!.Value))
                return false;

            var help = await provider.GetItemsAsync(session.Roslyn.Document, session.CursorPosition, trigger, cancellationToken).ConfigureAwait(false);
            if (!sendIfEmpty && help == null)
                return false;

            session.Roslyn.CurrentSignatureHelp = help != null ? new CurrentSignatureHelp(provider, help) : (CurrentSignatureHelp?)null;
            await SendSignatureHelpAsync(help, sender, cancellationToken).ConfigureAwait(false);
            return true;
        }

        private Task SendSignatureHelpAsync(SignatureHelpItemsData? items, ICommandResultSender sender, CancellationToken cancellationToken) {
            var writer = sender.StartJsonMessage("signatures");
            if (items == null)
                return sender.SendJsonMessageAsync(cancellationToken);

            var selectedItemIndex = items.SelectedItemIndex;
            writer.WriteSpanProperty("span", items.ApplicableSpan);
            writer.WritePropertyStartArray("signatures");
            var itemIndex = 0;
            foreach (var item in items.Items) {
                writer.WriteStartObject();
                if (selectedItemIndex == null && items.ArgumentCount <= item.ParameterCount)
                    selectedItemIndex = itemIndex;
                var isSelected = (itemIndex == selectedItemIndex);
                if (isSelected)
                    writer.WriteProperty("selected", true);

                WriteSignatureParts(writer, item, items, out var selectedParameter);
                if (isSelected)
                    WriteSignatureDocumentation(writer, item, selectedParameter, cancellationToken);

                writer.WriteEndObject();
                itemIndex += 1;
            }
            writer.WriteEndArray();
            return sender.SendJsonMessageAsync(cancellationToken);
        }

        private void WriteSignatureParts(
            IFastJsonWriter writer,
            SignatureHelpItemData item,
            SignatureHelpItemsData items,
            out SignatureHelpParameterData? selectedParameter
        ) {
            selectedParameter = null;
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
                if (selected)
                    selectedParameter = parameter;
                parameterIndex += 1;
            }
            writer.WriteTaggedTexts(item.SuffixDisplayParts);
            writer.WriteEndArray();
        }

        private void WriteSignatureDocumentation(
            IFastJsonWriter writer,
            SignatureHelpItemData selectedItem,
            SignatureHelpParameterData? selectedParameter,
            CancellationToken cancellationToken
        ) {
            writer.WritePropertyStartObject("info");
            writer.WritePropertyStartArray("parts");
            var documentation = selectedItem.DocumentationFactory(cancellationToken);
            writer.WriteTaggedTexts(documentation);
            writer.WriteEndArray();

            if (selectedParameter == null) {
                writer.WriteEndObject();
                return;
            }

            writer.WritePropertyStartObject("parameter");
            writer.WriteProperty("name", selectedParameter.Name);

            writer.WritePropertyStartArray("parts");
            var parameterDocumentation = selectedParameter.DocumentationFactory(cancellationToken);
            writer.WriteTaggedTexts(parameterDocumentation);
            writer.WriteEndArray();

            writer.WriteEndObject();
            writer.WriteEndObject();
        }
    }
}
