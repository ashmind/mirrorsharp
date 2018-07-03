using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis.Text;

namespace MirrorSharp.QuickInfo.Internal {
    internal class QuickInfoItem {
        public TextSpan TextSpan { get; }
        public DeferredQuickInfoContentData Content { get; }

        public QuickInfoItemData(TextSpan textSpan) {
            TextSpan = textSpan;
        }
    }
}
