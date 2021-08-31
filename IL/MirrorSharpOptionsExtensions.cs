using System;
using Microsoft.CodeAnalysis;
using MirrorSharp.IL;
using MirrorSharp.IL.Internal;
using MirrorSharp.Internal;

// ReSharper disable once CheckNamespace
namespace MirrorSharp {
    public static class MirrorSharpOptionsExtensions
    {
        // ReSharper disable once InconsistentNaming
        public static MirrorSharpOptions EnableIL(this MirrorSharpOptions options, Action<MirrorSharpILOptions>? setup = null) {
            Argument.NotNull(nameof(options), options);
            options.Languages.Add(ILLanguage.Name, () => {
                var ilOptions = new MirrorSharpILOptions();
                setup?.Invoke(ilOptions);
                return new ILLanguage(ilOptions);
            });
            return options;
        }
    }
}
