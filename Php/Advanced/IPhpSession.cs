using Pchp.CodeAnalysis;

namespace MirrorSharp.Php.Advanced {
    /// <summary>Represents a user session based on the Peachpie PHP compiler.</summary>
    public interface IPhpSession {
        /// <summary>Object containing the syntax and semantic information.</summary>
        /// <note>We work directly with the <see cref="PhpCompilation"/> object, because the project system is not yet implemented in Peachpie.</note>
        PhpCompilation Compilation { get; set; }
    }
}
