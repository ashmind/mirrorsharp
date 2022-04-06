namespace MirrorSharp.Internal.Roslyn {
    // TODO: Refactor to a better object model
    internal enum RoslynInternalsLoadStrategy {
        // Default
        MatchVersion,
        // SharpLab
        TryMatchVersionThenLatest
    }
}