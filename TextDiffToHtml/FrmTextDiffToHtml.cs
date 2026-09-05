
using DiffLibLLM.Models;

using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using static TextDiffToHtml.TextDiffToHtmlEnums;
using TextDiffToHtml.TextDiffAPI;

// https://www.nuget.org/packages/Vereyon.Windows.WebBrowser
// https://github.com/Vereyon/WebBrowser
using Vereyon.Windows;

namespace TextDiffToHtml
{
    public partial class FrmTextDiffToHtml : Form
    {
        public Parameter prm = new();

        private readonly DiffLibLLM.HtmlRenderer htmlRenderer;

        readonly string title = "";
        private bool init = false;
        private string htmlResultFilePath = "";
        private IReadOnlyList<double> _renderThresholds = Array.Empty<double>();
        private bool _updatingRenderThresholdUi = false;

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

            LbModels.Items.Clear();

#pragma warning disable CS0162
            if (Const.debugTextDiffLMMModels)
            {
                LbModels.Items.Add(ModelEnum.AllMinilm.ToShortDescription());
                LbModels.Items.Add(ModelEnum.NomicEmbedText.ToShortDescription());
                LbModels.Items.Add(ModelEnum.MxbaiEmbedLarge.ToShortDescription());
                LbModels.Items.Add(ModelEnum.EmbeddingGemma.ToShortDescription());
                LbModels.Items.Add(ModelEnum.GraniteEmbedding278m.ToShortDescription());
                LbModels.Items.Add(ModelEnum.BgeM3.ToShortDescription());
                LbModels.Items.Add(ModelEnum.QllamaMultilingualE5Base.ToShortDescription());
                LbModels.Items.Add(ModelEnum.Qwen3Embedding06b.ToShortDescription());
                LbModels.Items.Add(ModelEnum.NomicEmbedTextV2Moe.ToShortDescription());
                LbModels.Items.Add(ModelEnum.ParaphraseMultilingual278m.ToShortDescription());
                LbModels.Items.Add(ModelEnum.ParaphraseMultilingual.ToShortDescription());
                LbModels.Items.Add(ModelEnum.ZylonaiMultilingualE5Large.ToShortDescription());
                LbModels.Items.Add(ModelEnum.SnowflakeArcticEmbed2.ToShortDescription());
                LbModels.Items.Add(ModelEnum.Qwen3Embedding.ToShortDescription());
            }
            else 
            {
                if (!Properties.Settings.Default.TextDiffLLMConfigured)
                {
                    // Example: TextDiffLLMConfigured : true, TextDiffLLMModels : "all-minilm;nomic-embed-text"
                    LbModels.Items.Add("[not configured]");
                    toolTip1.SetToolTip(LbModels, "TextDiffLLM is not configured in the settings: see TextDiffLLMConfigured and TextDiffLLMModels parameters in TextDiffToHtml.dll.config");
                }
                else
                {
                    var models = Properties.Settings.Default.TextDiffLLMModels.Split(
                        new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var model in models) LbModels.Items.Add(model);
                }
            }
#pragma warning restore CS0162

            LbModels.SelectedIndex = 0;

            LbLibrary.Items.Clear();
            LbLibrary.Items.Add(TextDiffToHtmlEnums.LibraryEnum.DiffPlex);
            LbLibrary.Items.Add(TextDiffToHtmlEnums.LibraryEnum.DiffLib);
            LbLibrary.Items.Add(TextDiffToHtmlEnums.LibraryEnum.TextDiffSharp);
            LbLibrary.Items.Add(TextDiffToHtmlEnums.LibraryEnum.CSharpDiff);
            LbLibrary.Items.Add(TextDiffToHtmlEnums.LibraryEnum.DiffLibLLM);
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

            htmlRenderer = new DiffLibLLM.HtmlRenderer() { OnPartialRender = RenderInWebBrowser };

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

