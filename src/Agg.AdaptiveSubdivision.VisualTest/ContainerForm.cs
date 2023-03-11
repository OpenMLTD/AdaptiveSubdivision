using System;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace Agg.AdaptiveSubdivision.VisualTest;

[SupportedOSPlatform("windows")]
public partial class ContainerForm : Form
{

    public ContainerForm()
    {
        InitializeComponent();
        RegisterEventHandlers();
    }

    ~ContainerForm()
    {
        UnregisterEventHandlers();
    }

    private void UnregisterEventHandlers()
    {
        mnuDemosArc.Click -= MnuDemosArc_Click;
        mnuDemosBezier.Click -= MnuDemosBezier_Click;
    }

    private void RegisterEventHandlers()
    {
        mnuDemosArc.Click += MnuDemosArc_Click;
        mnuDemosBezier.Click += MnuDemosBezier_Click;
    }

    private void MnuDemosBezier_Click(object? sender, EventArgs e)
    {
        BezierForm.ShowInstance(this);
    }

    private void MnuDemosArc_Click(object? sender, EventArgs e)
    {
        ArcForm.ShowInstance(this);
    }

}
