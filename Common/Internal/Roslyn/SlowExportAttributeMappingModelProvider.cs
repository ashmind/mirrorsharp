using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Convention;
using System.Reflection;
using MirrorSharp.Internal.Reflection;

#if QUICKINFO
namespace MirrorSharp.Internal.Roslyn {
    internal class SlowExportAttributeMappingModelProvider : AttributedModelProvider {
        public override IEnumerable<Attribute> GetCustomAttributes(Type reflectedType, MemberInfo member) {
            var exports = member.GetCustomAttributes<System.ComponentModel.Composition.ExportAttribute>();
            return MapAttributesSlow(exports);
        }

        public override IEnumerable<Attribute> GetCustomAttributes(Type reflectedType, ParameterInfo parameter) {
            var exports = parameter.GetCustomAttributes<System.ComponentModel.Composition.ExportAttribute>();
            return MapAttributesSlow(exports);
        }

        private IEnumerable<ExportAttribute> MapAttributesSlow(IEnumerable<System.ComponentModel.Composition.ExportAttribute> exports) {
            foreach (var export in exports) {
                if (export.GetType() == RoslynTypes.ExportQuickInfoProviderAttribute)
                    yield return new ExportQuickInfoProviderAttributeWrapper(export);
                if (export.GetType() == typeof(System.ComponentModel.Composition.ExportAttribute))
                    yield return new ExportAttribute(export.ContractType);
                // unknown export attribute -- not supported
            }
        }
    }
}
#endif