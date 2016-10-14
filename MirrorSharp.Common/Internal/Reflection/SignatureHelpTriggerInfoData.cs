using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MirrorSharp.Internal.Reflection {
    internal struct SignatureHelpTriggerInfoData {
        public SignatureHelpTriggerReason TriggerReason { get; }
        public char? TriggerCharacter { get; }

        public SignatureHelpTriggerInfoData(SignatureHelpTriggerReason triggerReason, char? triggerCharacter = null) {
            TriggerReason = triggerReason;
            TriggerCharacter = triggerCharacter;
        }

        public static Expression ToInternalTypeExpressionSlow(Expression expression) {
            return Expression.New(
                RoslynTypes.SignatureHelpTriggerInfo.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single(),
                expression.Property(nameof(TriggerReason)).Convert(RoslynTypes.SignatureHelpTriggerReason.AsType()),
                expression.Property(nameof(TriggerCharacter))
            );
        }
    }
}