using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace MirrorSharp.Php {
    /// <summary>MirrorSharp options for PHP</summary>
    public class MirrorSharpPhpOptions {
        /// <summary>Contains the list of assembly reference paths to be used, not configurable.</summary>
        public static readonly ImmutableArray<string> AssemblyReferencePaths = GatherPeachpieReferences();

        internal MirrorSharpPhpOptions() { }

        /// <summary>Whether to compile in Debug mode.</summary>
        public bool? Debug { get; set; }

        private static ImmutableArray<string> GatherPeachpieReferences() {
            var refKnownTypes = new[] {
                typeof(object),                                         // mscorlib
                typeof(Pchp.Core.Context),                              // Peachpie.Runtime
                typeof(Pchp.Library.Strings),                           // Peachpie.Library
                typeof(Peachpie.Library.XmlDom.XmlDom),                 // Peachpie.Library.XmlDom
                typeof(Peachpie.Library.Scripting.ScriptingProvider)    // Peachpie.Library.Scripting
            };

            var list = refKnownTypes.Select(type => type.GetTypeInfo().Assembly).Distinct().ToList();
            var set = new HashSet<Assembly>(list);

            for (var i = 0; i < list.Count; i++) {
                var assembly = list[i];
                var refs = assembly.GetReferencedAssemblies();
                foreach (var refname in refs) {
                    var refassembly = Assembly.Load(refname);
                    if (refassembly != null && set.Add(refassembly)) {
                        list.Add(refassembly);
                    }
                }
            }

            return list.Select(ass => ass.Location).ToImmutableArray();
        }
    }
}
