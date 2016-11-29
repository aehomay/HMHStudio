namespace HeuristicStudio.Windows.Visualization
{
    partial class fVisualization<T>
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tpDataTree = new System.Windows.Forms.TabPage();
            this.tpVisualization = new System.Windows.Forms.TabPage();
            this.tcMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tcMain
            // 
            this.tcMain.Controls.Add(this.tpDataTree);
            this.tcMain.Controls.Add(this.tpVisualization);
            this.tcMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcMain.Location = new System.Drawing.Point(0, 0);
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            this.tcMain.Size = new System.Drawing.Size(2382, 1283);
            this.tcMain.TabIndex = 0;
            // 
            // tpDataTree
            // 
            this.tpDataTree.Location = new System.Drawing.Point(8, 39);
            this.tpDataTree.Name = "tpDataTree";
            this.tpDataTree.Padding = new System.Windows.Forms.Padding(3);
            this.tpDataTree.Size = new System.Drawing.Size(2366, 1236);
            this.tpDataTree.TabIndex = 0;
            this.tpDataTree.Text = "Tree View";
            this.tpDataTree.UseVisualStyleBackColor = true;
            // 
            // tpVisualization
            // 
            this.tpVisualization.Location = new System.Drawing.Point(8, 39);
            this.tpVisualization.Name = "tpVisualization";
            this.tpVisualization.Padding = new System.Windows.Forms.Padding(3);
            this.tpVisualization.Size = new System.Drawing.Size(2366, 1236);
            this.tpVisualization.TabIndex = 1;
            this.tpVisualization.Text = "Visualization";
            this.tpVisualization.UseVisualStyleBackColor = true;
            // 
            // DataVisualizationWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2382, 1283);
            this.Controls.Add(this.tcMain);
            this.Name = "DataVisualizationWindow";
            this.Text = "DataVisualizationWindow";
            this.tcMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tcMain;
        private System.Windows.Forms.TabPage tpDataTree;
        private System.Windows.Forms.TabPage tpVisualization;
    }
}