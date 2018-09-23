using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using MirrorSharp.Internal;
using MirrorSharp.Internal.Roslyn;

namespace MirrorSharp.Advanced {
    /// <summary>Base class for Roslyn-based language options. Should not be used directly.</summary>
    /// <typeparam name="TSelf">Type of the specific subclass (provided by that subclass).</typeparam>
    /// <typeparam name="TParseOptions">Type of <see cref="ParseOptions" /> for this language.</typeparam>
    /// <typeparam name="TCompilationOptions">Type of <see cref="CompilationOptions" /> for this language.</typeparam>
    [PublicAPI]
    public abstract class MirrorSharpRoslynOptions<TSelf, TParseOptions, TCompilationOptions> : IRoslynLanguageOptions
        where TSelf: MirrorSharpRoslynOptions<TSelf, TParseOptions, TCompilationOptions>
        where TParseOptions : ParseOptions
        where TCompilationOptions : CompilationOptions
    {
        [NotNull] private TParseOptions _parseOptions;
        [NotNull] private TCompilationOptions _compilationOptions;
        [NotNull] private ImmutableList<MetadataReference> _metadataReferences;
        private bool _isScript;
        [CanBeNull] private Type _hostObjectType;

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

        /// <summary>Sets or unsets script mode for this language.</summary>
        /// <param name="isScript">Whether the language should use script mode.</param>
        /// <param name="hostObjectType">Host object type for the session; must be <c>null</c> if <paramref name="isScript" /> is <c>false</c>.</param>
        /// <returns>Current instance (for convenience).</returns>
        /// <remarks>
        /// Members of <paramref name="hostObjectType" /> are directly available to the script. For example
        /// if you set <c>hostObjectType</c> is <see cref="Random" />, you can use <see cref="Random.Next()" />
        /// in the script by just writing <c>Next()</c>.
        /// </remarks>
        /// <seealso cref="ProjectInfo.IsSubmission"/>
        /// <seealso cref="ProjectInfo.HostObjectType"/>
        public TSelf SetScriptMode(bool isScript = true, Type hostObjectType = null) {
            RoslynScriptHelper.Validate(isScript, hostObjectType);

            ParseOptions = (TParseOptions)ParseOptions.WithKind(RoslynScriptHelper.GetSourceKind(isScript));
            _isScript = isScript;
            _hostObjectType = hostObjectType;

            return (TSelf)this;
        }

        /// <summary>
        /// Adds assembly references to <see cref="MetadataReferences"/> and attempts to discover corresponding XML documentation.
        /// </summary>
        /// <param name="paths">Paths to assemblies to be added.</param>
        /// <returns>Current instance (for convenience).</returns>
        /// <remarks>
        /// This method will attempt to automatically discover XML documentation files for the assemblies being added.
        /// However it will always succeed, whether the XML documentation is available or not.
        /// </remarks>
        /// <seealso cref="MetadataReferences"/>
        public TSelf AddMetadataReferencesFromFiles(params string[] paths) {
            _metadataReferences = _metadataReferences.AddRange(MetadataReferenceFactory.CreateFromFiles(paths));
            return (TSelf)this;
        }

        ParseOptions IRoslynLanguageOptions.ParseOptions => ParseOptions;
        CompilationOptions IRoslynLanguageOptions.CompilationOptions => CompilationOptions;
        ImmutableList<MetadataReference> IRoslynLanguageOptions.MetadataReferences => MetadataReferences;
        bool IRoslynLanguageOptions.IsScript => _isScript;
        Type IRoslynLanguageOptions.HostObjectType => _hostObjectType;
    }
}
