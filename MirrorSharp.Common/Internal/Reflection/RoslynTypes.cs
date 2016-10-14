using System.Reflection;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Completion;

namespace MirrorSharp.Internal.Reflection {
    internal static class RoslynTypes {
        private static readonly Assembly MicrosoftCodeAnalysisFeatures = typeof(CompletionProvider).GetTypeInfo().Assembly;

        public static readonly TypeInfo CodeAction = typeof(CodeAction).GetTypeInfo();
        public static readonly TypeInfo ISignatureHelpProvider = MicrosoftCodeAnalysisFeatures.GetType("Microsoft.CodeAnalysis.SignatureHelp.ISignatureHelpProvider", true).GetTypeInfo();
        public static readonly TypeInfo SignatureHelpTriggerInfo = MicrosoftCodeAnalysisFeatures.GetType("Microsoft.CodeAnalysis.SignatureHelp.SignatureHelpTriggerInfo", true).GetTypeInfo();
        public static readonly TypeInfo SignatureHelpTriggerReason = MicrosoftCodeAnalysisFeatures.GetType("Microsoft.CodeAnalysis.SignatureHelp.SignatureHelpTriggerReason", true).GetTypeInfo();
        public static readonly TypeInfo SignatureHelpItems = MicrosoftCodeAnalysisFeatures.GetType("Microsoft.CodeAnalysis.SignatureHelp.SignatureHelpItems", true).GetTypeInfo();
        public static readonly TypeInfo SignatureHelpItem = MicrosoftCodeAnalysisFeatures.GetType("Microsoft.CodeAnalysis.SignatureHelp.SignatureHelpItem", true).GetTypeInfo();
        public static readonly TypeInfo SignatureHelpParameter = MicrosoftCodeAnalysisFeatures.GetType("Microsoft.CodeAnalysis.SignatureHelp.SignatureHelpParameter", true).GetTypeInfo();
    }
}