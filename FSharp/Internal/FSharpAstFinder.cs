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

    internal class FSharpAstFinder : AstTraversal.AstVisitorBase<(Range.range range, FSharpList<string> names, int tag)> {
        private static readonly FSharpFunc<Ast.Ident, string> IdentToName = FSharpFunc<Ast.Ident, string>.FromConverter(i => i.idText);
        private readonly Range.pos _position;

        public FSharpAstFinder(Range.pos position) {
            _position = position;
        }

        public override FSharpOption<Result> VisitExpr(
            FSharpList<AstTraversal.TraverseStep> path,
            FSharpFunc<Ast.SynExpr, FSharpOption<Result>> traverseSynExpr,
            FSharpFunc<Ast.SynExpr, FSharpOption<Result>> defaultTraverse,
            Ast.SynExpr expr
        ) {
            return (expr.Range, FSharpList<string>.Empty, expr.Tag);
        }

        public override FSharpOption<Result> VisitComponentInfo(Ast.SynComponentInfo info) {
            var idStart = (Range.pos?)null;
            var idEnd = (Range.pos?)null;
            foreach (var id in info.longId) {
                idStart = (idStart == null || Range.posLt(id.idRange.Start, idStart.Value)) ? id.idRange.Start : idStart;
                idEnd = (idEnd == null || Range.posGt(id.idRange.End, idEnd.Value)) ? id.idRange.End : idEnd;
            }

            if (idStart == null || idEnd == null)
                return FSharpOption<Result>.None;

            var idRange = Range.mkRange("", idStart.Value, idEnd.Value);
            if (!Range.rangeContainsPos(idRange, _position))
                return FSharpOption<Result>.None;

            var names = ListModule.Map(IdentToName, info.longId);
            return (idRange, names, FSharpTokenTag.Identifier);
        }

        public override FSharpOption<Result> VisitPat(FSharpFunc<Ast.SynPat, FSharpOption<Result>> defaultTraverse, Ast.SynPat pat) {
            if (pat.IsLongIdent) {
                var id = (Ast.SynPat.LongIdent)pat;
                if (Range.rangeContainsPos(pat.Range, _position))
                    return (pat.Range, ListModule.Map(IdentToName, id.longDotId.id), FSharpTokenTag.Identifier);
            }

            return defaultTraverse.Invoke(pat);
        }
    }
}