using System;
using System.Windows.Forms;

namespace Agg.AdaptiveSubdivision.VisualTest {
    public partial class ContainerForm : Form {

        public ContainerForm() {
            InitializeComponent();
            RegisterEventHandlers();
        }

        ~ContainerForm() {
            UnregisterEventHandlers();
        }

        private void UnregisterEventHandlers() {
            mnuDemosArc.Click -= MnuDemosArc_Click;
            mnuDemosBezier.Click -= MnuDemosBezier_Click;
        }

        private void RegisterEventHandlers() {
            mnuDemosArc.Click += MnuDemosArc_Click;
            mnuDemosBezier.Click += MnuDemosBezier_Click;
        }

        private void MnuDemosBezier_Click(object sender, EventArgs e) {
            BezierForm.ShowInstance(this);
        }

        private void MnuDemosArc_Click(object sender, EventArgs e) {
            ArcForm.ShowInstance(this);
        }

    }
}
