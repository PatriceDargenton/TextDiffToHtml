
using System.Diagnostics;
using System.Text;
using static TextDiffToHtml.TextDiffToHtmlEnums;

namespace TextDiffToHtml
{
    public static class Program
    {
        /// <summary>
        ///  The main entry point for the application
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var prm = new Parameter();
            var bOk = false;
            string displayModeArg; 
            ShowIdenticalLinesEnum showIdenticalLinesEnum;

            string showIdenticalLinesArg;
            var filePathLeft = "";
            var filePathRight = "";
            if (args != null)
            {
                var nbArgs = args.Length;
                switch (nbArgs)
                {
                    case 2:
                        filePathLeft = args[0];
                        filePathRight = args[1];
                        bOk = true;
                        break;

                    case 3:
                        displayModeArg = args[0];
                        filePathLeft = args[1];
                        filePathRight = args[2];
                        prm.DisplayMode = TextDiffToHtmlEnums.DisplayModeFromValue(displayModeArg);
                        bOk = true;
                        break;
                    
                    case 4:
                        showIdenticalLinesArg = args[1];
                        showIdenticalLinesEnum = TextDiffToHtmlEnums.ShowHideIdenticalLinesFromValue(showIdenticalLinesArg);
                        if (showIdenticalLinesEnum == TextDiffToHtmlEnums.ShowIdenticalLinesEnum.HideIdenticalLines)
                            prm.ShowIdenticalLines = false;
                        displayModeArg = args[1];
                        filePathLeft = args[2];
                        filePathRight = args[3];
                        prm.DisplayMode = TextDiffToHtmlEnums.DisplayModeFromValue(displayModeArg);
                        bOk = true;
                        break;
                    
                    case 5:
                        var libraryArg = args[0];
                        prm.Library = TextDiffToHtmlEnums.LibraryFromValue(libraryArg);
                        showIdenticalLinesArg = args[1];
                        showIdenticalLinesEnum = TextDiffToHtmlEnums.ShowHideIdenticalLinesFromValue(showIdenticalLinesArg);
                        if (showIdenticalLinesEnum == TextDiffToHtmlEnums.ShowIdenticalLinesEnum.HideIdenticalLines)
                            prm.ShowIdenticalLines = false;
                        displayModeArg = args[2];
                        filePathLeft = args[3];
                        filePathRight = args[4];
                        prm.DisplayMode = TextDiffToHtmlEnums.DisplayModeFromValue(displayModeArg);
                        bOk = true;
                        break;
                }
            }

