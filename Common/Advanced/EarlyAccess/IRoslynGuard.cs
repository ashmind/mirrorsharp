using Microsoft.CodeAnalysis;

namespace MirrorSharp.Advanced.EarlyAccess {
    internal interface IRoslynGuard {
        void ValidateCompilation(Compilation compilation, IRoslynSession session);
    }
}
