
// https://github.com/thomashambach/csharpdiff
// https://www.nuget.org/packages/CSharpDiff

using CSharpDiff.Diffs;
using CSharpDiff.Diffs.Models;
using System.Text;

namespace TextDiffToHtml.TextDiffAPI
{
    internal class CSharpDiffAPI
    {
        // Sample from jsdiff / CSharpDiff README
        const string LeftLineSample_ =
@"Here im.
Rock you like old man.
Yeah";

        const string RightLineSample_ =
@"Here im.
Rock you like hurricane.
Yeah";
        public const string LeftSentenceSample = 
            "Here im. Rock you like old man.";
        public const string RightSentenceSample =
            "Here im. Rock you like hurricane.";

        public static string LeftLineSample =
            LeftLineSample_.Replace(Const.newlineCRLF, Const.newline);
        public static string RightLineSample =
            RightLineSample_.Replace(Const.newlineCRLF, Const.newline);

        /// <summary>
        /// Returns the list of DiffResult from a line-level diff
        /// Each DiffResult has:
        ///   added   == true → inserted line(s)
        ///   removed == true → deleted line(s)
        ///   otherwise       → equal line(s)
        /// value includes the trailing newline character when present
        /// </summary>
        private static IList<DiffResult> GetLineDiff(string left, string right)
        {
            var diff = new DiffLines();
            return diff.diff(left, right);
        }

        /// <summary>
        /// Returns the char-level DiffResult list for two strings
        /// </summary>
        private static IList<DiffResult> GetCharDiff(string left, string right)
        {
            var diff = new Diff();
            return diff.diff(left, right);
        }

        /// <summary>
        /// Splits a sequence of DiffResults into individual line entries
        /// Each entry is (value without trailing newline, added, removed)
        /// Consecutive removed+added segments are paired as "modified"
        /// </summary>
        private static List<(string value, bool added, bool removed)> SplitToLines(
            IList<DiffResult> results)
        {
            var flat = new List<(string value, bool added, bool removed)>();
            foreach (var r in results)
            {
                // Value may contain several lines (count > 1); split them
                var lines = r.value?.Split(Const.newlineChar) ?? [];
                // The last element is empty when value ends with \n — drop it
                if (lines.Length > 0 && lines[^1] == string.Empty)
                    lines = lines[..^1];
                foreach (var line in lines)
                    flat.Add((line, r.added == true, r.removed == true));
            }
            return flat;
        }

        private class DiffLineData
        {
            public string Left { get; set; } = string.Empty;
            public string Right { get; set; } = string.Empty;
            public bool IsModified { get; set; }
            public bool IsAdded { get; set; }
            public bool IsRemoved { get; set; }
            public bool IsEqual { get; set; }
        }

        /// <summary>
        /// Pairs consecutive removed+added lines into modified pairs
        /// </summary>
        private static List<DiffLineData> PairLines(IList<DiffResult> results)
        {
            var lines = SplitToLines(results);
            var paired = new List<DiffLineData>();

            int i = 0;
            while (i < lines.Count)
            {
                var (value, added, removed) = lines[i];
                if (removed && i + 1 < lines.Count && lines[i + 1].added)
                {
                    // Modified pair
                    paired.Add(new DiffLineData
                    {
                        Left = value,
                        Right = lines[i + 1].value,
                        IsModified = true
                    });
                    i += 2;
                }
                else if (removed)
                {
                    paired.Add(new DiffLineData
                    {
                        Left = value,
                        Right = string.Empty,
                        IsRemoved = true
                    });
                    i++;
                }
                else if (added)
                {
                    paired.Add(new DiffLineData
                    {
                        Left = string.Empty,
                        Right = value,
                        IsAdded = true
                    });
                    i++;
                }
                else
                {
                    paired.Add(new DiffLineData
                    {
                        Left = value,
                        Right = value,
                        IsEqual = true
                    });
                    i++;
                }
            }
            return paired;
        }

        #region Side by side

