using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Compiler;
using Microsoft.FSharp.Compiler.SourceCodeServices;

namespace MirrorSharp.FSharp.Advanced {
    /// <summary>Represents a user session based on F# parser.</summary>
    [PublicAPI]
    public interface IFSharpSession {
        /// <summary>Returns the combined <see cref="FSharpParseAndCheckResults" /> for the current session.</summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the call.</param>
        /// <returns>Last <see cref="FSharpParseAndCheckResults"/> if still valid, otherwise results of a new forced parse and check.</returns>
        [ItemNotNull] ValueTask<FSharpParseAndCheckResults> ParseAndCheckAsync(CancellationToken cancellationToken);

        /// <summary>Return last parse result (if text hasn't changed since), but doesn't force a new reparse.</summary>
        /// <returns>Last <see cref="FSharpParseFileResults"/> if still valid, otherwise <c>null</c>.</returns>
        [CanBeNull] FSharpParseFileResults GetLastParseResults();

        /// <summary>Return last check result (if text hasn't changed since), but doesn't force a new check.</summary>
        /// <returns>Last <see cref="FSharpCheckFileAnswer"/> if still valid, otherwise <c>null</c>.</returns>
        [CanBeNull] FSharpCheckFileAnswer GetLastCheckAnswer();

        /// <summary>Converts <see cref="FSharpErrorInfo" /> to a <see cref="Diagnostic" />.</summary>
        /// <param name="error"><see cref="FSharpErrorInfo" /> value to convert.</param>
        /// <returns><see cref="Diagnostic" /> value that corresponds to <paramref name="error" />.</returns>
        [NotNull] Diagnostic ConvertToDiagnostic([NotNull] FSharpErrorInfo error);

        /// <summary>Converts line and column into a text offset.</summary>
        /// <param name="line">Line to convert (0-based).</param>
        /// <param name="column">Column to convert (0-based).</param>
        /// <returns>Text offset that corresponds to given line and column.</returns>
        int ConvertToOffset(int line, int column);

        /// <summary>Gets the <see cref="FSharpChecker" /> associated with this session.</summary>
        [NotNull] FSharpChecker Checker { get; }

        /// <summary>Gets or sets the <see cref="ProjectOptions" /> associated with this session.</summary>
        [NotNull] FSharpProjectOptions ProjectOptions { get; set; }

        /// <summary>Gets the assembly reference paths associated with this session.</summary>
        ImmutableArray<string> AssemblyReferencePaths { get; }

        /// <summary>Gets the <see cref="AssemblyReferencePaths" /> as a <see cref="FSharpList{T}"/>.</summary>
        [NotNull] FSharpList<string> AssemblyReferencePathsAsFSharpList { get; }
    }
}
