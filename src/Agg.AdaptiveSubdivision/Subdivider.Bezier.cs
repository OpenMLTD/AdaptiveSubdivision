using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Agg.AdaptiveSubdivision;

partial class Subdivider
{

    public static Vector2[] DivideBezier(float fromX, float fromY, float controlX1, float controlY1, float controlX2, float controlY2, float toX, float toY,
        float distanceTolerance = DefaultBezierDistanceTolerance, float angleTolerance = DefaultBezierAngleTolerance, float cuspLimit = DefaultBezierCuspLimit)
    {
        if (distanceTolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(distanceTolerance), distanceTolerance, "Distance tolerance should be greater than zero.");
        }

        if (angleTolerance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(angleTolerance), angleTolerance, "Angle tolerance should be no less than zero.");
        }

        cuspLimit = TranslateCuspLimit(cuspLimit);

        var points = new List<Vector2>(30);
        points.Add(new Vector2(fromX, fromY));
        RecursiveBezier(fromX, fromY, controlX1, controlY1, controlX2, controlY2, toX, toY, 0,
            distanceTolerance * distanceTolerance, angleTolerance * angleTolerance, cuspLimit, points);
        points.Add(new Vector2(toX, toY));

        return points.ToArray();
    }

    private static void RecursiveBezier(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4,
        uint level, float distanceToleranceSquared, float angleTolerance, float cuspLimit, List<Vector2> points)
    {
        if (level > BezierRecursionLimit)
        {
            return;
        }

        var x12 = (x1 + x2) / 2;
        var y12 = (y1 + y2) / 2;
        var x23 = (x2 + x3) / 2;
        var y23 = (y2 + y3) / 2;
        var x34 = (x3 + x4) / 2;
        var y34 = (y3 + y4) / 2;
        var x123 = (x12 + x23) / 2;
        var y123 = (y12 + y23) / 2;
        var x234 = (x23 + x34) / 2;
        var y234 = (y23 + y34) / 2;

        var dx = x4 - x1;
        var dy = y4 - y1;

        var d2 = Math.Abs((x2 - x4) * dy - (y2 - y4) * dx);
        var d3 = Math.Abs((x3 - x4) * dy - (y3 - y4) * dx);

        var sw = ((d2 > BezierCollinearityEpsilon ? 1 : 0) << 1) | (d3 > BezierCollinearityEpsilon ? 1 : 0);
        float da1, da2, k;

        switch (sw)
        {
            case 0:
            {
                k = dx * dx + dy * dy;

                if (k.Equals(0))
                {
                    d2 = CalculateDistanceSquared(x1, y1, x2, y2);
                    d3 = CalculateDistanceSquared(x4, y4, x3, y3);
                }
                else
                {
                    k = 1 / k;
                    da1 = x2 - x1;
                    da2 = y2 - y1;
                    d2 = k * (da1 * dx + da2 * dy);
                    da1 = x3 - x1;
                    da2 = y3 - y1;
                    d3 = k * (da1 * dx + da2 * dy);

                    if (d2 > 0 && d2 < 1 && d3 > 0 && d3 < 1)
                    {
                        return;
                    }

                    if (d2 <= 0)
                    {
                        d2 = CalculateDistanceSquared(x2, y2, x1, y1);
                    }
                    else if (d2 >= 1)
                    {
                        d2 = CalculateDistanceSquared(x2, y2, x4, y4);
                    }
                    else
                    {
                        d2 = CalculateDistanceSquared(x2, y2, x1 + d2 * dx, y1 + d2 * dy);
                    }

                    if (d3 <= 0)
                    {
                        d3 = CalculateDistanceSquared(x3, y3, x1, y1);
                    }
                    else if (d3 >= 1)
                    {
                        d3 = CalculateDistanceSquared(x3, y3, x4, y4);
                    }
                    else
                    {
                        d3 = CalculateDistanceSquared(x3, y3, x1 + d3 * dx, y1 + d3 * dy);
                    }
                }

                if (d2 > d3)
                {
                    if (d2 < distanceToleranceSquared)
                    {
                        points.Add(new Vector2(x2, y2));
                        return;
                    }
                }
                else
                {
                    if (d3 < distanceToleranceSquared)
                    {
                        points.Add(new Vector2(x3, y3));
                        return;
                    }
                }

                break;
            }
            case 1:
            {
                if (d3 * d3 <= distanceToleranceSquared * (dx * dx + dy * dy))
                {
                    if (angleTolerance < BezierAngleToleranceEpsilon)
                    {
                        points.Add(new Vector2(x23, y23));
                        return;
                    }

                    da1 = Math.Abs(MathF.Atan2(y4 - y3, x4 - x3) - MathF.Atan2(y3 - y2, x3 - x2));
                    if (da1 >= MathHelper.Pi)
                    {
                        da1 = MathHelper.TwoPi - da1;
                    }

                    if (da1 < angleTolerance)
                    {
                        points.Add(new Vector2(x2, y2));
                        points.Add(new Vector2(x3, y3));
                        return;
                    }

                    if (!cuspLimit.Equals(0))
                    {
                        if (da1 > cuspLimit)
                        {
                            points.Add(new Vector2(x3, y3));
                            return;
                        }
                    }
                }

                break;
            }
            case 2:
            {
                if (d2 * d2 <= distanceToleranceSquared * (dx * dx + dy * dy))
                {
                    if (angleTolerance < BezierAngleToleranceEpsilon)
                    {
                        points.Add(new Vector2(x23, y23));
                        return;
                    }

                    da1 = Math.Abs(MathF.Atan2(y3 - y2, x3 - x2) - MathF.Atan2(y2 - y1, x2 - x1));

                    if (da1 >= MathHelper.Pi)
                    {
                        da1 = MathHelper.TwoPi - da1;
                    }

                    if (da1 < angleTolerance)
                    {
                        points.Add(new Vector2(x2, y2));
                        points.Add(new Vector2(x3, y3));
                        return;
                    }

                    if (!cuspLimit.Equals(0))
                    {
                        if (da1 > cuspLimit)
                        {
                            points.Add(new Vector2(x2, y2));
                            return;
                        }
                    }
                }

                break;
            }
            case 3:
            {
                if ((d2 + d3) * (d2 + d3) <= distanceToleranceSquared * (dx * dx + dy * dy))
                {
                    if (angleTolerance < BezierAngleToleranceEpsilon)
                    {
                        points.Add(new Vector2(x23, y23));
                        return;
                    }

                    k = MathF.Atan2(y3 - y2, x3 - x2);
                    da1 = Math.Abs(k - MathF.Atan2(y2 - y1, x2 - x1));
                    da2 = Math.Abs(MathF.Atan2(y4 - y3, x4 - x3) - k);

                    if (da1 >= MathHelper.Pi)
                    {
                        da1 = MathHelper.TwoPi - da1;
                    }

                    if (da2 >= MathHelper.Pi)
                    {
                        da2 = MathHelper.TwoPi - da2;
                    }

                    if (da1 + da2 < angleTolerance)
                    {
                        points.Add(new Vector2(x23, y23));
                        return;
                    }

                    if (!cuspLimit.Equals(0))
                    {
                        if (da1 > cuspLimit)
                        {
                            points.Add(new Vector2(x2, y2));
                            return;
                        }

                        if (da2 > cuspLimit)
                        {
                            points.Add(new Vector2(x3, y3));
                            return;
                        }
                    }
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        var x1234 = (x123 + x234) / 2;
        var y1234 = (y123 + y234) / 2;

        RecursiveBezier(x1, y1, x12, y12, x123, y123, x1234, y1234, level + 1,
            distanceToleranceSquared, angleTolerance, cuspLimit, points);
        RecursiveBezier(x1234, y1234, x234, y234, x34, y34, x4, y4, level + 1,
            distanceToleranceSquared, angleTolerance, cuspLimit, points);
    }

}
