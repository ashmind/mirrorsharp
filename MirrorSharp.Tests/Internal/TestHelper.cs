using System;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using MirrorSharp.Internal;

namespace MirrorSharp.Tests.Internal {
    public static class TestHelper {
        public static WorkSession SessionFromTextWithCursor(string textWithCursor) {
            var cursorPosition = textWithCursor.LastIndexOf('|');
            var text = textWithCursor.Remove(cursorPosition, 1);

            var session = new WorkSession {
                CursorPosition = cursorPosition,
                SourceText = SourceText.From(text)
            };
            return session;
        }

        public static ArraySegment<byte> ToByteArraySegment(params char[] chars) {
            return new ArraySegment<byte>(Encoding.UTF8.GetBytes(chars));
        }
    }
}
