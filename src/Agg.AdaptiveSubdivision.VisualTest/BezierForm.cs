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

namespace Agg.AdaptiveSubdivision.VisualTest;

[SupportedOSPlatform("windows")]
[SuppressMessage("ReSharper", "LocalizableElement")]
public partial class BezierForm : SingletonForm
{

    public BezierForm()
    {
        InitializeComponent();
        RegisterEventHandlers();

        _brushFrom = new SolidBrush(Color.Red);
        _brushTo = new SolidBrush(Color.Blue);
        _brushC1 = new SolidBrush(Color.ForestGreen);
        _brushC2 = new SolidBrush(Color.ForestGreen);
        _brushPt = new SolidBrush(Color.Black);

        _penControlLine = new Pen(Color.DarkMagenta, 2);
        _penStandard = new Pen(Color.FromArgb(23, 0, 0, 0), 20);
        _penPoly = new Pen(Color.Orange, 1.5f);
    }

    ~BezierForm()
    {
        UnregisterEventHandlers();
    }

    public static BezierForm? ShowInstance(Form mdiParent)
    {
        return ShowInstance<BezierForm>(mdiParent);
    }

    private void UnregisterEventHandlers()
    {
        pictureBox1.MouseMove -= PictureBox1_MouseMove;
        pictureBox1.Paint -= PictureBox1_Paint;
        Load -= BezierForm_Load;
        trackBar1.ValueChanged -= TrackBar1_ValueChanged;
        trackBar2.ValueChanged -= TrackBar2_ValueChanged;
        pictureBox1.SizeChanged -= PictureBox1_SizeChanged;
        pictureBox1.MouseDown -= PictureBox1_MouseDown;
        pictureBox1.MouseUp -= PictureBox1_MouseUp;
        checkBox1.CheckedChanged -= CheckBox1_CheckedChanged;
        button1.Click -= Button1_Click;
    }

    private void RegisterEventHandlers()
    {
        pictureBox1.MouseMove += PictureBox1_MouseMove;
        pictureBox1.Paint += PictureBox1_Paint;
        Load += BezierForm_Load;
        trackBar1.ValueChanged += TrackBar1_ValueChanged;
        trackBar2.ValueChanged += TrackBar2_ValueChanged;
        pictureBox1.SizeChanged += PictureBox1_SizeChanged;
        pictureBox1.MouseDown += PictureBox1_MouseDown;
        pictureBox1.MouseUp += PictureBox1_MouseUp;
        checkBox1.CheckedChanged += CheckBox1_CheckedChanged;
        button1.Click += Button1_Click;
    }

    private void Button1_Click(object? sender, EventArgs e)
    {
        ref var from = ref _from;
        ref var c1 = ref _c1;
        ref var c2 = ref _c2;
        ref var to = ref _to;

        var rnd = new Random();
        var size = pictureBox1.ClientSize;

        from.X = rnd.Next(size.Width);
        from.Y = rnd.Next(size.Height);
        c1.X = rnd.Next(size.Width);
        c1.Y = rnd.Next(size.Height);
        c2.X = rnd.Next(size.Width);
        c2.Y = rnd.Next(size.Height);
        to.X = rnd.Next(size.Width);
        to.Y = rnd.Next(size.Height);

        pictureBox1.Invalidate();
    }

    private void CheckBox1_CheckedChanged(object? sender, EventArgs e)
    {
        _drawPoints = checkBox1.Checked;
        pictureBox1.Invalidate();
    }

