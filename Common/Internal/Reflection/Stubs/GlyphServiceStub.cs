#if QUICKINFO
using System.Composition;
using Microsoft.VisualStudio.Language.Intellisense;

namespace MirrorSharp.Internal.Reflection.Stubs {
    [Export(typeof(IGlyphService))]
    internal class GlyphServiceStub : IGlyphService {
    }
}
#endif