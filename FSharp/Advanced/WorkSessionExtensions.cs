using JetBrains.Annotations;
using MirrorSharp.Advanced;
using MirrorSharp.Internal;

namespace MirrorSharp.FSharp.Advanced {
    [PublicAPI]
    public static class WorkSessionExtensions {
        public static bool IsFSharp(this IWorkSession session) {
            return ((WorkSession)session).LanguageSession is IFSharpSession;
        }

        public static IFSharpSession FSharp(this IWorkSession session) {
            return (IFSharpSession)((WorkSession)session).LanguageSession;
        }
    }
}
