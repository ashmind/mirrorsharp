using System.Text;
using MirrorSharp.IL.Advanced;
using MirrorSharp.Internal.Abstraction;

namespace MirrorSharp.IL.Internal {
    internal interface IILSessionInternal : IILSession, ILanguageSessionInternal {
        int TextLength { get; }
        StringBuilder GetTextBuilderForReadsOnly();
    }
}
