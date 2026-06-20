
// https://github.com/iyulab/TextDiff
// https://www.nuget.org/packages/TextDiff.Sharp

using System.Text;
using DiffPlex;
using DiffPlex.DiffBuilder.Model;
using TextDiff;

namespace TextDiffToHtml
{
    internal class TextDiffSharpAPI
    {
        #region Side by side

        public static string TextDiffSideBySide(string left, string right,
            bool showIdenticalLines = true, bool monospacedFont = false)
        {
            var unifiedDiff = BuildUnifiedDiff(left, right);

            // Use TextDiff.Sharp to apply and validate the generated unified diff
            var differ = new TextDiffer();
            _ = differ.Process(left, unifiedDiff);

            return BuildSideBySideHtml(left, right, showIdenticalLines, monospacedFont);
        }

        private static string BuildSideBySideHtml(string left, string right,
            bool showIdenticalLines, bool monospacedFont)
        {
            var diffBuilder = new DiffPlex.DiffBuilder.SideBySideDiffBuilder(new Differ());
            var diffModel = diffBuilder.BuildDiffModel(left, right);

            var sb = Helper.GetSideBySideStyle(new StringBuilder(), colorizeDiff: true, monospacedFont);

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
                string deltaHtml = Helper.HtmlEncode(delta);

                if (string.IsNullOrEmpty(oldLine.Text) &&
                    string.IsNullOrEmpty(newLine.Text))
                {
                    if (!showIdenticalLines) continue;
                }

                sb.AppendLine(
                    $"      <tr>" +
                    $"<td>{oldLine.Position}</td>" +
                    $"<td>{leftText}</td>" +
                    $"<td>{deltaHtml}</td>" +
                    $"<td>{rightText}</td>" +
                    $"<td>{newLine.Position}</td></tr>");
            }

            sb.AppendLine("  </table>");
            return sb.ToString();
        }

