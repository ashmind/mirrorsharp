using JetBrains.Annotations;
using Microsoft.FSharp.Compiler.SourceCodeServices;

namespace MirrorSharp.FSharp.Advanced {
    public class FSharpParseAndCheckResults {
        public FSharpParseAndCheckResults(FSharpParseFileResults parseResults, FSharpCheckFileAnswer checkAnswer) {
            ParseResults = parseResults;
            CheckAnswer = checkAnswer;
        }

        [NotNull] public FSharpParseFileResults ParseResults { get; }
        [NotNull] public FSharpCheckFileAnswer CheckAnswer { get; }
    }
}
