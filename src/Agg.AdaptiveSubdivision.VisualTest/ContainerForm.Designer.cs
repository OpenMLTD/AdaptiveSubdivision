namespace Agg.AdaptiveSubdivision.VisualTest {
    partial class ContainerForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.mnuDemos = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDemosArc = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDemosBezier = new System.Windows.Forms.ToolStripMenuItem();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuDemos,
            this.windowToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.MdiWindowListItem = this.windowToolStripMenuItem;
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(917, 25);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // mnuDemos
            // 
            this.mnuDemos.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuDemosArc,
            this.mnuDemosBezier});
            this.mnuDemos.Name = "mnuDemos";
            this.mnuDemos.Size = new System.Drawing.Size(112, 21);
            this.mnuDemos.Text = "&Demonstrations";
            // 
            // mnuDemosArc
            // 
            this.mnuDemosArc.Name = "mnuDemosArc";
            this.mnuDemosArc.Size = new System.Drawing.Size(112, 22);
            this.mnuDemosArc.Text = "&Arc";
            // 
            // mnuDemosBezier
            // 
            this.mnuDemosBezier.Name = "mnuDemosBezier";
            this.mnuDemosBezier.Size = new System.Drawing.Size(112, 22);
            this.mnuDemosBezier.Text = "&Bezier";
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(67, 21);
            this.windowToolStripMenuItem.Text = "&Window";
            // 
            // ContainerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(917, 508);
            this.Controls.Add(this.menuStrip1);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ContainerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Adaptive Subdivision Viewer";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuDemos;
        private System.Windows.Forms.ToolStripMenuItem mnuDemosArc;
        private System.Windows.Forms.ToolStripMenuItem mnuDemosBezier;
        private System.Windows.Forms.ToolStripMenuItem windowToolStripMenuItem;
    }
}