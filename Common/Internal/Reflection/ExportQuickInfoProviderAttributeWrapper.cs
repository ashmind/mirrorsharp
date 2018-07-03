using System.Composition;
using System.Reflection;

#if QUICKINFO
namespace MirrorSharp.Internal.Reflection {
    [MetadataAttribute]
    internal class ExportQuickInfoProviderAttributeWrapper : ExportAttribute {
        public string Language { get; }

        public ExportQuickInfoProviderAttributeWrapper(System.ComponentModel.Composition.ExportAttribute attribute)
            : base(attribute.ContractType)
        {
            Language = (string)RoslynReflection.EnsureFound(attribute.GetType().GetTypeInfo(), "Language", (t, n) => t.GetProperty(n))
                .GetValue(attribute);
        }
    }
}
#endif