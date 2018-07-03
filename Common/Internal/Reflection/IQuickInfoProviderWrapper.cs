#if QUICKINFO
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Reflection {
    internal interface IQuickInfoProviderWrapper {
        Task<QuickInfoItemData> GetItemAsync(Document document, int position, CancellationToken cancellationToken);
    }
}
#endif