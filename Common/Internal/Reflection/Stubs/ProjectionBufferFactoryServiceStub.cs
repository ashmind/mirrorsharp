#if QUICKINFO
using System.Composition;
using Microsoft.VisualStudio.Text.Projection;

namespace MirrorSharp.Internal.Reflection.Stubs {
    [Export(typeof(IProjectionBufferFactoryService))]
    internal class ProjectionBufferFactoryServiceStub : IProjectionBufferFactoryService {
    }
}
#endif