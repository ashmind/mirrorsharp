using MirrorSharp.Advanced;
using MirrorSharp.Internal;

namespace MirrorSharp.IL.Advanced {
    /// <summary>Provides IL-related extensions to the <see cref="IWorkSession" />.</summary>
    public static class WorkSessionExtensions {
        /// <summary>Specifies whether the <see cref="IWorkSession" /> is using IL.</summary>
        /// <param name="session">The session</param>
        /// <returns><c>true</c> if the session is using IL; otherwise, <c>false</c></returns>
        // ReSharper disable once InconsistentNaming
        public static bool IsIL(this IWorkSession session) {
            Argument.NotNull(nameof(session), session);
            return session is WorkSession { LanguageSession: IILSession };
        }

        /// <summary>Returns IL session associated with the <see cref="IWorkSession" />, if any; throws otherwise.</summary>
        /// <param name="session">The session</param>
        /// <returns><see cref="IILSession" /> if the session is using IL</returns>
        // ReSharper disable once InconsistentNaming
        public static IILSession IL(this IWorkSession session) {
            Argument.NotNull(nameof(session), session);
            return (IILSession)((WorkSession)session).LanguageSession;
        }
    }
}
