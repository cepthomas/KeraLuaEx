namespace KeraLuaEx.Host
{
    partial class HostForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            btnClearOnRun = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            btnGoMain = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            txtPos = new System.Windows.Forms.ToolStripTextBox();
            btnGoPlay = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            btnFile = new System.Windows.Forms.ToolStripDropDownButton();
            openMenu = new System.Windows.Forms.ToolStripMenuItem();
            saveMenu = new System.Windows.Forms.ToolStripMenuItem();
            rtbScript = new System.Windows.Forms.RichTextBox();
            rtbOutput = new System.Windows.Forms.RichTextBox();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            splitContainer3 = new System.Windows.Forms.SplitContainer();
            rtbStack = new System.Windows.Forms.RichTextBox();
            splitContainer2 = new System.Windows.Forms.SplitContainer();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnFile, toolStripSeparator3, btnClearOnRun, toolStripSeparator4, btnGoMain, toolStripSeparator1, txtPos, btnGoPlay, toolStripSeparator2 });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(1286, 27);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 27);
            // 
            // btnClearOnRun
            // 
            btnClearOnRun.CheckOnClick = true;
            btnClearOnRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnClearOnRun.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnClearOnRun.Name = "btnClearOnRun";
            btnClearOnRun.Size = new System.Drawing.Size(99, 24);
            btnClearOnRun.Text = "Clear On Run";
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 27);
            // 
            // btnGoMain
            // 
            btnGoMain.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnGoMain.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnGoMain.Name = "btnGoMain";
            btnGoMain.Size = new System.Drawing.Size(69, 24);
            btnGoMain.Text = "Go Main";
            btnGoMain.Click += GoMain_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // txtPos
            // 
            txtPos.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            txtPos.AutoSize = false;
            txtPos.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtPos.Name = "txtPos";
            txtPos.ReadOnly = true;
            txtPos.Size = new System.Drawing.Size(70, 20);
            // 
            // btnGoPlay
            // 
            btnGoPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnGoPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnGoPlay.Name = "btnGoPlay";
            btnGoPlay.Size = new System.Drawing.Size(63, 24);
            btnGoPlay.Text = "Go Play";
            btnGoPlay.Click += GoPlay_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // btnFile
            // 
            btnFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { openMenu, saveMenu });
            btnFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnFile.Name = "btnFile";
            btnFile.Size = new System.Drawing.Size(46, 24);
            btnFile.Text = "File";
            // 
            // openMenu
            // 
            openMenu.Name = "openMenu";
            openMenu.Size = new System.Drawing.Size(224, 26);
            openMenu.Text = "Open";
            openMenu.Click += Open_Click;
            // 
            // saveMenu
            // 
            saveMenu.Name = "saveMenu";
            saveMenu.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            saveMenu.Size = new System.Drawing.Size(224, 26);
            saveMenu.Text = "Save";
            saveMenu.Click += Save_Click;
            // 
            // rtbScript
            // 
            rtbScript.BackColor = System.Drawing.Color.LightCyan;
            rtbScript.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            rtbScript.Dock = System.Windows.Forms.DockStyle.Fill;
            rtbScript.Location = new System.Drawing.Point(0, 0);
            rtbScript.Name = "rtbScript";
            rtbScript.Size = new System.Drawing.Size(960, 385);
            rtbScript.TabIndex = 1;
            rtbScript.Text = "";
            // 
            // rtbOutput
            // 
            rtbOutput.BackColor = System.Drawing.Color.Linen;
            rtbOutput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            rtbOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            rtbOutput.Location = new System.Drawing.Point(0, 0);
            rtbOutput.Name = "rtbOutput";
            rtbOutput.Size = new System.Drawing.Size(960, 422);
            rtbOutput.TabIndex = 2;
            rtbOutput.Text = "";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 27);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(splitContainer3);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new System.Drawing.Size(1286, 811);
            splitContainer1.SplitterDistance = 322;
            splitContainer1.TabIndex = 3;
            // 
            // splitContainer3
            // 
            splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer3.Location = new System.Drawing.Point(0, 0);
            splitContainer3.Name = "splitContainer3";
            splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(rtbStack);
            splitContainer3.Size = new System.Drawing.Size(322, 811);
            splitContainer3.SplitterDistance = 346;
            splitContainer3.TabIndex = 0;
            // 
            // rtbStack
            // 
            rtbStack.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            rtbStack.Dock = System.Windows.Forms.DockStyle.Fill;
            rtbStack.Location = new System.Drawing.Point(0, 0);
            rtbStack.Name = "rtbStack";
            rtbStack.ReadOnly = true;
            rtbStack.Size = new System.Drawing.Size(322, 461);
            rtbStack.TabIndex = 0;
            rtbStack.Text = "";
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer2.Location = new System.Drawing.Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(rtbScript);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(rtbOutput);
            splitContainer2.Size = new System.Drawing.Size(960, 811);
            splitContainer2.SplitterDistance = 385;
            splitContainer2.TabIndex = 0;
            // 
            // HostForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.Control;
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            ClientSize = new System.Drawing.Size(1286, 838);
            Controls.Add(splitContainer1);
            Controls.Add(toolStrip1);
            Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            Name = "HostForm";
            Text = "KeraLuaEx Test Host";
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.RichTextBox rtbScript;
        private System.Windows.Forms.RichTextBox rtbOutput;
        private System.Windows.Forms.ToolStripButton btnGoMain;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.RichTextBox rtbStack;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripTextBox txtPos;
        private System.Windows.Forms.ToolStripButton btnGoPlay;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnClearOnRun;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripDropDownButton btnFile;
        private System.Windows.Forms.ToolStripMenuItem openMenu;
        private System.Windows.Forms.ToolStripMenuItem saveMenu;
    }
}