            toolTip1.SetToolTip(tbGapPenalty,
                "Insertion/deletion penalty for semantic alignment (higher value forces line matching: experimental)");

            hScrollBarRender.SmallChange = 1;
            hScrollBarRender.LargeChange = 1;
            hScrollBarRender.Enabled = false;

            SemanticActivation(activation: false);
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

            bool configMode = false;
            if (this.LbSample.Visible) configMode = true;
            if (configMode)
            {
                CmdAddShortcut.Visible = true;
                CmdRemoveShortcut.Visible = true;
                CheckShortcut();
            }
            else
            {
                CmdAddShortcut.Visible = false;
                CmdRemoveShortcut.Visible = false;
            }

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

        private void LbModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            Render();
        }

        private void chkVectSamples_CheckedChanged(object sender, EventArgs e)
        {
            this.chkUpperCase.Enabled = false;
            if (this.chkVectSamples.Checked) this.chkUpperCase.Enabled = true;
            Render();
        }

        private void chkUpperCase_CheckedChanged(object sender, EventArgs e)
        {
            Render();
        }

        private void tbChunks_Validated(object sender, EventArgs e)
        {
            //Render();
        }

        //private bool _thresholdModifierByUser = false;
        private void tbInfThreshold_Validated(object sender, EventArgs e)
        {
            //_thresholdModifierByUser = true;
            //SyncScrollBarWithThresholdText();
            //Render();
        }

        private void tbGapPenalty_Validated(object sender, EventArgs e)
        {
            //Render();
        }
        
        private void SyncScrollBarWithThresholdText()
        {
            if (_renderThresholds.Count == 0) return;

            var threshold = ParseThreshold(tbInfThreshold.Text, (float)_renderThresholds[0]);
            var thresholdIndex = FindClosestThresholdIndex(threshold);
            //Debug.WriteLine($"Threshold: {threshold}, Index: {thresholdIndex}");
            UpdateRenderThresholdUi(_renderThresholds, thresholdIndex);
        }

        private void hScrollBarRender_ValueChanged(object sender, EventArgs e)
        {
            /*
            if (_thresholdModifierByUser) 
            { 
                Render(); 
                _thresholdModifierByUser = false; 
                return; 
            }
            */

            if (_updatingRenderThresholdUi) return;
            if (_renderThresholds.Count == 0) return;

            var index = Math.Clamp(hScrollBarRender.Value, 0, _renderThresholds.Count - 1);
            tbInfThreshold.Text = _renderThresholds[index].ToString("0.00", CultureInfo.CurrentCulture);
            //Debug.WriteLine("tbInfThreshold = " + tbInfThreshold.Text);

            Render();
        }

