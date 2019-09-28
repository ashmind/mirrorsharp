namespace MirrorSharp.Testing.Internal {
    internal class TextWithCursor {
        public static TextWithCursor Parse(string textWithCursor, char cursor = '|') {
            var cursorPosition = textWithCursor.LastIndexOf(cursor);
            var text = textWithCursor.Remove(cursorPosition, 1);

            return new TextWithCursor(text, cursorPosition);
        }

        private TextWithCursor(string text, int cursorPosition) {
            Text = text;
            CursorPosition = cursorPosition;
        }

        public string Text { get; }
        public int CursorPosition { get; }
    }
}
