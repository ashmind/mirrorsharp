using MirrorSharp.Advanced;

namespace MirrorSharp.Internal {
    internal interface IFastJsonWriterInternal : IFastJsonWriter {
        void WriteProperty(string name, CharArrayString value);
        void WriteValue(CharArrayString value);
    }
}
