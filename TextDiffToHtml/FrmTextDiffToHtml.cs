
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using static TextDiffToHtml.TextDiffToHtmlEnums;

// https://www.nuget.org/packages/Vereyon.Windows.WebBrowser
// https://github.com/Vereyon/WebBrowser
using Vereyon.Windows;

namespace TextDiffToHtml
{
    public partial class FrmTextDiffToHtml : Form
    {
        public Parameter prm = new();

        private readonly HtmlRenderer htmlRenderer;

        readonly string title = "";
        private bool init = false;
        private string htmlResultFilePath = "";

        private const string ExeTextDiffToHtml = "TextDiffToHtml.exe";
        private const string ShortcutTextDiffToHtml = ExeTextDiffToHtml + ".lnk";
        private string _shortcutPath =
            Environment.GetFolderPath(Environment.SpecialFolder.SendTo) +
            "\\" + ShortcutTextDiffToHtml;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScriptingBridge Bridge { get; private set; }

        public FrmTextDiffToHtml()
        {
            InitializeComponent();

            Bridge = new ScriptingBridge(webBrowser, true);
            Bridge.Initialized += new EventHandler(Bridge_Initialized);

            LbLibrary.Items.Clear();
            LbLibrary.Items.Add(TextDiffToHtmlEnums.LibraryEnum.DiffPlex);
            LbLibrary.Items.Add(TextDiffToHtmlEnums.LibraryEnum.DiffLib);
            LbLibrary.Items.Add(TextDiffToHtmlEnums.LibraryEnum.TextDiffSharp);
            LbLibrary.Items.Add(TextDiffToHtmlEnums.LibraryEnum.CSharpDiff);
            LbLibrary.SelectedIndex = 0;

            LbDisplayMode.Items.Clear();
            LbDisplayMode.Items.Add(TextDiffToHtmlEnums.DisplayModeEnum.SideBySide.ToShortDescription());
            LbDisplayMode.Items.Add(TextDiffToHtmlEnums.DisplayModeEnum.Inline.ToShortDescription());
            LbDisplayMode.Items.Add(TextDiffToHtmlEnums.DisplayModeEnum.Compact.ToShortDescription());
            LbDisplayMode.Items.Add(TextDiffToHtmlEnums.DisplayModeEnum.TrackChanges.ToShortDescription());
            LbDisplayMode.SelectedIndex = 0;
            LbSample.SelectedIndex = 0;

            var asm = Assembly.GetExecutingAssembly();
            var asmName = asm?.GetName();
            Version? version = asmName?.Version;
            var versionTxt = version?.Major + "." + version?.Minor + version?.Build;
            this.title = this.Text + " " + versionTxt + " (" + Const.dateVersion + ")";
            UpdateTitle();

            htmlRenderer = new HtmlRenderer() { OnPartialRender = RenderInWebBrowser };

            var txt = EnumHelper.GetEnumDescription<ShowIdenticalLinesEnum>();
            toolTip1.SetToolTip(ChkIdenticalLines, txt);

            txt = EnumHelper.GetEnumDescription<ShowIdenticalPartsEnum>();
            toolTip1.SetToolTip(ChkIdenticalParts, txt);

            txt = EnumHelper.GetEnumDescription<MonospacedFontEnum>();
            toolTip1.SetToolTip(ChkMonospacedFont, txt);

            txt = EnumHelper.GetEnumDescription<LineThroughEnum>();
            toolTip1.SetToolTip(ChkLineThrough, txt);

            txt = EnumHelper.GetEnumDescription<CharLevelEnum>();
            toolTip1.SetToolTip(ChkCharLevel, txt);

            txt = EnumHelper.GetEnumDescription<SwapLeftRightEnum>();
            toolTip1.SetToolTip(ChkSwapLeftRight, txt);

            toolTip1.SetToolTip(CmdWebBrowser,
                "Click to view differences in default external browser");

            toolTip1.SetToolTip(CmdCancel, "Click to cancel a long operation");

            toolTip1.SetToolTip(LbSample, "Choose a sample to test");

            bool configMode = true;
            if (!String.IsNullOrEmpty(prm.LeftText) && 
                !String.IsNullOrEmpty(prm.RightText)) configMode = false;
            if (configMode)
            {
                CmdAddShortcut.Visible = true;
                CmdRemoveShortcut.Visible = true;
                CheckShortcut();
                return;
            }
            else
            {
                CmdAddShortcut.Visible = false;
                CmdRemoveShortcut.Visible = false;
            }
        }

