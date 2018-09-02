using System;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Advanced {
    /// <summary>Represents a user session based on Roslyn.</summary>
    [PublicAPI]
    public interface IRoslynSession {
        /// <summary>Roslyn <see cref="Microsoft.CodeAnalysis.Project"/> associated with the current session.</summary>
        [PublicAPI, NotNull] Project Project { get; set; }

        /// <summary>
        /// Sets or unsets Script mode for the Roslyn session.
        /// </summary>
        /// <param name="isScript">Whether the session should use script mode.</param>
        /// <param name="hostObjectType">Host object type for the session; must be <c>null</c> if <paramref name="isScript" /> is <c>false</c>.</param>
        /// <remarks>
        /// Members of <paramref name="hostObjectType" /> are directly available to the script. For example
        /// if you set <c>hostObjectType</c> is <see cref="Random" />, you can use <see cref="Random.Next()" />
        /// in the script by just writing <c>Next()</c>.
        /// </remarks>
        /// <seealso cref="ProjectInfo.IsSubmission"/>
        /// <seealso cref="ProjectInfo.HostObjectType"/>
        void SetScriptMode(bool isScript = true, [CanBeNull] Type hostObjectType = null);
    }
}