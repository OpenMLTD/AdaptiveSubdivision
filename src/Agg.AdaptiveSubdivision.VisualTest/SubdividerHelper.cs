using System;

namespace Agg.AdaptiveSubdivision.VisualTest;

internal static class SubdividerHelper
{

    internal static float DistanceToleranceToApproximationScale(float tolerance)
    {
        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), tolerance, "Distance tolerance should be greater than zero.");
        }

        return 0.5f / tolerance;
    }

    internal static float ApproximationScaleToDistanceTolerance(float scale)
    {
        if (scale <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(scale), scale, "Approximation scale should be greater than zero.");
        }

        return 0.5f / scale;
    }

}