        // Char level = true : GetLineDiff, BuildCharLevelHtml
        public static string TextDiffSideBySide(string left, string right,
            bool showIdenticalLines, bool monospacedFont = false)
        {
            var results = GetLineDiff(left, right);
            var paired = PairLines(results);

            // ToDo: char level diff: use green and red colors instead of yellow for modified lines
            var sb = Helper.GetSideBySideStyle(new StringBuilder(), colorizeDiff: true, monospacedFont);

            int numLeft = 1;
            int numRight = 1;

            foreach (DiffLineData p in paired)
            {
                if (p.IsEqual)
                {
                    if (!showIdenticalLines) { numLeft++; numRight++; continue; }
                    sb.AppendLine(
                        $"      <tr>" +
                        $"<td>{numLeft++}</td>" +
                        $"<td>{Helper.HtmlEncode(p.Left)}</td>" +
                        $"<td>==</td>" +
                        $"<td>{Helper.HtmlEncode(p.Right)}</td>" +
                        $"<td>{numRight++}</td></tr>");
                }
                else if (p.IsModified)
                {
                    var (leftHtml, rightHtml) = BuildCharLevelHtml(p.Left, p.Right);
                    sb.AppendLine(
                        $"      <tr>" +
                        $"<td>{numLeft++}</td>" +
                        $"<td>{leftHtml}</td>" +
                        $"<td>&lt;&gt;</td>" +
                        $"<td>{rightHtml}</td>" +
                        $"<td>{numRight++}</td></tr>");
                }
                else if (p.IsRemoved)
                {
                    sb.AppendLine(
                        $"      <tr>" +
                        $"<td>{numLeft++}</td>" +
                        $"<td><span class='diff'>{Helper.HtmlEncode(p.Left)}</span></td>" +
                        $"<td>&lt;&lt;</td>" +
                        $"<td></td>" +
                        $"<td></td></tr>");
                }
                else if (p.IsAdded)
                {
                    sb.AppendLine(
                        $"      <tr>" +
                        $"<td></td>" +
                        $"<td></td>" +
                        $"<td>&gt;&gt;</td>" +
                        $"<td><span class='diff'>{Helper.HtmlEncode(p.Right)}</span></td>" +
                        $"<td>{numRight++}</td></tr>");
                }
            }

            sb.AppendLine("  </table>");
            return sb.ToString();
        }

        /// <summary>
        /// Builds HTML for both sides of a modified line using char-level diff with yellow highlight
        /// </summary>
        private static (string leftHtml, string rightHtml) BuildCharLevelHtml(
            string leftLine, string rightLine)
        {
            var charResults = GetCharDiff(leftLine, rightLine);

            var leftSb = new StringBuilder();
            var rightSb = new StringBuilder();
            bool leftSpan = false;
            bool rightSpan = false;

            foreach (var cr in charResults)
            {
                if (cr.removed == true)
                {
                    if (!leftSpan) { leftSb.Append("<span style='background: yellow;'>"); leftSpan = true; }
                    leftSb.Append(Helper.HtmlEncode(cr.value));
                }
                else if (cr.added == true)
                {
                    if (!rightSpan) { rightSb.Append("<span style='background: yellow;'>"); rightSpan = true; }
                    rightSb.Append(Helper.HtmlEncode(cr.value));
                }
                else
                {
                    if (leftSpan) { leftSb.Append("</span>"); leftSpan = false; }
                    if (rightSpan) { rightSb.Append("</span>"); rightSpan = false; }
                    leftSb.Append(Helper.HtmlEncode(cr.value));
                    rightSb.Append(Helper.HtmlEncode(cr.value));
                }
            }

            if (leftSpan) leftSb.Append("</span>");
            if (rightSpan) rightSb.Append("</span>");

            return (leftSb.ToString(), rightSb.ToString());
        }

        #endregion

        #region Inline

