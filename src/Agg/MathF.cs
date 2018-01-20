using System;
using System.Runtime.CompilerServices;

namespace Agg {
    internal static class MathF {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin(float radian) {
            return (float)Math.Sin(radian);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos(float radian) {
            return (float)Math.Cos(radian);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Asin(float radian) {
            return (float)Math.Asin(radian);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Acos(float radian) {
            return (float)Math.Acos(radian);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Atan2(float y, float x) {
            return (float)Math.Atan2(y, x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sqrt(float value) {
            return (float)Math.Sqrt(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Round(float value) {
            return (float)Math.Round(value);
        }

    }
}
