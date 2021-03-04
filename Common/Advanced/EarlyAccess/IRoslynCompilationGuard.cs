using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MirrorSharp.Advanced.EarlyAccess {
    internal interface IRoslynCompilationGuard {
        void ValidateCompilation(Compilation compilation, IRoslynSession session);
    }
}