        // Char level = true : GetLineDiff, BuildCharLevelHtmlOneColor
        public static string TextDiffInline(string left, string right,
            bool showIdenticalLines = true, bool showIdenticalParts = true,
            bool monospacedFont = true)
        {
            var results = GetLineDiff(left, right);
            var paired = PairLines(results);

            var sb = Helper.GetInlineStyle(new StringBuilder(), monospacedFont);

            int numLeft = 1;
            int numRight = 1;

            var red = HtmlColors.ToRgbString(Const.UpdateCharLevelLeft);
            var green = HtmlColors.ToRgbString(Const.UpdateCharLevelRight);

            foreach (DiffLineData p in paired)
            {
                if (p.IsEqual)
                {
                    if (!showIdenticalLines) { numLeft++; numRight++; continue; }
                    var record = new Record
                    {
                        L = numLeft++,
                        R = numRight++,
                        Delta = "",
                        Text = Helper.HtmlEncode(p.Left)
                    };
                    sb.AppendLine(Helper.GetHtmlTableRow(record));
                }
                else if (p.IsModified)
                {
                    var leftRecord = new Record
                    {
                        L = numLeft++,
                        R = null,
                        Delta = "-",
                        Text = BuildCharLevelHtmlOneColor(p.Left, p.Right, red, 
                            isLeft: true, showIdenticalParts)
                    };
                    sb.AppendLine(Helper.GetHtmlTableRow(leftRecord, cssClass: "redClass"));

                    var rightRecord = new Record
                    {
                        L = null,
                        R = numRight++,
                        Delta = "+",
                        Text = BuildCharLevelHtmlOneColor(p.Right, p.Left, green, 
                            isLeft: false, showIdenticalParts)
                    };
                    sb.AppendLine(Helper.GetHtmlTableRow(rightRecord, cssClass: "greenClass"));
                }
                else if (p.IsRemoved)
                {
                    var record = new Record
                    {
                        L = numLeft++,
                        R = null,
                        Delta = "-",
                        Text = Helper.HtmlEncode(p.Left)
                    };
                    sb.AppendLine(Helper.GetHtmlTableRow(record, cssClass: "redClass"));
                }
                else if (p.IsAdded)
                {
                    var record = new Record
                    {
                        L = null,
                        R = numRight++,
                        Delta = "+",
                        Text = Helper.HtmlEncode(p.Right)
                    };
                    sb.AppendLine(Helper.GetHtmlTableRow(record, cssClass: "greenClass"));
                }
            }

            sb.Append("</tbody></table>");
            return sb.ToString();
        }

        /// <summary>
        /// Builds HTML for one side of a modified line using char-level diff
        ///  with a specific background color
        /// </summary>
        private static string BuildCharLevelHtmlOneColor(
            string thisLine, string otherLine, string rgbColor, bool isLeft, bool showIdenticalParts)
        {
            var charResults = isLeft
                ? GetCharDiff(thisLine, otherLine)
                : GetCharDiff(otherLine, thisLine);

            var sb = new StringBuilder();
            bool inSpan = false;

            foreach (var cr in charResults)
            {
                bool changed = isLeft ? cr.removed == true : cr.added == true;
                if (changed)
                {
                    if (!inSpan)
                    {
                        sb.Append($"<span style='background-color: rgb({rgbColor});'>");
                        inSpan = true;
                    }
                    sb.Append(Helper.HtmlEncode(cr.value));
                }
                else if (cr.removed != true && cr.added != true)
                {
                    if (inSpan) { sb.Append("</span>"); inSpan = false; }
                    if (showIdenticalParts) sb.Append(Helper.HtmlEncode(cr.value));
                }
                // Skip the opposite-side changes
            }

            if (inSpan) sb.Append("</span>");
            return sb.ToString();
        }

        #endregion

        #region Compact

