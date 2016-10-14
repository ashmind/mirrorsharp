using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Reflection {
    internal interface ISignatureHelpProviderWrapper {
        bool IsTriggerCharacter(char ch);
        bool IsRetriggerCharacter(char ch);
        Task<SignatureHelpItemsData> GetItemsAsync(Document document, int position, SignatureHelpTriggerInfoData triggerInfo, CancellationToken cancellationToken);
    }
}