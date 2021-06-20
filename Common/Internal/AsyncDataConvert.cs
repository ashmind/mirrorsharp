using System;
using System.Buffers;
using System.Text;
using System.Threading.Tasks;

namespace MirrorSharp.Internal {
    internal static class AsyncDataConvert {
        public static async ValueTask<string> ToUtf8StringAsync(AsyncData data, int offsetInFirstSegment, ArrayPool<char> charArrayPool) {
            var first = data.GetFirst().Slice(offsetInFirstSegment);
            if (!data.MightHaveNext)
                return Encoding.UTF8.GetString(first.Span);

            var decoder = Encoding.UTF8.GetDecoder();
            using var chars = new PooledGrowableArray<char>(first.Length * 2, charArrayPool);

            decoder.Convert(first.Span, chars.Array.AsSpan(), false, out int bytesUsed, out int charsUsed, out bool completed);
            var charsTotalCount = charsUsed;
            var next = await data.GetNextAsync().ConfigureAwait(false);
            while (next != null) {
                var requiredCharCount = charsTotalCount + next.Value.Length;
                if (requiredCharCount > chars.Array.Length)
                    chars.Grow(requiredCharCount);

                decoder.Convert(next.Value.Span, chars.Array.AsSpan().Slice(charsTotalCount), false, out bytesUsed, out charsUsed, out completed);
                charsTotalCount += charsUsed;
                next = await data.GetNextAsync().ConfigureAwait(false);
            }
            if (!completed) {
                // Flush. Ignore the first array -- can be anything
                decoder.Convert(ReadOnlySpan<byte>.Empty, chars.Array.AsSpan().Slice(charsTotalCount), true, out bytesUsed, out charsUsed, out completed);
                charsTotalCount += charsUsed;
            }
            return new string(chars.Array, 0, charsTotalCount);
        }
    }
}