        private void UpdateTitle()
        {
            this.Text = this.title;
        }

        private void FrmTextDiffToHtml_Load(object sender, EventArgs e)
        {
            LoadWindowsPositionAndSettings();
        }

        private void FrmTextDiffToHtml_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveWindowsPositionAndSettings();
        }

        private void FrmTextDiffToHtml_Activated(object sender, EventArgs e)
        {
            if (this.init) return;

            if (string.IsNullOrEmpty(this.prm.LeftText) ||
                string.IsNullOrEmpty(this.prm.RightText))
                this.LbSample.Visible = true;
            else
                this.LbSample.Visible = false;

            this.init = true;
            Render();
        }

        private void LoadWindowsPositionAndSettings()
        {
            if (Properties.Settings.Default.WindowMax)
                this.WindowState = FormWindowState.Maximized;

            if (Properties.Settings.Default.WindowPositionX >= 0 &&
                Properties.Settings.Default.WindowPositionY >= 0)
                this.Location = new Point(
                    Properties.Settings.Default.WindowPositionX,
                    Properties.Settings.Default.WindowPositionY);

            if (Properties.Settings.Default.WindowWidth > 0 &&
                Properties.Settings.Default.WindowHeight > 0)
                this.Size = new Size(
                    Properties.Settings.Default.WindowWidth,
                    Properties.Settings.Default.WindowHeight);

            this.LbLibrary.Text = Properties.Settings.Default.Library;
            this.LbDisplayMode.Text = Properties.Settings.Default.DisplayMode;
            this.ChkCharLevel.Checked = Properties.Settings.Default.CharLevel;
            this.ChkLineThrough.Checked = Properties.Settings.Default.LineThrough;
            this.ChkIdenticalLines.Checked = Properties.Settings.Default.ShowIdenticalLines;
            this.ChkIdenticalParts.Checked = Properties.Settings.Default.ShowIdenticalParts;
            this.ChkSwapLeftRight.Checked = Properties.Settings.Default.SwapLeftRight;

            // DiffLib.TrackChanges is too slow for long texts
            if (this.LbLibrary.Text == TextDiffToHtmlEnums.LibraryEnum.DiffLib.ToString() &&
                this.LbDisplayMode.Text == TextDiffToHtmlEnums.DisplayModeEnum.TrackChanges.ToShortDescription())
                this.LbDisplayMode.Text = TextDiffToHtmlEnums.DisplayModeEnum.SideBySide.ToShortDescription();
        }

        private void SaveWindowsPositionAndSettings()
        {
            Properties.Settings.Default.WindowMax =
                (this.WindowState == FormWindowState.Maximized);
            if (!Properties.Settings.Default.WindowMax)
            {
                Properties.Settings.Default.WindowPositionX = this.Location.X;
                Properties.Settings.Default.WindowPositionY = this.Location.Y;
                Properties.Settings.Default.WindowWidth = this.Size.Width;
                Properties.Settings.Default.WindowHeight = this.Size.Height;
            }

            Properties.Settings.Default.Library = this.LbLibrary.Text;
            Properties.Settings.Default.DisplayMode = this.LbDisplayMode.Text;
            Properties.Settings.Default.CharLevel = this.ChkCharLevel.Checked;
            Properties.Settings.Default.LineThrough = this.ChkLineThrough.Checked;
            Properties.Settings.Default.ShowIdenticalLines = this.ChkIdenticalLines.Checked;
            Properties.Settings.Default.ShowIdenticalParts = this.ChkIdenticalParts.Checked;
            Properties.Settings.Default.SwapLeftRight = this.ChkSwapLeftRight.Checked;

            Properties.Settings.Default.Save();
        }

        void Bridge_Initialized(object? sender, EventArgs e)
        {
            var html = "TextDiffToHtml";
            string path = AppContext.BaseDirectory;
            string filePath = Path.Combine(path, Const.outputFilename);
            File.WriteAllText(filePath, html);
            webBrowser.Url = new Uri(filePath);
        }

