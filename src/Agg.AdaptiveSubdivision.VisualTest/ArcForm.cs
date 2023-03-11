using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Color = System.Drawing.Color;
using Matrix = System.Drawing.Drawing2D.Matrix;

namespace Agg.AdaptiveSubdivision.VisualTest;

[SupportedOSPlatform("windows")]
[SuppressMessage("ReSharper", "LocalizableElement")]
public partial class ArcForm : SingletonForm
{

    public ArcForm()
    {
        InitializeComponent();
        RegisterEventHandlers();

        _brushCP = new SolidBrush(Color.Red);
        _brushPt = new SolidBrush(Color.Black);

        _penStandard = new Pen(Color.FromArgb(23, 0, 0, 0), 20);
        _penPoly = new Pen(Color.Orange, 1.5f);
    }

    ~ArcForm()
    {
        UnregisterEventHandlers();
    }

    public static ArcForm? ShowInstance(Form mdiParent)
    {
        return ShowInstance<ArcForm>(mdiParent);
    }

    private void UnregisterEventHandlers()
    {
        pictureBox1.Paint -= PictureBox1_Paint;
        pictureBox1.MouseDown -= PictureBox1_MouseDown;
        pictureBox1.MouseUp -= PictureBox1_MouseUp;
        pictureBox1.MouseMove -= PictureBox1_MouseMove;
        trackBar1.ValueChanged -= TrackBar1_ValueChanged;
        trackBar2.ValueChanged -= TrackBar2_ValueChanged;
        trackBar3.ValueChanged -= TrackBar3_ValueChanged;
        trackBar4.ValueChanged -= TrackBar4_ValueChanged;
        Load -= ArcForm_Load;
        checkBox1.CheckedChanged -= CheckBox1_CheckedChanged;
        trackBar5.ValueChanged -= TrackBar5_ValueChanged;
        trackBar6.ValueChanged -= TrackBar6_ValueChanged;
        pictureBox1.SizeChanged -= PictureBox1_SizeChanged;
        trackBar7.ValueChanged -= TrackBar7_ValueChanged;
    }

    private void RegisterEventHandlers()
    {
        pictureBox1.Paint += PictureBox1_Paint;
        pictureBox1.MouseDown += PictureBox1_MouseDown;
        pictureBox1.MouseUp += PictureBox1_MouseUp;
        pictureBox1.MouseMove += PictureBox1_MouseMove;
        trackBar1.ValueChanged += TrackBar1_ValueChanged;
        trackBar2.ValueChanged += TrackBar2_ValueChanged;
        trackBar3.ValueChanged += TrackBar3_ValueChanged;
        trackBar4.ValueChanged += TrackBar4_ValueChanged;
        Load += ArcForm_Load;
        checkBox1.CheckedChanged += CheckBox1_CheckedChanged;
        trackBar5.ValueChanged += TrackBar5_ValueChanged;
        trackBar6.ValueChanged += TrackBar6_ValueChanged;
        pictureBox1.SizeChanged += PictureBox1_SizeChanged;
        trackBar7.ValueChanged += TrackBar7_ValueChanged;
    }

    private void TrackBar7_ValueChanged(object? sender, EventArgs e)
    {
        var deg = trackBar7.Value;
        var rad = MathHelper.ToRadians(deg);
        label8.Text = $"Rotation: {deg} deg/{rad:0.##} rad";

        _rotation = rad;

        pictureBox1.Invalidate();
    }

    private void PictureBox1_SizeChanged(object? sender, EventArgs e)
    {
        pictureBox1.Invalidate();
    }

    private void TrackBar6_ValueChanged(object? sender, EventArgs e)
    {
        var angle = MathHelper.ToRadians(trackBar6.Value + 1);
        label6.Text = $"Angle tolerance:{Environment.NewLine}{trackBar6.Value + 1} deg/{angle:0.00} rad";
        _angleTolerance = angle;
        pictureBox1.Invalidate();
    }

    private void TrackBar5_ValueChanged(object? sender, EventArgs e)
    {
        var scale = (trackBar5.Value + 1) / 20f;
        var dist = SubdividerHelper.ApproximationScaleToDistanceTolerance(scale);

        _distTolerance = dist;

        label5.Text = $"Approx. scale: {scale:0.00}{Environment.NewLine}Dist. tolerance: {dist:0.00}";

        pictureBox1.Invalidate();
    }

    private void CheckBox1_CheckedChanged(object? sender, EventArgs e)
    {
        _drawPoints = checkBox1.Checked;
        pictureBox1.Invalidate();
    }

    private void ArcForm_Load(object? sender, EventArgs e)
    {
        TrackBar1_ValueChanged(null, EventArgs.Empty);
        TrackBar2_ValueChanged(null, EventArgs.Empty);
        TrackBar3_ValueChanged(null, EventArgs.Empty);
        TrackBar4_ValueChanged(null, EventArgs.Empty);
        CheckBox1_CheckedChanged(null, EventArgs.Empty);
        TrackBar5_ValueChanged(null, EventArgs.Empty);
        TrackBar6_ValueChanged(null, EventArgs.Empty);
        TrackBar7_ValueChanged(null, EventArgs.Empty);

        _center = new Vector2(300, 300);

        BackColor = SystemColors.Window;

        _isFormLoaded = true;
    }

