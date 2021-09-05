using System;
using MirrorSharp.IL;
using MirrorSharp.IL.Internal;
using MirrorSharp.Internal;

// ReSharper disable once CheckNamespace
namespace MirrorSharp {
    /// <summary>Extensions to <see cref="MirrorSharpOptions" /> related to IL.</summary>
    public static class MirrorSharpOptionsExtensions
    {
        /// <summary>Enables and configures IL support in the <see cref="MirrorSharpOptions" />.</summary>
        /// <param name="options">Options to configure</param>
        /// <param name="setup">Setup delegate used to configure <see cref="MirrorSharpILOptions" /></param>
        /// <returns>Value of <paramref name="options" />, for convenience.</returns>
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