        private float ParseThreshold(string text, float defaultValue)
        {
            if (float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var threshold) ||
                float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out threshold))
            {
                return Math.Clamp(threshold, 0f, 1f);
            }

            return Math.Clamp(defaultValue, 0f, 1f);
        }

        private double ParseGapPenalty(string text, double defaultValue)
        {
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var gapPenalty) ||
                double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out gapPenalty))
            {
                return Math.Clamp(gapPenalty, 0d, 10d);
            }

            return Math.Clamp(defaultValue, 0d, 10d);
        }

        private int FindClosestThresholdIndex(float targetThreshold)
        {
            if (_renderThresholds.Count == 0) return 0;

            var bestIndex = 0;
            var bestDelta = double.MaxValue;
            for (var i = 0; i < _renderThresholds.Count; i++)
            {
                var delta = Math.Abs(_renderThresholds[i] - targetThreshold);
                if (delta < bestDelta)
                {
                    bestDelta = delta;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private void UpdateRenderThresholdUi(IReadOnlyList<double> thresholds, int selectedIndex)
        {
            _renderThresholds = thresholds;
            _updatingRenderThresholdUi = true;
            try
            {
                if (_renderThresholds.Count == 0)
                {
                    hScrollBarRender.Enabled = false;
                    hScrollBarRender.Minimum = 0;
                    hScrollBarRender.Maximum = 0;
                    hScrollBarRender.Value = 0;
                    return;
                }

                var index = Math.Clamp(selectedIndex, 0, _renderThresholds.Count - 1);
                hScrollBarRender.Enabled = true;
                hScrollBarRender.Minimum = 0;
                hScrollBarRender.Maximum = _renderThresholds.Count - 1;
                hScrollBarRender.SmallChange = 1;
                hScrollBarRender.LargeChange = 1;
                hScrollBarRender.Value = index;
                tbInfThreshold.Text = _renderThresholds[index].ToString("0.00", CultureInfo.CurrentCulture);
            }
            finally
            {
                _updatingRenderThresholdUi = false;
            }
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

            LongOperation(activation: true);

            var libraryText = this.LbLibrary.Text;
            var library = TextDiffToHtmlEnums.LibraryFromValue(libraryText);
            var displayModeText = this.LbDisplayMode.Text;
            //var displayMode = TextDiffToHtmlEnums.DisplayModeFromValue(displayModeText);
            var displayMode = TextDiffToHtmlEnums.DisplayModeFromDescription(displayModeText);

            // Only one display mode is available for semantic diff, so disable the display mode selection
            var semanticDiff = false;
            var lib = this.LbLibrary.Text;
            if (lib == TextDiffToHtmlEnums.LibraryEnum.DiffLibLLM.ToString()) semanticDiff = true;
            this.LbDisplayMode.Enabled = !semanticDiff;

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

                case TextDiffToHtmlEnums.LibraryEnum.DiffLibLLM:
                    this.ChkIdenticalLines.Enabled = true;
                    this.ChkMonospacedFont.Enabled = true;
                    this.ChkIdenticalParts.Checked = true;
                    this.ChkLineThrough.Checked = false;
                    this.ChkCharLevel.Checked = false;

                    if (displayMode != TextDiffToHtmlEnums.DisplayModeEnum.SideBySide)
                    {
                        this.LbDisplayMode.Text = TextDiffToHtmlEnums.DisplayModeEnum.SideBySide.ToShortDescription();
                    }
                    break;
            }
            activation = false;
        }

        private void SemanticActivation(bool activation)
        {
            this.chkVectSamples.Enabled = activation;
            //this.chkUpperCase.Enabled = activation;
            this.chkUpperCase.Enabled = false;
            if (activation && this.chkVectSamples.Checked) this.chkUpperCase.Enabled = true;

            this.tbInfThreshold.Enabled = activation;
            this.tbChunks.Enabled = activation;
            this.tbGapPenalty.Enabled = activation;
            //this.LbDisplayMode.Enabled = activation;
            this.LbModels.Enabled = activation;
            this.hScrollBarRender.Enabled = activation;

            // Only one display mode is available for semantic diff, so disable the display mode selection
            var semanticDiff = false;
            var lib = this.LbLibrary.Text;
            if (lib == TextDiffToHtmlEnums.LibraryEnum.DiffLibLLM.ToString()) semanticDiff = true;
            if (semanticDiff) 
            { 
                this.LbDisplayMode.Enabled = false;
                tabControlLibraryType.SelectedTab = tabPageSemanticDiff;
            }
            else
            {
                this.LbDisplayMode.Enabled = true;
                tabControlLibraryType.SelectedTab = tabPageTextDiff;
            }
        }

        private void LongOperation(bool activation = false)
        {
            this.CmdCancel.Enabled = !activation;

            this.ChkMonospacedFont.Enabled = activation;
            this.ChkIdenticalLines.Enabled = activation;
            this.ChkIdenticalParts.Enabled = activation;
            this.ChkLineThrough.Enabled = activation;
            this.ChkCharLevel.Enabled = activation;
            this.ChkSwapLeftRight.Enabled = activation;
            this.LbLibrary.Enabled = activation;
            this.LbDisplayMode.Enabled = activation;
            this.LbSample.Enabled = activation;
            this.LbModels.Enabled = activation;
            this.chkUpperCase.Enabled = activation;
            this.chkVectSamples.Enabled = activation;
            this.tbChunks.Enabled = activation;
            this.tbInfThreshold.Enabled = activation;
            this.tbGapPenalty.Enabled = activation;
            this.hScrollBarRender.Enabled = activation && _renderThresholds.Count > 0;

            if (!activation)
            {
                this.CmdAddShortcut.Enabled = false;
                this.CmdRemoveShortcut.Enabled = false;
            }
            else
            {
                this.CmdAddShortcut.Enabled = !_shortcutExists;
                this.CmdRemoveShortcut.Enabled = _shortcutExists;
            }
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
            webBrowser.Refresh(); // Required, otherwise, there will be a one-tick delay from this.Text update
            //var fileUri = new Uri(this.htmlResultFilePath);
            //var refreshUri = new Uri($"{fileUri.AbsoluteUri}?v={Environment.TickCount64}");
            //webBrowser.Url = refreshUri;
            //Debug.WriteLine("ShowInInternalBrowser : " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + " : " + html);
            //Application.DoEvents(); // One tick late from this.Text refresh ?
        }

        private void RenderInWebBrowser(string text)
        {
            if (this.htmlRenderer.line == 0) this.Text = this.title + "...";
            else
            {
                ShowInInternalBrowser(text);
                this.Text = this.title + " : " + this.htmlRenderer.status +
                    this.htmlRenderer.line + "/" +
                    this.htmlRenderer.lines + " : " +
                    this.htmlRenderer.progress.ToString("0.00") + " %";
                //Debug.WriteLine("RenderInWebBrowser : " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + " : " + text);
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

            var semanticDiff = false;
            var lib = this.LbLibrary.Text;
            if (lib == TextDiffToHtmlEnums.LibraryEnum.DiffLibLLM.ToString()) semanticDiff = true;

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

                if (semanticDiff)
                {
                    SemanticActivation(activation: true);

                    switch (sample)
                    {
                        case "Sample 1":
                            left = DiffLibLLMAPI.DiffLibLLMLeftSample1;
                            right = DiffLibLLMAPI.DiffLibLLMRightSample1;
                            break;

                        case "Sample 2":
                            left = DiffLibLLMAPI.DiffLibLLMLeftSample2;
                            right = DiffLibLLMAPI.DiffLibLLMRightSample2;
                            break;

                        case "Sample 3":
                            left = DiffLibLLMAPI.DiffLibLLMLeftSample3;
                            right = DiffLibLLMAPI.DiffLibLLMRightSample3;
                            break;

                        case "Sample 4":
                            left = DiffLibLLMAPI.DiffLibLLMLeftSample4;
                            right = DiffLibLLMAPI.DiffLibLLMRightSample4;
                            break;

                        case "Sample 5":
                            left = DiffLibLLMAPI.DiffLibLLMLeftSample5;
                            right = DiffLibLLMAPI.DiffLibLLMRightSample5;
                            break;
                    }
                }
                else
                {
                    SemanticActivation(activation: false);
                }

                if (this.ChkSwapLeftRight.Checked)
                {
                    // Swap left and right texts
                    var tmp = left;
                    left = right;
                    right = tmp;
                }

                if (semanticDiff)
                    htmlSample +=
                        "<p>" + this.LbSample.Text + ":</p>" + Const.newline
                        + "<p>Left sample:</p>" + Const.newline
                        + "<p>" + left.Replace(Const.newline, Const.htmlNewline) + "</p>" + Const.newline
                        + "<p>Right sample:</p>" + Const.newline
                        + "<p>" + right.Replace(Const.newline, Const.htmlNewline) + "</p>";
                else
                    htmlSample +=
                        "<p>" + this.LbSample.Text + ":</p>" + Const.newline
                        + "<p>" + left.Replace(Const.newline, Const.htmlNewline) + "</p>" + Const.newline
                        + "<p>" + right.Replace(Const.newline, Const.htmlNewline) + "</p>";
            }

            var html = htmlSample;
            if (semanticDiff) html = "";

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

                case TextDiffToHtmlEnums.LibraryEnum.DiffLibLLM:

                    if (!Const.debugTextDiffLMMModels && 
                        !Properties.Settings.Default.TextDiffLLMConfigured) 
                    {
                        html += "<br><b>DiffLibLLM is not configured. Please configure it in the settings.</b><br>";
                        html += "<br>To configure DiffLibLLM:<br>";
                        html += "<br>1°) Download and install Ollama<br>";
                        html += "<br>2°) Download some Ollama embedding models: Ollama pull all-minilm, Ollama pull nomic-embed-text...<br>";
                        html += "<br>3°) Configure TextDiffToHtml.dll.config with them: TextDiffLLMModels: all-minilm;nomic-embed-text<br>";
                        break;
                    }

                    LongOperation();
                    var htmlDiffLibLLMSideBySide = "";
                    if (samples) htmlDiffLibLLMSideBySide = "<br>" + this.LbSample.Text +
                            ": DiffLibLLM side by side<br>\n";

                    var modelName = LbModels.Text;
                    var upperCase = this.chkUpperCase.Checked;
                    var vectorizationTest = this.chkVectSamples.Checked;
                    if (vectorizationTest)
                    {
                        var result = DiffLibLLMAPI.TestVectorization(modelName, 
                            capitalizeFirstChar: upperCase);
                        htmlDiffLibLLMSideBySide += DiffLibLLMAPI.GetMetaData(modelName);
                        htmlDiffLibLLMSideBySide += result;
                    }
                    else
                    {
                        var maxChunkLength = int.Parse(tbChunks.Text);
                        const float semanticInferiorThresholdDefault = 0.9f;
                        var infThreshold = ParseThreshold(tbInfThreshold.Text, 
                            semanticInferiorThresholdDefault);
                        // This is experimental:
                        var gapPenalty = ParseGapPenalty(tbGapPenalty.Text, 0.25);

                        var renderResult = DiffLibLLMAPI.RenderTextDiffSideBySide(left, right,
                            modelName, maxChunkLength, infThreshold,
                            gapPenalty,
                            this.ChkIdenticalLines.Checked,
                            this.ChkMonospacedFont.Checked, this.htmlRenderer);
                        var isModified = renderResult.Modified;

                        if (isModified) 
                        {
                            SyncScrollBarWithThresholdText();
                            var thresholdIndex = renderResult.SimilarityThresholds.Count == 0
                                ? 0
                                : renderResult.SimilarityThresholds
                                    .Select((value, index) => new { 
                                        value, index, delta = Math.Abs(value - infThreshold) })
                                    .OrderBy(x => x.delta)
                                    .ThenBy(x => x.index)
                                    .Select(x => x.index)
                                    .FirstOrDefault();
                            UpdateRenderThresholdUi(renderResult.SimilarityThresholds, thresholdIndex);
                            //_thresholdModifierByUser = false; // Reset the flag after updating the UI
                        }

                        htmlDiffLibLLMSideBySide += renderResult.Html;
                        bool cancelled = renderResult.Cancelled;
                        if (samples && !cancelled) htmlDiffLibLLMSideBySide += htmlSample;
                    }

                    html += htmlDiffLibLLMSideBySide;
                    Activation();
                    this.htmlRenderer.Init();
                    UpdateTitle();
                    break;
            }
            html += Const.htmlEnd;
            return html;
        }

        private bool _shortcutExists = false;
        private void CheckShortcut()
        {
            bool exists = File.Exists(_shortcutPath);
            _shortcutExists = exists;
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
