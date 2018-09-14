using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Convention;
using System.Reflection;
using MirrorSharp.Internal.Reflection;

#if QUICKINFO
using ComponentModel = System.ComponentModel;

namespace MirrorSharp.Internal.Roslyn {
    internal class SlowLegacyAttributeMappingModelProvider : AttributedModelProvider {
        public override IEnumerable<Attribute> GetCustomAttributes(Type reflectedType, MemberInfo member) {
            if (reflectedType.Name == "ClassificationTypeMap" && member is ConstructorInfo)
                System.Diagnostics.Debugger.Break();
            return MapAttributesSlow(member.GetCustomAttributes());
        }

        public override IEnumerable<Attribute> GetCustomAttributes(Type reflectedType, ParameterInfo parameter) {
            return MapAttributesSlow(parameter.GetCustomAttributes());
        }

        private IEnumerable<Attribute> MapAttributesSlow(IEnumerable<Attribute> attributes) {
            foreach (var attribute in attributes) {
                if (attribute.GetType() == RoslynTypes.ExportQuickInfoProviderAttribute)
                    yield return new ExportQuickInfoProviderAttributeWrapper((ComponentModel.Composition.ExportAttribute)attribute);
                if (attribute.GetType() == typeof(ComponentModel.Composition.ExportAttribute))
                    yield return new ExportAttribute(((ComponentModel.Composition.ExportAttribute)attribute).ContractType);

                if (attribute.GetType() == typeof(ComponentModel.Composition.ImportingConstructorAttribute))
                    yield return new ImportingConstructorAttribute();
            }
        }
    }
}
#endif