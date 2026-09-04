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
            CmdAddShortcut = new Button();
            CmdRemoveShortcut = new Button();
            chkVectSamples = new CheckBox();
            chkUpperCase = new CheckBox();
            tabPageTextDiff = new TabPage();
            tabPageSemanticDiff = new TabPage();
            tbChunks = new TextBox();
            LbModels = new ListBox();
            tbGapPenalty = new TextBox();
            lblGapPenalty = new Label();
            lblChunks = new Label();
            hScrollBarRender = new HScrollBar();
            tbInfThreshold = new TextBox();
            lblInfThreshold = new Label();
            CmdCancel = new Button();
            ChkSwapLeftRight = new CheckBox();
            tabControlLibraryType = new TabControl();
            tabPageTextDiff.SuspendLayout();
            tabPageSemanticDiff.SuspendLayout();
            tabControlLibraryType.SuspendLayout();
            SuspendLayout();
            // 
            // webBrowser
            // 
            webBrowser.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webBrowser.Location = new Point(13, 144);
            webBrowser.Margin = new Padding(4, 3, 4, 3);
            webBrowser.MinimumSize = new Size(23, 23);
            webBrowser.Name = "webBrowser";
            webBrowser.Size = new Size(945, 307);
            webBrowser.TabIndex = 2;
            webBrowser.Url = new Uri("http://examplepage.html/", UriKind.Absolute);
            // 
            // LbLibrary
            // 
            LbLibrary.FormattingEnabled = true;
            LbLibrary.Location = new Point(26, 21);
            LbLibrary.Name = "LbLibrary";
            LbLibrary.Size = new Size(101, 79);
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
            LbSample.Items.AddRange(new object[] { "Sample 1", "Sample 2", "Sample 3", "Sample 4", "Sample 5" });
            LbSample.Location = new Point(779, 14);
            LbSample.Name = "LbSample";
            LbSample.Size = new Size(101, 79);
            LbSample.TabIndex = 5;
            LbSample.SelectedIndexChanged += LbSample_SelectedIndexChanged;
            // 
            // CmdWebBrowser
            // 
            CmdWebBrowser.Location = new Point(779, 100);
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
            ChkIdenticalLines.Location = new Point(146, 95);
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
            ChkMonospacedFont.Location = new Point(17, 40);
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
            ChkLineThrough.Location = new Point(156, 16);
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
            ChkCharLevel.Location = new Point(156, 40);
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
            ChkIdenticalParts.Location = new Point(17, 16);
            ChkIdenticalParts.Name = "ChkIdenticalParts";
            ChkIdenticalParts.Size = new Size(100, 19);
            ChkIdenticalParts.TabIndex = 11;
            ChkIdenticalParts.Text = "Identical parts";
            ChkIdenticalParts.UseVisualStyleBackColor = true;
            ChkIdenticalParts.CheckedChanged += ChkIdenticalParts_CheckedChanged;
            // 
            // CmdAddShortcut
            // 
            CmdAddShortcut.Location = new Point(892, 14);
            CmdAddShortcut.Name = "CmdAddShortcut";
            CmdAddShortcut.Size = new Size(29, 29);
            CmdAddShortcut.TabIndex = 14;
            CmdAddShortcut.Text = "+";
            toolTip1.SetToolTip(CmdAddShortcut, "Add a \"Send To\" shortcut to TextDiffToHtml in the Windows Explorer context menu");
            CmdAddShortcut.UseVisualStyleBackColor = true;
            CmdAddShortcut.Click += CmdAddShortcut_Click;
            // 
            // CmdRemoveShortcut
            // 
            CmdRemoveShortcut.Location = new Point(892, 50);
            CmdRemoveShortcut.Name = "CmdRemoveShortcut";
            CmdRemoveShortcut.Size = new Size(29, 29);
            CmdRemoveShortcut.TabIndex = 15;
            CmdRemoveShortcut.Text = "-";
            toolTip1.SetToolTip(CmdRemoveShortcut, "Remove the \"Send To\" shortcut from Windows Explorer");
            CmdRemoveShortcut.UseVisualStyleBackColor = true;
            CmdRemoveShortcut.Click += CmdRemoveShortcut_Click;
            // 
            // chkVectSamples
            // 
            chkVectSamples.AutoSize = true;
            chkVectSamples.Location = new Point(18, 58);
            chkVectSamples.Name = "chkVectSamples";
            chkVectSamples.Size = new Size(97, 19);
            chkVectSamples.TabIndex = 17;
            chkVectSamples.Text = "Vect. samples";
            chkVectSamples.TextAlign = ContentAlignment.BottomCenter;
            toolTip1.SetToolTip(chkVectSamples, "Run vectorization samples");
            chkVectSamples.UseVisualStyleBackColor = true;
            chkVectSamples.CheckedChanged += chkVectSamples_CheckedChanged;
            // 
            // chkUpperCase
            // 
            chkUpperCase.AutoSize = true;
            chkUpperCase.Enabled = false;
            chkUpperCase.Location = new Point(136, 58);
            chkUpperCase.Name = "chkUpperCase";
            chkUpperCase.Size = new Size(84, 19);
            chkUpperCase.TabIndex = 18;
            chkUpperCase.Text = "Upper case";
            chkUpperCase.TextAlign = ContentAlignment.MiddleRight;
            toolTip1.SetToolTip(chkUpperCase, "Run vectorization examples with capitalized initials");
            chkUpperCase.UseVisualStyleBackColor = true;
            chkUpperCase.CheckedChanged += chkUpperCase_CheckedChanged;
            // 
            // tabPageTextDiff
            // 
            tabPageTextDiff.Controls.Add(ChkIdenticalParts);
            tabPageTextDiff.Controls.Add(ChkLineThrough);
            tabPageTextDiff.Controls.Add(ChkCharLevel);
            tabPageTextDiff.Controls.Add(ChkMonospacedFont);
            tabPageTextDiff.Location = new Point(4, 24);
            tabPageTextDiff.Margin = new Padding(3, 2, 3, 2);
            tabPageTextDiff.Name = "tabPageTextDiff";
            tabPageTextDiff.Padding = new Padding(3, 2, 3, 2);
            tabPageTextDiff.Size = new Size(482, 90);
            tabPageTextDiff.TabIndex = 0;
            tabPageTextDiff.Text = "TextDiff library type";
            toolTip1.SetToolTip(tabPageTextDiff, "Tab options for the text diff library type");
            tabPageTextDiff.UseVisualStyleBackColor = true;
            // 
            // tabPageSemanticDiff
            // 
            tabPageSemanticDiff.Controls.Add(tbChunks);
            tabPageSemanticDiff.Controls.Add(LbModels);
            tabPageSemanticDiff.Controls.Add(chkUpperCase);
            tabPageSemanticDiff.Controls.Add(tbGapPenalty);
            tabPageSemanticDiff.Controls.Add(chkVectSamples);
            tabPageSemanticDiff.Controls.Add(lblGapPenalty);
            tabPageSemanticDiff.Controls.Add(lblChunks);
            tabPageSemanticDiff.Controls.Add(hScrollBarRender);
            tabPageSemanticDiff.Controls.Add(tbInfThreshold);
            tabPageSemanticDiff.Controls.Add(lblInfThreshold);
            tabPageSemanticDiff.Location = new Point(4, 24);
            tabPageSemanticDiff.Margin = new Padding(3, 2, 3, 2);
            tabPageSemanticDiff.Name = "tabPageSemanticDiff";
            tabPageSemanticDiff.Padding = new Padding(3, 2, 3, 2);
            tabPageSemanticDiff.Size = new Size(482, 90);
            tabPageSemanticDiff.TabIndex = 1;
            tabPageSemanticDiff.Text = "Semantic library type";
            toolTip1.SetToolTip(tabPageSemanticDiff, "Tab options for the semantic diff library type");
            tabPageSemanticDiff.UseVisualStyleBackColor = true;
            // 
            // tbChunks
            // 
            tbChunks.Location = new Point(63, 5);
            tbChunks.Name = "tbChunks";
            tbChunks.Size = new Size(62, 23);
            tbChunks.TabIndex = 19;
            tbChunks.Text = "10";
            toolTip1.SetToolTip(tbChunks, "Select the number of chunks-words by sentence");
            tbChunks.Validated += tbChunks_Validated;
            // 
            // LbModels
            // 
            LbModels.FormattingEnabled = true;
            LbModels.Location = new Point(277, 14);
            LbModels.Name = "LbModels";
            LbModels.Size = new Size(185, 64);
            LbModels.TabIndex = 16;
            toolTip1.SetToolTip(LbModels, "Select the model of embeddings");
            LbModels.SelectedIndexChanged += LbModels_SelectedIndexChanged;
            // 
            // tbGapPenalty
            // 
            tbGapPenalty.Location = new Point(186, 5);
            tbGapPenalty.Name = "tbGapPenalty";
            tbGapPenalty.Size = new Size(43, 23);
            tbGapPenalty.TabIndex = 27;
            tbGapPenalty.Text = "0,25";
            tbGapPenalty.Validated += tbGapPenalty_Validated;
            // 
            // lblGapPenalty
            // 
            lblGapPenalty.AutoSize = true;
            lblGapPenalty.Location = new Point(152, 8);
            lblGapPenalty.Name = "lblGapPenalty";
            lblGapPenalty.Size = new Size(28, 15);
            lblGapPenalty.TabIndex = 26;
            lblGapPenalty.Text = "Gap";
            // 
            // lblChunks
            // 
            lblChunks.AutoSize = true;
            lblChunks.Location = new Point(10, 8);
            lblChunks.Name = "lblChunks";
            lblChunks.Size = new Size(47, 15);
            lblChunks.TabIndex = 20;
            lblChunks.Text = "Chunks";
            // 
            // hScrollBarRender
            // 
            hScrollBarRender.Location = new Point(150, 32);
            hScrollBarRender.Name = "hScrollBarRender";
            hScrollBarRender.Size = new Size(114, 18);
            hScrollBarRender.TabIndex = 25;
            toolTip1.SetToolTip(hScrollBarRender, "Select the threshold for similarity");
            hScrollBarRender.ValueChanged += hScrollBarRender_ValueChanged;
            // 
            // tbInfThreshold
            // 
            tbInfThreshold.Location = new Point(85, 29);
            tbInfThreshold.Name = "tbInfThreshold";
            tbInfThreshold.Size = new Size(52, 23);
            tbInfThreshold.TabIndex = 21;
            tbInfThreshold.Text = "0,92";
            toolTip1.SetToolTip(tbInfThreshold, "Select the threshold for similarity");
            tbInfThreshold.Validated += tbInfThreshold_Validated;
            // 
            // lblInfThreshold
            // 
            lblInfThreshold.AutoSize = true;
            lblInfThreshold.Location = new Point(18, 32);
            lblInfThreshold.Name = "lblInfThreshold";
            lblInfThreshold.Size = new Size(59, 15);
            lblInfThreshold.TabIndex = 22;
            lblInfThreshold.Text = "Threshold";
            // 
            // CmdCancel
            // 
            CmdCancel.Enabled = false;
            CmdCancel.Location = new Point(867, 100);
            CmdCancel.Name = "CmdCancel";
            CmdCancel.Size = new Size(70, 29);
            CmdCancel.TabIndex = 12;
            CmdCancel.Text = "Cancel";
            CmdCancel.UseVisualStyleBackColor = true;
            CmdCancel.Click += CmdCancel_Click;
            // 
            // ChkSwapLeftRight
            // 
            ChkSwapLeftRight.AutoSize = true;
            ChkSwapLeftRight.Location = new Point(146, 119);
            ChkSwapLeftRight.Name = "ChkSwapLeftRight";
            ChkSwapLeftRight.Size = new Size(110, 19);
            ChkSwapLeftRight.TabIndex = 13;
            ChkSwapLeftRight.Text = "Swap Left-Right";
            ChkSwapLeftRight.UseVisualStyleBackColor = true;
            ChkSwapLeftRight.CheckedChanged += ChkSwapLeftRight_CheckedChanged;
            // 
            // tabControlLibraryType
            // 
            tabControlLibraryType.Controls.Add(tabPageTextDiff);
            tabControlLibraryType.Controls.Add(tabPageSemanticDiff);
            tabControlLibraryType.Location = new Point(271, 21);
            tabControlLibraryType.Margin = new Padding(3, 2, 3, 2);
            tabControlLibraryType.Name = "tabControlLibraryType";
            tabControlLibraryType.SelectedIndex = 0;
            tabControlLibraryType.Size = new Size(490, 118);
            tabControlLibraryType.TabIndex = 28;
            // 
            // FrmTextDiffToHtml
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(971, 463);
            Controls.Add(tabControlLibraryType);
            Controls.Add(CmdRemoveShortcut);
            Controls.Add(CmdAddShortcut);
            Controls.Add(ChkSwapLeftRight);
            Controls.Add(CmdCancel);
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
            tabPageTextDiff.ResumeLayout(false);
            tabPageTextDiff.PerformLayout();
            tabPageSemanticDiff.ResumeLayout(false);
            tabPageSemanticDiff.PerformLayout();
            tabControlLibraryType.ResumeLayout(false);
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
        private CheckBox ChkSwapLeftRight;
        private Button CmdAddShortcut;
        private Button CmdRemoveShortcut;
        private ListBox LbModels;
        private CheckBox chkVectSamples;
        private CheckBox chkUpperCase;
        private TextBox tbChunks;
        private Label lblChunks;
        private TextBox tbInfThreshold;
        private Label lblInfThreshold;
        private HScrollBar hScrollBarRender;
        private Label lblGapPenalty;
        private TextBox tbGapPenalty;
        private TabControl tabControlLibraryType;
        private TabPage tabPageTextDiff;
        private TabPage tabPageSemanticDiff;
    }
}