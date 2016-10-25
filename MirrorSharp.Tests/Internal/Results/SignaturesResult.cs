using System.Collections.Generic;
using Microsoft.CodeAnalysis;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Tests.Internal.Results {
    public class SignaturesResult {
        public ResultSpan Span { get; set; }
        public IList<ResultSignature> Signatures { get; } = new List<ResultSignature>();

        public class ResultSpan {
            public int Start { get; set; }
            public int Length { get; set; }
        }

        public class ResultSignature {
            public bool Selected { get; set; }
            public IList<ResultSignaturePart> Parts { get; } = new List<ResultSignaturePart>();
        }

        public class ResultSignaturePart {
            public string Text { get; set; }
            public SymbolDisplayPartKind Kind { get; set; }
            public bool Selected { get; set; }
        }
    }
}