        // Char level = true : GetLineDiff, GetCharDiff
        public static string TextDiffCompact(string left, string right,
            bool showIdenticalLines = true, bool showIdenticalParts = true,
            bool linethrough = true, bool monospacedFont = true)
        {
            var results = GetLineDiff(left, right);
            var paired = PairLines(results);

            var html = new StringBuilder();
            if (monospacedFont) html.Append($"<div style='font-family: {Const.htmlMonospacedFont};'>");

            var insertBkGrndColor = HtmlColors.ToHexString(Const.InsertBkGrndColor);
            var deleteBkGrndColor = HtmlColors.ToHexString(Const.DeleteBkGrndColor);
            var insertColorCharLevel = HtmlColors.ToHexString(Const.InsertColorCharLevel);
            var deleteColorCharLevel = HtmlColors.ToHexString(Const.DeleteColorCharLevel);

            var underlineStyle = " text-decoration: underline;";
            var linethroughStyle = " text-decoration: line-through;";
            if (!linethrough) { linethroughStyle = ""; underlineStyle = ""; }

            foreach (DiffLineData p in paired)
            {
                if (p.IsEqual)
                {
                    if (showIdenticalLines)
                        html.Append(
                            "<div>" +
                            Const.htmlSpace + Const.htmlSpace +
                            Helper.HtmlEncode(p.Left) +
                            "</div>");
                }
                else if (p.IsAdded)
                {
                    html.Append(
                        "<div style='background-color: " + insertBkGrndColor + "'>+" +
                        Const.htmlSpace +
                        Helper.HtmlEncode(p.Right) +
                        "</div>");
                }
                else if (p.IsRemoved)
                {
                    html.Append(
                        "<div style='background-color: " + deleteBkGrndColor + "'>-" +
                        Const.htmlSpace +
                        Helper.HtmlEncode(p.Left) +
                        "</div>");
                }
                else if (p.IsModified)
                {
                    html.Append("<div>*" + Const.htmlSpace);

                    var charResults = GetCharDiff(p.Left, p.Right);
                    foreach (var cr in charResults)
                    {
                        if (cr.removed == true)
                        {
                            // Look ahead for matching added
                            html.Append("<span style='background-color: " +
                                deleteColorCharLevel + ";" + linethroughStyle + "'>" +
                                Helper.HtmlEncode(cr.value) + "</span>");
                        }
                        else if (cr.added == true)
                        {
                            html.Append("<span style='background-color: " +
                                insertColorCharLevel + ";" + underlineStyle + "'>" +
                                Helper.HtmlEncode(cr.value) + "</span>");
                        }
                        else
                        {
                            if (showIdenticalParts)
                                html.Append(Helper.HtmlEncode(cr.value));
                        }
                    }

                    html.Append("</div>");
                }
            }

            if (monospacedFont) html.Append("</div>");
            return html.ToString();
        }

        #endregion

        #region Track changes

        // Char level = true : GetLineDiff, GetCharDiff
        public static string TextDiffTrackChanges(string left, string right,
            bool showIdenticalLines = true, bool linethrough = true,
            bool monospacedFont = false)
        {
            var results = GetLineDiff(left, right);
            var paired = PairLines(results);

            var html = new StringBuilder();
            if (monospacedFont) html.Append($"<div style='font-family: {Const.htmlMonospacedFont};'>");

            var underlineStyle = " text-decoration: underline;";
            var linethroughStyle = " text-decoration: line-through;";
            if (!linethrough) { linethroughStyle = ""; underlineStyle = ""; }

            var insertBkGrndColor = HtmlColors.ToHexString(Const.InsertBkGrndColorTC);
            var deleteBkGrndColor = HtmlColors.ToHexString(Const.DeleteBkGrndColorTC);

            foreach (DiffLineData p in paired)
            {
                if (p.IsEqual)
                {
                    if (showIdenticalLines)
                    {
                        html.Append(Helper.HtmlEncode(p.Left));
                        html.Append(Const.htmlNewline);
                    }
                }
                else if (p.IsAdded)
                {
                    html.Append(
                        "<span style='background-color: " + insertBkGrndColor + ";" +
                        underlineStyle + "'>" +
                        Helper.HtmlEncode(p.Right) +
                        "</span>");
                    html.Append(Const.htmlNewline);
                }
                else if (p.IsRemoved)
                {
                    html.Append(
                        "<span style='background-color: " + deleteBkGrndColor + ";" +
                        linethroughStyle + "'>" +
                        Helper.HtmlEncode(p.Left) +
                        "</span>");
                    html.Append(Const.htmlNewline);
                }
                else if (p.IsModified)
                {
                    var charResults = GetCharDiff(p.Left, p.Right);
                    foreach (var cr in charResults)
                    {
                        if (cr.removed == true)
                        {
                            html.Append(
                                "<span style='background-color: " + deleteBkGrndColor + ";" +
                                linethroughStyle + "'>" +
                                Helper.HtmlEncode(cr.value) +
                                "</span>");
                        }
                        else if (cr.added == true)
                        {
                            html.Append(
                                "<span style='background-color: " + insertBkGrndColor + ";" +
                                underlineStyle + "'>" +
                                Helper.HtmlEncode(cr.value) +
                                "</span>");
                        }
                        else
                        {
                            // ToDo: showIdenticalParts option can be added here if needed
                            html.Append(Helper.HtmlEncode(cr.value));
                        }
                    }
                    html.Append(Const.htmlNewline);
                }
            }

            if (monospacedFont) html.Append("</div>");
            return html.ToString();
        }

        #endregion
    }
}
