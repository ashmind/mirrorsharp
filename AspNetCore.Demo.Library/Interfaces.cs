using System;
using System.Collections.Generic;

namespace AspNetCore.Demo.Library
{
    public interface IScriptGlobals
    {
        IScriptContext Context { get; }
    }

    public interface IScriptContext
    {
        string Arguments { get; }

        IReadOnlyList<string> Messages { get; }
    }
}