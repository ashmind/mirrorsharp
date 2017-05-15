using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Compiler;
using Microsoft.FSharp.Compiler.SourceCodeServices;

namespace MirrorSharp.FSharp.Advanced {
    [PublicAPI]
    public interface IFSharpSession {
        [ItemNotNull] ValueTask<FSharpParseAndCheckResults> ParseAndCheckAsync(CancellationToken cancellationToken);

        /// <summary>Return last parse result (if text hasn't changed since), but doesn't force a new reparse.</summary>
        /// <returns>Last <see cref="FSharpParseFileResults"/> if still valid, otherwise <c>null</c>.</returns>
        [CanBeNull] FSharpParseFileResults GetLastParseResults();

        /// <summary>Return last check result (if text hasn't changed since), but doesn't force a new check.</summary>
        /// <returns>Last <see cref="FSharpCheckFileAnswer"/> if still valid, otherwise <c>null</c>.</returns>
        [CanBeNull] FSharpCheckFileAnswer GetLastCheckAnswer();

        [NotNull] Diagnostic ConvertToDiagnostic([NotNull] FSharpErrorInfo error);

        [NotNull] FSharpChecker Checker { get; }
        [NotNull] FSharpProjectOptions ProjectOptions { get; }
        ImmutableArray<string> AssemblyReferencePaths { get; }
        [NotNull] FSharpList<string> AssemblyReferencePathsAsFSharpList { get; }
    }
}
