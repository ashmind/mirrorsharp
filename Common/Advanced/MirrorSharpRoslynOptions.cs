using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Advanced {
    /// <summary>Base class for Roslyn-based language options. Should not be used directly.</summary>
    /// <typeparam name="TParseOptions">Type of <see cref="ParseOptions" /> for this language.</typeparam>
    /// <typeparam name="TCompilationOptions">Type of <see cref="CompilationOptions" /> for this language.</typeparam>
    [PublicAPI]
    public class MirrorSharpRoslynOptions<TParseOptions, TCompilationOptions>
        where TParseOptions : ParseOptions
        where TCompilationOptions : CompilationOptions
    {
        [NotNull] private TParseOptions _parseOptions;
        [NotNull] private TCompilationOptions _compilationOptions;
        [NotNull] private ImmutableList<MetadataReference> _metadataReferences;

        internal MirrorSharpRoslynOptions(
            [NotNull] TParseOptions parseOptions,
            [NotNull] TCompilationOptions compilationOptions,
            [NotNull] ImmutableList<MetadataReference> metadataReferences
        ) {
            _parseOptions = parseOptions;
            _compilationOptions = compilationOptions;
            _metadataReferences = metadataReferences;
        }

        /// <summary><see cref="ParseOptions" /> for this language.</summary>
        [NotNull]
        public TParseOptions ParseOptions {
            get => _parseOptions;
            set => _parseOptions = Argument.NotNull(nameof(value), value);
        }

        /// <summary><see cref="CompilationOptions" /> for this language.</summary>
        [NotNull]
        public TCompilationOptions CompilationOptions {
            get => _compilationOptions;
            set => _compilationOptions = Argument.NotNull(nameof(value), value);
        }

        /// <summary><see cref="MetadataReference" />s for this language.</summary>
        [NotNull]
        public ImmutableList<MetadataReference> MetadataReferences {
            get => _metadataReferences;
            set => _metadataReferences = Argument.NotNull(nameof(value), value);
        }
    }
}
