using System;
using MirrorSharp.Internal;
using MirrorSharp.Php;
using MirrorSharp.Php.Internal;

namespace MirrorSharp {
    /// <summary>Extensions to <see cref="MirrorSharpOptions" /> related to Visual Basic .NET.</summary>
    public static class MirrorSharpOptionsExtensions {
        /// <summary>Enables and configures PHP .NET support in the <see cref="MirrorSharpOptions" />.</summary>
        /// <param name="options">Options to configure</param>
        /// <param name="setup">Setup delegate used to configure <see cref="MirrorSharpPhpOptions" /></param>
        /// <returns>Value of <paramref name="options" />, for convenience.</returns>
        public static MirrorSharpOptions EnablePhp(this MirrorSharpOptions options, Action<MirrorSharpPhpOptions>? setup = null) {
            Argument.NotNull(nameof(options), options);
            options.Languages.Add(PhpLanguage.Name, c => {
                var phpOptions = new MirrorSharpPhpOptions();
                setup?.Invoke(phpOptions);
                return new PhpLanguage(phpOptions);
            });
            return options;
        }
    }
}
