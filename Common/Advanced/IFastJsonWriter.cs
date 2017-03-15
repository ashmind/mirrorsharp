using System;
using JetBrains.Annotations;

namespace MirrorSharp.Advanced {
    [PublicAPI]
    public interface IFastJsonWriter : IDisposable {
        void WriteStartObject();
        void WriteEndObject();
        void WriteStartArray();
        void WriteEndArray();
        void WriteProperty([NotNull] string name, [CanBeNull] string value);
        void WriteProperty([NotNull] string name, char value);
        void WriteProperty([NotNull] string name, int value);
        void WriteProperty([NotNull] string name, bool value);
        void WritePropertyStartObject([NotNull] string name);
        void WritePropertyStartArray([NotNull] string name);
        void WritePropertyName([NotNull] string name);
        void WriteValue([CanBeNull] string value);
        void WriteValue(char value);
        void WriteValue(int value);
        void WriteValue(bool value);
    }
}