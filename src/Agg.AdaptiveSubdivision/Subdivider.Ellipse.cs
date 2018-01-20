using System;
using Microsoft.Xna.Framework;

namespace Agg.AdaptiveSubdivision {
    partial class Subdivider {

        public static Vector2[] DivideEllipse(float centerX, float centerY, float radiusX, float radiusY, bool isClockwise, float approximationScale = DefaultEllipseApproximationScale) {
            if (approximationScale <= 0) {
                throw new ArgumentOutOfRangeException(nameof(approximationScale), approximationScale, "Approximation scale must be greater than zero.");
            }

            var ra = (radiusX + radiusY) / 2;
            var da = MathF.Acos(ra / (ra + 0.125f / approximationScale)) * 2;
            var steps = (uint)Math.Round(MathHelper.TwoPi / da);

            var points = new Vector2[steps + 1];

            for (uint i = 0; i < steps; ++i) {
                var angle = i * MathHelper.TwoPi / steps;

                if (isClockwise) {
                    angle = MathHelper.TwoPi - angle;
                }

                var x = centerX + MathF.Cos(angle) * radiusX;
                var y = centerY + MathF.Cos(angle) * radiusY;
                points[i] = new Vector2(x, y);
            }

            // Close the ellipse.
            points[steps] = points[0];

            return points;
        }

        private const float DefaultEllipseApproximationScale = 1f;

    }
}
