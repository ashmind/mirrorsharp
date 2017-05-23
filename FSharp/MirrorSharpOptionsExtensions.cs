using System;
using JetBrains.Annotations;
using MirrorSharp.FSharp;
using MirrorSharp.FSharp.Internal;

// This is run only once, on startup, so:
// ReSharper disable HeapView.ClosureAllocation

// ReSharper disable once CheckNamespace
namespace MirrorSharp {
    /// <summary>Extensions to <see cref="MirrorSharpOptions" /> related to F#.</summary>
    public static class MirrorSharpOptionsExtensions {
        /// <summary>Enables and configures F# support in the <see cref="MirrorSharpOptions" />.</summary>
        /// <param name="options">Options to configure</param>
        /// <param name="setup">Setup delegate used to configure <see cref="MirrorSharpFSharpOptions" /></param>
        /// <returns>Value of <paramref name="options" />, for convenience.</returns>
        [NotNull]
        public static MirrorSharpOptions EnableFSharp([NotNull] this MirrorSharpOptions options, [CanBeNull] Action<MirrorSharpFSharpOptions> setup = null) {
            Argument.NotNull(nameof(options), options);
            options.Languages.Add(FSharpLanguage.Name, () => {
                var fsharp = new MirrorSharpFSharpOptions();
                setup?.Invoke(fsharp);
                return new FSharpLanguage(fsharp);
            });
            return options;
        }
    }
}
