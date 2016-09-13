using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MirrorSharp.Owin.Internal {
    public static class OwinWebSocketMessageType {
        public const int Text = 1;
        public const int Binary = 2;
        public const int Close = 8;
    }
}