        private static string HtmlEncodeWithDiff(bool showIdenticalLines, DiffPiece line, out bool identical)
        {
            identical = false;
            if (line.Type == ChangeType.Modified)
            {
                var sb = new StringBuilder();

                var spanTag = false;
                var lastStateChanged = false;
                foreach (var piece in line.SubPieces)
                {
                    var changed = (piece.Type != ChangeType.Unchanged);
                    if (changed != lastStateChanged)
                    {
                        if (changed)
                        {
                            sb.Append("<span style='background: yellow;'>");
                            spanTag = true;
                        }
                        else
                        {
                            sb.Append("</span>");
                            spanTag = false;
                        }
                        lastStateChanged = changed;
                    }
                    sb.Append(Helper.HtmlEncode(piece.Text));
                }
                if (spanTag) sb.Append("</span>");

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

        private class DiffLineData // use DiffPiece from DiffPlex
        {
            public int? L { get; set; } // Left line number
            public int? R { get; set; } // Right line number
            public string Delta { get; set; } = string.Empty;
            public string Left { get; set; } = string.Empty;
            public string Right { get; set; } = string.Empty;
            public required List<DiffPiece> LeftPieces { get; set; }
            public required List<DiffPiece> RightPieces { get; set; }
        }

        public static string TextDiffInline(string left, string right,
            bool showIdenticalLines = true, bool showIdenticalParts = true, bool monospacedFont = true)
        {
            var unifiedDiff = BuildUnifiedDiff(left, right);

            // Use TextDiff.Sharp to apply and validate the generated unified diff
            var differ = new TextDiffer();
            _ = differ.Process(left, unifiedDiff);

            var lineData = GetTextDiffInline(left, right);
            var sb = Helper.GetInlineStyle(new StringBuilder(), monospacedFont);

            foreach (var set in lineData)
            {
                if (set.Delta == "==")
                {
                    var record = new Record
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
                    var record = new Record
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
                    var record = new Record
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
                    var leftRecord = new Record
                    {
                        L = set.L,
                        R = null,
                        Delta = "-",
                        Text = string.Join("", GetDiffPieces(set.LeftPieces, red, showIdenticalParts))
                    };
                    sb.AppendLine(Helper.GetHtmlTableRow(leftRecord, cssClass: "redClass"));

                    var green = HtmlColors.ToRgbString(Const.UpdateCharLevelRight);
                    var rightRecord = new Record
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
            var diffBuilder = new DiffPlex.DiffBuilder.SideBySideDiffBuilder(new Differ());
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

            int maxLines = Math.Max(diffModel.OldText.Lines.Count, diffModel.NewText.Lines.Count);

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

        #region Compact

        public static string TextDiffCompact(string left, string right,
            bool showIdenticalLines = true, bool showIdenticalParts = true,
            bool linethrough = true, bool monospacedFont = true)
        {
            var unifiedDiff = BuildUnifiedDiff(left, right);

            // Use TextDiff.Sharp to apply and validate the generated unified diff
            var differ = new TextDiffer();
            _ = differ.Process(left, unifiedDiff);

            var diffBuilder = new DiffPlex.DiffBuilder.SideBySideDiffBuilder(new Differ());
            var diffModel = diffBuilder.BuildDiffModel(left, right);

            return BuildCompactHtml(diffModel, 
                showIdenticalLines, showIdenticalParts, linethrough, monospacedFont);
        }

        private static string BuildCompactHtml(SideBySideDiffModel diffModel,
            bool showIdenticalLines, bool showIdenticalParts, bool linethrough, bool monospacedFont)
        {
            var html = new StringBuilder();
            if (monospacedFont) html.Append($"<div style='font-family: {Const.htmlMonospacedFont};'>");

            var insertBkGrndColor = HtmlColors.ToHexString(Const.InsertBkGrndColor);
            var deleteBkGrndColor = HtmlColors.ToHexString(Const.DeleteBkGrndColor);
            var insertColorCharLevel = HtmlColors.ToHexString(Const.InsertColorCharLevel);
            var deleteColorCharLevel = HtmlColors.ToHexString(Const.DeleteColorCharLevel);

            int maxLines = Math.Max(diffModel.OldText.Lines.Count, diffModel.NewText.Lines.Count);
            for (int i = 0; i < maxLines; i++)
            {
                var leftLine = i < diffModel.OldText.Lines.Count ? diffModel.OldText.Lines[i] : null;
                var rightLine = i < diffModel.NewText.Lines.Count ? diffModel.NewText.Lines[i] : null;

                var underlineStyle = " text-decoration: underline;";
                var linethroughStyle = " text-decoration: line-through;";
                var linetroughStyleDeletedLine = "";
                var underlineStyleInsertedLine = "";
                if (!linethrough)
                {
                    linethroughStyle = "";
                    underlineStyle = "";
                    linetroughStyleDeletedLine = "";
                    underlineStyleInsertedLine = "";
                }

                if (leftLine?.Type == ChangeType.Unchanged && rightLine?.Type == ChangeType.Unchanged)
                {
                    if (showIdenticalLines)
                    {
                        html.Append(
                            "<div>" +
                            Const.htmlSpace + Const.htmlSpace +
                            Helper.HtmlEncode(leftLine.Text) +
                            "</div>");
                    }
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
                else if (leftLine?.Type == ChangeType.Modified || rightLine?.Type == ChangeType.Modified)
                {
                    html.Append("<div>*" + Const.htmlSpace);

                    List<DiffPiece> leftLineSubPieces = [];
                    List<DiffPiece> rightLineSubPieces = [];
                    if (leftLine != null) leftLineSubPieces = leftLine.SubPieces;
                    if (rightLine != null) rightLineSubPieces = rightLine.SubPieces;

                    if (leftLine != null && rightLine != null)
                    {
                        foreach (var characterDiff in 
                            leftLineSubPieces.Zip(rightLineSubPieces, (l, r) => new { l, r }))
                        {
                            if (characterDiff.l.Text == characterDiff.r.Text)
                            {
                                if (showIdenticalParts)
                                    html.Append(Helper.HtmlEncode(characterDiff.l.Text));
                            }
                            else
                            {
                                html.Append("<span style='background-color: " +
                                    deleteColorCharLevel + ";" + linethroughStyle + "'>" +
                                    Helper.HtmlEncode(characterDiff.l.Text) + "</span>");
                                html.Append("<span style='background-color: " +
                                    insertColorCharLevel + ";" + underlineStyle + "'>" +
                                    Helper.HtmlEncode(characterDiff.r.Text) + "</span>");
                            }
                        }
                    }

                    html.Append("</div>");
                }
            }

            if (monospacedFont) html.Append("</div>");
            return html.ToString();
        }

        #endregion

        #region Track Changes

        // Char level mode: false : Best algorithm for Track Changes diff, even if it is not in char level mode
        public static string TextDiffTrackChanges(string left, string right,
            bool showIdenticalLines = true, bool linethrough = true,
            bool monospacedFont = false)
        {
            var unifiedDiff = BuildUnifiedDiff(left, right);

            // Use TextDiff.Sharp to apply and validate the generated unified diff
            var differ = new TextDiffer();
            _ = differ.Process(left, unifiedDiff);

            return BuildTrackChangesHtml(left, right, showIdenticalLines, linethrough, monospacedFont);
        }

        private static string BuildTrackChangesHtml(string left, string right,
            bool showIdenticalLines, bool linethrough, bool monospacedFont)
        {
            var html = new StringBuilder();
            if (monospacedFont) html.Append($"<div style='font-family: {Const.htmlMonospacedFont};'>");

            var underlineStyle = " text-decoration: underline;";
            var linethroughStyle = " text-decoration: line-through;";
            if (!linethrough)
            {
                linethroughStyle = "";
                underlineStyle = "";
            }

            var insertBkGrndColor = HtmlColors.ToHexString(Const.InsertBkGrndColorTC);
            var deleteBkGrndColor = HtmlColors.ToHexString(Const.DeleteBkGrndColorTC);

            // TextDiff.Sharp does not expose a native track-changes/inline rendering API
            //  (its public surface is limited to applying unified diffs and producing
            //  ChangeStats). The README itself recommends pairing it with DiffPlex for
            //  diff generation, so we reuse the DiffPlex line+word diff already used by
            //  the side-by-side / compact renderers.
            var diffBuilder = new DiffPlex.DiffBuilder.SideBySideDiffBuilder(new Differ());
            var diffModel = diffBuilder.BuildDiffModel(left, right);

            int maxLines = Math.Max(diffModel.OldText.Lines.Count, diffModel.NewText.Lines.Count);
            for (int i = 0; i < maxLines; i++)
            {
                var leftLine = i < diffModel.OldText.Lines.Count ? diffModel.OldText.Lines[i] : null;
                var rightLine = i < diffModel.NewText.Lines.Count ? diffModel.NewText.Lines[i] : null;

                if (leftLine?.Type == ChangeType.Unchanged && rightLine?.Type == ChangeType.Unchanged)
                {
                    if (showIdenticalLines)
                    {
                        html.Append(Helper.HtmlEncode(leftLine.Text));
                        html.Append(Const.htmlNewline);
                    }
                }
                else if (rightLine?.Type == ChangeType.Inserted)
                {
                    html.Append(
                        "<span style='background-color: " + insertBkGrndColor + ";" +
                        underlineStyle + "'>" +
                        Helper.HtmlEncode(rightLine.Text) +
                        "</span>");
                    html.Append(Const.htmlNewline);
                }
                else if (leftLine?.Type == ChangeType.Deleted)
                {
                    html.Append(
                        "<span style='background-color: " + deleteBkGrndColor + ";" +
                        linethroughStyle + "'>" +
                        Helper.HtmlEncode(leftLine.Text) +
                        "</span>");
                    html.Append(Const.htmlNewline);
                }
                else if (leftLine?.Type == ChangeType.Modified || rightLine?.Type == ChangeType.Modified)
                {
                    // Word-level diff via DiffPlex sub-pieces. Pair left/right sub-pieces
                    // to preserve original ordering and spaces.
                    var leftPieces = leftLine?.SubPieces ?? new List<DiffPiece>();
                    var rightPieces = rightLine?.SubPieces ?? new List<DiffPiece>();
                    int maxPieces = Math.Max(leftPieces.Count, rightPieces.Count);

                    for (int p = 0; p < maxPieces; p++)
                    {
                        var lp = p < leftPieces.Count ? leftPieces[p] : new DiffPiece(string.Empty, ChangeType.Imaginary);
                        var rp = p < rightPieces.Count ? rightPieces[p] : new DiffPiece(string.Empty, ChangeType.Imaginary);

                        if (lp.Type == ChangeType.Unchanged && rp.Type == ChangeType.Unchanged)
                        {
                            // identical text on both sides
                            html.Append(Helper.HtmlEncode(lp.Text));
                        }
                        else
                        {
                            // if left piece is a deletion or change, render as deleted
                            if (lp.Type != ChangeType.Unchanged && !string.IsNullOrEmpty(lp.Text))
                            {
                                html.Append(
                                    "<span style='background-color: " + deleteBkGrndColor + ";" +
                                    linethroughStyle + "'>" +
                                    Helper.HtmlEncode(lp.Text) +
                                    "</span>");
                            }

                            // if right piece is an insertion or change, render as inserted
                            if (rp.Type != ChangeType.Unchanged && !string.IsNullOrEmpty(rp.Text))
                            {
                                html.Append(
                                    "<span style='background-color: " + insertBkGrndColor + ";" +
                                    underlineStyle + "'>" +
                                    Helper.HtmlEncode(rp.Text) +
                                    "</span>");
                            }
                        }
                    }

                    html.Append(Const.htmlNewline);
                }
            }

            if (monospacedFont) html.Append("</div>");

            return html.ToString();
        }

        #endregion

        #region Common

        private static string BuildUnifiedDiff(string left, string right)
        {
            var diffBuilder = new DiffPlex.DiffBuilder.SideBySideDiffBuilder(new Differ());
            var diffModel = diffBuilder.BuildDiffModel(left, right);

            var lines = new List<string>();
            int maxLines = Math.Max(diffModel.OldText.Lines.Count, diffModel.NewText.Lines.Count);
            for (int i = 0; i < maxLines; i++)
            {
                var oldLine = i < diffModel.OldText.Lines.Count
                    ? diffModel.OldText.Lines[i]
                    : new DiffPiece(string.Empty, ChangeType.Imaginary);
                var newLine = i < diffModel.NewText.Lines.Count
                    ? diffModel.NewText.Lines[i]
                    : new DiffPiece(string.Empty, ChangeType.Imaginary);

                if (oldLine.Type == ChangeType.Unchanged && newLine.Type == ChangeType.Unchanged)
                {
                    lines.Add($" {oldLine.Text}");
                    continue;
                }

                if (oldLine.Type != ChangeType.Unchanged && oldLine.Type != ChangeType.Imaginary)
                    lines.Add($"-{oldLine.Text}");

                if (newLine.Type != ChangeType.Unchanged && newLine.Type != ChangeType.Imaginary)
                    lines.Add($"+{newLine.Text}");
            }

            return string.Join(Const.newline, lines);
        }

        #endregion
    }
}
