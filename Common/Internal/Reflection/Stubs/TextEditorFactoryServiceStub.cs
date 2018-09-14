#if QUICKINFO
using System.Composition;
using Microsoft.VisualStudio.Text.Editor;

namespace MirrorSharp.Internal.Reflection.Stubs {
    [Export(typeof(ITextEditorFactoryService))]
    internal class TextEditorFactoryServiceStub : ITextEditorFactoryService {
    }
}
#endif