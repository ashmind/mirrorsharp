using System;
using System.Collections.Generic;
using System.Text;
using MirrorSharp.Advanced;
using MirrorSharp.Internal;

namespace MirrorSharp.IL.Advanced {
    public static class WorkSessionExtensions {
        // ReSharper disable once InconsistentNaming
        public static bool IsIL(this IWorkSession session) {
            Argument.NotNull(nameof(session), session);
            return ((WorkSession)session).LanguageSession is IILSession;
        }
        // ReSharper disable once InconsistentNaming
        public static IILSession IL(this IWorkSession session) {
            Argument.NotNull(nameof(session), session);
            return (IILSession)((WorkSession)session).LanguageSession;
        }
    }
}
