using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Agg.AdaptiveSubdivision;

partial class Subdivider
{

    public static Vector2[] DivideQuadraticBezier(float fromX, float fromY, float controlX, float controlY, float toX, float toY,
        float distanceTolerance = DefaultBezierDistanceTolerance, float angleTolerance = DefaultBezierAngleTolerance)
    {
        if (distanceTolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(distanceTolerance), distanceTolerance, "Distance tolerance should be greater than zero.");
        }

        if (angleTolerance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(angleTolerance), angleTolerance, "Angle tolerance should be no less than zero.");
        }

        var points = new List<Vector2>(30);
        points.Add(new Vector2(fromX, fromY));
        RecursiveQuadraticBezier(fromX, fromY, controlX, controlY, toX, toY, 0,
            distanceTolerance * distanceTolerance, angleTolerance * angleTolerance, points);
        points.Add(new Vector2(toX, toY));

        return points.ToArray();
    }

    private static void RecursiveQuadraticBezier(float x1, float y1, float x2, float y2, float x3, float y3,
        uint level, float distanceToleranceSquared, float angleTolerance, List<Vector2> points)
    {
        if (level > BezierRecursionLimit)
        {
            return;
        }

        var x12 = (x1 + x2) / 2;
        var y12 = (y1 + y2) / 2;
        var x23 = (x2 + x3) / 2;
        var y23 = (y2 + y3) / 2;
        var x123 = (x12 + x23) / 2;
        var y123 = (y12 + y23) / 2;

        var dx = x3 - x1;
        var dy = y3 - y1;
        var d = Math.Abs((x2 - x3) * dy - (y2 - y3) * dx);
        float da;

        if (d > BezierCollinearityEpsilon)
        {
            if (d * d <= distanceToleranceSquared * (dx * dx + dy * dy))
            {
                if (angleTolerance < BezierAngleToleranceEpsilon)
                {
                    points.Add(new Vector2(x123, y123));
                    return;
                }

                da = Math.Abs(MathF.Atan2(y3 - y2, x3 - x2) - MathF.Atan2(y2 - y1, x2 - x1));

                if (da >= MathHelper.Pi)
                {
                    da = MathHelper.TwoPi - da;
                }

                if (da < angleTolerance)
                {
                    points.Add(new Vector2(x123, y123));
                    return;
                }
            }
        }
        else
        {
            da = dx * dx + dy * dy;
            if (da.Equals(0))
            {
                d = CalculateDistanceSquared(x1, y1, x2, y2);
            }
            else
            {
                d = ((x2 - x1) * dx + (y2 - y1) * dy) / da;
                if (d > 0 && d < 1)
                {
                    return;
                }

                if (d <= 0)
                {
                    d = CalculateDistanceSquared(x2, y2, x1, y1);
                }
                else if (d >= 1)
                {
                    d = CalculateDistanceSquared(x2, y2, x3, y3);
                }
                else
                {
                    d = CalculateDistanceSquared(x2, y2, x1 + d * dx, y1 + d * dy);
                }
            }

            if (d < distanceToleranceSquared)
            {
                points.Add(new Vector2(x2, y2));
                return;
            }
        }

        RecursiveQuadraticBezier(x1, y1, x12, y12, x123, y123, level + 1,
            distanceToleranceSquared, angleTolerance, points);
        RecursiveQuadraticBezier(x123, y123, x23, y23, x3, y3, level + 1,
            distanceToleranceSquared, angleTolerance, points);
    }

}
