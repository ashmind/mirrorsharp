using MirrorSharp.Advanced;
using MirrorSharp.Internal;

namespace MirrorSharp.Php.Advanced {
    /// <summary>Provides PHP-related extensions to the <see cref="IWorkSession" />.</summary>
    public static class WorkSessionExtensions {
        /// <summary>Specifies whether the <see cref="IWorkSession" /> is using PHP.</summary>
        /// <param name="session">The session</param>
        /// <returns><c>true</c> if the session is using PHP; otherwise, <c>false</c></returns>
        public static bool IsPhp(this IWorkSession session) {
            Argument.NotNull(nameof(session), session);
            return ((WorkSession)session).LanguageSession is IPhpSession;
        }

        /// <summary>Returns PHP session associated with the <see cref="IWorkSession" />, if any; throws otherwise.</summary>
        /// <param name="session">The session</param>
        /// <returns><see cref="IPhpSession" /> if the session is using PHP</returns>
        public static IPhpSession Php(this IWorkSession session) {
            Argument.NotNull(nameof(session), session);
            return (IPhpSession)((WorkSession)session).LanguageSession;
        }
    }
}
