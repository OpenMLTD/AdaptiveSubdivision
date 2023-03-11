using System;
using Microsoft.Xna.Framework;

namespace Agg.AdaptiveSubdivision;

partial class Subdivider
{

    public static Vector2[] DivideSvgArc(float fromX, float fromY, float toX, float toY, float radiusX, float radiusY, float angle,
        bool isLarge, bool isClockwise,
        float distanceTolerance = DefaultBezierDistanceTolerance, float angleTolerance = DefaultBezierAngleTolerance, float cuspLimit = DefaultBezierCuspLimit)
    {
        return DivideSvgArc(fromX, fromY, toX, toY, radiusX, radiusY, angle, isLarge, isClockwise, out var _,
            distanceTolerance, angleTolerance, cuspLimit);
    }

    public static Vector2[] DivideSvgArc(float fromX, float fromY, float toX, float toY, float radiusX, float radiusY, float angle,
        bool isLarge, bool isClockwise, out bool radiiOk,
        float distanceTolerance = DefaultBezierDistanceTolerance, float angleTolerance = DefaultBezierAngleTolerance, float cuspLimit = DefaultBezierCuspLimit)
    {
        radiiOk = true;

        radiusX = Math.Abs(radiusX);
        radiusY = Math.Abs(radiusY);

        var dx2 = (fromX - toX) / 2;
        var dy2 = (fromY - toY) / 2;
        var cosA = MathF.Cos(angle);
        var sinA = MathF.Sin(angle);

        var x1 = cosA * dx2 + sinA * dy2;
        var y1 = -sinA * dx2 + cosA * dy2;

        var prx = radiusX * radiusX;
        var pry = radiusY * radiusY;
        var px1 = x1 * x1;
        var py1 = y1 * y1;

        var radiiCheck = px1 / prx + py1 / pry;

        if (radiiCheck > 1)
        {
            var root = MathF.Sqrt(radiiCheck);
            radiusX = root * radiusX;
            radiusY = root * radiusY;
            prx = radiusX * radiusX;
            pry = radiusY * radiusY;

            if (radiiCheck > 10)
            {
                radiiOk = false;
            }
        }

        float sign = isLarge == isClockwise ? -1 : 1;
        var sq = (prx * pry - prx * py1 - pry * px1) / (prx * py1 + pry * px1);
        var coef = sign * (sq < 0 ? 0 : MathF.Sqrt(sq));
        var cx1 = coef * ((radiusX * y1) / radiusY);
        var cy1 = coef * -((radiusY * x1) / radiusX);

        var sx2 = (fromX + toX) / 2;
        var sy2 = (fromY + toY) / 2;
        var cx = sx2 + (cosA * cx1 - sinA * cy1);
        var cy = sy2 + (sinA * cx1 + cosA * cy1);

        var ux = (x1 - cx1) / radiusX;
        var uy = (y1 - cy1) / radiusY;
        var vx = (-x1 - cx1) / radiusX;
        var vy = (-y1 - cy1) / radiusY;

        var n = MathF.Sqrt(ux * ux + uy * uy);
        var p = ux; // = (1 * ux) + (0 * uy)

        sign = uy < 0 ? -1 : 1;
        var v = p / n;
        v = MathHelper.Clamp(v, -1, 1);
        var startAngle = sign * MathF.Acos(v);

        n = MathF.Sqrt((ux * ux + uy * uy) * (vx * vx + vy * vy));
        p = ux * vx + uy * vy;
        sign = (ux * vy - uy * vx) < 0 ? -1 : 1;
        v = p / n;
        v = MathHelper.Clamp(v, -1, 1);

        var sweepAngle = sign * MathF.Acos(v);

        if (!isClockwise && sweepAngle > 0)
        {
            sweepAngle -= MathHelper.TwoPi;
        }
        else if (isClockwise && sweepAngle < 0)
        {
            sweepAngle += MathHelper.TwoPi;
        }

        var arcVertices = DivideArc(cx, cy, radiusX, radiusY, startAngle, sweepAngle, angle,
            distanceTolerance, angleTolerance, cuspLimit);

        return arcVertices;
    }

}
