namespace MineWorld
{
    partial class Form1
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
            this.grid = new System.Windows.Forms.FlowLayoutPanel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DLGopenMap = new System.Windows.Forms.OpenFileDialog();
            this.TXTgoto = new System.Windows.Forms.TextBox();
            this.BTNgoto = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // grid
            // 
            this.grid.Location = new System.Drawing.Point(0, 24);
            this.grid.Margin = new System.Windows.Forms.Padding(0);
            this.grid.Name = "grid";
            this.grid.Size = new System.Drawing.Size(640, 640);
            this.grid.TabIndex = 1;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1025, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // DLGopenMap
            // 
            this.DLGopenMap.RestoreDirectory = true;
            this.DLGopenMap.Title = "Open MineWorld Map";
            // 
            // TXTgoto
            // 
            this.TXTgoto.Location = new System.Drawing.Point(643, 27);
            this.TXTgoto.Name = "TXTgoto";
            this.TXTgoto.Size = new System.Drawing.Size(100, 20);
            this.TXTgoto.TabIndex = 3;
            // 
            // BTNgoto
            // 
            this.BTNgoto.Location = new System.Drawing.Point(748, 25);
            this.BTNgoto.Name = "BTNgoto";
            this.BTNgoto.Size = new System.Drawing.Size(75, 23);
            this.BTNgoto.TabIndex = 4;
            this.BTNgoto.Text = "Open Layer";
            this.BTNgoto.UseVisualStyleBackColor = true;
            this.BTNgoto.Click += new System.EventHandler(this.BTNgoto_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1025, 664);
            this.Controls.Add(this.BTNgoto);
            this.Controls.Add(this.TXTgoto);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel grid;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog DLGopenMap;
        private System.Windows.Forms.TextBox TXTgoto;
        private System.Windows.Forms.Button BTNgoto;
    }
}

