using System;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using MirrorSharp.VisualBasic;
using MirrorSharp.VisualBasic.Internal;

// This is run only once, on startup, so:
// ReSharper disable HeapView.ClosureAllocation

// ReSharper disable once CheckNamespace
namespace MirrorSharp {
    [PublicAPI]
    public static class MirrorSharpOptionsExtensions {
        [NotNull]
        public static MirrorSharpOptions EnableVisualBasic([NotNull] this MirrorSharpOptions options, [CanBeNull] Action<MirrorSharpVisualBasicOptions> setup = null) {
            Argument.NotNull(nameof(options), options);

            var visualBasicOptions = new MirrorSharpVisualBasicOptions();
            setup?.Invoke(visualBasicOptions);
            options.Languages.Add(LanguageNames.VisualBasic, () => new VisualBasicLanguage(visualBasicOptions));
            return options;
        }
    }
}
