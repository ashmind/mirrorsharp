using System.Collections.Immutable;
using FSharp.Compiler.Symbols;

namespace MirrorSharp.FSharp.Internal {
    internal static class SymbolTags {
        private static ImmutableArray<string> Namespace { get; } = ImmutableArray.Create("Namespace");

        private static ImmutableArray<string> Delegate { get; } = ImmutableArray.Create("Delegate");
        private static ImmutableArray<string> Enum { get; } = ImmutableArray.Create("Enum");
        private static ImmutableArray<string> Union { get; } = ImmutableArray.Create("Union");
        private static ImmutableArray<string> Structure { get; } = ImmutableArray.Create("Structure");
        private static ImmutableArray<string> Class { get; } = ImmutableArray.Create("Class");
        private static ImmutableArray<string> Interface { get; } = ImmutableArray.Create("Interface");
        private static ImmutableArray<string> TypeParameter { get; } = ImmutableArray.Create("TypeParameter");
        private static ImmutableArray<string> Module { get; } = ImmutableArray.Create("Module");

        private static ImmutableArray<string> Property { get; } = ImmutableArray.Create("Property");
        private static ImmutableArray<string> Method { get; } = ImmutableArray.Create("Method");
        private static ImmutableArray<string> Field { get; } = ImmutableArray.Create("Field");

        private static ImmutableArray<string> Local { get; } = ImmutableArray.Create("Local");

        private static ImmutableArray<string> None { get; } = ImmutableArray<string>.Empty;

        public static ImmutableArray<string> From(FSharpSymbol symbol) => symbol switch {
            FSharpField _ => Field,
            FSharpEntity e => FromEntity(e),
            FSharpMemberOrFunctionOrValue m => m switch {
                { IsProperty: true } => Property,
                { FullType: { IsFunctionType: true } } => Method,
                { IsConstructor: true } => Method,
                { IsValue: true } => Local,
                _ => None
            },
            _ => None
        };

        private static ImmutableArray<string> FromEntity(FSharpEntity entity) => entity switch {
            { IsNamespace: true } => Namespace,
            { IsClass: true } => Class,
            { IsInterface: true } => Interface,
            { IsDelegate: true } => Delegate,
            { IsEnum: true } => Enum,
            { IsFSharpUnion: true } => Union,
            { IsValueType: true } => Structure,
            { IsFSharpModule: true } => Module,
            { IsFSharpAbbreviation: true } => FromType(entity.AbbreviatedType),
            _ => None
        };

        private static ImmutableArray<string> FromType(FSharpType type) => type switch {
            { IsFunctionType: true } => Delegate,
            { IsAnonRecordType: true } => Class,
            { IsTupleType: true } => Class,
            { IsStructTupleType: true } => Structure,
            { IsGenericParameter: true } => TypeParameter,
            { IsAbbreviation: true } => FromType(type.AbbreviatedType),
            { HasTypeDefinition: true } => FromEntity(type.TypeDefinition),
            _ => None
        };
    }
}