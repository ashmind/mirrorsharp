using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MirrorSharp.Advanced;
using MirrorSharp.Internal;

namespace MirrorSharp.FSharp.Advanced {
    public static class WorkSessionExtensions {
        public static bool IsFSharp(this IWorkSession session) {
            return ((WorkSession)session).LanguageSession is IFSharpSession;
        }

        public static IFSharpSession FSharp(this IWorkSession session) {
            return (IFSharpSession)((WorkSession)session).LanguageSession;
        }
    }
}
