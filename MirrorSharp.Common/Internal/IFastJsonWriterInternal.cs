using MirrorSharp.Advanced;

namespace MirrorSharp.Internal {
    internal interface IFastJsonWriterInternal : IFastJsonWriter {
        void WriteProperty(string name, CharListString value);
        void WriteValue(CharListString value);
    }
}