    private void PictureBox1_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        _currentHitLoc = HitLoc.None;
    }

    private void PictureBox1_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        var size = pictureBox1.ClientSize;

        var from = Invert(_from, size);
        var c1 = Invert(_c1, size);
        var c2 = Invert(_c2, size);
        var to = Invert(_to, size);

        var loc = new Vector2(e.X, e.Y);

        HitLoc hit;
        if (Vector2.Distance(from, loc) <= ControlPointRadius)
        {
            hit = HitLoc.From;
        }
        else if (Vector2.Distance(c1, loc) <= ControlPointRadius)
        {
            hit = HitLoc.CP1;
        }
        else if (Vector2.Distance(c2, loc) <= ControlPointRadius)
        {
            hit = HitLoc.CP2;
        }
        else if (Vector2.Distance(to, loc) <= ControlPointRadius)
        {
            hit = HitLoc.To;
        }
        else
        {
            hit = HitLoc.None;
        }

        _currentHitLoc = hit;
    }

    private void PictureBox1_SizeChanged(object? sender, EventArgs e)
    {
        pictureBox1.Invalidate();
    }

    private void TrackBar2_ValueChanged(object? sender, EventArgs e)
    {
        var angle = MathHelper.ToRadians(trackBar2.Value + 1);
        label2.Text = $"Angle tolerance:{Environment.NewLine}{trackBar2.Value + 1} deg/{angle:0.00} rad";
        _angleTolerance = angle;
        pictureBox1.Invalidate();
    }

    private void TrackBar1_ValueChanged(object? sender, EventArgs e)
    {
        var scale = (trackBar1.Value + 1) / 20f;
        var dist = SubdividerHelper.ApproximationScaleToDistanceTolerance(scale);

        _distTolerance = dist;

        label1.Text = $"Approx. scale: {scale:0.00}{Environment.NewLine}Dist. tolerance: {dist:0.00}";

        pictureBox1.Invalidate();
    }

    private void BezierForm_Load(object? sender, EventArgs e)
    {
        _from = new Vector2(10, 10);
        _to = new Vector2(120, 120);
        _c1 = new Vector2(30, 100);
        _c2 = new Vector2(70, 100);

        TrackBar1_ValueChanged(null, EventArgs.Empty);
        TrackBar2_ValueChanged(null, EventArgs.Empty);

        BackColor = SystemColors.Window;

        _isFormLoaded = true;
    }

    private void PictureBox1_Paint(object? sender, PaintEventArgs e)
    {
        if (!_isFormLoaded)
        {
            return;
        }

        e.Graphics.Clear(pictureBox1.BackColor);

        var smoothing = e.Graphics.SmoothingMode;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var size = pictureBox1.ClientSize;

        var from = Invert(_from, size);
        var c1 = Invert(_c1, size);
        var c2 = Invert(_c2, size);
        var to = Invert(_to, size);

        e.Graphics.DrawBezier(_penStandard, V2P(from), V2P(c1), V2P(c2), V2P(to));

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var pts = Subdivider.DivideBezier(from.X, from.Y, c1.X, c1.Y, c2.X, c2.Y, to.X, to.Y, _distTolerance, _angleTolerance);
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

        e.Graphics.DrawLine(_penControlLine, from.X, from.Y, c1.X, c1.Y);
        e.Graphics.DrawLine(_penControlLine, to.X, to.Y, c2.X, c2.Y);

        var elRect = new RectangleF(from.X - ControlPointRadius, from.Y - ControlPointRadius, ControlPointDiameter, ControlPointDiameter);
        e.Graphics.FillEllipse(_brushFrom, elRect);
        elRect = new RectangleF(c1.X - ControlPointRadius, c1.Y - ControlPointRadius, ControlPointDiameter, ControlPointDiameter);
        e.Graphics.FillEllipse(_brushC1, elRect);
        elRect = new RectangleF(c2.X - ControlPointRadius, c2.Y - ControlPointRadius, ControlPointDiameter, ControlPointDiameter);
        e.Graphics.FillEllipse(_brushC2, elRect);
        elRect = new RectangleF(to.X - ControlPointRadius, to.Y - ControlPointRadius, ControlPointDiameter, ControlPointDiameter);
        e.Graphics.FillEllipse(_brushTo, elRect);

        e.Graphics.SmoothingMode = smoothing;

        label3.Text = $"Segments: {pts.Length - 1}{Environment.NewLine}Division time: {stopwatch.Elapsed.TotalMilliseconds} ms";
    }

    private void PictureBox1_MouseMove(object? sender, MouseEventArgs e)
    {
        var hit = _currentHitLoc;

        if (hit == HitLoc.None)
        {
            return;
        }

        var loc = new Vector2(e.X, e.Y);
        var size = pictureBox1.ClientSize;

        switch (hit)
        {
            case HitLoc.From:
                _from = Invert(loc, size);
                break;
            case HitLoc.CP1:
                _c1 = Invert(loc, size);
                break;
            case HitLoc.CP2:
                _c2 = Invert(loc, size);
                break;
            case HitLoc.To:
                _to = Invert(loc, size);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        pictureBox1.Invalidate();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 Invert(Vector2 v, Size s)
    {
        return new Vector2(v.X, s.Height - v.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static PointF V2P(Vector2 v)
    {
        return new PointF(v.X, v.Y);
    }

    private enum HitLoc
    {

        None = 0,
        From = 1,
        CP1 = 2,
        CP2 = 3,
        To = 4

    }

    private HitLoc _currentHitLoc = HitLoc.None;

    private const float ControlPointDiameter = 20;
    private const float ControlPointRadius = ControlPointDiameter / 2;
    private const float RectEdge = 4;

    private bool _isFormLoaded;
    private Vector2 _from, _to, _c1, _c2;
    private readonly SolidBrush _brushFrom, _brushTo, _brushC1, _brushC2, _brushPt;
    private readonly Pen _penControlLine, _penStandard, _penPoly;

    private float _distTolerance;
    private float _angleTolerance;

    private bool _drawPoints = true;

}
