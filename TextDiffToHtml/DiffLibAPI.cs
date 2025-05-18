
// https://github.com/lassevk/DiffLib

using DiffLib;
using DiffLib.Alignment;
using System.Text;

namespace TextDiffToHtml
{
    internal class DiffLibAPI
    {
        // https://github.com/lassevk/DiffLib/tree/main/Examples

        // 000 - Basic diffing of two texts
        public const string LassevkLeftSample1 = 
            "This is a test of the diff implementation, with some text that is deleted.";
        public const string LassevkRightSample1 = 
            "This is another test of the same implementation, with some more text.";

        // 001 - Basic diffing of two text files
        const string LassevkLeftSample2_ =
@"This line is the same
This line is also the same
This line has been deleted
This line is yet another equal line
This is also another equal line
This line is changed
This line is also changed
This is the final equal line";

        const string LassevkRightSample2_ =
@"This line is the same
This line is also the same
This line is yet another equal line
This line has been added
This is also another equal line
This line was changed to this
And then this was added
And this line was changed to this
This is the final equal line";

        public static string LassevkLeftSample2 = 
            LassevkLeftSample2_.Replace(Const.newlineCRLF, Const.newline);
        public static string LassevkRightSample2 = 
            LassevkRightSample2_.Replace(Const.newlineCRLF, Const.newline);

        const bool colorizeDiff = true;

        #region Side by side

        public static string TextDiffSideBySideSplitByLine(string left, string right,
            bool showIdenticalLines, bool charLevel = false, bool linethrough = false, 
            bool monospacedFont = false)
        {
            var leftArray = left.Split(Const.newlineChar);
            var rightArray = right.Split(Const.newlineChar);
            var sections = Diff.CalculateSections(leftArray, rightArray);
            var aligner = new StringSimilarityDiffElementAligner();
            var nullableAligner = (IDiffElementAligner<string?>)aligner;
            var elements = Diff.AlignElements(leftArray, rightArray, sections, nullableAligner);
            return DumpDiffSideBySide(showIdenticalLines, charLevel, linethrough, monospacedFont, 
                elements);
        }

