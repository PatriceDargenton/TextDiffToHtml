
// https://github.com/google/diff-match-patch
// https://www.nuget.org/packages/DiffMatchPatch

using DiffMatchPatch;

using System.Text;

namespace TextDiffToHtml
{
    internal class DiffMatchPatchAPI
    {
        const bool colorizeDiff = true;

        public static string Demos()
        {
            var sb = new StringBuilder();
            var dmp = new diff_match_patch();
            

            sb.AppendLine(Const.htmlNewline);
            sb.AppendLine("Sample n°1 using DiffMatchPatch" + Const.htmlNewline);
            string texte1a = "Hello World.";
            string texte2a = "Goodbye World.";
            sb.AppendLine(texte1a + Const.htmlNewline);
            sb.AppendLine(texte2a + Const.htmlNewline);
            List<Diff> diff1 = dmp.diff_main(texte1a, texte2a);
            // Result: [(-1, "Hell"), (1, "G"), (0, "o"), (1, "odbye"), (0, " World.")]
            dmp.diff_cleanupSemantic(diff1);
            // Result: [(-1, "Hello"), (1, "Goodbye"), (0, " World.")]
            sb.AppendLine("Differences:" + Const.htmlNewline);
            for (int i = 0; i < diff1.Count; i++)
            {
                sb.AppendLine(diff1[i].ToString() + Const.htmlNewline);
            }
            // To Html
            dmp.diff_cleanupSemantic(diff1);
            string htmlDiff4 = dmp.diff_prettyHtml(diff1);
            sb.AppendLine(Const.htmlNewline);
            sb.AppendLine("Using Html:" + Const.htmlNewline);
            sb.AppendLine(htmlDiff4 + Const.htmlNewline);


            sb.AppendLine(Const.htmlNewline);
            sb.AppendLine("Sample n°2 Two strings to compare" + Const.htmlNewline);
            string texte1 = "The cat is on the carpet";
            string texte2 = "The dog is under the carpet";
            sb.AppendLine(texte1 + Const.htmlNewline);
            sb.AppendLine(texte2 + Const.htmlNewline);
            var diff2 = dmp.diff_main(texte1, texte2);
            dmp.diff_cleanupSemantic(diff2);
            sb.AppendLine("Différences :" + Const.htmlNewline);
            foreach (var diff in diff2)
            {
                sb.AppendLine($"{diff.operation}: {diff.text}" + Const.htmlNewline);
            }
            // To Html
            dmp.diff_cleanupSemantic(diff2);
            string htmlDiff3 = dmp.diff_prettyHtml(diff2);
            sb.AppendLine(Const.htmlNewline);
            sb.AppendLine("Using Html:" + Const.htmlNewline);
            sb.AppendLine(htmlDiff3 + Const.htmlNewline);


            sb.AppendLine(Const.htmlNewline);
            sb.AppendLine("Sample n°3 Creating and applying a patch" + Const.htmlNewline);
            string originalText = "The cat is on the carpet";
            string modifiedText = "The cat is on the sofa";
            sb.AppendLine(originalText + Const.htmlNewline);
            sb.AppendLine(modifiedText + Const.htmlNewline);

            var patchs = dmp.patch_make(originalText, modifiedText);
            string patchedText = dmp.patch_toText(patchs);
            sb.AppendLine("Patch generated: " + patchedText + Const.htmlNewline);

            var result = dmp.patch_apply(patchs, originalText);
            string texteResultat = (string)result[0];
            bool[] patchsApplied = (bool[])result[1];

            sb.AppendLine("Text after applying the patch: " + texteResultat + Const.htmlNewline);
            sb.AppendLine("Patch Application Results:" + Const.htmlNewline);
            foreach (bool applied in patchsApplied)
            {
                sb.AppendLine("Patch applied? " + applied + Const.htmlNewline);
            }


            sb.AppendLine(Const.htmlNewline);
            sb.AppendLine("Sample n°4 Diff on multi-line texts" + Const.htmlNewline);
            string texte1b = "Line1\nLine2\nLine3";
            string texte2b = "Line1\nLineModified\nLine3";
            sb.AppendLine(texte1b + Const.htmlNewline);
            sb.AppendLine(texte2b + Const.htmlNewline);

            var dmp2 = new MyDiffMatchPatch();
            object[] result2 = dmp2.PublicDiffLinesToChars(texte1b, texte2b);
            string chars1 = (string)result2[0];
            string chars2 = (string)result2[1];
            List<string> lines = (List<string>)result2[2];

            List<Diff> diffs = dmp2.diff_main(chars1, chars2, false);

            dmp2.PublicDiffCharsToLines(diffs, lines);

            sb.AppendLine("Line by line differences:" + Const.htmlNewline);
            foreach (var diff in diffs)
            {
                sb.AppendLine($"{diff.operation}: {diff.text}" + Const.htmlNewline);
            }
            // To Html
            dmp.diff_cleanupSemantic(diffs);
            string htmlDiff2 = dmp.diff_prettyHtml(diffs);
            sb.AppendLine(Const.htmlNewline);
            sb.AppendLine("Using Html:" + Const.htmlNewline);
            sb.AppendLine(htmlDiff2 + Const.htmlNewline);

            return sb.ToString();
        }

        public class MyDiffMatchPatch : diff_match_patch
        {
            // Public method that calls the protected method diff_charsToLines
            public void PublicDiffCharsToLines(List<Diff> diffs, List<string> lines)
            {
                base.diff_charsToLines(diffs, lines);
            }

            // Public method that calls the protected method diff_linesToChars
            public object[] PublicDiffLinesToChars(string texte1, string texte2)
            { 
                return base.diff_linesToChars(texte1, texte2);
            }
        }

        #region Track changes

        static public string TextDiffTrackChanges(string left, string right)
        {
            var sb = new StringBuilder();
            // Converting texts to simplified strings and retrieving the list of lines
            // diff_linesToChars return object[]
            //   [0] => simplified string of text1,
            //   [1] => simplified string of text2,
            //   [2] => List<string> containing all unique lines.
            var dmp = new MyDiffMatchPatch();
            // checklines - Speedup flag. 
            // If false, then don't run a line-level diff first to identify the changed areas.
            // If true, then run a faster slightly less optimal diff.
            var diff = dmp.diff_main(left, right); //, checklines: false);
            dmp.diff_cleanupSemantic(diff);
            string htmlDiff = dmp.diff_prettyHtml(diff);
            //htmlDiff = Helper.HtmlEncode(htmlDiff);
            htmlDiff = htmlDiff.Replace(Const.htmlPilcrowSign, ""); // Remove &para; tags
            sb.AppendLine(htmlDiff); 
            return sb.ToString();
        }

        #endregion
    }
}