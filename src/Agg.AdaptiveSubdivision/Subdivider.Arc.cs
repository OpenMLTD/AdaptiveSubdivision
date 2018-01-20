using System;
using Microsoft.Xna.Framework;

namespace Agg.AdaptiveSubdivision {
    partial class Subdivider {

        public static unsafe Vector2[] DivideArc(float centerX, float centerY, float radiusX, float radiusY,
            float startAngle, float sweepAngle) {
            startAngle = startAngle % MathHelper.TwoPi;

            if (sweepAngle > MathHelper.TwoPi) {
                sweepAngle = MathHelper.TwoPi;
            } else if (sweepAngle < -MathHelper.TwoPi) {
                sweepAngle = -MathHelper.TwoPi;
            }

            Vector2[] ret;

            if (Math.Abs(sweepAngle) < ArcEpsilon) {
                ret = new Vector2[2];
                ret[0] = new Vector2(centerX + radiusX * MathF.Cos(startAngle), centerY + radiusY * MathF.Sin(startAngle));
                ret[1] = new Vector2(centerX + radiusX * MathF.Cos(startAngle + sweepAngle), centerY + radiusY * MathF.Sin(startAngle + sweepAngle));

                return ret;
            }

            var vertices = stackalloc Vector2[ArcMaxVertices];
            float totalSweep = 0;
            var numVertices = 1;
            var done = false;

            do {
                float localSweep, prevSweep;

                if (sweepAngle < 0) {
                    prevSweep = totalSweep;
                    localSweep = -MathHelper.PiOver2;
                    totalSweep -= MathHelper.PiOver2;

                    if (totalSweep <= sweepAngle + BezierToArcAngleEpsilon) {
                        localSweep = sweepAngle - prevSweep;
                        done = true;
                    }
                } else {
                    prevSweep = totalSweep;
                    localSweep = MathHelper.PiOver2;
                    totalSweep += MathHelper.PiOver2;

                    if (totalSweep >= sweepAngle - BezierToArcAngleEpsilon) {
                        localSweep = sweepAngle - prevSweep;
                        done = true;
                    }
                }

                BezierHelper.ArcToBezier(centerX, centerY, radiusX, radiusY, startAngle, localSweep, vertices + numVertices - 1);
                numVertices += 3;
                startAngle += localSweep;
            } while (!done && numVertices < ArcMaxVertices);

            ret = new Vector2[numVertices];

            for (var i = 0; i < numVertices; ++i) {
                ret[i] = vertices[i];
            }

            return ret;
        }

        private const float ArcEpsilon = 1e-10f;
        private const float BezierToArcAngleEpsilon = 0.01f;
        private const int ArcMaxVertices = 13;

    }
}
