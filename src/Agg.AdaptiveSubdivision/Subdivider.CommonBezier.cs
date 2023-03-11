using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Agg.AdaptiveSubdivision;

partial class Subdivider
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float CalculateDistanceSquared(float x1, float y1, float x2, float y2)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        return dx * dx + dy * dy;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float TranslateCuspLimit(float v)
    {
        return v.Equals(0) ? 0 : MathHelper.Pi - v;
    }

    private const float DefaultBezierApproximationScale = 1.0f;
    private const float DefaultBezierDistanceTolerance = 0.5f;
    private const float DefaultBezierAngleTolerance = 15f * MathHelper.PiOver2 / 180;
    private const float DefaultBezierCuspLimit = 0f;
    private const float BezierCollinearityEpsilon = 1e-30f;
    private const float BezierAngleToleranceEpsilon = 0.01f;
    private const uint BezierRecursionLimit = 32;

}
