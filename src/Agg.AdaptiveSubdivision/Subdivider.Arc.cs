using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Agg.AdaptiveSubdivision;

partial class Subdivider
{

    public static Vector2[] DivideArc(float centerX, float centerY, float radiusX, float radiusY, float startAngle, float sweepAngle, float rotation = DefaultArcRotation,
        float distanceTolerance = DefaultBezierDistanceTolerance, float angleTolerance = DefaultBezierAngleTolerance, float cuspLimit = DefaultBezierCuspLimit)
    {
        if (radiusX.Equals(radiusY))
        {
            var bezier = GetCircularArcBezierPoints(centerX, centerY, radiusX, radiusY, startAngle, sweepAngle, rotation);

            if (bezier.Length <= 2)
            {
                return bezier;
            }

            var points = new List<Vector2>(30);
            var currentPoint = bezier[0];

            for (var i = 1; i < bezier.Length; i += 3)
            {
                var segPoints = DivideBezier(currentPoint.X, currentPoint.Y, bezier[i].X, bezier[i].Y, bezier[i + 1].X, bezier[i + 1].Y, bezier[i + 2].X, bezier[i + 2].Y,
                    distanceTolerance, angleTolerance, cuspLimit);

                points.AddRange(segPoints);
                currentPoint = bezier[i + 2];
            }

            return points.ToArray();
        }
        else
        {
            var arc = new EllipticalArc(centerX, centerY, radiusX, radiusY, rotation, startAngle, startAngle + sweepAngle);
            var points = arc.Divide(ArcApproximator.Bezier, null, distanceTolerance, angleTolerance, cuspLimit);

            return points;
        }
    }

    private static unsafe Vector2[] GetCircularArcBezierPoints(float centerX, float centerY, float radiusX, float radiusY, float startAngle, float sweepAngle, float rotation)
    {
        // Rotation can be achieved by simply increasing the starting angle, when the curve is circular.
        startAngle += rotation;

        startAngle = startAngle % MathHelper.TwoPi;

        if (sweepAngle > MathHelper.TwoPi)
        {
            sweepAngle = MathHelper.TwoPi;
        }
        else if (sweepAngle < -MathHelper.TwoPi)
        {
            sweepAngle = -MathHelper.TwoPi;
        }

        Vector2[] ret;

        if (Math.Abs(sweepAngle) < ArcEpsilon)
        {
            ret = new Vector2[2];
            ret[0] = new Vector2(centerX + radiusX * MathF.Cos(startAngle), centerY + radiusY * MathF.Sin(startAngle));
            ret[1] = new Vector2(centerX + radiusX * MathF.Cos(startAngle + sweepAngle), centerY + radiusY * MathF.Sin(startAngle + sweepAngle));

            return ret;
        }

        var vertices = stackalloc Vector2[ArcMaxVertices];
        float totalSweep = 0;
        var numVertices = 1;
        var done = false;

        do
        {
            float localSweep, prevSweep;

            if (sweepAngle < 0)
            {
                prevSweep = totalSweep;
                localSweep = -MathHelper.PiOver2;
                totalSweep -= MathHelper.PiOver2;

                if (totalSweep <= sweepAngle + BezierToArcAngleEpsilon)
                {
                    localSweep = sweepAngle - prevSweep;
                    done = true;
                }
            }
            else
            {
                prevSweep = totalSweep;
                localSweep = MathHelper.PiOver2;
                totalSweep += MathHelper.PiOver2;

                if (totalSweep >= sweepAngle - BezierToArcAngleEpsilon)
                {
                    localSweep = sweepAngle - prevSweep;
                    done = true;
                }
            }

            ArcHelper.ArcToBezier(centerX, centerY, radiusX, radiusY, startAngle, localSweep, vertices + numVertices - 1);
            numVertices += 3;
            startAngle += localSweep;
        }
        while (!done && numVertices < ArcMaxVertices);

        ret = new Vector2[numVertices];

        for (var i = 0; i < numVertices; ++i)
        {
            ret[i] = vertices[i];
        }

        return ret;
    }

    private const float DefaultArcRotation = 0;
    private const float ArcEpsilon = 1e-10f;
    private const float BezierToArcAngleEpsilon = 0.01f;
    private const int ArcMaxVertices = 13;

}