        static string DumpDiffSideBySide(bool showIdenticalLines, bool charLevel, 
            bool linethrough, bool monospacedFont,
            IEnumerable<DiffElement<string?>> elements)
        {
            var sb = new StringBuilder();

            sb = Helper.GetSideBySideStyle(sb, colorizeDiff, monospacedFont);

            var underlineStyle = " text-decoration: underline;";
            var linethroughStyle = " text-decoration: line-through;";
            if (!linethrough) { linethroughStyle = ""; underlineStyle = ""; }

            int numLineLeft = 0;
            int numLineRight = 0;
            foreach (var element in elements)
            {
                numLineLeft = 1 + element.ElementIndexFromCollection1 ?? numLineLeft;
                numLineRight = 1 + element.ElementIndexFromCollection2 ?? numLineRight;
                sb.Append("<tr>");

                var insertBkGrndColor = HtmlColors.ToHexString(Const.InsertBkGrndColor);
                var deleteBkGrndColor = HtmlColors.ToHexString(Const.DeleteBkGrndColor);
                string modifiedColorLeft = HtmlColors.ToHexString(Const.UpdateCharLevelLeft);
                string modifiedColorRight = HtmlColors.ToHexString(Const.UpdateCharLevelRight);

                if (!charLevel)
                {
                    insertBkGrndColor = "yellow";
                    deleteBkGrndColor = "yellow";
                    modifiedColorLeft = "yellow";
                    modifiedColorRight = "yellow";
                    linethroughStyle = ""; 
                    underlineStyle = "";
                }

                switch (element.Operation)
                {
                    case DiffOperation.Match:
                        if (!showIdenticalLines) break;
                        sb.Append("<td>" + numLineLeft + "</td>");
                        sb.Append("<td>" + Helper.HtmlEncode(element.ElementFromCollection1.Value) + "</td>");
                        sb.Append("<td>==</td>");
                        sb.Append("<td>" + Helper.HtmlEncode(element.ElementFromCollection2.Value) + "</td>");
                        sb.Append("<td>" + numLineRight + "</td>");
                        break;

                    case DiffOperation.Insert:
                        sb.Append("<td></td>");
                        sb.Append("<td></td>");
                        //sb.Append("<td>>></td>");
                        sb.Append($"<td>{Helper.HtmlEncode(">>")}</td>");
                        sb.Append(
                            "<td><span style='background-color: " + 
                            insertBkGrndColor + ";" + underlineStyle + "'>" +
                            Helper.HtmlEncode(element.ElementFromCollection2.Value) +
                            "</span></td>");
                        sb.Append("<td>" + numLineRight + "</td>");
                        break;

                    case DiffOperation.Delete:
                        sb.Append("<td>" + numLineLeft + "</td>");
                        sb.Append(
                            "<td><span style='background-color: " + 
                            deleteBkGrndColor + ";" + linethroughStyle + "'>" +
                            Helper.HtmlEncode(element.ElementFromCollection1.Value) +
                            "</span></td>");
                        //sb.Append("<td><<</td>");
                        sb.Append($"<td>{Helper.HtmlEncode("<<")}</td>");
                        sb.Append("<td></td>");
                        sb.Append("<td></td>");
                        break;

                    case DiffOperation.Replace:
                    case DiffOperation.Modify:
                        if (element.ElementFromCollection1.Value == null) break;
                        if (element.ElementFromCollection2.Value == null) break;

                        sb.Append("<td>" + numLineLeft + "</td>");

                        var sections = CalculateDiffSections(element, charLevel);

                        sb.Append("<td>");
                        AppendFormattedDiff(sb, sections, element.ElementFromCollection1.Value,
                            charLevel, modifiedColorLeft, linethrough, isLeft: true); 
                        sb.Append("</td>");
                        //sb.Append("<td><></td>");
                        sb.Append($"<td>{Helper.HtmlEncode("<>")}</td>");

                        sb.Append("<td>");
                        AppendFormattedDiff(sb, sections, element.ElementFromCollection2.Value,
                            charLevel, modifiedColorRight, linethrough, isLeft: false); 
                        sb.Append("</td>");
                        sb.Append("<td>" + numLineRight + "</td>");

                        break;
                        
                }
                sb.Append("</tr>");
            }

            sb.Append("</table>");
            return sb.ToString();
        }

        private static IEnumerable<DiffSection> CalculateDiffSections(
            DiffElement<string?> element, bool charLevel)
        {
            if (element.ElementFromCollection1.Value == null) 
                return []; // Enumerable.Empty<DiffSection>();
            if (element.ElementFromCollection2.Value == null) return []; 
            if (charLevel)
            {
                return Diff.CalculateSections(
                    element.ElementFromCollection1.Value.ToCharArray(),
                    element.ElementFromCollection2.Value.ToCharArray());
            }
            else
            {
                var words1 = element.ElementFromCollection1.Value?.Split(' ');
                var words2 = element.ElementFromCollection2.Value?.Split(' ');
                if (words1 == null || words2 == null) return []; 
                return Diff.CalculateSections(words1, words2);
            }
        }
        
        private static void AppendFormattedDiff(StringBuilder html, 
            IEnumerable<DiffSection> sections, string value, bool charLevel, string color,
            bool linethrough, bool isLeft) 
        {
            int index = 0;
            var words = charLevel ? null : value.Split(' ');

            var underlineStyle = " text-decoration: underline;";
            var linethroughStyle = " text-decoration: line-through;";
            if (!linethrough) { linethroughStyle = ""; underlineStyle = ""; }

            foreach (var section in sections)
            {
                int length = isLeft ? section.LengthInCollection1 : section.LengthInCollection2;
                string style = isLeft ? linethroughStyle : underlineStyle;

                var text = "";
                if (charLevel) text = value.Substring(index, length);
                else {
                    if (words == null) text = " ";
                    else text = string.Join(" ", words.Skip(index).Take(length));
                }

                if (section.IsMatch)
                {
                    if (!charLevel)
                        html.Append(Helper.HtmlEncode(text + " "));
                    else
                        html.Append(Helper.HtmlEncode(text));
                }
                else
                {
                    html.Append("<span style='background-color: " + color + ";" + style + "'>" +
                        Helper.HtmlEncode(text)); 
                    html.Append("</span>");
                    if (!charLevel) html.Append(' ');
                }
                index += length;
            }
        }

