using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Compiler;
using Microsoft.FSharp.Compiler.SourceCodeServices;
using Microsoft.FSharp.Core;

namespace MirrorSharp.FSharp.Internal {
    using Result = ValueTuple<Range.range, FSharpList<string>, int>;

    internal class FSharpAstProjector : AstTraversal.AstVisitorBase<(Range.range range, FSharpList<string> names, int tag)> {
        private static readonly FSharpFunc<Ast.Ident, string> IdentToName = FSharpFunc<Ast.Ident, string>.FromConverter(i => i.idText);

        public static FSharpAstProjector Default { get; } = new FSharpAstProjector();

        public override FSharpOption<Result> VisitExpr(
            FSharpList<AstTraversal.TraverseStep> path,
            FSharpFunc<Ast.SynExpr, FSharpOption<Result>> traverseSynExpr,
            FSharpFunc<Ast.SynExpr, FSharpOption<Result>> defaultTraverse,
            Ast.SynExpr expr
        ) {
            return (expr.Range, FSharpList<string>.Empty, expr.Tag);
        }

        public override FSharpOption<Result> VisitComponentInfo(Ast.SynComponentInfo info) {
            var names = ListModule.Map(IdentToName, info.longId);
            return (info.Range, names, FSharpTokenTag.Identifier);
        }
    }
}