// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MirrorSharp.Testing.Internal.Results {
    internal class ResultChange {
        public ResultChange(int start, int length, string text) {
            Start = start;
            Length = length;
            Text = text;
        }

        public int Start { get; }
        public int Length { get; }
        public string Text { get; }
    }
}