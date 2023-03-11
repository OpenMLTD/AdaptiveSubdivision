using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Agg.AdaptiveSubdivision;

// https://www.spaceroots.org/documents/ellipse/elliptical-arc.pdf
// https://www.spaceroots.org/documents/ellipse/EllipticalArc.java
internal sealed class EllipticalArc
{

    internal EllipticalArc(float cx, float cy, float a, float b, float theta, float lambda1, float lambda2, float flatness = DefaultFlatness)
    {
        _cx = cx;
        _cy = cy;
        _a = a;
        _b = b;

        _eta1 = MathF.Atan2(MathF.Sin(lambda1) / b, MathF.Cos(lambda1) / a);
        _eta2 = MathF.Atan2(MathF.Sin(lambda2) / b, MathF.Cos(lambda2) / a);
        _cosTheta = MathF.Cos(theta);
        _sinTheta = MathF.Sin(theta);
        _flatness = flatness;

        _eta2 -= MathHelper.TwoPi * MathF.Floor((_eta2 - _eta1) / MathHelper.TwoPi);

        if (lambda2 - lambda1 > MathHelper.Pi && _eta2 - _eta1 < MathHelper.Pi)
        {
            _eta2 += 2 * MathHelper.Pi;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Vector2[] Divide(ArcApproximator approximator, Matrix3x2? at, float distanceTolerance, float angleTolerance, float cuspLimit)
    {
        return Divide(approximator, at, distanceTolerance, angleTolerance, cuspLimit, _flatness);
    }

    private Vector2[] Divide(ArcApproximator approximator, Matrix3x2? at, float distanceTolerance, float angleTolerance, float cuspLimit, float threshold)
    {
        float dEta, etaB;

        var maxBezierCurvesNeededFound = false;
        var n = 1;

        while (!maxBezierCurvesNeededFound && n < 1024)
        {
            dEta = (_eta2 - _eta1) / n;
            if (dEta <= 0.5 * Math.PI)
            {
                etaB = _eta1;
                maxBezierCurvesNeededFound = true;
                for (var i = 0; maxBezierCurvesNeededFound && (i < n); ++i)
                {
                    var etaA = etaB;
                    etaB += dEta;
                    maxBezierCurvesNeededFound = EstimateError(approximator, etaA, etaB) <= threshold;
                }
            }

            n = n << 1;
        }

        dEta = (_eta2 - _eta1) / n;
        etaB = _eta1;

        var cosEtaB = MathF.Cos(etaB);
        var sinEtaB = MathF.Sin(etaB);
        var aCosEtaB = _a * cosEtaB;
        var bSinEtaB = _b * sinEtaB;
        var aSinEtaB = _a * sinEtaB;
        var bCosEtaB = _b * cosEtaB;
        var xB = _cx + aCosEtaB * _cosTheta - bSinEtaB * _sinTheta;
        var yB = _cy + aCosEtaB * _sinTheta + bSinEtaB * _cosTheta;
        var xBDot = -aSinEtaB * _cosTheta - bCosEtaB * _sinTheta;
        var yBDot = -aSinEtaB * _sinTheta + bCosEtaB * _cosTheta;

        var result = new List<Vector2>(30);
        var currentPoint = new Vector2(xB, yB);

        var t = MathF.Tan(0.5f * dEta);
        var alpha = MathF.Sin(dEta) * (MathF.Sqrt(4 + 3 * t * t) - 1) / 3;

        for (var i = 0; i < n; ++i)
        {
            var xA = xB;
            var yA = yB;
            var xADot = xBDot;
            var yADot = yBDot;

            etaB += dEta;
            cosEtaB = MathF.Cos(etaB);
            sinEtaB = MathF.Sin(etaB);
            aCosEtaB = _a * cosEtaB;
            bSinEtaB = _b * sinEtaB;
            aSinEtaB = _a * sinEtaB;
            bCosEtaB = _b * cosEtaB;
            xB = _cx + aCosEtaB * _cosTheta - bSinEtaB * _sinTheta;
            yB = _cy + aCosEtaB * _sinTheta + bSinEtaB * _cosTheta;
            xBDot = -aSinEtaB * _cosTheta - bCosEtaB * _sinTheta;
            yBDot = -aSinEtaB * _sinTheta + bCosEtaB * _cosTheta;

            switch (approximator)
            {
                case ArcApproximator.Bezier:
                {
                    var cp1 = new Vector2(xA + alpha * xADot, yA + alpha * yADot);
                    var cp2 = new Vector2(xB - alpha * xBDot, yB - alpha * yBDot);
                    var p2 = new Vector2(xB, yB);

                    if (at != null)
                    {
                        currentPoint = Matrix3x2.Transform(at.Value, currentPoint);
                        cp1 = Matrix3x2.Transform(at.Value, cp1);
                        cp2 = Matrix3x2.Transform(at.Value, cp2);
                        p2 = Matrix3x2.Transform(at.Value, p2);
                    }

                    var bezierSegments = Subdivider.DivideBezier(currentPoint.X, currentPoint.Y, cp1.X, cp1.Y, cp2.X, cp2.Y, p2.X, p2.Y,
                        distanceTolerance, angleTolerance, cuspLimit);

                    result.AddRange(bezierSegments);

                    break;
                }
                case ArcApproximator.QuadraticBezier:
                {
                    var k = (yBDot * (xB - xA) - xBDot * (yB - yA)) / (xADot * yBDot - yADot * xBDot);

                    var cp = new Vector2(xA + k * xADot, yA + k * yADot);
                    var p2 = new Vector2(xB, yB);

                    if (at != null)
                    {
                        currentPoint = Matrix3x2.Transform(at.Value, currentPoint);
                        cp = Matrix3x2.Transform(at.Value, cp);
                        p2 = Matrix3x2.Transform(at.Value, p2);
                    }

                    var bezierSegments = Subdivider.DivideQuadraticBezier(currentPoint.X, currentPoint.Y, cp.X, cp.Y, p2.X, p2.Y,
                        distanceTolerance, angleTolerance);

                    result.AddRange(bezierSegments);

                    break;
                }
                case ArcApproximator.Line:
                {
                    var p2 = new Vector2(xB, yB);

                    if (at != null)
                    {
                        p2 = Matrix3x2.Transform(at.Value, p2);
                    }

                    result.Add(p2);

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(approximator), approximator, null);
            }

            currentPoint = new Vector2(xB, yB);
        }

        return result.ToArray();
    }

    private float EstimateError(ArcApproximator approximator, float etaA, float etaB)
    {
        var eta = 0.5f * (etaA + etaB);

        switch (approximator)
        {
            case ArcApproximator.Bezier:
            case ArcApproximator.QuadraticBezier:
            {
                var x = _b / _a;
                var dEta = etaB - etaA;
                var cos2 = MathF.Cos(2 * eta);
                var cos4 = MathF.Cos(4 * eta);
                var cos6 = MathF.Cos(6 * eta);

                float[][][] coeffs;
                float[] safety;

                if (approximator == ArcApproximator.QuadraticBezier)
                {
                    coeffs = x < 0.25f ? Coeffs2Low : Coeffs2High;
                    safety = Safety2;
                }
                else
                {
                    coeffs = x < 0.25f ? Coeffs3Low : Coeffs3High;
                    safety = Safety3;
                }

                var c0 = RationalFunction(x, coeffs[0][0])
                         + cos2 * RationalFunction(x, coeffs[0][1])
                         + cos4 * RationalFunction(x, coeffs[0][2])
                         + cos6 * RationalFunction(x, coeffs[0][3]);

                var c1 = RationalFunction(x, coeffs[1][0])
                         + cos2 * RationalFunction(x, coeffs[1][1])
                         + cos4 * RationalFunction(x, coeffs[1][2])
                         + cos6 * RationalFunction(x, coeffs[1][3]);

                return RationalFunction(x, safety) * _a * MathF.Exp(c0 + c1 * dEta);
            }
            case ArcApproximator.Line:
            {
                // start point
                var aCosEtaA = _a * MathF.Cos(etaA);
                var bSinEtaA = _b * MathF.Sin(etaA);
                var xA = _cx + aCosEtaA * _cosTheta - bSinEtaA * _sinTheta;
                var yA = _cy + aCosEtaA * _sinTheta + bSinEtaA * _cosTheta;

                var aCosEtaB = _a * MathF.Cos(etaB);
                var bSinEtaB = _b * MathF.Sin(etaB);
                var xB = _cx + aCosEtaB * _cosTheta - bSinEtaB * _sinTheta;
                var yB = _cy + aCosEtaB * _sinTheta + bSinEtaB * _cosTheta;

                var aCosEta = _a * MathF.Cos(eta);
                var bSinEta = _b * MathF.Sin(eta);
                var x = _cx + aCosEta * _cosTheta - bSinEta * _sinTheta;
                var y = _cy + aCosEta * _sinTheta + bSinEta * _cosTheta;

                var dx = xB - xA;
                var dy = yB - yA;

                return Math.Abs(x * dy - y * dx + xB * yA - xA * yB) / MathF.Sqrt(dx * dx + dy * dy);
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(approximator), approximator, null);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float RationalFunction(float x, float[] c)
    {
        return (x * (x * c[0] + c[1]) + c[2]) / (x + c[3]);
    }

    private static readonly float[][][] Coeffs2Low =
    {
        new[]
        {
            new[] { 3.92478f, -13.5822f, -0.233377f, 0.0128206f },
            new[] { -1.08814f, 0.859987f, 0.000362265f, 0.000229036f },
            new[] { -0.942512f, 0.390456f, 0.0080909f, 0.00723895f },
            new[] { -0.736228f, 0.20998f, 0.0129867f, 0.0103456f }
        },
        new[]
        {
            new[] { -0.395018f, 6.82464f, 0.0995293f, 0.0122198f },
            new[] { -0.545608f, 0.0774863f, 0.0267327f, 0.0132482f },
            new[] { 0.0534754f, -0.0884167f, 0.012595f, 0.0343396f },
            new[] { 0.209052f, -0.0599987f, -0.00723897f, 0.00789976f }
        }
    };

    private static readonly float[][][] Coeffs2High =
    {
        new[]
        {
            new[] { 0.0863805f, -11.5595f, -2.68765f, 0.181224f },
            new[] { 0.242856f, -1.81073f, 1.56876f, 1.68544f },
            new[] { 0.233337f, -0.455621f, 0.222856f, 0.403469f },
            new[] { 0.0612978f, -0.104879f, 0.0446799f, 0.00867312f }
        },
        new[]
        {
            new[] { 0.028973f, 6.68407f, 0.171472f, 0.0211706f },
            new[] { 0.0307674f, -0.0517815f, 0.0216803f, -0.0749348f },
            new[] { -0.0471179f, 0.1288f, -0.0781702f, 2.0f },
            new[] { -0.0309683f, 0.0531557f, -0.0227191f, 0.0434511f }
        }
    };

    private static readonly float[] Safety2 =
    {
        0.02f, 2.83f, 0.125f, 0.01f
    };

    private static readonly float[][][] Coeffs3Low =
    {
        new[]
        {
            new[] { 3.85268f, -21.229f, -0.330434f, 0.0127842f },
            new[] { -1.61486f, 0.706564f, 0.225945f, 0.263682f },
            new[] { -0.910164f, 0.388383f, 0.00551445f, 0.00671814f },
            new[] { -0.630184f, 0.192402f, 0.0098871f, 0.0102527f }
        },
        new[]
        {
            new[] { -0.162211f, 9.94329f, 0.13723f, 0.0124084f },
            new[] { -0.253135f, 0.00187735f, 0.0230286f, 0.01264f },
            new[] { -0.0695069f, -0.0437594f, 0.0120636f, 0.0163087f },
            new[] { -0.0328856f, -0.00926032f, -0.00173573f, 0.00527385f }
        }
    };

    private static readonly float[][][] Coeffs3High =
    {
        new[]
        {
            new[] { 0.0899116f, -19.2349f, -4.11711f, 0.183362f },
            new[] { 0.138148f, -1.45804f, 1.32044f, 1.38474f },
            new[] { 0.230903f, -0.450262f, 0.219963f, 0.414038f },
            new[] { 0.0590565f, -0.101062f, 0.0430592f, 0.0204699f }
        },
        new[]
        {
            new[] { 0.0164649f, 9.89394f, 0.0919496f, 0.00760802f },
            new[] { 0.0191603f, -0.0322058f, 0.0134667f, -0.0825018f },
            new[] { 0.0156192f, -0.017535f, 0.00326508f, -0.228157f },
            new[] { -0.0236752f, 0.0405821f, -0.0173086f, 0.176187f }
        }
    };

    private static readonly float[] Safety3 =
    {
        0.001f, 4.98f, 0.207f, 0.0067f
    };

    private readonly float _cx;
    private readonly float _cy;
    private readonly float _a;
    private readonly float _b;

    private readonly float _cosTheta;
    private readonly float _sinTheta;

    private readonly float _eta1;

    private readonly float _eta2;

    private readonly float _flatness;

    private const float DefaultFlatness = 0.5f;

}
