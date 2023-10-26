namespace KeraLuaEx.Test
{
    partial class TestForm
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
            btnFile = new System.Windows.Forms.ToolStripDropDownButton();
            openMenu = new System.Windows.Forms.ToolStripMenuItem();
            saveMenu = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            btnClearOnRun = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            btnRunPlay = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            btnRunGlobal = new System.Windows.Forms.ToolStripButton();
            txtPos = new System.Windows.Forms.ToolStripTextBox();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            btnRunModule = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            btnRunErrors = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            btnRunApi = new System.Windows.Forms.ToolStripButton();
            rtbScript = new System.Windows.Forms.RichTextBox();
            rtbOutput = new System.Windows.Forms.RichTextBox();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            splitContainer2 = new System.Windows.Forms.SplitContainer();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnFile, toolStripSeparator3, btnClearOnRun, toolStripSeparator4, btnRunPlay, toolStripSeparator5, btnRunGlobal, txtPos, toolStripSeparator6, btnRunModule, toolStripSeparator7, btnRunErrors, toolStripSeparator1, btnRunApi, toolStripSeparator2 });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(1286, 27);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
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
            openMenu.Size = new System.Drawing.Size(173, 26);
            openMenu.Text = "Open";
            openMenu.Click += Open_Click;
            // 
            // saveMenu
            // 
            saveMenu.Name = "saveMenu";
            saveMenu.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            saveMenu.Size = new System.Drawing.Size(173, 26);
            saveMenu.Text = "Save";
            saveMenu.Click += Save_Click;
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
            // btnRunPlay
            // 
            btnRunPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnRunPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnRunPlay.Name = "btnRunPlay";
            btnRunPlay.Size = new System.Drawing.Size(69, 24);
            btnRunPlay.Text = "Run Play";
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(6, 27);
            // 
            // btnRunGlobal
            // 
            btnRunGlobal.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnRunGlobal.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnRunGlobal.Name = "btnRunGlobal";
            btnRunGlobal.Size = new System.Drawing.Size(86, 24);
            btnRunGlobal.Text = "Run Global";
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
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new System.Drawing.Size(6, 27);
            // 
            // btnRunModule
            // 
            btnRunModule.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnRunModule.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnRunModule.Name = "btnRunModule";
            btnRunModule.Size = new System.Drawing.Size(93, 24);
            btnRunModule.Text = "Run Module";
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new System.Drawing.Size(6, 27);
            // 
            // btnRunErrors
            // 
            btnRunErrors.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnRunErrors.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnRunErrors.Name = "btnRunErrors";
            btnRunErrors.Size = new System.Drawing.Size(80, 24);
            btnRunErrors.Text = "Run Errors";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // btnRunApi
            // 
            btnRunApi.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnRunApi.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnRunApi.Name = "btnRunApi";
            btnRunApi.Size = new System.Drawing.Size(65, 24);
            btnRunApi.Text = "Run Api";
            // 
            // rtbScript
            // 
            rtbScript.BackColor = System.Drawing.Color.LightCyan;
            rtbScript.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            rtbScript.Dock = System.Windows.Forms.DockStyle.Fill;
            rtbScript.Location = new System.Drawing.Point(0, 0);
            rtbScript.Name = "rtbScript";
            rtbScript.Size = new System.Drawing.Size(1182, 249);
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
            rtbOutput.Size = new System.Drawing.Size(1182, 558);
            rtbOutput.TabIndex = 2;
            rtbOutput.Text = "";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 27);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new System.Drawing.Size(1286, 811);
            splitContainer1.SplitterDistance = 100;
            splitContainer1.TabIndex = 3;
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
            splitContainer2.Size = new System.Drawing.Size(1182, 811);
            splitContainer2.SplitterDistance = 249;
            splitContainer2.TabIndex = 0;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // TestForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.Control;
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            ClientSize = new System.Drawing.Size(1286, 838);
            Controls.Add(splitContainer1);
            Controls.Add(toolStrip1);
            Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            Name = "TestForm";
            Text = "KeraLuaEx Test Test";
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
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
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripTextBox txtPos;
        private System.Windows.Forms.ToolStripButton btnClearOnRun;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripDropDownButton btnFile;
        private System.Windows.Forms.ToolStripMenuItem openMenu;
        private System.Windows.Forms.ToolStripMenuItem saveMenu;

        private System.Windows.Forms.ToolStripButton btnRunGlobal;
        private System.Windows.Forms.ToolStripButton btnRunPlay;
        private System.Windows.Forms.ToolStripButton btnRunModule;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton btnRunErrors;
        private System.Windows.Forms.ToolStripButton btnRunApi;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}

