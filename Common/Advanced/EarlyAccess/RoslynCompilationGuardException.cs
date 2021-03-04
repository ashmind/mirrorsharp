using System;

namespace MirrorSharp.Advanced.EarlyAccess {
    internal class RoslynCompilationGuardException : Exception {
        public RoslynCompilationGuardException() { }
        public RoslynCompilationGuardException(string message) : base(message) { }
        public RoslynCompilationGuardException(string message, Exception inner) : base(message, inner) { }
    }
}
