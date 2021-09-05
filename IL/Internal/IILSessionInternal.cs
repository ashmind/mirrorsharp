using System.Text;
using MirrorSharp.IL.Advanced;

namespace MirrorSharp.IL.Internal {
    internal interface IILSessionInternal : IILSession {
        int TextLength { get; }
        StringBuilder GetTextBuilderForReadsOnly();
    }
}
