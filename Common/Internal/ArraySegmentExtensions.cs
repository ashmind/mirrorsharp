using System;

namespace MirrorSharp.Internal {
    internal static class ArraySegmentExtensions {
        public static ArraySegment<T> Skip<T>(this ArraySegment<T> segment, int count) {
            return new ArraySegment<T>(segment.Array, segment.Offset + count, segment.Count - count);
        }
    }
}
