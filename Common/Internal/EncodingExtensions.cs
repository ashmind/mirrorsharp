using System;
using System.Text;

namespace MirrorSharp.Internal {
    internal static class EncodingExtensions {
        public static string GetString(this Encoding encoding, ArraySegment<byte> segment) {
            return encoding.GetString(segment.Array, segment.Offset, segment.Count);
        }

        public static void Convert(this Decoder decoder, ArraySegment<byte> bytes, char[] chars, int charIndex, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed) {
            decoder.Convert(bytes.Array, bytes.Offset, bytes.Count, chars, charIndex, charCount, flush, out bytesUsed, out charsUsed, out completed);
        }
    }
}
