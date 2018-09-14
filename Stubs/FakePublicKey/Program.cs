using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace FakePublicKey {
    public class Program {
        public static void Main(string[] args) {
            var assemblyPath = args[0];

            var assembly = AssemblyDefinition.ReadAssembly(
                new MemoryStream(File.ReadAllBytes(assemblyPath))
            );
            assembly.Name.HasPublicKey = true;
            assembly.Name.PublicKey = SoapHexBinary.Parse("002400000480000094000000060200000024000052534131000400000100010007d1fa57c4aed9f0a32e84aa0faefd0de9e8fd6aec8f87fb03766c834c99921eb23be79ad9d5dcc1dd9ad236132102900b723cf980957fc4e177108fc607774f29e8320e92ea05ece4e821c0a5efe8f1645c4c0c93c1ab99285d622caa652c1dfad63d745d6f2de5f17e5eaf0fc4963d261c8a12436518206dc093344d5ad293").Value;
            assembly.Write(assemblyPath);
        }
    }
}
