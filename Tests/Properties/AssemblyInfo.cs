using System;
using System.Net.WebSockets;
using MirrorSharp.Advanced;
using MirrorSharp.Internal.Handlers;
using SourceMock;
using Xunit;

// https://github.com/dotnet/roslyn/issues/16184 (not actually fixed, need to re-submit)
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

[assembly: GenerateMocksForTypes(
    typeof(ICommandHandler),
    typeof(IDisposable),
    typeof(ISlowUpdateExtension),
    typeof(ISetOptionsFromClientExtension),
    typeof(WebSocket)
)]