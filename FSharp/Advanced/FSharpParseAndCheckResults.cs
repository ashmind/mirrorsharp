using FSharp.Compiler.SourceCodeServices;

namespace MirrorSharp.FSharp.Advanced {
    /// <summary>Represent combined Parse and Check results from the <see cref="FSharpChecker" />.</summary>
    public class FSharpParseAndCheckResults {
        internal FSharpParseAndCheckResults(FSharpParseFileResults parseResults, FSharpCheckFileAnswer checkAnswer) {
            ParseResults = parseResults;
            CheckAnswer = checkAnswer;
        }

        /// <summary>Gets the Parse results.</summary>
        public FSharpParseFileResults ParseResults { get; }

        /// <summary>Gets the Check answer.</summary>
        public FSharpCheckFileAnswer CheckAnswer { get; }
    }
}
