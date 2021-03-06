using System;
using MirrorSharp.Advanced;

namespace MirrorSharp.Owin.Demo.Extensions {
    public class SetOptionsFromClientExtension : ISetOptionsFromClientExtension {
        public bool TrySetOption(IWorkSession session, string name, string value) {
            if (name != "x-mode")
                return false;

            if (!session.IsRoslyn)
                throw new NotSupportedException("Only Roslyn sessions support script mode.");

            switch (value) {
                case "script":
                    session.Roslyn.SetScriptMode(true, typeof(Random));
                    break;
                case "regular":
                    session.Roslyn.SetScriptMode(false);
                    break;
                default:
                    throw new ArgumentException($"Unknown mode: {value}.");
            }

            return true;
        }
    }
}