        private void LbLibrary_SelectedIndexChanged(object sender, EventArgs e)
        {
            Render();
        }

        private void LbDisplayMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            Render();
        }

        private void LbSample_SelectedIndexChanged(object sender, EventArgs e)
        {
            Render();
        }

        private void ChkIdenticalLines_CheckedChanged(object sender, EventArgs e)
        {
            Render();
        }

        private void ChkIdenticalParts_CheckedChanged(object sender, EventArgs e)
        {
            Render();
        }

        private void ChkMonospacedFont_CheckedChanged(object sender, EventArgs e)
        {
            Render();
        }

        private void ChkLineThrough_CheckedChanged(object sender, EventArgs e)
        {
            Render();
        }

        private void ChkCharLevel_CheckedChanged(object sender, EventArgs e)
        {
            Render();
        }

        private void ChkSwapLeftRight_CheckedChanged(object sender, EventArgs e)
        {
            Render();
        }

        private void CmdWebBrowser_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo(this.htmlResultFilePath) { UseShellExecute = true });
        }

        private bool activation = false;
        private void Activation()
        {
            if (activation) return;
            activation = true;

            var libraryText = this.LbLibrary.Text;
            var library = TextDiffToHtmlEnums.LibraryFromValue(libraryText);
            var displayModeText = this.LbDisplayMode.Text;
            //var displayMode = TextDiffToHtmlEnums.DisplayModeFromValue(displayModeText);
            var displayMode = TextDiffToHtmlEnums.DisplayModeFromDescription(displayModeText);

            this.ChkMonospacedFont.Enabled = false;
            this.ChkIdenticalLines.Enabled = false;
            this.ChkIdenticalParts.Enabled = false;
            this.ChkLineThrough.Enabled = false;
            this.ChkCharLevel.Enabled = false;

            switch (library)
            {
                case TextDiffToHtmlEnums.LibraryEnum.DiffPlex:
                    switch (displayMode)
                    {
                        case TextDiffToHtmlEnums.DisplayModeEnum.SideBySide:
                            this.ChkIdenticalLines.Enabled = true;
                            this.ChkMonospacedFont.Enabled = true;
                            this.ChkIdenticalParts.Checked = true;
                            this.ChkCharLevel.Checked = false;
                            this.ChkLineThrough.Checked = false;
                            break;
                        case TextDiffToHtmlEnums.DisplayModeEnum.Inline:
                            this.ChkIdenticalLines.Enabled = true;
                            this.ChkIdenticalParts.Enabled = true;
                            this.ChkMonospacedFont.Enabled = true;
                            //this.ChkCharLevel.Checked = true;
                            this.ChkCharLevel.Checked = false; // 06/06/2026
                            this.ChkLineThrough.Checked = false;
                            break;
                        case TextDiffToHtmlEnums.DisplayModeEnum.Compact:
                            this.ChkIdenticalLines.Enabled = true;
                            //this.ChkIdenticalParts.Enabled = true; // Not possible
                            this.ChkLineThrough.Enabled = true;
                            this.ChkMonospacedFont.Enabled = true;
                            this.ChkIdenticalParts.Checked = true; // No choice
                            //this.ChkCharLevel.Checked = true;
                            this.ChkCharLevel.Checked = false; // 06/06/2026
                            break;
                        case TextDiffToHtmlEnums.DisplayModeEnum.TrackChanges:
                            this.ChkLineThrough.Enabled = false;
                            this.ChkLineThrough.Checked = true;
                            this.ChkIdenticalLines.Checked = true;
                            this.ChkIdenticalParts.Checked = true;
                            this.ChkMonospacedFont.Checked = false;
                            this.ChkCharLevel.Checked = true; // 06/06/2026
                            break;
                    }
                    break;

                case TextDiffToHtmlEnums.LibraryEnum.DiffLib:

                    switch (displayMode)
                    {
                        case TextDiffToHtmlEnums.DisplayModeEnum.SideBySide:
                            this.ChkIdenticalLines.Enabled = true;
                            this.ChkCharLevel.Enabled = true;
                            this.ChkLineThrough.Enabled = true;
                            this.ChkMonospacedFont.Enabled = true;
                            this.ChkIdenticalParts.Checked = true;
                            break;
                        case TextDiffToHtmlEnums.DisplayModeEnum.Inline:
                            this.ChkIdenticalLines.Enabled = true;
                            this.ChkIdenticalParts.Enabled = true;
                            this.ChkMonospacedFont.Enabled = true;
                            this.ChkCharLevel.Checked = true; // 06/06/2026 Mandatory for DiffLib inline
                            this.ChkLineThrough.Checked = false;
                            break;
                        case TextDiffToHtmlEnums.DisplayModeEnum.Compact:
                            this.ChkIdenticalLines.Enabled = true;
                            //this.ChkIdenticalParts.Enabled = true; // Not possible
                            this.ChkLineThrough.Enabled = true;
                            this.ChkMonospacedFont.Enabled = true;
                            this.ChkIdenticalParts.Checked = true; // No choice
                            this.ChkCharLevel.Checked = true; // 06/06/2026 Mandatory for DiffLib compact
                            break;
                        case TextDiffToHtmlEnums.DisplayModeEnum.TrackChanges:
                            this.ChkIdenticalParts.Enabled = true;
                            this.ChkLineThrough.Enabled = true;
                            this.ChkMonospacedFont.Enabled = true;
                            this.ChkCharLevel.Checked = true; // 06/06/2026 Mandatory for DiffLib track changes
                            break;
                    }
                    break;

                case TextDiffToHtmlEnums.LibraryEnum.TextDiffSharp: // 06/06/2026
                    switch (displayMode)
                    {
                        case TextDiffToHtmlEnums.DisplayModeEnum.SideBySide:
                            this.ChkIdenticalLines.Enabled = true;
                            this.ChkMonospacedFont.Enabled = true;
                            this.ChkCharLevel.Checked = false;
                            break;
                        case TextDiffToHtmlEnums.DisplayModeEnum.Inline:
                            this.ChkIdenticalLines.Enabled = true;
                            this.ChkIdenticalParts.Enabled = true;
                            this.ChkMonospacedFont.Enabled = true;
                            this.ChkCharLevel.Checked = false;
                            this.ChkLineThrough.Checked = false;
                            break;
                        case TextDiffToHtmlEnums.DisplayModeEnum.Compact:
                            this.ChkIdenticalLines.Enabled = true;
                            this.ChkLineThrough.Enabled = true;
                            this.ChkMonospacedFont.Enabled = true;
                            this.ChkIdenticalParts.Enabled = true;
                            this.ChkMonospacedFont.Enabled = true;
                            this.ChkCharLevel.Checked = false;
                            break;
                        case TextDiffToHtmlEnums.DisplayModeEnum.TrackChanges:
                            this.ChkIdenticalLines.Enabled = true;
                            this.ChkLineThrough.Enabled = true;
                            this.ChkMonospacedFont.Enabled = true;
                            this.ChkIdenticalParts.Checked = true;
                            this.ChkCharLevel.Checked = false;
                            break;
                    }
                    break;

                case TextDiffToHtmlEnums.LibraryEnum.CSharpDiff:
                    switch (displayMode)
                    {
                        case TextDiffToHtmlEnums.DisplayModeEnum.SideBySide:
                            this.ChkIdenticalLines.Enabled = true;
                            this.ChkMonospacedFont.Enabled = true;
                            this.ChkCharLevel.Checked = true;
                            this.ChkIdenticalParts.Checked = true;
                            break;
                        case TextDiffToHtmlEnums.DisplayModeEnum.Inline:
                            this.ChkIdenticalLines.Enabled = true;
                            this.ChkMonospacedFont.Enabled = true;
                            this.ChkCharLevel.Checked = true;
                            this.ChkLineThrough.Checked = false;
                            this.ChkIdenticalParts.Checked = true;
                            break;
                        case TextDiffToHtmlEnums.DisplayModeEnum.Compact:
                            this.ChkIdenticalLines.Enabled = true;
                            this.ChkLineThrough.Enabled = true;
                            this.ChkMonospacedFont.Enabled = true;
                            this.ChkIdenticalParts.Checked = true;
                            this.ChkCharLevel.Checked = true;
                            break;
                        case TextDiffToHtmlEnums.DisplayModeEnum.TrackChanges:
                            this.ChkIdenticalLines.Enabled = true;
                            this.ChkLineThrough.Enabled = true;
                            this.ChkMonospacedFont.Enabled = true;
                            this.ChkIdenticalParts.Checked = true;
                            this.ChkCharLevel.Checked = true;
                            break;
                    }
                    break;
            }
            activation = false;
        }

        private void Render()
        {
            if (!this.init) return;
            Activation();

            var library = EnumHelper.GetEnumDescription<LibraryEnum>();
            var libraryValue = TextDiffToHtmlEnums.LibraryFromValue(this.LbLibrary.Text);
            var txt = library + ": " + libraryValue.ToDescription();
            toolTip1.SetToolTip(LbLibrary, txt);

            var displayMode = EnumHelper.GetEnumDescription<DisplayModeEnum>();
            //var displayModeValue = TextDiffToHtmlEnums.DisplayModeFromValue(this.LbDisplayMode.Text);
            var displayModeValue = TextDiffToHtmlEnums.DisplayModeFromDescription(this.LbDisplayMode.Text);
            txt = displayMode + ": " + displayModeValue.ToDescription();
            toolTip1.SetToolTip(LbDisplayMode, txt);

            string path = AppContext.BaseDirectory; // Application.StartupPath() equivalent in .Net9;
            this.htmlResultFilePath = Path.Combine(path, Const.outputFilename);

            string htmlSample = Const.htmlCharset + Const.newline;
            var htmlLoading = htmlSample + "...";
            ShowInInternalBrowser(htmlLoading);

            this.DisplayTimerInit.Interval = 50;
            this.DisplayTimerInit.Start();
        }

        private void DisplayTimerInit_Tick(object sender, EventArgs e)
        {
            this.DisplayTimerInit.Stop();
            this.CmdWebBrowser.Enabled = false;
            var html = HtmlRender();
            ShowInInternalBrowser(html);
            this.CmdWebBrowser.Enabled = true;
        }

        private void ShowInInternalBrowser(string html)
        {
            File.WriteAllText(this.htmlResultFilePath, html);
            webBrowser.Url = new Uri(this.htmlResultFilePath);
        }

        private void RenderInWebBrowser(string text)
        {
            if (this.htmlRenderer.line == 0) this.Text = this.title + "...";
            else
            {
                ShowInInternalBrowser(text);
                this.Text = this.title + " : " +
                    this.htmlRenderer.line + "/" +
                    this.htmlRenderer.lines + " : " +
                    this.htmlRenderer.progress.ToString("0.00") + " %";
            }
            Application.DoEvents(); // Check for cancel
        }

        private void CmdCancel_Click(object sender, EventArgs e)
        {
            this.htmlRenderer.cancel = true;
        }

        private string HtmlRender()
        {
            string left = "";
            string right = "";
            //string htmlSample = Const.htmlCharset + Const.newline;
            string htmlSample = Const.htmlStart;

            var samples = true;
            if (!string.IsNullOrEmpty(this.prm.LeftText) &&
                !string.IsNullOrEmpty(this.prm.RightText))
            {
                left = this.prm.LeftText;
                right = this.prm.RightText;
                if (this.ChkSwapLeftRight.Checked)
                {
                    // Swap left and right texts
                    var tmp = left;
                    left = right;
                    right = tmp;
                }
                samples = false;
            }
            else
            {
                var sample = this.LbSample.Text;
                switch (sample)
                {
                    // Sample 1: Aiikon's TextDiff Demo
                    // https://github.com/Aiikon/TextDiff
                    case "Sample 1":
                        left = DiffPlexAPI.AiikonLeftSample;
                        right = DiffPlexAPI.AiikonRightSample;
                        break;

                    // Sample 2 & 3: Lassevk's DiffLib Demos
                    // https://github.com/lassevk/DiffLib/tree/main/Examples

                    // 000 - Basic diffing of two texts
                    case "Sample 2":
                        left = DiffLibAPI.LassevkLeftSample1;
                        right = DiffLibAPI.LassevkRightSample1;
                        break;

                    // 001 - Basic diffing of two text files
                    case "Sample 3":
                        left = DiffLibAPI.LassevkLeftSample2;
                        right = DiffLibAPI.LassevkRightSample2;
                        break;

                    case "Sample 4":
                        left = CSharpDiffAPI.LeftSentenceSample;
                        right = CSharpDiffAPI.RightSentenceSample;
                        break;
                    case "Sample 5":
                        left = CSharpDiffAPI.LeftLineSample;
                        right = CSharpDiffAPI.RightLineSample;
                        break;
                }
                if (this.ChkSwapLeftRight.Checked)
                {
                    // Swap left and right texts
                    var tmp = left;
                    left = right;
                    right = tmp;
                }
                htmlSample +=
                    "<p>" + this.LbSample.Text + ":</p>" + Const.newline
                    + "<p>" + left.Replace(Const.newline, Const.htmlNewline) + "</p>" + Const.newline
                    + "<p>" + right.Replace(Const.newline, Const.htmlNewline) + "</p>";
            }

            var html = htmlSample;

            var libraryText = this.LbLibrary.Text;
            var library = TextDiffToHtmlEnums.LibraryFromValue(libraryText);
            var displayModeText = this.LbDisplayMode.Text;
            //var displayMode = TextDiffToHtmlEnums.DisplayModeFromValue(displayModeText);
            var displayMode = TextDiffToHtmlEnums.DisplayModeFromDescription(displayModeText);
            switch (library)
            {
                case TextDiffToHtmlEnums.LibraryEnum.DiffPlex:
                    switch (displayMode)
                    {
                        case TextDiffToHtmlEnums.DisplayModeEnum.SideBySide:
                            var htmlDiffPlexSideBySide = "";
                            if (samples) htmlDiffPlexSideBySide =
                                "<br>" + this.LbSample.Text +
                                ": DiffPlex side by side: Original DiffPlex sample from Aiikon<br>\n";
                            htmlDiffPlexSideBySide += DiffPlexAPI.TextDiffSideBySide(left, right,
                                this.ChkIdenticalLines.Checked, this.ChkMonospacedFont.Checked);
                            html += htmlDiffPlexSideBySide;
                            break;
                        case TextDiffToHtmlEnums.DisplayModeEnum.Inline:
                            var htmlDiffPlexInline = "";
                            if (samples) htmlDiffPlexInline = "<br>" + this.LbSample.Text +
                                    ": DiffPlex inline<br>\n";
                            htmlDiffPlexInline += DiffPlexAPI.TextDiffInline(left, right,
                                this.ChkIdenticalLines.Checked, this.ChkIdenticalParts.Checked,
                                this.ChkMonospacedFont.Checked);
                            html += htmlDiffPlexInline;
                            break;
                        case TextDiffToHtmlEnums.DisplayModeEnum.Compact:
                            var htmlDiffPlexCompact = "";
                            if (samples) htmlDiffPlexCompact = "<br>" + this.LbSample.Text +
                                    ": DiffPlex compact<br>\n";
                            htmlDiffPlexCompact += DiffPlexAPI.TextDiffCompact(left, right,
                                this.ChkIdenticalLines.Checked, this.ChkIdenticalParts.Checked,
                                this.ChkLineThrough.Checked, this.ChkMonospacedFont.Checked);
                            html += htmlDiffPlexCompact;
                            break;
                        case TextDiffToHtmlEnums.DisplayModeEnum.TrackChanges:
                            var htmlDiffPlexTC = "";
                            if (samples) htmlDiffPlexTC = "<br>" + this.LbSample.Text +
                                    ": DiffPlex (DiffMatchPatch) track changes<br>\n";
                            htmlDiffPlexTC += DiffPlexAPI.TextDiffTrackChanges(left, right);
                            html += htmlDiffPlexTC;
                            break;
                    }
                    break;

                case TextDiffToHtmlEnums.LibraryEnum.DiffLib:

                    switch (displayMode)
                    {
                        case TextDiffToHtmlEnums.DisplayModeEnum.SideBySide:
                            var htmlDiffLibSideBySide = "";
                            if (samples) htmlDiffLibSideBySide = "<br>" + this.LbSample.Text +
                                    ": DiffLib side by side:<br>\n";
                            htmlDiffLibSideBySide +=
                                DiffLibAPI.TextDiffSideBySideSplitByLine(left, right,
                                    this.ChkIdenticalLines.Checked, this.ChkCharLevel.Checked,
                                    this.ChkLineThrough.Checked, this.ChkMonospacedFont.Checked);
                            html += htmlDiffLibSideBySide;
                            break;
                        case TextDiffToHtmlEnums.DisplayModeEnum.Inline:
                            var htmlDiffLibInline = "";
                            if (samples) htmlDiffLibInline = "<br>" + this.LbSample.Text +
                                    ": DiffLib inline<br>\n";
                            htmlDiffLibInline += DiffLibAPI.TextDiffInline(left, right,
                                this.ChkIdenticalLines.Checked, this.ChkIdenticalParts.Checked,
                                this.ChkMonospacedFont.Checked);
                            html += htmlDiffLibInline;
                            break;
                        case TextDiffToHtmlEnums.DisplayModeEnum.Compact:
                            var htmlDiffLibCompact = "";
                            if (samples) htmlDiffLibCompact = "<br> " + this.LbSample.Text +
                                    ": DiffLib compact<br>\n";
                            htmlDiffLibCompact += DiffLibAPI.TextDiffCompactSplitByLine(left, right,
                                this.ChkIdenticalLines.Checked, /* this.ChkIdenticalParts.Checked, */
                                this.ChkLineThrough.Checked, this.ChkMonospacedFont.Checked);
                            html += htmlDiffLibCompact;
                            break;
                        case TextDiffToHtmlEnums.DisplayModeEnum.TrackChanges:
                            this.CmdCancel.Enabled = true;
                            var htmlDiffLibTC = "";
                            if (samples) htmlDiffLibTC = "<br>" + this.LbSample.Text +
                                    ": DiffLib track changes<br>\n";
                            htmlDiffLibTC += DiffLibAPI.TextDiffTrackChangesSplitByChar(left, right,
                                this.ChkIdenticalParts.Checked, this.ChkLineThrough.Checked,
                                this.ChkMonospacedFont.Checked,
                                this.htmlRenderer, prm.AverageLength);
                            html += htmlDiffLibTC;
                            this.CmdCancel.Enabled = false;
                            this.htmlRenderer.Init();
                            UpdateTitle();
                            break;
                    }
                    break;

                case TextDiffToHtmlEnums.LibraryEnum.TextDiffSharp:
                    switch (displayMode)
                    {
                        case TextDiffToHtmlEnums.DisplayModeEnum.SideBySide:
                            var htmlTextDiffSharpSideBySide = "";
                            if (samples) htmlTextDiffSharpSideBySide = "<br>" + this.LbSample.Text +
                                    ": TextDiff.Sharp side by side<br>\n";
                            htmlTextDiffSharpSideBySide += TextDiffSharpAPI.TextDiffSideBySide(left, right,
                                this.ChkIdenticalLines.Checked,
                                this.ChkMonospacedFont.Checked);
                            html += htmlTextDiffSharpSideBySide;
                            break;

                        case TextDiffToHtmlEnums.DisplayModeEnum.Inline:
                            var htmlTextDiffSharpInline = "";
                            if (samples) htmlTextDiffSharpInline = "<br>" + this.LbSample.Text +
                                    ": TextDiff.Sharp inline<br>\n";
                            htmlTextDiffSharpInline += TextDiffSharpAPI.TextDiffInline(left, right,
                                this.ChkIdenticalLines.Checked,
                                this.ChkIdenticalParts.Checked,
                                this.ChkMonospacedFont.Checked);
                            html += htmlTextDiffSharpInline;
                            break;

                        case TextDiffToHtmlEnums.DisplayModeEnum.Compact:
                            var htmlTextDiffSharpCompact = "";
                            if (samples) htmlTextDiffSharpCompact = "<br>" + this.LbSample.Text +
                                    ": TextDiff.Sharp compact<br>\n";
                            htmlTextDiffSharpCompact += TextDiffSharpAPI.TextDiffCompact(left, right,
                                this.ChkIdenticalLines.Checked,
                                this.ChkIdenticalParts.Checked,
                                this.ChkLineThrough.Checked,
                                this.ChkMonospacedFont.Checked);
                            html += htmlTextDiffSharpCompact;
                            break;

                        case TextDiffToHtmlEnums.DisplayModeEnum.TrackChanges:
                            this.CmdCancel.Enabled = true;
                            var htmlTextDiffSharpTC = "";
                            if (samples) htmlTextDiffSharpTC = "<br>" + this.LbSample.Text +
                                    ": TextDiff.Sharp track changes<br>\n";
                            htmlTextDiffSharpTC += TextDiffSharpAPI.TextDiffTrackChanges(left, right,
                                this.ChkIdenticalLines.Checked,
                                this.ChkLineThrough.Checked,
                                this.ChkMonospacedFont.Checked);
                            html += htmlTextDiffSharpTC;
                            this.CmdCancel.Enabled = false;
                            this.htmlRenderer.Init();
                            UpdateTitle();
                            break;
                    }
                    break;

                case TextDiffToHtmlEnums.LibraryEnum.CSharpDiff:
                    switch (displayMode)
                    {
                        case TextDiffToHtmlEnums.DisplayModeEnum.SideBySide:
                            var htmlCSharpDiffSideBySide = "";
                            if (samples) htmlCSharpDiffSideBySide = "<br>" + this.LbSample.Text +
                                    ": CSharpDiff side by side<br>\n";
                            htmlCSharpDiffSideBySide += CSharpDiffAPI.TextDiffSideBySide(left, right,
                                this.ChkIdenticalLines.Checked,
                                this.ChkMonospacedFont.Checked);
                            html += htmlCSharpDiffSideBySide;
                            break;

                        case TextDiffToHtmlEnums.DisplayModeEnum.Inline:
                            var htmlCSharpDiffInline = "";
                            if (samples) htmlCSharpDiffInline = "<br>" + this.LbSample.Text +
                                    ": CSharpDiff inline<br>\n";
                            htmlCSharpDiffInline += CSharpDiffAPI.TextDiffInline(left, right,
                                this.ChkIdenticalLines.Checked,
                                this.ChkIdenticalParts.Checked,
                                this.ChkMonospacedFont.Checked);
                            html += htmlCSharpDiffInline;
                            break;

                        case TextDiffToHtmlEnums.DisplayModeEnum.Compact:
                            var htmlCSharpDiffCompact = "";
                            if (samples) htmlCSharpDiffCompact = "<br>" + this.LbSample.Text +
                                    ": CSharpDiff compact<br>\n";
                            htmlCSharpDiffCompact += CSharpDiffAPI.TextDiffCompact(left, right,
                                this.ChkIdenticalLines.Checked,
                                this.ChkIdenticalParts.Checked,
                                this.ChkLineThrough.Checked,
                                this.ChkMonospacedFont.Checked);
                            html += htmlCSharpDiffCompact;
                            break;

                        case TextDiffToHtmlEnums.DisplayModeEnum.TrackChanges:
                            var htmlCSharpDiffTC = "";
                            if (samples) htmlCSharpDiffTC = "<br>" + this.LbSample.Text +
                                    ": CSharpDiff track changes<br>\n";
                            htmlCSharpDiffTC += CSharpDiffAPI.TextDiffTrackChanges(left, right,
                                this.ChkIdenticalLines.Checked,
                                this.ChkLineThrough.Checked,
                                this.ChkMonospacedFont.Checked);
                            html += htmlCSharpDiffTC;
                            break;
                    }
                    break;
            }
            html += Const.htmlEnd;
            return html;
        }
        
        private void CheckShortcut()
        {
            bool exists = File.Exists(_shortcutPath);
            CmdAddShortcut.Enabled = !exists;
            CmdRemoveShortcut.Enabled = exists;
        }

        private void CmdAddShortcut_Click(object sender, EventArgs e)
        {
            string link = _shortcutPath;
            string target = Application.StartupPath + "\\" + ExeTextDiffToHtml;
            Shortcut.Helper.ShortcutHelper.CreateShortcut(ref link, ref target);
            CheckShortcut();
        }

        private void CmdRemoveShortcut_Click(object sender, EventArgs e)
        {
            if (!File.Exists(_shortcutPath)) return;
            File.Delete(_shortcutPath);
            CheckShortcut();
        }
    }
}
