using System.Collections.Generic;

namespace MirrorSharp.Internal {
    internal interface IMiddlewareOptions : IWorkSessionOptions, IConnectionOptions, ILanguageManagerOptions {
        IList<(char commandId, string commandText)> StatusTestCommands { get; }
    }
}
