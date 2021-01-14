using System;

namespace MirrorSharp.Advanced.EarlyAccess {
    internal class RoslynGuardException : Exception {
        public RoslynGuardException() { }
        public RoslynGuardException(string message) : base(message) { }
        public RoslynGuardException(string message, Exception inner) : base(message, inner) { }
    }
}
