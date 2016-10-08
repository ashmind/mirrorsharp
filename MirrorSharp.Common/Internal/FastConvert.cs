using System;
using System.Buffers;
using System.Linq;
using System.Text;

namespace MirrorSharp.Internal {
    internal static class FastConvert {
        private static readonly ArrayPool<char> CharPool = ArrayPool<char>.Shared;
        private const byte Utf8Zero = (byte)'0';
        private const byte Utf8Nine = (byte)'9';

        private static readonly string[] CharStringMap =
            Enumerable.Range(0, 128).Select(c => ((char)c).ToString()).ToArray();

        public static int Utf8ByteArrayToInt32(ArraySegment<byte> bytes) {
            var result = 0;
            var array = bytes.Array;
            var count = bytes.Offset + bytes.Count;
            for (var i = bytes.Offset; i < count; i++) {
                var @byte = array[i];
                if (@byte < Utf8Zero || @byte > Utf8Nine)
                    throw new FormatException($"String '{SlowUtf8ByteArrayToString(bytes)}' is not a valid positive number.");

                result = (10 * result) + (@byte - Utf8Zero);
            }
            return result;
        }

        public static char Utf8ByteArrayToChar(ArraySegment<byte> bytes) {
            if (bytes.Count == 1)
                return (char)bytes.Array[bytes.Offset];

            var buffer = CharPool.Rent(2);
            int charCount;
            try {
                charCount = Encoding.UTF8.GetChars(bytes.Array, bytes.Offset, bytes.Count, buffer, 0);
            }
            finally {
                CharPool.Return(buffer);
            }

            if (charCount != 1)
                throw new FormatException($"Expected one char, but conversion produced {charCount}.");

            return buffer[0];
        }

        public static string CharToString(char c) {
            if (c <= 127)
                return CharStringMap[c];

            return c.ToString();
        }

        private static string SlowUtf8ByteArrayToString(ArraySegment<byte> bytes) {
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
