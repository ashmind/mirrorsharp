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

        //public override FSharpOption<Result> VisitComponentInfo(Ast.SynComponentInfo info) {
        //    var idStart = (Range.pos?)null;
        //    var idEnd = (Range.pos?)null;
        //    var names = FSharpList<string>.Empty;
        //    foreach (var id in info.longId) {
        //        idStart = (idStart == null || Range.posLt(id.idRange.Start, idStart.Value)) ? id.idRange.Start : idStart;
        //        idEnd = (idEnd == null || Range.posGt(id.idRange.End, idEnd.Value)) ? id.idRange.End : idEnd;
        //        names = FSharpList<string>.Cons(id.idText, names);
        //    }

        //    if (idStart == null || idEnd == null)
        //        return FSharpOption<Result>.None;

        //    var range = Range.mkRange("", idStart.Value, idEnd.Value);
        //    return (range, names, FSharpTokenTag.Identifier);
        //}

        public override FSharpOption<Result> VisitBinding(FSharpFunc<Ast.SynBinding, FSharpOption<Result>> defaultTraverse, Ast.SynBinding binding) {
            return base.VisitBinding(defaultTraverse, binding);
        }

        public override FSharpOption<Result> VisitSimplePats(FSharpList<Ast.SynSimplePat> _arg6) {
            return base.VisitSimplePats(_arg6);
        }
    }
}