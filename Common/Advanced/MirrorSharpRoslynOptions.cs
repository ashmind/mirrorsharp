using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Advanced {
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

        [NotNull]
        public TParseOptions ParseOptions {
            get => _parseOptions;
            set => _parseOptions = Argument.NotNull(nameof(value), value);
        }

        [NotNull]
        public TCompilationOptions CompilationOptions {
            get => _compilationOptions;
            set => _compilationOptions = Argument.NotNull(nameof(value), value);
        }

        [NotNull]
        public ImmutableList<MetadataReference> MetadataReferences {
            get => _metadataReferences;
            set => _metadataReferences = Argument.NotNull(nameof(value), value);
        }
    }
}
