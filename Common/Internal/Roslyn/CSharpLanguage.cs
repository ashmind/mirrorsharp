using System;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Roslyn {
    internal class CSharpLanguage : RoslynLanguageBase {
        public CSharpLanguage(LanguageCreationContext context, MirrorSharpCSharpOptions options) : base(
            context,
            LanguageNames.CSharp,
            "Microsoft.CodeAnalysis.CSharp.Features",
            "Microsoft.CodeAnalysis.CSharp.Workspaces",
            options
        ) {
        }

        protected override bool ShouldConsiderForHostServices(Type type)
            => base.ShouldConsiderForHostServices(type)
            // IntelliCode type, not available in normal environments
            && type.FullName != "Microsoft.CodeAnalysis.ExternalAccess.Pythia.PythiaSignatureHelpProvider";
    }
}
