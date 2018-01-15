using System.Collections.Immutable;
using System.Reflection;
using JetBrains.Annotations;

namespace MirrorSharp.Php
{
    /// <summary>MirrorSharp options for PHP</summary>
    [PublicAPI]
    public class MirrorSharpPhpOptions {
        /// <summary>Contains the list of assembly reference paths to be used, not configurable.</summary>
        public static readonly ImmutableArray<string> AssemblyReferencePaths;

        static MirrorSharpPhpOptions() {
            AssemblyReferencePaths = ImmutableArray.Create(
                typeof(object).GetTypeInfo().Assembly.Location,                                         // mscorlib
                typeof(Pchp.Core.Context).GetTypeInfo().Assembly.Location,                              // Peachpie.Runtime
                typeof(Pchp.Library.Strings).GetTypeInfo().Assembly.Location,                           // Peachpie.Library
                typeof(Peachpie.Library.XmlDom.XmlDom).GetTypeInfo().Assembly.Location,                 // Peachpie.Library.XmlDom
                typeof(Peachpie.Library.Scripting.ScriptingProvider).GetTypeInfo().Assembly.Location    // Peachpie.Library.Scripting
            );
        }

        internal MirrorSharpPhpOptions() { }

        /// <summary>Whether to compile in Debug mode.</summary>
        public bool? Debug { get; set; }
    }
}
