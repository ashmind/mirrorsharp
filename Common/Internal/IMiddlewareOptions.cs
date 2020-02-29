using System;
using System.Collections.Generic;
using System.Text;

namespace MirrorSharp.Internal {
    internal interface IMiddlewareOptions : IWorkSessionOptions, IConnectionOptions, ILanguageManagerOptions {
    }
}
