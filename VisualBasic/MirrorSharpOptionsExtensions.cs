using System;
using Microsoft.CodeAnalysis;
using MirrorSharp.Internal;
using MirrorSharp.VisualBasic;
using MirrorSharp.VisualBasic.Internal;

// This is run only once, on startup, so:
// ReSharper disable HeapView.ClosureAllocation

// ReSharper disable once CheckNamespace
namespace MirrorSharp {
    /// <summary>Extensions to <see cref="MirrorSharpOptions" /> related to Visual Basic .NET.</summary>
    public static class MirrorSharpOptionsExtensions {
        /// <summary>Enables and configures Visual Basic .NET support in the <see cref="MirrorSharpOptions" />.</summary>
        /// <param name="options">Options to configure</param>
        /// <param name="setup">Setup delegate used to configure <see cref="MirrorSharpVisualBasicOptions" /></param>
        /// <returns>Value of <paramref name="options" />, for convenience.</returns>
        public static MirrorSharpOptions EnableVisualBasic(this MirrorSharpOptions options, Action<MirrorSharpVisualBasicOptions>? setup = null) {
            Argument.NotNull(nameof(options), options);
            options.Languages.Add(LanguageNames.VisualBasic, c => {
                var visualBasicOptions = new MirrorSharpVisualBasicOptions();
                setup?.Invoke(visualBasicOptions);
                return new VisualBasicLanguage(c, visualBasicOptions);
            });
            return options;
        }
    }
}
