using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorSharp.Internal {
    public static class EncodingExtensions {
        public static string GetString(this Encoding encoding, ArraySegment<byte> segment) {
            return encoding.GetString(segment.Array, segment.Offset, segment.Count);
        }
    }
}
