using System;
using JetBrains.Annotations;
using MirrorSharp.FSharp.Internal;

// This is run only once, on startup, so:
// ReSharper disable HeapView.ClosureAllocation

namespace MirrorSharp.FSharp {
    public static class MirrorSharpOptionsExtensions {
        [NotNull]
        public static MirrorSharpOptions EnableFSharp([NotNull] this MirrorSharpOptions options, [CanBeNull] Action<MirrorSharpFSharpOptions> setup = null) {
            Argument.NotNull(nameof(options), options);

            var fsharp = new MirrorSharpFSharpOptions();
            setup?.Invoke(fsharp);
            options.OtherLanguages.Add(FSharpLanguage.Name, () => new FSharpLanguage(fsharp));
            return options;
        }
    }
}
