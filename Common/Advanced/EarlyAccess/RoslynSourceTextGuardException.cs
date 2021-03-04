using System;

namespace MirrorSharp.Advanced.EarlyAccess {
    internal class RoslynSourceTextGuardException : Exception {
        public RoslynSourceTextGuardException() { }
        public RoslynSourceTextGuardException(string message) : base(message) { }
        public RoslynSourceTextGuardException(string message, Exception inner) : base(message, inner) { }
    }
}
