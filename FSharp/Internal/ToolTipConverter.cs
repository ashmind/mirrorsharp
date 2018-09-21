using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.QuickInfo;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Compiler.SourceCodeServices;
using Microsoft.FSharp.Core;
using global::Internal.Utilities.StructuredFormat;
using System.Collections.Generic;

namespace MirrorSharp.FSharp.Internal {
    using TaggedText = Microsoft.CodeAnalysis.TaggedText;

    internal static class ToolTipConverter {
        private static readonly IReadOnlyDictionary<LayoutTag, string> LayoutTagToTextTagMap = new Dictionary<LayoutTag, string> {
            { LayoutTag.ActivePatternCase, "TODO" },
            { LayoutTag.ActivePatternResult, "TODO" },
            { LayoutTag.Alias, TextTags.Alias },
            { LayoutTag.Class, TextTags.Class },
            { LayoutTag.Delegate, TextTags.Delegate },
            { LayoutTag.Enum, TextTags.Enum },
            { LayoutTag.Event, TextTags.Event },
            { LayoutTag.Field, TextTags.Field },
            { LayoutTag.Interface, TextTags.Interface },
            { LayoutTag.Keyword, TextTags.Keyword },
            { LayoutTag.LineBreak, TextTags.LineBreak },
            { LayoutTag.Local, TextTags.Local },
            { LayoutTag.Member, "Member" },
            { LayoutTag.Method, TextTags.Method },
            { LayoutTag.Module, TextTags.Module },
            { LayoutTag.ModuleBinding, "TODO" },
            { LayoutTag.Namespace, TextTags.Namespace },
            { LayoutTag.NumericLiteral, TextTags.NumericLiteral },
            { LayoutTag.Operator, TextTags.Operator },
            { LayoutTag.Parameter, TextTags.Parameter },
            { LayoutTag.Property, TextTags.Property },
            { LayoutTag.Punctuation, TextTags.Punctuation },
            { LayoutTag.Record, "TODO" },
            { LayoutTag.RecordField, "TODO" },
            { LayoutTag.Space, TextTags.Space },
            { LayoutTag.StringLiteral, TextTags.StringLiteral },
            { LayoutTag.Struct, TextTags.Struct },
            { LayoutTag.Text, TextTags.Text },
            { LayoutTag.TypeParameter, TextTags.TypeParameter },
            { LayoutTag.Union, "Union" },
            { LayoutTag.UnionCase, "TODO" },
            { LayoutTag.UnknownEntity, "TODO" },
            { LayoutTag.UnknownType, "TODO" }
        };

        public static ImmutableArray<QuickInfoSection> ToQuickInfo(FSharpList<FSharpToolTipElement<Layout>> elements) {
            var sections = ImmutableArray.CreateBuilder<QuickInfoSection>(elements.Length);
            foreach (var element in elements) {
                AddQuickInfo(sections, element);
            }
            return sections.MoveToImmutable();
        }

        private static void AddQuickInfo(ImmutableArray<QuickInfoSection>.Builder sections, FSharpToolTipElement<Layout> element) {
            switch (element.Tag) {
                case FSharpToolTipElement<Layout>.Tags.Group:
                    var items = ((FSharpToolTipElement<Layout>.Group)element).Item;
                    foreach (var item in items) {
                        AddQuickInfo(sections, item);
                    }
                    break;
                case FSharpToolTipElement<Layout>.Tags.CompositionError:
                    var error = (FSharpToolTipElement<Layout>.CompositionError)element;
                    sections.Add(QuickInfoSection.Create("FSharpCompositionError", ImmutableArray.Create(new TaggedText(TextTags.Text, error.Item))));
                    break;
                case FSharpToolTipElement<Layout>.Tags.None:
                    break;
            }
        }

        private static void AddQuickInfo(ImmutableArray<QuickInfoSection>.Builder sections, FSharpToolTipElementData<Layout> item) {
            AddQuickInfo(sections, item.MainDescription, QuickInfoSectionKinds.Description);
            AddQuickInfo(sections, item.XmlDoc);
            AddQuickInfo(sections, item.TypeMapping, QuickInfoSectionKinds.AnonymousTypes);
            AddQuickInfo(sections, item.Remarks, QuickInfoSectionKinds.Description);
        }

        private static void AddQuickInfo(ImmutableArray<QuickInfoSection>.Builder sections, FSharpOption<Layout> layout, string kind) {
            if (layout.IsNone())
                return;
            AddQuickInfo(sections, layout.Value, kind);
        }

        private static void AddQuickInfo(ImmutableArray<QuickInfoSection>.Builder sections, FSharpList<Layout> layouts, string kind) {
            foreach (var layout in layouts) {
                AddQuickInfo(sections, layout, kind);
            }
        }

        private static void AddQuickInfo(ImmutableArray<QuickInfoSection>.Builder sections, Layout layout, string kind) {
            var taggedTextBuilder = ImmutableArray.CreateBuilder<TaggedText>();
            AddQuickInfo(taggedTextBuilder, layout);

            var section = QuickInfoSection.Create(kind, taggedTextBuilder.ToImmutable());
            sections.Add(section);
        }

        private static void AddQuickInfo(ImmutableArray<TaggedText>.Builder texts, Layout layout) {
            switch (layout.Tag) {
                case Layout.Tags.Leaf:
                    var leaf = (Layout.Leaf)layout;
                    var textTag = LayoutTagToTextTagMap[leaf.Item2.Tag];
                    texts.Add(new TaggedText(textTag, leaf.Item2.Text));
                    break;
                case Layout.Tags.Node:
                    var node = (Layout.Node)layout;
                    AddQuickInfo(texts, node.Item2);
                    texts.Add(new TaggedText(TextTags.Space, ToString(node.Item6)));
                    AddQuickInfo(texts, node.Item4);
                    break;
                case Layout.Tags.Attr:
                    break;
                case Layout.Tags.ObjLeaf:
                    break;
            }
        }

        private static string ToString(Joint joint) {
            switch (joint.Tag) {
                case Joint.Tags.Breakable:
                    return " ";
                case Joint.Tags.Unbreakable:
                    return "\u00a0";
                case Joint.Tags.Broken:
                    return "\r\n";
                default:
                    throw new ArgumentOutOfRangeException(nameof(joint));
            }
        }

        private static void AddQuickInfo(ImmutableArray<QuickInfoSection>.Builder sections, FSharpXmlDoc xmlDoc) {
            
        }
    }
}
