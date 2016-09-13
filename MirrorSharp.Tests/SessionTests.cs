using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MirrorSharp.Internal;
using Xunit;

namespace MirrorSharp.Tests {
    public class SessionTests {
        [Fact]
        public async Task TypeChar_InsertsSingleChar() {
            var session = SessionFromTextWithCursor("class A| {}");

            await session.TypeCharAsync('1');

            Assert.Equal("class A1 {}", session.SourceText.ToString());
        }

        [Fact]
        public async Task TypeChar_MovesCursorBySingleChar() {
            var session = SessionFromTextWithCursor("class A| {}");
            var cursorPosition = session.CursorPosition;

            await session.TypeCharAsync('1');

            Assert.Equal(cursorPosition + 1, session.CursorPosition);
        }

        [Fact]
        public async Task TypeChar_ProducesExpectedCompletion() {
            var session = SessionFromTextWithCursor(@"
                class A { public int x; }
                class B { void M(A a) { a| } }
            ");

            var result = await session.TypeCharAsync('.');

            Assert.Equal(
                new[] { "x" },
                result.Completions.Items.Select(i => i.DisplayText)
            );
        }

        private WorkSession SessionFromTextWithCursor(string textWithCursor) {
            var cursorPosition = textWithCursor.LastIndexOf('|');
            var text = textWithCursor.Remove(cursorPosition, 1);

            var session = new WorkSession();
            session.ReplaceText(0, 0, text, cursorPosition);
            return session;
        }
    }
}
