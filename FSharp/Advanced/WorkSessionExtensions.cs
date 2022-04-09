using MirrorSharp.Advanced;
using MirrorSharp.Internal;

namespace MirrorSharp.FSharp.Advanced {
    /// <summary>Provides F#-related extensions to the <see cref="IWorkSession" />.</summary>
    public static class WorkSessionExtensions {
        /// <summary>Specifies whether the <see cref="IWorkSession" /> is using F#.</summary>
        /// <param name="session">The session</param>
        /// <returns><c>true</c> if the session is using F#; otherwise, <c>false</c></returns>
        public static bool IsFSharp(this IWorkSession session) {
            Argument.NotNull(nameof(session), session);
            return session is WorkSession { LanguageSession: IFSharpSession };
        }

        /// <summary>Returns F# session associated with the <see cref="IWorkSession" />, if any; throws otherwise.</summary>
        /// <param name="session">The session</param>
        /// <returns><see cref="IFSharpSession" /> if the session is using F#</returns>
        public static IFSharpSession FSharp(this IWorkSession session) {
            Argument.NotNull(nameof(session), session);
            return (IFSharpSession)((WorkSession)session).LanguageSession;
        }
    }
}