        #endregion

        #region Inline
        
        private class DiffLineData
        {
            public int? L { get; set; } // Left line number
            public int? R { get; set; } // Right line number
            public string Delta { get; set; } = string.Empty;
            public string? Left { get; set; }
            public string? Right { get; set; }
            public List<DiffLib.DiffSection>? CharSections { get; set; }
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
                        Text = Helper.HtmlEncode(set.Left ?? "")
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
                        Text = Helper.HtmlEncode(set.Left ?? "")
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
                        Text = Helper.HtmlEncode(set.Right ?? "")
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
                        Text = string.Join("",
                            GetDiffPieces(set.CharSections, set.Left ?? "", 
                                isProcessingLeftLine:true, red, showIdenticalParts))
                    };
                    sb.AppendLine(Helper.GetHtmlTableRow(leftRecord, cssClass: "redClass"));

                    var green = HtmlColors.ToRgbString(Const.UpdateCharLevelRight);
                    var rightRecord = new TextDiffToHtml.Record
                    {
                        L = null,
                        R = set.R,
                        Delta = "+",
                        Text = string.Join("",
                            GetDiffPieces(set.CharSections, set.Right ?? "", 
                                isProcessingLeftLine: false, green, showIdenticalParts))
                    };
                    sb.AppendLine(Helper.GetHtmlTableRow(rightRecord, cssClass: "greenClass"));
                }
            }

            sb.Append("</tbody></table>");
            return sb.ToString();
        }

        private static List<string> GetDiffPieces(
            List<DiffSection>? charSections, string lineTextForContext, 
            bool isProcessingLeftLine, string bkGrndRGBColor, bool showIdenticalParts)
        {
            var result = new List<string>();

            if (charSections == null || string.IsNullOrEmpty(lineTextForContext))
            {
                result.Add(Helper.HtmlEncode(lineTextForContext));
                return result;
            }

            bool lastStateChanged = false;
            int currentTextIndex = 0;

            foreach (var section in charSections)
            {
                int lengthInThisLine = isProcessingLeftLine ? 
                    section.LengthInCollection1 : section.LengthInCollection2;
                if (lengthInThisLine == 0) continue;

                string pieceText = lineTextForContext.Substring(currentTextIndex, lengthInThisLine);
                bool changed = !section.IsMatch;

                if (changed != lastStateChanged)
                {
                    if (changed)
                        result.Add($"<span style='background-color: rgb({bkGrndRGBColor});'>");
                    else
                        result.Add("</span>");
                    lastStateChanged = changed;
                }

                if (changed || showIdenticalParts)
                {
                    result.Add(Helper.HtmlEncode(pieceText));
                }
                else if (!showIdenticalParts && result.Count > 0 && !result.Last().EndsWith(' '))
                {
                    result.Add(" ");
                }
                currentTextIndex += lengthInThisLine;
            }

            if (lastStateChanged)
            {
                result.Add("</span>");
            }
            return result;
        }

        private static List<DiffLineData> GetTextDiffInline(string left, string right)
        {
            var leftLines = left.Split(Const.newlineChar);
            var rightLines = right.Split(Const.newlineChar);

            var aligner = new StringSimilarityDiffElementAligner();
            var nullableAligner = (IDiffElementAligner<string?>)aligner;

            var sections = Diff.CalculateSections(leftLines, rightLines);
            var alignedElements = Diff.AlignElements(leftLines, rightLines,
                sections, nullableAligner).ToList();
            var resultList = new List<DiffLineData>();

            int currentLeftLineNum = 1;
            int currentRightLineNum = 1;
            int i = 0;
            while (i < alignedElements.Count)
            {
                var element = alignedElements[i];

                if (element.Operation == DiffOperation.Match)
                {
                    resultList.Add(new DiffLineData
                    {
                        L = currentLeftLineNum++,
                        R = currentRightLineNum++,
                        Left = element.ElementFromCollection1.Value,
                        Right = element.ElementFromCollection2.Value,
                        Delta = "=="
                    });
                    i++;
                }
                else if (element.Operation == DiffOperation.Delete)
                {
                    if (i + 1 < alignedElements.Count && 
                        alignedElements[i + 1].Operation == DiffOperation.Insert)
                    {
                        var nextElement = alignedElements[i + 1];
                        string oldLine = element.ElementFromCollection1.Value ?? "";
                        string newLine = nextElement.ElementFromCollection2.Value ?? "";

                        resultList.Add(new DiffLineData
                        {
                            L = currentLeftLineNum++,
                            R = currentRightLineNum++,
                            Left = oldLine,
                            Right = newLine,
                            Delta = "<>",
                            CharSections = Diff.CalculateSections(
                                oldLine.ToCharArray(), newLine.ToCharArray()).ToList()
                        });
                        i += 2;
                    }
                    else
                    {
                        resultList.Add(new DiffLineData
                        {
                            L = currentLeftLineNum++,
                            R = null,
                            Left = element.ElementFromCollection1.Value,
                            Right = null,
                            Delta = "<<"
                        });
                        i++;
                    }
                }
                else if (element.Operation == DiffOperation.Insert)
                {
                    resultList.Add(new DiffLineData
                    {
                        L = null,
                        R = currentRightLineNum++,
                        Left = null,
                        Right = element.ElementFromCollection2.Value,
                        Delta = ">>"
                    });
                    i++;
                }
                else if (element.Operation == DiffOperation.Modify)
                {
                    string oldLine = element.ElementFromCollection1.Value ?? "";
                    string newLine = element.ElementFromCollection2.Value ?? "";

                    resultList.Add(new DiffLineData
                    {
                        L = currentLeftLineNum++,
                        R = currentRightLineNum++,
                        Left = oldLine,
                        Right = newLine,
                        Delta = "<>",
                        CharSections = Diff.CalculateSections(
                            oldLine.ToCharArray(), newLine.ToCharArray()).ToList()
                    });
                    i++;
                }
                else
                {
                    i++;
                }
            }
            return resultList;
        }

        #endregion

        #region Compact

        static public string TextDiffCompactSplitByLine(string left, string right, 
            bool showIdenticalLines = true, bool showIdenticalParts = true,
            bool linethrough = true, bool monospacedFont = true)
        {
            var leftArray = left.Split(Const.newlineChar);
            var rightArray = right.Split(Const.newlineChar);
            var sections = Diff.CalculateSections(leftArray, rightArray);
            var aligner = new StringSimilarityDiffElementAligner();
            var nullableAligner = (IDiffElementAligner<string?>)aligner;
            var elements = Diff.AlignElements(leftArray, rightArray, sections, nullableAligner);
            return DumpDiffElementCompact(elements, showIdenticalLines, showIdenticalParts, 
                linethrough, monospacedFont);
        }

        static string DumpDiffElementCompact(IEnumerable<DiffElement<string?>> elements, 
            bool showIdenticalLines, bool showIdenticalParts, bool linethrough, bool monospacedFont)
        {
            var html = new StringBuilder();
            if (monospacedFont) html.Append($"<div style='font-family: {Const.htmlMonospacedFont};'>");

            var insertBkGrndColor = HtmlColors.ToHexString(Const.InsertBkGrndColor);
            var deleteBkGrndColor = HtmlColors.ToHexString(Const.DeleteBkGrndColor);
            var insertColorCharLevel = HtmlColors.ToHexString(Const.InsertColorCharLevel);
            var deleteColorCharLevel = HtmlColors.ToHexString(Const.DeleteColorCharLevel);

            foreach (var element in elements)
            {
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

                switch (element.Operation)
                {
                    case DiffOperation.Match:
                        if (!showIdenticalLines) break;
                        html.Append(
                            "<div>" + Const.htmlSpace + Const.htmlSpace +
                            Helper.HtmlEncode(element.ElementFromCollection1.Value) +
                            "</div>");
                        break;

                    case DiffOperation.Insert:
                        html.Append(
                            "<div style='background-color: " + insertBkGrndColor + ";" + 
                            underlineStyleInsertedLine + "'>+" + Const.htmlSpace +
                            Helper.HtmlEncode(element.ElementFromCollection2.Value) +
                            "</div>");
                        break;

                    case DiffOperation.Delete:
                        html.Append(
                            "<div style='background-color: " + deleteBkGrndColor + ";" + 
                            linetroughStyleDeletedLine + "'>-" + Const.htmlSpace +
                            Helper.HtmlEncode(element.ElementFromCollection1.Value) +
                            "</div>");
                        break;

                    case DiffOperation.Replace:
                    case DiffOperation.Modify:
                        if (element.ElementFromCollection1.Value == null) break;
                        if (element.ElementFromCollection2.Value == null) break;
                        var section1 = element.ElementFromCollection1.Value;
                        if (section1 == null) break;
                        var section2 = element.ElementFromCollection2.Value;
                        if (section2 == null) break;
                        char[]? section1Array = section1.ToCharArray();
                        char[]? section2Array = section2.ToCharArray();
                        DiffSection[]? sections = null;
                        if (section1Array != null && section2Array != null)
                            sections = Diff.CalculateSections(
                                section1Array, section2Array).ToArray();
                        if (sections == null) break;
                        int ii1 = 0;
                        int ii2 = 0;
                        html.Append("<div>*" + Const.htmlSpace);
                        foreach (var section in sections)
                        {
                            if (section.IsMatch)
                                //if (showIdenticalParts) // It is not possible to mask identical parts
                                //  because for exemple in sample1: 
                                // element.ElementFromCollection1.Value = "DEF def"
                                // element.ElementFromCollection2.Value = "DEF DEF"
                                html.Append(Helper.HtmlEncode(
                                        element.ElementFromCollection1.Value.Substring(ii1,
                                            section.LengthInCollection1)));
                            else
                            {
                                html.Append(
                                    "<span style='background-color: " + deleteColorCharLevel + ";" + 
                                    linethroughStyle + "'>" +
                                    Helper.HtmlEncode(element.ElementFromCollection1.Value.Substring(ii1,
                                        section.LengthInCollection1)) +
                                    "</span>"); // Delete
                                html.Append(
                                    "<span style='background-color: " + insertColorCharLevel + ";" + 
                                    underlineStyle + "'>" +
                                    Helper.HtmlEncode(element.ElementFromCollection2.Value.Substring(ii2,
                                        section.LengthInCollection2)) +
                                    "</span>"); // Insert
                            }

                            ii1 += section.LengthInCollection1;
                            ii2 += section.LengthInCollection2;
                        }
                        html.Append("</div>");
                        break;
                }
            }
            if (monospacedFont) html.Append("</div>");
            return html.ToString();
        }

        #endregion

        #region Track changes

        static public string TextDiffTrackChangesSplitByChar(string left, string right,
                bool showIdenticalParts = true, bool linethrough = true, bool monospacedFont = false, 
                HtmlRenderer? renderer = null, long? averageLength = 0)
        {
            var timeStart1 = DateTime.Now;
            // ToDo: there should be a better way to do this:
            var sections = Diff.CalculateSections(left.ToCharArray(), right.ToCharArray());

            if (renderer != null)
            {
                System.Diagnostics.Debug.WriteLine(DateTime.Now + " : CalculateSections: Done.");
                var htmlTmp = new StringBuilder();
                htmlTmp.AppendLine("<p>CalculateSections: Done.</p>");
                renderer.OnPartialRender(htmlTmp.ToString());
            }

            var timeStart2 = DateTime.Now;
            var result = DumpDiffSectionsTrackChanges(left, right, sections,
                showIdenticalParts, monospacedFont, linethrough, renderer, averageLength);
            var timeEnd = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("Diff.CalculateSections: " + 
                (timeStart2 - timeStart1).TotalMilliseconds + " ms");
            System.Diagnostics.Debug.WriteLine("DumpDiffSectionsTrackChanges: " + 
                (timeEnd - timeStart2).TotalMilliseconds + " ms");

            return result;
        }

        static string DumpDiffSectionsTrackChanges(string left, string right,
                IEnumerable<DiffSection> sections, 
                bool showIdenticalParts, bool monospacedFont, bool linethrough,
                HtmlRenderer? renderer, long? averageLength)
        {
            var html = new StringBuilder();

            if (monospacedFont) html.Append($"<div style='font-family: {Const.htmlMonospacedFont};'>");

            var underlineStyle = " text-decoration: underline;";
            var linethroughStyle = " text-decoration: line-through;";
            if (!linethrough) { linethroughStyle = ""; underlineStyle = ""; }

            var insertBkGrndColor = HtmlColors.ToHexString(Const.InsertBkGrndColorTC);
            var deleteBkGrndColor = HtmlColors.ToHexString(Const.DeleteBkGrndColorTC);

            if (averageLength == null || averageLength <= 0) averageLength = 1;
            int progressRatio = (int)(1000 / averageLength);
            if (progressRatio < 1) progressRatio = 1;
            int i1 = 0;
            int i2 = 0;
            int line = 0;
            int nbLines = sections.Count();
            foreach (var section in sections)
            {
                line++;
                if (renderer != null && (line == 1 || line == nbLines || line % progressRatio == 0)) 
                {
                    System.Diagnostics.Debug.WriteLine(DateTime.Now +  " : Line n°" + line + "/" + nbLines);
                    renderer.line = line;
                    renderer.lines = nbLines;
                    if (nbLines > 0) renderer.progress = 100 * (float)line / nbLines;
                    var htmlTmp = new StringBuilder();
                    htmlTmp.AppendLine("<p>Line " + line + "/" + nbLines + "</p>");
                    htmlTmp.Append(html);
                    renderer.OnPartialRender(htmlTmp.ToString());
                    if (renderer.cancel)
                    {
                        html.Append("<p>Line " + line + "/" + nbLines + "</p>");
                        html.Append("<p>Display canceled by user.</p>");
                        break;
                    }
                }

                if (section.IsMatch)
                {
                    if (showIdenticalParts) 
                    {
                        string identicalText = left.Substring(i1, section.LengthInCollection1);
                        identicalText = Helper.HtmlEncode(identicalText);
                        html.Append(identicalText);
                    }
                }
                else
                {
                    string deletedText = left.Substring(i1, section.LengthInCollection1);
                    deletedText = Helper.HtmlEncode(deletedText);
                    var s1 = string.Concat(
                        "<span style='background-color: " + deleteBkGrndColor + ";" + 
                        linethroughStyle + "'>",
                        deletedText,
                        "</span>");
                    html.Append(s1);

                    string insertedText = right.Substring(i2, section.LengthInCollection2);
                    insertedText = Helper.HtmlEncode(insertedText);
                    var s2 = string.Concat(
                        "<span style='background-color: " + insertBkGrndColor + ";" + 
                        underlineStyle + "'>",
                        insertedText,
                        "</span>");
                    html.Append(s2);
                }

                i1 += section.LengthInCollection1;
                i2 += section.LengthInCollection2;
            }

            if (monospacedFont) html.Append("</div>"); // Close div style

            return html.ToString();
        }

        #endregion
    }
}