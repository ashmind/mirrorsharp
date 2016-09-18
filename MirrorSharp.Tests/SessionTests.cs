using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MirrorSharp.Internal;
using Xunit;

namespace MirrorSharp.Tests {
    public class SessionTests {
        private static readonly string[] ObjectMemberNames = {
            nameof(Equals),
            nameof(GetHashCode),
            nameof(GetType),
            nameof(ToString)
        };

        [Fact]
        public async Task TypeChar_InsertsSingleChar() {
            var session = SessionFromTextWithCursor("class A| {}");

            await session.TypeCharAsync('1', CancellationToken.None);

            Assert.Equal("class A1 {}", session.SourceText.ToString());
        }

        [Fact]
        public async Task TypeChar_MovesCursorBySingleChar() {
            var session = SessionFromTextWithCursor("class A| {}");
            var cursorPosition = session.CursorPosition;

            await session.TypeCharAsync('1', CancellationToken.None);

            Assert.Equal(cursorPosition + 1, session.CursorPosition);
        }

        [Fact]
        public async Task TypeChar_ProducesExpectedCompletion() {
            var session = SessionFromTextWithCursor(@"
                class A { public int x; }
                class B { void M(A a) { a| } }
            ");

            var result = await session.TypeCharAsync('.', CancellationToken.None);

            Assert.Equal(
                new[] { "x" }.Concat(ObjectMemberNames).OrderBy(n => n),
                result.Completions.Items.Select(i => i.DisplayText).OrderBy(n => n)
            );
        }

        [Fact]
        public async Task SlowUpdate_ProducesDiagnosticWithCustomTagUnnecessary_ForUnusedNamespace() {
            var session = SessionFromTextWithCursor(@"using System;|");
            var result = await session.GetSlowUpdateAsync(CancellationToken.None);

            Assert.Contains(
                new { Severity = DiagnosticSeverity.Hidden, IsUnnecessary = true },
                result.Diagnostics.Select(
                    d => new { d.Severity, IsUnnecessary = d.Descriptor.CustomTags.Contains(WellKnownDiagnosticTags.Unnecessary) }
                ).ToArray()
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
