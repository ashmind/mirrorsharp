namespace MirrorSharp.Advanced {
    public interface ISetOptionsFromClientExtension {
        bool TrySetOption(IWorkSession session, string name, string value);
    }
}