using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Completion;

namespace MirrorSharp.Internal {
    internal class Completion {
        public Completion(CompletionService service) {
            Service = service;
        }

        public CompletionService Service { get; }
        [CanBeNull] public CompletionList CurrentList { get; set; }
        public bool ChangeEchoPending { get; set; }
        public char? PendingChar { get; set; }

        public void ResetPending() {
            ChangeEchoPending = false;
            PendingChar = null;
        }
    }
}

