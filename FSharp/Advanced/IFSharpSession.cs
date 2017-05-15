using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.FSharp.Compiler.SourceCodeServices;

namespace MirrorSharp.FSharp.Advanced {
    [PublicAPI]
    public interface IFSharpSession {
        [ItemNotNull] ValueTask<FSharpParseAndCheckResults> ParseAndCheckAsync(CancellationToken cancellationToken);

        [NotNull] FSharpChecker Checker { get; }
        [NotNull] FSharpProjectOptions ProjectOptions { get; }
    }
}