    private void TrackBar4_ValueChanged(object? sender, EventArgs e)
    {
        label4.Text = $"Radius Y: {trackBar4.Value}";
        _radius.Y = trackBar4.Value;

        pictureBox1.Invalidate();
    }

    private void TrackBar3_ValueChanged(object? sender, EventArgs e)
    {
        label3.Text = $"Radius X: {trackBar3.Value}";
        _radius.X = trackBar3.Value;

        pictureBox1.Invalidate();
    }

    private void TrackBar2_ValueChanged(object? sender, EventArgs e)
    {
        var deg = (float)trackBar2.Value;
        var rad = MathHelper.ToRadians(deg);
        label2.Text = $"Sweep: {deg} deg/ {rad:0.##} rad";
        _sweepAngle = rad;

        pictureBox1.Invalidate();
    }

    private void TrackBar1_ValueChanged(object? sender, EventArgs e)
    {
        var deg = (float)trackBar1.Value;
        var rad = MathHelper.ToRadians(deg);
        label1.Text = $"Start: {deg} deg/ {rad:0.##} rad";
        _startAngle = rad;

        pictureBox1.Invalidate();
    }

    private void PictureBox1_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_hitLoc == HitLoc.None)
        {
            return;
        }

        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        var loc = new Vector2(e.X, e.Y);
        loc = Invert(loc, pictureBox1.ClientSize);

        _center = loc;

        pictureBox1.Invalidate();
    }

    private void PictureBox1_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _hitLoc = HitLoc.None;
        }
    }

    private void PictureBox1_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        var size = pictureBox1.ClientSize;
        var center = Invert(_center, size);
        var isHit = Vector2.Distance(new Vector2(e.X, e.Y), center) <= ControlPointRadius;

        _hitLoc = isHit ? HitLoc.CP : HitLoc.None;
    }

    private void PictureBox1_Paint(object? sender, PaintEventArgs e)
    {
        if (!_isFormLoaded)
        {
            return;
        }

        var size = pictureBox1.ClientSize;

        e.Graphics.Clear(pictureBox1.BackColor);

        var smoothingMode = e.Graphics.SmoothingMode;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var center = Invert(_center, size);

        var rect = new RectangleF(center.X - ControlPointRadius, center.Y - ControlPointRadius, 2 * ControlPointRadius, 2 * ControlPointRadius);
        e.Graphics.FillEllipse(_brushCP, rect);

        var mat = new Matrix();
        mat.RotateAt(MathHelper.ToDegrees(_rotation), new PointF(center.X, center.Y));
        e.Graphics.Transform = mat;
        e.Graphics.DrawArc(_penStandard, center.X - _radius.X, center.Y - _radius.Y, _radius.X * 2, _radius.Y * 2, MathHelper.ToDegrees(_startAngle), MathHelper.ToDegrees(_sweepAngle));
        e.Graphics.ResetTransform();

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var pts = Subdivider.DivideArc(center.X, center.Y, _radius.X, _radius.Y, _startAngle, _sweepAngle, _rotation, _distTolerance, _angleTolerance);
        stopwatch.Stop();

        for (var i = 0; i < pts.Length - 1; ++i)
        {
            e.Graphics.DrawLine(_penPoly, pts[i].X, pts[i].Y, pts[i + 1].X, pts[i + 1].Y);
        }

        if (_drawPoints)
        {
            for (var i = 0; i < pts.Length; ++i)
            {
                var pt = pts[i];
                e.Graphics.FillRectangle(_brushPt, new RectangleF(pt.X - RectEdge / 2, pt.Y - RectEdge / 2, RectEdge, RectEdge));
            }
        }

        e.Graphics.SmoothingMode = smoothingMode;

        label7.Text = $"Segments: {pts.Length - 1}{Environment.NewLine}Division time: {stopwatch.Elapsed.TotalMilliseconds} ms";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 Invert(Vector2 v, Size s)
    {
        return new Vector2(v.X, s.Height - v.Y);
    }

    private enum HitLoc
    {

        None = 0,
        CP = 1

    }

    private HitLoc _hitLoc = HitLoc.None;

    private bool _isFormLoaded;

    private const float ControlPointRadius = 10;
    private const float RectEdge = 4;

    private readonly SolidBrush _brushCP, _brushPt;
    private readonly Pen _penStandard, _penPoly;

    private bool _drawPoints;

    private Vector2 _center;
    private float _startAngle;
    private float _sweepAngle;
    private Vector2 _radius;
    private float _rotation;

    private float _distTolerance;
    private float _angleTolerance;

}
