using System;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using MirrorSharp.VisualBasic;
using MirrorSharp.VisualBasic.Internal;

// This is run only once, on startup, so:
// ReSharper disable HeapView.ClosureAllocation

// ReSharper disable once CheckNamespace
namespace MirrorSharp {
    /// <summary>Extensions to <see cref="MirrorSharpOptions" /> related to Visual Basic .NET.</summary>
    [PublicAPI]
    public static class MirrorSharpOptionsExtensions {
        /// <summary>Enables and configures Visual Basic .NET support in the <see cref="MirrorSharpOptions" />.</summary>
        /// <param name="options">Options to configure</param>
        /// <param name="setup">Setup delegate used to configure <see cref="MirrorSharpVisualBasicOptions" /></param>
        /// <returns>Value of <paramref name="options" />, for convenience.</returns>
        [NotNull]
        public static MirrorSharpOptions EnableVisualBasic([NotNull] this MirrorSharpOptions options, [CanBeNull] Action<MirrorSharpVisualBasicOptions> setup = null) {
            Argument.NotNull(nameof(options), options);
            options.Languages.Add(LanguageNames.VisualBasic, () => {
                var visualBasicOptions = new MirrorSharpVisualBasicOptions();
                setup?.Invoke(visualBasicOptions);
                return new VisualBasicLanguage(visualBasicOptions);
            });
            return options;
        }
    }
}
