using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;

namespace MirrorSharp.Internal {
    internal class CustomWorkspace : Workspace {
        public CustomWorkspace(HostServices host) : base(host, "Custom" /* same as AdHoc */) {
        }

        public override bool CanOpenDocuments => true;

        public override bool CanApplyChange(ApplyChangesKind feature) {
            return feature == ApplyChangesKind.ChangeDocument;
        }

        public new Solution SetCurrentSolution(Solution solution) {
            return base.SetCurrentSolution(solution);
        }
    }
}
