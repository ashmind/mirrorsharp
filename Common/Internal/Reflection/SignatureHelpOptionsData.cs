using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Reflection {
    internal struct SignatureHelpOptionsData {
        private readonly Project _fromProject;

        private SignatureHelpOptionsData(Project fromProject) {
            _fromProject = fromProject;
        }

        public static SignatureHelpOptionsData From(Project project) => new (project);

        public static Expression ToInternalTypeExpressionSlow(Expression expression) {
            if (RoslynTypes.SignatureHelpOptions == null)
                throw new InvalidOperationException("Current version of Roslyn does not include SignatureHelpOptions.");

            var from = RoslynTypes.SignatureHelpOptions.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Single(m => m.Name == "From" && m.GetParameters() is { Length: 1 } ps && ps[0].ParameterType == typeof(Project));
            return Expression.Call(
                from,
                expression.Field(nameof(_fromProject))
            );
        }
    }
}