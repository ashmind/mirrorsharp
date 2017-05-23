namespace MirrorSharp.Advanced {
    /// <summary>An interface used to implement custom (extension) options.</summary>
    public interface ISetOptionsFromClientExtension {
        /// <summary>Method called each time MirrorSharp encounters an extension option (<c>x-*</c>).</summary>
        /// <param name="session">Current <see cref="IWorkSession" />.</param>
        /// <param name="name">Name of the extension option; always starts with 'x-'.</param>
        /// <param name="value">Value of the extension option, as provided by the client.</param>
        /// <returns><c>true</c> if extension options is recognized; otherwise, <c>false</c>.</returns>
        bool TrySetOption(IWorkSession session, string name, string value);
    }
}