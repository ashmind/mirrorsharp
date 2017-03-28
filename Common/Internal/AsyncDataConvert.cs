using System;
using System.Buffers;
using System.Text;
using System.Threading.Tasks;

namespace MirrorSharp.Internal {
    internal static class AsyncDataConvert {
        public static async ValueTask<string> ToUtf8StringAsync(AsyncData data, int offsetInFirstSegment, ArrayPool<char> charArrayPool) {
            var first = data.GetFirst();
            var startOffset = first.Offset + offsetInFirstSegment;
            var firstCount = first.Count - offsetInFirstSegment;
            if (!data.MightHaveNext)
                return Encoding.UTF8.GetString(first.Array, startOffset, firstCount);

            var decoder = Encoding.UTF8.GetDecoder();
            using (var chars = new PooledGrowableArray<char>(first.Count * 2, charArrayPool)) {
                decoder.Convert(first.Array, startOffset, firstCount, chars.Array, 0, chars.Array.Length, false, out int bytesUsed, out int charsUsed, out bool completed);
                var charsTotalCount = charsUsed;
                var next = await data.GetNextAsync().ConfigureAwait(false);
                while (next != null) {
                    var requiredCharCount = charsTotalCount + next.Value.Count;
                    if (requiredCharCount > chars.Array.Length)
                        chars.Grow(requiredCharCount);

                    decoder.Convert(next.Value, chars.Array, charsTotalCount, chars.Array.Length - charsTotalCount, false, out bytesUsed, out charsUsed, out completed);
                    charsTotalCount += charsUsed;
                    next = await data.GetNextAsync().ConfigureAwait(false);
                }
                if (!completed) {
                    // Flush. Ignore the first array -- can be any empty or non-empty array, using existing one to avoid allocations.
                    decoder.Convert(first.Array, 0, 0, chars.Array, charsTotalCount, chars.Array.Length - charsTotalCount, true, out bytesUsed, out charsUsed, out completed);
                    charsTotalCount += charsUsed;
                }
                return new string(chars.Array, 0, charsTotalCount);
            }
        }
    }
}
