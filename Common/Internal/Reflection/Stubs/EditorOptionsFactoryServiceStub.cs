#if QUICKINFO
using System.Composition;
using Microsoft.VisualStudio.Text.Editor;

namespace MirrorSharp.Internal.Reflection.Stubs {
    [Export(typeof(IEditorOptionsFactoryService))]
    internal class EditorOptionsFactoryServiceStub : IEditorOptionsFactoryService {
    }
}
#endif