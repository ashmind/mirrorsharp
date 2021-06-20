using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirrorSharp.Internal {
    internal static class FastConvert {
        private const byte Utf8Zero = (byte)'0';
        private const byte Utf8Nine = (byte)'9';

        private static readonly string[] CharStringMap =
            Enumerable.Range(0, 128).Select(c => ((char)c).ToString()).ToArray();

        private static readonly ConcurrentDictionary<string, string> LowerInvariantStrings = new();

        public static int Utf8BytesToInt32(ReadOnlySpan<byte> bytes) {
            var result = 0;
            var length = bytes.Length;
            foreach (var @byte in bytes) {
                if (@byte < Utf8Zero || @byte > Utf8Nine)
                    throw new FormatException($"String '{SlowUtf8BytesToString(bytes)}' is not a valid positive number.");

                result = (10 * result) + (@byte - Utf8Zero);
            }
            return result;
        }

        public static char Utf8BytesToChar(ReadOnlySpan<byte> bytes) {
            if (bytes.Length == 1)
                return (char)bytes[0];

            var chars = (Span<char>)stackalloc char[2];
            var charCount = Encoding.UTF8.GetChars(bytes, chars);
            if (charCount != 1)
                throw new FormatException($"Expected one char, but conversion produced {charCount}. Bytes: {SlowBytesToHexString(bytes)}");

            return chars[0];
        }

        public static string CharToString(char c) {
            if (c <= 127)
                return CharStringMap[c];

            return c.ToString();
        }

        public static string StringToLowerInvariantString(string value) {
            if (LowerInvariantStrings.TryGetValue(value, out var result))
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

        private static string SlowUtf8BytesToString(ReadOnlySpan<byte> bytes) {
            return Encoding.UTF8.GetString(bytes);
        }

        private static string SlowBytesToHexString(ReadOnlySpan<byte> bytes) {
            return "0x" + string.Join("", bytes.ToArray().Select(b => b.ToString("X2")));
        }

        private static class EnumCache<TEnum>
            where TEnum: struct, IFormattable
        {
            public static readonly IReadOnlyDictionary<TEnum, string> LowerInvariantStrings =
                Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToDictionary(e => e, e => e.ToString("G", null).ToLowerInvariant());
        }
    }
}