            if (!bOk)
            {
                var dr = MessageBox.Show(
                    "Syntax:\n" +
                    "arg. n°1: Left file path\n" +
                    "arg. n°2: Right file path\n" +
                    "(more arguments available, see README.md documentation)\n" +
                    "Do you want to see all samples?", Const.appTitle,
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (dr == DialogResult.OK)
                {
                    Demo();
                    return;
                }

                var frm2 = new FrmTextDiffToHtml();
                Application.Run(frm2);
                return;
            }

            if (!File.Exists(filePathLeft))
            {
                MessageBox.Show(
                    "Can't find:"+ Path.GetFileName(filePathLeft) + "\n" + filePathLeft,
                    Const.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!File.Exists(filePathRight))
            {
                MessageBox.Show(
                    "Can't find:" + Path.GetFileName(filePathRight) + "\n" + filePathRight,
                    Const.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            // To fix: "No data is available for encoding 1252"
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var encod = Encoding.GetEncoding(Const.pageCode);
            var left = File.ReadAllText(filePathLeft, encod);
            var right = File.ReadAllText(filePathRight, encod);
            var fiLeft = new FileInfo(filePathLeft);
            var fiRight = new FileInfo(filePathLeft);
            var lenLeft = fiLeft.Length;
            var lenRight = fiRight.Length;
            var averageLength  = (lenLeft + lenRight)/2;
            averageLength = averageLength / 1024; // Kb

            if (Properties.Settings.Default.PreviewHtml) 
            { 
                var frm = new FrmTextDiffToHtml { prm = prm };
                frm.prm.LeftText = left;
                frm.prm.RightText = right;
                frm.prm.averageLength = averageLength;
                Application.Run(frm);
            }
            else 
            {
                // Skip first blank lines
                left = left.TrimStart();
                right = right.TrimStart();

                var LibraryName = prm.Library.ToString();
                string path = AppContext.BaseDirectory; // Application.StartupPath() equivalent in .Net9;
                string filePath = "";
                switch (prm.DisplayMode)
                {
                    case TextDiffToHtmlEnums.DisplayModeEnum.SideBySide:
                        filePath = Path.Combine(path, LibraryName + Const.sideBySideFile);
                        break;
                    case TextDiffToHtmlEnums.DisplayModeEnum.Inline:
                        filePath = Path.Combine(path, LibraryName + Const.inlineFile);
                        break;
                    case TextDiffToHtmlEnums.DisplayModeEnum.Compact:
                        filePath = Path.Combine(path, LibraryName + Const.compactFile);
                        break;
                    case TextDiffToHtmlEnums.DisplayModeEnum.TrackChanges:
                        filePath = Path.Combine(path, LibraryName + Const.trackChanges);
                        break;
                }

                string html = "";
                if (prm.Library == TextDiffToHtmlEnums.LibraryEnum.DiffPlex)
                {
                    if (prm.DisplayMode == TextDiffToHtmlEnums.DisplayModeEnum.SideBySide)
                    {
                        html = DiffPlexAPI.TextDiffSideBySide(left, right, prm.ShowIdenticalLines);
                    }
                    else if (prm.DisplayMode == TextDiffToHtmlEnums.DisplayModeEnum.Inline)
                    {
                        html = DiffPlexAPI.TextDiffInline(left, right);
                    }
                    else if (prm.DisplayMode == TextDiffToHtmlEnums.DisplayModeEnum.Compact)
                    {
                        html = DiffPlexAPI.TextDiffCompact(left, right, prm.ShowIdenticalLines);
                    }
                    else if (prm.DisplayMode == TextDiffToHtmlEnums.DisplayModeEnum.TrackChanges)
                    {
                        html = DiffPlexAPI.TextDiffTrackChanges(left, right);
                    }
                }
                else if(prm.Library == TextDiffToHtmlEnums.LibraryEnum.DiffLib)
                {
                    if (prm.DisplayMode == TextDiffToHtmlEnums.DisplayModeEnum.SideBySide)
                    {
                        html = DiffLibAPI.TextDiffSideBySideSplitByLine(left, right, prm.ShowIdenticalLines);
                    }
                    else if (prm.DisplayMode == TextDiffToHtmlEnums.DisplayModeEnum.Inline)
                    {
                        html = DiffLibAPI.TextDiffInline(left, right);
                    }
                    else if (prm.DisplayMode == TextDiffToHtmlEnums.DisplayModeEnum.Compact)
                    {
                        html = DiffLibAPI.TextDiffCompactSplitByLine(left, right, prm.ShowIdenticalLines);
                    }
                    else if(prm.DisplayMode == TextDiffToHtmlEnums.DisplayModeEnum.TrackChanges)
                    {
                        html = DiffLibAPI.TextDiffTrackChangesSplitByChar(left, right, 
                            averageLength: averageLength);
                    }
                }

                File.WriteAllText(filePath, html);
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
        }

        public static string Demo(bool showHtml = true)
        {
            // Sample 1: Aiikon's TextDiff Demo
            // https://github.com/Aiikon/TextDiff
            string left = DiffPlexAPI.AiikonLeftSample;
            string right = DiffPlexAPI.AiikonRightSample;
            string htmlSample1 =
                "<p>Sample 1:</p>"
                + "<p>" + left.Replace(Const.newline, Const.htmlNewline) + "</p>"
                + "<p>" + right.Replace(Const.newline, Const.htmlNewline) + "</p>";


            // Sample 2 & 3: Lassevk's DiffLib Demos
            // https://github.com/lassevk/DiffLib/tree/main/Examples

            // Sample 2: 000 - Basic diffing of two texts
            const string text1 = DiffLibAPI.LassevkLeftSample1;
            const string text2 = DiffLibAPI.LassevkRightSample1;
            string htmlSample2 =
                "<p>Sample 2:</p>"
                + "<p>" + text1 + "</p>"
                + "<p>" + text2 + "</p>";

            // Sample 3: 001 - Basic diffing of two text files
            string left3 = DiffLibAPI.LassevkLeftSample2;
            string right3 = DiffLibAPI.LassevkRightSample2;
            string htmlSample3 =
                "<p>Sample 3:</p>"
                + "<p>" + left3.Replace(Const.newline, Const.htmlNewline) + "</p>"
                + "<p>" + right3.Replace(Const.newline, Const.htmlNewline) + "</p>";


            var htmlS1DiffPlexSideBySide =
                "<br>Sample 1: DiffPlex side by side: Original DiffPlex sample from Aiikon<br>" +
                DiffPlexAPI.TextDiffSideBySide(left, right, showIdenticalLines: true);

            //var htmlS1DiffPlexSideBySideOnlyDiff =
            //    "<br>Sample 1: DiffPlex side by side (only the differences):<br>" +
            //    DiffPlexAPI.TextDiffSideBySide(left, right, showIdenticalLines: false);

            var htmlS1DiffLibSideBySide = "<br>Sample 1: DiffLib side by side:<br>" +
                DiffLibAPI.TextDiffSideBySideSplitByLine(left, right, showIdenticalLines: true);

            var htmlS1DiffLibSideBySideCharLevel =
                "<br>Sample 1: DiffLib side by side (char level):<br>" +
                DiffLibAPI.TextDiffSideBySideSplitByLine(left, right, 
                    showIdenticalLines: true, charLevel: true);

            //var htmlS1DiffLibSideBySideOnlyDiff =
            //    "<br>Sample 1: DiffLib side by side (only the differences):<br>" +
            //    DiffLibAPI.TextDiffSideBySideSplitByLine(left, right, showIdenticalLines: false);

            var htmlS3DiffPlexSideBySide = "<br>Sample 3: DiffPlex side by side: Aiikon yellow style<br>" +
                DiffPlexAPI.TextDiffSideBySide(left3, right3, 
                    showIdenticalLines: true);

            var htmlS3DiffLibSideBySide = "<br>Sample 3: DiffLib side by side: Aiikon yellow style<br>" +
                DiffLibAPI.TextDiffSideBySideSplitByLine(left3, right3, 
                    showIdenticalLines: true);

            var htmlS3DiffLibSideBySideCharLevel =
                "<br>Sample 3: DiffLib side by side (char level):<br>" +
                DiffLibAPI.TextDiffSideBySideSplitByLine(left3, right3, 
                    showIdenticalLines: true, charLevel: true);

            //var htmlS3DiffLibSideBySideDiffOnly =
            //    "<br>Sample 3: DiffLib side by side (only the differences):<br>" +
            //    DiffLibAPI.TextDiffSideBySideSplitByLine(left3, right3, showIdenticalLines: false);
            
            var htmlS3HtmlDiffLibSideBySideCharLevel =
                "<br>Sample 3: DiffLib side by side (char level):<br>" +
                DiffLibAPI.TextDiffSideBySideSplitByLine(left3, right3, 
                showIdenticalLines: true, charLevel: true);
            

            var htmlS1DiffPlexInline =
                "<br>Sample 1: DiffPlex inline: Second original DiffPlex sample from Aiikon<br>" +
                DiffPlexAPI.TextDiffInline(left, right);

            var htmlS1DiffLibInline = "<br>Sample 1: DiffLib inline:<br>" +
                DiffLibAPI.TextDiffInline(left, right);

            var htmlS3DiffPlexInline =
                "<br>Sample 3: DiffPlex inline:<br>" +
                DiffPlexAPI.TextDiffInline(left3, right3);

            var htmlS3DiffLibInline = "<br>Sample 3: DiffLib inline:<br>" +
                DiffLibAPI.TextDiffInline(left3, right3);


            var htmlS1DiffLibCompact =
                "<br>Sample 1: DiffLib compact:<br>" +
                DiffLibAPI.TextDiffCompactSplitByLine(left, right);

            var htmlS1DiffPlexCompact = "<br>Sample 1: DiffPlex compact:<br>" +
                DiffPlexAPI.TextDiffCompact(left, right);

            var htmlS2DiffPlexCompact =
                "<br>Sample 2: DiffPlex compact:<br>" +
                DiffPlexAPI.TextDiffCompact(text1, text2, showIdenticalLines: true); 

            var htmlS2DiffLibCompactTrackChanges =
                "<br>Sample 2: DiffLib compact :<br>" +
                DiffLibAPI.TextDiffCompactSplitByLine(text1, text2) + "<br>";


            var htmlS1DiffLibTrackChangesSplitByChar =
                "<br>Sample 1: DiffLib track changes:<br>" +
                DiffLibAPI.TextDiffTrackChangesSplitByChar(left, right) + "<br>";

            var htmlS1DiffMatchPatch =
                "<br>Sample 1: DiffMatchPatch track changes:<br>" +
                DiffMatchPatchAPI.TextDiffTrackChanges(left, right) + "<br>";

            var htmlS2DiffLibTrackChanges =
                "<br>Sample 2: DiffLib track changes: Original DiffLib 000 sample (Basic diffing of two texts)<br>" +
                DiffLibAPI.TextDiffTrackChangesSplitByChar(text1, text2) + "<br>";

            var htmlS2DiffMatchPatch =
                "<br>Sample 2: DiffMatchPatch track changes:<br>" +
                DiffMatchPatchAPI.TextDiffTrackChanges(text1, text2) + "<br>";

            var htmlS3DiffLibCompact =
                "<br>Sample 3: DiffLib compact: Original DiffLib 001 sample (Basic diffing of two text files)<br>" +
                DiffLibAPI.TextDiffCompactSplitByLine(left3, right3, 
                    showIdenticalLines: true, linethrough: true);

            var htmlS3DiffPlexCompact =
                "<br>Sample 3: DiffPlex compact: Not the same as the original DiffLib sample, but not bad:<br>" +
                DiffPlexAPI.TextDiffCompact(left3, right3, 
                    showIdenticalLines: true, linethrough: true);

            
            var htmlS3DiffLibTrackChangesSplitByChar =
                "<br>Sample 3: DiffLib track changes:<br>" +
                DiffLibAPI.TextDiffTrackChangesSplitByChar(left3, right3) + "<br>";

            var htmlS3DiffMatchPatch =
                "<br>Sample 3: DiffMatchPatch track changes:<br>" +
                DiffMatchPatchAPI.TextDiffTrackChanges(left3, right3) + "<br>";


            var htmlS1Demos = "<br><h1>Sample 1 Demos:</h1><br>"
                + htmlSample1
                + htmlS1DiffPlexSideBySide
                //+ htmlS1DiffPlexSideBySideOnlyDiff
                + htmlS1DiffLibSideBySide
                //+ htmlS1DiffLibSideBySideOnlyDiff
                + htmlS1DiffLibSideBySideCharLevel
                + htmlS1DiffPlexInline
                + htmlS1DiffLibInline
                + htmlS1DiffLibCompact
                + htmlS1DiffPlexCompact
                + htmlS1DiffLibTrackChangesSplitByChar
                + htmlS1DiffMatchPatch
                ;

            var htmlS2Demos = "<br><h1>Sample 2 Demos:</h1><br>"
                + htmlSample2
                + htmlS2DiffPlexCompact
                + htmlS2DiffLibTrackChanges
                + htmlS2DiffMatchPatch
                ;

            var htmlS3Demos = "<br><h1>Sample 3 Demos:</h1><br>"
                + htmlSample3
                + htmlS3DiffPlexSideBySide
                + htmlS3DiffLibSideBySide
                //+ htmlS3DiffLibSideBySideDiffOnly
                + htmlS3DiffLibSideBySideCharLevel
                + htmlS3HtmlDiffLibSideBySideCharLevel
                + htmlS3DiffPlexInline
                + htmlS3DiffLibInline
                + htmlS3DiffLibCompact
                + htmlS3DiffPlexCompact
                + htmlS3DiffLibTrackChangesSplitByChar
                + htmlS3DiffMatchPatch
                ;

            var htmlSideBySideDemos = "<br><h1>Side by side Demos:</h1><br>"
                + htmlS1DiffPlexSideBySide
                //+ htmlS1DiffPlexSideBySideOnlyDiff
                + htmlS1DiffLibSideBySide 
                //+ htmlS1DiffLibSideBySideOnlyDiff
                + htmlS1DiffLibSideBySideCharLevel
                + htmlS3DiffPlexSideBySide
                + htmlS3DiffLibSideBySide
                //+ htmlS3DiffLibSideBySideDiffOnly
                + htmlS3DiffLibSideBySideCharLevel
                ;

            var htmlInlineDemos = "<br><h1>Inline Demos:</h1><br>"
                + htmlS1DiffPlexInline
                + htmlS1DiffLibInline
                + htmlS3DiffPlexInline
                + htmlS3DiffLibInline
                ;

            var htmlCompactDemos = "<br><h1>Compact Demos:</h1><br>"
                + htmlS1DiffLibCompact
                + htmlS1DiffPlexCompact
                + htmlS3DiffLibCompact
                + htmlS3DiffPlexCompact
                + htmlS2DiffPlexCompact
                + htmlS2DiffLibCompactTrackChanges 
                ;

            var htmlTrackChangesDemos = "<br><h1>Track changes Demos:</h1><br>"
                + htmlSample1
                + htmlS1DiffLibTrackChangesSplitByChar
                + htmlS1DiffMatchPatch

                + htmlSample2
                + htmlS2DiffLibTrackChanges
                + htmlS2DiffPlexCompact
                + htmlS2DiffLibCompactTrackChanges
                + htmlS2DiffMatchPatch

                + htmlSample3
                + htmlS3DiffLibTrackChangesSplitByChar
                + htmlS3DiffMatchPatch
                ;

            var htmlDiffMatchPatchDemos = "<br><h1>DiffMatchPatch Demos:</h1><br>"
                + DiffMatchPatchAPI.Demos();

            var html = Const.htmlStart
                + htmlSideBySideDemos
                + htmlInlineDemos 
                + htmlCompactDemos 
                + htmlTrackChangesDemos
                + htmlS1Demos
                + htmlS2Demos
                + htmlS3Demos
                + htmlDiffMatchPatchDemos
                + Const.htmlEnd;

            if (showHtml) 
            { 
                string path = AppContext.BaseDirectory; // Application.StartupPath() equivalent in .Net8;
                string filePath = Path.Combine(path, Const.outputFilename);
                File.WriteAllText(filePath, html);
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }

            return html;
        }
    }
}