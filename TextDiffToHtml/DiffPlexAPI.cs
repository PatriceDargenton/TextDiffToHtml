
// https://github.com/Aiikon/TextDiff
// https://github.com/mmanela/diffplex

using System.Text;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace TextDiffToHtml
{
    internal class DiffPlexAPI
    {
        // Aiikon's TextDiff Demo
        const string AiikonLeftSample_ = 
@"ABC abc
DEF def
HIJ
KLM";

        const string AiikonRightSample_ =
@"ABC abc
DEF DEF
KLM
XYZ";

        public static string AiikonLeftSample = 
            AiikonLeftSample_.Replace(Const.newlineCRLF, Const.newline);
        public static string AiikonRightSample = 
            AiikonRightSample_.Replace(Const.newlineCRLF, Const.newline);

        const bool colorizeDiff = true;

        #region Side by side

        public static string TextDiffSideBySide(string left, string right, 
                bool showIdenticalLines, bool monospacedFont = false)
        {
            var differ = new Differ();
            var diffBuilder = new SideBySideDiffBuilder(differ);
            var diffModel = diffBuilder.BuildDiffModel(left, right);

            var sb = new StringBuilder();

            sb = Helper.GetSideBySideStyle(sb, colorizeDiff, monospacedFont);

            var deltaLookup = new Dictionary<ChangeType, string>
            {
                { ChangeType.Unchanged, "==" },
                { ChangeType.Modified, "<>" },
                { ChangeType.Imaginary, ">>" },
                { ChangeType.Deleted, "<<" }
            };

            int lineCount = Math.Max(diffModel.OldText.Lines.Count, diffModel.NewText.Lines.Count);
            for (int i = 0; i < lineCount; i++)
            {
                var oldLine = i < diffModel.OldText.Lines.Count ? 
                    diffModel.OldText.Lines[i] : new DiffPiece(string.Empty, ChangeType.Imaginary);
                var newLine = i < diffModel.NewText.Lines.Count ? 
                    diffModel.NewText.Lines[i] : new DiffPiece(string.Empty, ChangeType.Imaginary);

                string leftText = HtmlEncodeWithDiff(showIdenticalLines, oldLine, out bool identicalLeft);
                string rightText = HtmlEncodeWithDiff(showIdenticalLines, newLine, out bool identicalRight);
                if (identicalLeft && identicalRight && !showIdenticalLines) continue;
                string delta = deltaLookup[oldLine.Type];

                if (string.IsNullOrEmpty(oldLine.Text) && 
                    string.IsNullOrEmpty(newLine.Text))
                {
                    if (!showIdenticalLines) continue;
                }

                sb.AppendLine(
                    $"      <tr>" +
                    $"<td>{oldLine.Position}</td>" +
                    $"<td>{leftText}</td>" +
                    $"<td>{delta}</td>" +
                    $"<td>{rightText}</td>" +
                    $"<td>{newLine.Position}</td></tr>");
            }

            sb.AppendLine("  </table>");

            return sb.ToString();
        }

        static string HtmlEncodeWithDiff(bool showIdenticalLines, DiffPiece line, out bool identical)
        {
            identical = false;
            if (line.Type == ChangeType.Modified)
            {
                var sb = new StringBuilder();

                var lastStateChanged = false;
                foreach (var piece in line.SubPieces)
                {
                    var changed = (piece.Type != ChangeType.Unchanged);
                    if (colorizeDiff && changed != lastStateChanged)
                    {
                        sb.Append(changed ? "<span style='background: yellow;'>" : "</span>");
                        lastStateChanged = changed;
                    }
                    sb.Append(Helper.HtmlEncode(piece.Text));
                }

                return sb.ToString();
            }
            else if (line.Type == ChangeType.Deleted || 
                     line.Type == ChangeType.Imaginary || 
                     line.Type == ChangeType.Inserted)
            {
                return $"<span class='diff'>{Helper.HtmlEncode(line.Text)}</span>";
            }
            else
            {
                identical = true;
                if (!showIdenticalLines) return string.Empty;
                return Helper.HtmlEncode(line.Text);
            }
        }

        #endregion

        #region Inline
        
        private class DiffLineData
        {
            public int? L { get; set; } // Left line number
            public int? R { get; set; } // Right line number
            public string Delta { get; set; } = string.Empty;
            public string Left { get; set; } = string.Empty;
            public string Right { get; set; } = string.Empty;
            public required List<DiffPlex.DiffBuilder.Model.DiffPiece> LeftPieces { get; set; }
            public required List<DiffPlex.DiffBuilder.Model.DiffPiece> RightPieces { get; set; }
        }

        public static string TextDiffInline(string left, string right, 
            bool showIdenticalLines = true, bool showIdenticalParts = true, 
            bool monospacedFont = true)
        {
            var lineData = GetTextDiffInline(left, right);

            var sb = new StringBuilder();

            sb = Helper.GetInlineStyle(sb, monospacedFont);

            foreach (var set in lineData)
            {
                if (set.Delta == "==")
                {
                    var record = new TextDiffToHtml.Record
                    {
                        L = set.L,
                        R = set.R,
                        Delta = "",
                        Text = Helper.HtmlEncode(set.Left)
                    };
                    if (showIdenticalLines) sb.AppendLine(Helper.GetHtmlTableRow(record));
                }
                else if (set.Delta == "<<")
                {
                    var record = new TextDiffToHtml.Record
                    {
                        L = set.L,
                        R = null,
                        Delta = "-",
                        Text = Helper.HtmlEncode(set.Left)
                    };
                    sb.AppendLine(Helper.GetHtmlTableRow(record, cssClass: "redClass"));
                }
                else if (set.Delta == ">>")
                {
                    var record = new TextDiffToHtml.Record
                    {
                        L = null,
                        R = set.R,
                        Delta = "+",
                        Text = Helper.HtmlEncode(set.Right)
                    };
                    sb.AppendLine(Helper.GetHtmlTableRow(record, cssClass: "greenClass"));
                }
                else if (set.Delta == "<>")
                {
                    var red = HtmlColors.ToRgbString(Const.UpdateCharLevelLeft);
                    var leftRecord = new TextDiffToHtml.Record
                    {
                        L = set.L,
                        R = null,
                        Delta = "-",
                        Text = string.Join("", GetDiffPieces(set.LeftPieces, red, showIdenticalParts))
                    };
                    sb.AppendLine(Helper.GetHtmlTableRow(leftRecord, cssClass: "redClass"));

                    var green = HtmlColors.ToRgbString(Const.UpdateCharLevelRight);
                    var rightRecord = new TextDiffToHtml.Record
                    {
                        L = null,
                        R = set.R,
                        Delta = "+",
                        Text = string.Join("", GetDiffPieces(set.RightPieces, green, showIdenticalParts))
                    };
                    sb.AppendLine(Helper.GetHtmlTableRow(rightRecord, cssClass: "greenClass"));
                }
            }

            sb.Append("</tbody></table>");
            return sb.ToString();
        }

        private static List<string> GetDiffPieces(List<DiffPiece> pieces, 
            string bkGrndRGBColor, bool showIdenticalParts)
        {
            bool lastStateChanged = false;
            var result = new List<string>();

            foreach (var piece in pieces)
            {
                bool changed = piece.Type != ChangeType.Unchanged;
                if (changed != lastStateChanged)
                {
                    if (changed)
                        result.Add($"<span style='background-color: rgb({bkGrndRGBColor});'>");
                    else
                    {
                        result.Add("</span>");
                        if (!showIdenticalParts) result.Add(" ");
                    }
                    lastStateChanged = changed;
                }
                if (changed || showIdenticalParts) result.Add(Helper.HtmlEncode(piece.Text));
            }
            if (lastStateChanged)
            {
                result.Add("</span>");
            }
            return result;
        }

        private static List<DiffLineData> GetTextDiffInline(string left, string right)
        {
            var differ = new Differ();
            var diffBuilder = new SideBySideDiffBuilder(differ);
            var diffModel = diffBuilder.BuildDiffModel(left, right);

            var deltaLookup = new Dictionary<ChangeType, string>
            {
                { ChangeType.Unchanged, "==" },
                { ChangeType.Modified, "<>" },
                { ChangeType.Imaginary, ">>" },
                { ChangeType.Deleted, "<<" },
                { ChangeType.Inserted, ">>" }
            };

            var resultList = new List<DiffLineData>();

            int maxLines = Math.Max(
                diffModel.OldText.Lines.Count,
                diffModel.NewText.Lines.Count);

            for (int i = 0; i < maxLines; i++)
            {
                var oldLine = i < diffModel.OldText.Lines.Count ?
                    diffModel.OldText.Lines[i] : new DiffPiece(
                        string.Empty, ChangeType.Imaginary, position: i + 1);
                var newLine = i < diffModel.NewText.Lines.Count ?
                    diffModel.NewText.Lines[i] : new DiffPiece(
                        string.Empty, ChangeType.Imaginary, position: i + 1);

                var delta = deltaLookup[oldLine.Type];

                var result = new DiffLineData
                {
                    L = oldLine.Position,
                    Left = oldLine.Text,
                    Delta = delta,
                    Right = newLine.Text,
                    R = newLine.Position,
                    LeftPieces = oldLine.SubPieces,
                    RightPieces = newLine.SubPieces
                };
                resultList.Add(result);
            }

            return resultList;
        }

        #endregion

        #region Inline (compact)

        static public string TextDiffCompact(string left, string right,
            bool showIdenticalLines = true, bool showIdenticalParts = true, 
            bool linethrough = true, bool monospacedFont = true)
        {
            var differ = new SideBySideDiffBuilder(new Differ());
            var diffModel = differ.BuildDiffModel(left, right);
            return DumpDiffElementCompact(diffModel, 
                showIdenticalLines, showIdenticalParts, linethrough, monospacedFont); 
        }

        static string DumpDiffElementCompact(SideBySideDiffModel diffModel, 
            bool showIdenticalLines, bool showIdenticalParts, bool linethrough, bool monospacedFont) 
        {
            var html = new StringBuilder();
            if (monospacedFont) html.Append($"<div style='font-family: {Const.htmlMonospacedFont};'>");

            var insertBkGrndColor = HtmlColors.ToHexString(Const.InsertBkGrndColor);
            var deleteBkGrndColor = HtmlColors.ToHexString(Const.DeleteBkGrndColor);
            var insertColorCharLevel = HtmlColors.ToHexString(Const.InsertColorCharLevel);
            var deleteColorCharLevel = HtmlColors.ToHexString(Const.DeleteColorCharLevel);

            // Loop through each row compared
            int maxLines = Math.Max(diffModel.OldText.Lines.Count, diffModel.NewText.Lines.Count);
            for (int i = 0; i < maxLines; i++)
            {
                var leftLine = i < diffModel.OldText.Lines.Count ? diffModel.OldText.Lines[i] : null;
                var rightLine = i < diffModel.NewText.Lines.Count ? diffModel.NewText.Lines[i] : null;

                var underlineStyle = " text-decoration: underline;"; 
                var linethroughStyle = " text-decoration: line-through;";
                var linetroughStyleDeletedLine = ""; //linethroughStyle;
                var underlineStyleInsertedLine = ""; //underlineStyle;
                if (!linethrough) 
                { 
                    linethroughStyle = ""; underlineStyle = ""; 
                    linetroughStyleDeletedLine = "";
                    underlineStyleInsertedLine = "";
                }

                if (leftLine?.Type == ChangeType.Unchanged && 
                    rightLine?.Type == ChangeType.Unchanged)
                {
                    if (showIdenticalLines)
                    html.Append(
                        "<div>" +
                        Const.htmlSpace + Const.htmlSpace +
                        Helper.HtmlEncode(leftLine.Text) +
                        "</div>");
                }
                else if (rightLine?.Type == ChangeType.Inserted)
                {
                    html.Append(
                        "<div style='background-color: " + insertBkGrndColor + ";" + 
                        underlineStyleInsertedLine + "'>+" +
                        Const.htmlSpace +
                        Helper.HtmlEncode(rightLine.Text) +
                        "</div>");
                }
                else if (leftLine?.Type == ChangeType.Deleted)
                {
                    html.Append(
                        "<div style='background-color: " + deleteBkGrndColor + ";" + 
                        linetroughStyleDeletedLine + "'>-" +
                        Const.htmlSpace +
                        Helper.HtmlEncode(leftLine.Text) +
                        "</div>");
                }
                else if (leftLine?.Type == ChangeType.Modified || 
                         rightLine?.Type == ChangeType.Modified)
                {
                    html.Append("<div>*" + Const.htmlSpace);
                    int index = 0;

                    List<DiffPiece> leftLineSubPieces, rightLineSubPieces;
                    leftLineSubPieces = []; //new List<DiffPiece>();
                    rightLineSubPieces = [];
                    if (leftLine != null) leftLineSubPieces = leftLine.SubPieces;
                    if (rightLine != null) rightLineSubPieces = rightLine.SubPieces;

                    if (leftLine != null && rightLine != null)
                        foreach (var characterDiff in 
                            leftLineSubPieces.Zip(rightLineSubPieces, (l, r) => new { l, r }))
                        {
                            if (characterDiff.l.Text == characterDiff.r.Text)
                                //if (showIdenticalParts) // It is not possible to mask identical parts as it is
                                html.Append(Helper.HtmlEncode(characterDiff.l.Text));
                            else
                            {
                                html.Append("<span style='background-color: " + 
                                    deleteColorCharLevel + ";" + linethroughStyle + "'>" +
                                    Helper.HtmlEncode(characterDiff.l.Text) + "</span>"); 
                                html.Append("<span style='background-color: " + 
                                    insertColorCharLevel + ";" + underlineStyle + "'>" +
                                    Helper.HtmlEncode(characterDiff.r.Text) + "</span>"); 
                            }
                            if (!string.IsNullOrEmpty(characterDiff.l.Text))
                                index += characterDiff.l.Text.Length;
                        }

                    html.Append("</div>");
                }
            }

            if (monospacedFont) html.Append("</div>");
            return html.ToString();
        }

        #endregion

        #region Track changes

        static public string TextDiffTrackChanges(string left, string right)
        {
            // Use DiffMatchPatch version
            return DiffMatchPatchAPI.TextDiffTrackChanges(left, right);
        }

        #endregion
    }
}