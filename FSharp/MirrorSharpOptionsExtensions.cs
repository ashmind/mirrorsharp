using System;
using MirrorSharp.FSharp;
using MirrorSharp.FSharp.Internal;
using MirrorSharp.Internal;

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
        public static MirrorSharpOptions EnableFSharp(this MirrorSharpOptions options, Action<MirrorSharpFSharpOptions>? setup = null) {
            Argument.NotNull(nameof(options), options);
            options.Languages.Add(FSharpLanguage.Name, () => {
                var fsharp = new MirrorSharpFSharpOptions();
                setup?.Invoke(fsharp);
                return new FSharpLanguage(fsharp, new Microsoft.IO.RecyclableMemoryStreamManager());
            });
            return options;
        }
    }
}
