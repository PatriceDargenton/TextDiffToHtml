namespace TextDiffToHtml
{
    partial class FrmTextDiffToHtml
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmTextDiffToHtml));
            webBrowser = new WebBrowser();
            LbLibrary = new ListBox();
            LbDisplayMode = new ListBox();
            LbSample = new ListBox();
            CmdWebBrowser = new Button();
            DisplayTimerInit = new System.Windows.Forms.Timer(components);
            ChkIdenticalLines = new CheckBox();
            ChkMonospacedFont = new CheckBox();
            ChkLineThrough = new CheckBox();
            ChkCharLevel = new CheckBox();
            ChkIdenticalParts = new CheckBox();
            toolTip1 = new ToolTip(components);
            CmdCancel = new Button();
            SuspendLayout();
            // 
            // webBrowser
            // 
            webBrowser.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webBrowser.Location = new Point(13, 92);
            webBrowser.Margin = new Padding(4, 3, 4, 3);
            webBrowser.MinimumSize = new Size(23, 23);
            webBrowser.Name = "webBrowser";
            webBrowser.Size = new Size(805, 485);
            webBrowser.TabIndex = 2;
            webBrowser.Url = new Uri("http://examplepage.html/", UriKind.Absolute);
            // 
            // LbLibrary
            // 
            LbLibrary.FormattingEnabled = true;
            LbLibrary.Items.AddRange(new object[] { "DiffPlex", "DiffLib" });
            LbLibrary.Location = new Point(26, 21);
            LbLibrary.Name = "LbLibrary";
            LbLibrary.Size = new Size(101, 34);
            LbLibrary.TabIndex = 3;
            LbLibrary.SelectedIndexChanged += LbLibrary_SelectedIndexChanged;
            // 
            // LbDisplayMode
            // 
            LbDisplayMode.FormattingEnabled = true;
            LbDisplayMode.Items.AddRange(new object[] { "SideBySide", "Inline", "Compact", "TrackChanges" });
            LbDisplayMode.Location = new Point(146, 21);
            LbDisplayMode.Name = "LbDisplayMode";
            LbDisplayMode.Size = new Size(101, 64);
            LbDisplayMode.TabIndex = 4;
            LbDisplayMode.SelectedIndexChanged += LbDisplayMode_SelectedIndexChanged;
            // 
            // LbSample
            // 
            LbSample.FormattingEnabled = true;
            LbSample.Items.AddRange(new object[] { "Sample 1", "Sample 2", "Sample 3" });
            LbSample.Location = new Point(717, 21);
            LbSample.Name = "LbSample";
            LbSample.Size = new Size(101, 49);
            LbSample.TabIndex = 5;
            LbSample.SelectedIndexChanged += LbSample_SelectedIndexChanged;
            // 
            // CmdWebBrowser
            // 
            CmdWebBrowser.Location = new Point(514, 36);
            CmdWebBrowser.Name = "CmdWebBrowser";
            CmdWebBrowser.Size = new Size(70, 29);
            CmdWebBrowser.TabIndex = 6;
            CmdWebBrowser.Text = "Browser";
            CmdWebBrowser.UseVisualStyleBackColor = true;
            CmdWebBrowser.Click += CmdWebBrowser_Click;
            // 
            // DisplayTimerInit
            // 
            DisplayTimerInit.Tick += DisplayTimerInit_Tick;
            // 
            // ChkIdenticalLines
            // 
            ChkIdenticalLines.AutoSize = true;
            ChkIdenticalLines.Checked = true;
            ChkIdenticalLines.CheckState = CheckState.Checked;
            ChkIdenticalLines.Location = new Point(266, 21);
            ChkIdenticalLines.Name = "ChkIdenticalLines";
            ChkIdenticalLines.Size = new Size(98, 19);
            ChkIdenticalLines.TabIndex = 7;
            ChkIdenticalLines.Text = "Identical lines";
            ChkIdenticalLines.UseVisualStyleBackColor = true;
            ChkIdenticalLines.CheckedChanged += ChkIdenticalLines_CheckedChanged;
            // 
            // ChkMonospacedFont
            // 
            ChkMonospacedFont.AutoSize = true;
            ChkMonospacedFont.Checked = true;
            ChkMonospacedFont.CheckState = CheckState.Checked;
            ChkMonospacedFont.Location = new Point(266, 66);
            ChkMonospacedFont.Name = "ChkMonospacedFont";
            ChkMonospacedFont.Size = new Size(120, 19);
            ChkMonospacedFont.TabIndex = 8;
            ChkMonospacedFont.Text = "Monospaced font";
            ChkMonospacedFont.UseVisualStyleBackColor = true;
            ChkMonospacedFont.CheckedChanged += ChkMonospacedFont_CheckedChanged;
            // 
            // ChkLineThrough
            // 
            ChkLineThrough.AutoSize = true;
            ChkLineThrough.Checked = true;
            ChkLineThrough.CheckState = CheckState.Checked;
            ChkLineThrough.Location = new Point(391, 21);
            ChkLineThrough.Name = "ChkLineThrough";
            ChkLineThrough.Size = new Size(94, 19);
            ChkLineThrough.TabIndex = 9;
            ChkLineThrough.Text = "Line through";
            ChkLineThrough.UseVisualStyleBackColor = true;
            ChkLineThrough.CheckedChanged += ChkLineThrough_CheckedChanged;
            // 
            // ChkCharLevel
            // 
            ChkCharLevel.AutoSize = true;
            ChkCharLevel.Location = new Point(391, 46);
            ChkCharLevel.Name = "ChkCharLevel";
            ChkCharLevel.Size = new Size(78, 19);
            ChkCharLevel.TabIndex = 10;
            ChkCharLevel.Text = "Char level";
            ChkCharLevel.UseVisualStyleBackColor = true;
            ChkCharLevel.CheckedChanged += ChkCharLevel_CheckedChanged;
            // 
            // ChkIdenticalParts
            // 
            ChkIdenticalParts.AutoSize = true;
            ChkIdenticalParts.Checked = true;
            ChkIdenticalParts.CheckState = CheckState.Checked;
            ChkIdenticalParts.Location = new Point(266, 46);
            ChkIdenticalParts.Name = "ChkIdenticalParts";
            ChkIdenticalParts.Size = new Size(100, 19);
            ChkIdenticalParts.TabIndex = 11;
            ChkIdenticalParts.Text = "Identical parts";
            ChkIdenticalParts.UseVisualStyleBackColor = true;
            ChkIdenticalParts.CheckedChanged += ChkIdenticalParts_CheckedChanged;
            // 
            // CmdCancel
            // 
            CmdCancel.Enabled = false;
            CmdCancel.Location = new Point(617, 36);
            CmdCancel.Name = "CmdCancel";
            CmdCancel.Size = new Size(70, 29);
            CmdCancel.TabIndex = 12;
            CmdCancel.Text = "Cancel";
            CmdCancel.UseVisualStyleBackColor = true;
            CmdCancel.Click += CmdCancel_Click;
            // 
            // FrmTextDiffToHtml
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(831, 589);
            Controls.Add(CmdCancel);
            Controls.Add(ChkIdenticalParts);
            Controls.Add(ChkCharLevel);
            Controls.Add(ChkLineThrough);
            Controls.Add(ChkMonospacedFont);
            Controls.Add(ChkIdenticalLines);
            Controls.Add(CmdWebBrowser);
            Controls.Add(LbSample);
            Controls.Add(LbDisplayMode);
            Controls.Add(LbLibrary);
            Controls.Add(webBrowser);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FrmTextDiffToHtml";
            Text = "TextDiffToHtml";
            Activated += FrmTextDiffToHtml_Activated;
            FormClosing += FrmTextDiffToHtml_FormClosing;
            Load += FrmTextDiffToHtml_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private WebBrowser webBrowser;
        private ListBox LbLibrary;
        private ListBox LbDisplayMode;
        private ListBox LbSample;
        private Button CmdWebBrowser;
        private System.Windows.Forms.Timer DisplayTimerInit;
        private CheckBox ChkIdenticalLines;
        private CheckBox ChkMonospacedFont;
        private CheckBox ChkLineThrough;
        private CheckBox ChkCharLevel;
        private CheckBox ChkIdenticalParts;
        private ToolTip toolTip1;
        private Button CmdCancel;
    }
}