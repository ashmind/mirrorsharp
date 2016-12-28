using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Completion;

namespace MirrorSharp.Internal {
    public class Completion {
        public Completion(CompletionService service) {
            Service = service;
        }

        public CompletionService Service { get; }
        [CanBeNull] public CompletionList CurrentList { get; set; }
        public bool ChangeEchoPending { get; set; }
        public CompletionTrigger? PendingTrigger { get; set; }
    }
}
