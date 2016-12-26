using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MirrorSharp.Tests.Internal.Results {
    public class OptionsEchoResult {
        public IDictionary<string, string> Options { get; } = new Dictionary<string, string>();
    }
}
