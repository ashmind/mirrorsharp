using System;
using System.Text;

namespace MirrorSharp.Internal {
    internal static class EncodingExtensions {
        public static string GetString(this Encoding encoding, ArraySegment<byte> segment) {
            return encoding.GetString(segment.Array, segment.Offset, segment.Count);
        }
    }
}
