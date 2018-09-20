using Microsoft.FSharp.Core;

namespace MirrorSharp.FSharp.Internal {
    internal static class FSharpExtensions {
        public static bool IsSome<T>(this FSharpOption<T> option)
            => FSharpOption<T>.get_IsSome(option);
        public static bool IsNone<T>(this FSharpOption<T> option)
            => FSharpOption<T>.get_IsNone(option);

        public static T ValueOrDefault<T>(this FSharpOption<T> option) {
            if (option.IsNone())
                return default(T);
            return option.Value;
        }

        public static FSharpOption<T> Coalesce<T>(this FSharpOption<T> option, T other) {
            if (option.IsNone())
                return other;
            return option;
        }
    }
}
