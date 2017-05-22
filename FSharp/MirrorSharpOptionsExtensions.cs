using System;
using JetBrains.Annotations;
using MirrorSharp.FSharp;
using MirrorSharp.FSharp.Internal;

// This is run only once, on startup, so:
// ReSharper disable HeapView.ClosureAllocation

// ReSharper disable once CheckNamespace
namespace MirrorSharp {
    public static class MirrorSharpOptionsExtensions {
        [NotNull]
        public static MirrorSharpOptions EnableFSharp([NotNull] this MirrorSharpOptions options, [CanBeNull] Action<MirrorSharpFSharpOptions> setup = null) {
            Argument.NotNull(nameof(options), options);

            var fsharp = new MirrorSharpFSharpOptions();
            setup?.Invoke(fsharp);
            options.Languages.Add(FSharpLanguage.Name, () => new FSharpLanguage(fsharp));
            return options;
        }
    }
}
