using System.Collections.Immutable;
using FSharp.Compiler.SourceCodeServices;

namespace MirrorSharp.FSharp.Internal {
    internal static class SymbolTags {
        private static ImmutableArray<string> Namespace { get; } = ImmutableArray.Create("Namespace");

        private static ImmutableArray<string> Delegate { get; } = ImmutableArray.Create("Delegate");
        private static ImmutableArray<string> Enum { get; } = ImmutableArray.Create("Enum");
        private static ImmutableArray<string> Union { get; } = ImmutableArray.Create("Union");
        private static ImmutableArray<string> Structure { get; } = ImmutableArray.Create("Structure");
        private static ImmutableArray<string> Class { get; } = ImmutableArray.Create("Class");
        private static ImmutableArray<string> Interface { get; } = ImmutableArray.Create("Interface");
        private static ImmutableArray<string> Module { get; } = ImmutableArray.Create("Module");
            
        private static ImmutableArray<string> Property { get; } = ImmutableArray.Create("Property");
        private static ImmutableArray<string> Method { get; } = ImmutableArray.Create("Method");
        private static ImmutableArray<string> Field { get; } = ImmutableArray.Create("Field");

        public static ImmutableArray<string> From(FSharpSymbol symbol) {
            switch (symbol) {
                case FSharpField _: return Field;
                case FSharpEntity e: return FromEntity(e);
                case FSharpMemberOrFunctionOrValue m: {
                    if (m.IsProperty) return Property;
                    if (m.IsConstructor || m.FullType.IsFunctionType) return Method;
                    return ImmutableArray<string>.Empty;
                }
                default: return ImmutableArray<string>.Empty;
            }
        }

        private static ImmutableArray<string> FromEntity(FSharpEntity entity) {
            if (entity.IsNamespace) return Namespace;
            if (entity.IsClass) return Class;
            if (entity.IsInterface) return Interface;
            if (entity.IsDelegate) return Delegate;
            if (entity.IsEnum) return Enum;
            if (entity.IsFSharpUnion) return Union;
            if (entity.IsValueType) return Structure;
            if (entity.IsFSharpModule) return Module;
            if (entity.IsFSharpAbbreviation) return FromEntity(entity.AbbreviatedType.TypeDefinition);
            return ImmutableArray<string>.Empty;
        }
    }
}