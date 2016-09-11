using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Internal;

namespace MirrorSharp {
    public sealed class MirrorSharpOptions : IConnectionOptions {
        public bool SendDebugCompareMessages { get; set; }
    }
}
