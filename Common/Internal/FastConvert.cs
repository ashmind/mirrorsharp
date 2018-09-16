using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirrorSharp.Internal {
    internal static class FastConvert {
        private static readonly ArrayPool<char> CharPool = ArrayPool<char>.Shared;
        private const byte Utf8Zero = (byte)'0';
        private const byte Utf8Nine = (byte)'9';

        private static readonly string[] CharStringMap =
            Enumerable.Range(0, 128).Select(c => ((char)c).ToString()).ToArray();

        private static readonly ConcurrentDictionary<string, string> LowerInvariantStrings =
            new ConcurrentDictionary<string, string>();

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

            if (charCount != 1) {
                // ReSharper disable once HeapView.BoxingAllocation
                throw new FormatException($"Expected one char, but conversion produced {charCount}.");
            }

            return buffer[0];
        }

        public static string CharToString(char c) {
            if (c <= 127)
                return CharStringMap[c];

            return c.ToString();
        }

        public static string StringToLowerInvariantString(string value) {
            if (LowerInvariantStrings.TryGetValue(value, out string result))
                return result;

            var lower = value.ToLowerInvariant();
            LowerInvariantStrings.TryAdd(value, lower);
            return lower;
        }

        public static string EnumToLowerInvariantString<TEnum>(TEnum value)
            where TEnum : struct, IFormattable
        {
            return EnumCache<TEnum>.LowerInvariantStrings[value];
        }

        private static string SlowUtf8ByteArrayToString(ArraySegment<byte> bytes) {
            return Encoding.UTF8.GetString(bytes);
        }

        private static class EnumCache<TEnum>
            where TEnum: struct, IFormattable
        {
            public static readonly IReadOnlyDictionary<TEnum, string> LowerInvariantStrings =
                Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToDictionary(e => e, e => e.ToString("G", null).ToLowerInvariant());
        }
    }
}
