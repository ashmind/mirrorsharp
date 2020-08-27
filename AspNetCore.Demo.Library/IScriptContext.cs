using System.Collections.Generic;

namespace AspNetCore.Demo.Library {
    public interface IScriptContext {
        string Arguments { get; }

        IReadOnlyList<string> Messages { get; }
    }
}