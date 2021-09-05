using Mobius.ILasm.Core;

namespace MirrorSharp.IL.Advanced {
    /// <summary>Represents a user session based on IL (intermediate language) parser.</summary>
    public interface IILSession {
        /// <summary>Gets or sets the <see cref="Driver.Target" /> associated with this session.</summary>
        Driver.Target Target { get; set; }
    }
}
