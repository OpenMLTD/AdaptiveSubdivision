﻿using System;
using Microsoft.Xna.Framework;

namespace Agg.AdaptiveSubdivision {
    internal static class BezierHelper {

        internal static unsafe void ArcToBezier(float x, float y, float rx, float ry, float startAngle, float sweepAngle, Vector2* buffer) {
            if (buffer == null) {
                throw new ArgumentNullException(nameof(buffer));
            }

            var halfSweep = sweepAngle / 2;

            var x0 = MathF.Cos(halfSweep);
            var y0 = MathF.Sin(halfSweep);
            var tx = (1 - x0) * 4 / 3;
            var ty = y0 - tx * x0 / y0;

            var px = stackalloc float[4];
            var py = stackalloc float[4];

            px[0] = x0;
            py[0] = -y0;
            px[1] = x0 + tx;
            py[1] = -ty;
            px[2] = x0 + tx;
            py[2] = ty;
            px[3] = x0;
            py[3] = y0;

            var s = MathF.Sin(startAngle + halfSweep);
            var c = MathF.Cos(startAngle + halfSweep);

            for (var i = 0; i < 4; ++i) {
                var xt = x + rx * (px[i] * c - py[i] * s);
                var yt = y + ry * (px[i] * s + py[i] * c);
                buffer[i] = new Vector2(xt, yt);
            }
        }

    }
}
