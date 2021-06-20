using System;
using System.Text;

namespace MirrorSharp.Internal {
    internal static class EncodingExtensions {
        public static string GetString(this Encoding encoding, ArraySegment<byte> segment) {
            return encoding.GetString(segment.Array, segment.Offset, segment.Count);
        }

        #if NETSTANDARD2_0
        public static unsafe int GetChars(this Encoding encoding, ReadOnlySpan<byte> bytes, Span<char> chars) {
            if (bytes.IsEmpty)
                return 0;

            fixed (byte* bytePointer = bytes)
            fixed (char* charPointer = chars) {
                return Encoding.UTF8.GetChars(bytePointer, bytes.Length, charPointer, chars.Length);
            }
        }

        public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes) {
            if (bytes.IsEmpty)
                return string.Empty;

            fixed (byte* bytePointer = bytes)
                return Encoding.UTF8.GetString(bytePointer, bytes.Length);
        }

        public static unsafe void Convert(this Decoder decoder, ReadOnlySpan<byte> bytes, Span<char> chars, bool flush, out int bytesUsed, out int charsUsed, out bool completed) {
            if (bytes.IsEmpty) {
                // Cannot just return, we might still need to flush
                fixed (byte* bytePointer = Array.Empty<byte>())
                fixed (char* charPointer = chars) {
                    decoder.Convert(bytePointer, 0, charPointer, chars.Length, flush, out bytesUsed, out charsUsed, out completed);
                }
                return;
            }

            fixed (byte* bytePointer = bytes)
            fixed (char* charPointer = chars) {
                decoder.Convert(bytePointer, bytes.Length, charPointer, chars.Length, flush, out bytesUsed, out charsUsed, out completed);
            }
        }
        #endif
    }
}
