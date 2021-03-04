using Microsoft.CodeAnalysis.Text;

namespace MirrorSharp.Advanced.EarlyAccess {
    internal interface IRoslynSourceTextGuard {
        void ValidateSourceText(SourceText sourceText);
    }
}
