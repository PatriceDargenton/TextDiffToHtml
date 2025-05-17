
using System.Text;

namespace TextDiffToHtml
{
    internal class Helper
    {
        public static string GetHtmlTableRow(Record record, string cssClass = "")
        {
            string classAttr = string.IsNullOrEmpty(cssClass) ? "" : $" class='{cssClass}'";
            string l = record.L.HasValue ? record.L.Value.ToString() : "";
            string r = record.R.HasValue ? record.R.Value.ToString() : "";
            string d = record.Delta ?? "";
            string text = record.Text ?? "";

            return $"<tr{classAttr}><td class='rb'>{l}</td><td class='rb'>{r}</td><td>{d}</td><td>{text}</td></tr>";
        }

        

        public static StringBuilder GetSideBySideStyle(StringBuilder sb, bool colorizeDiff, bool monospacedFont)
        {
            // max-width: 100vw : works fine with Chrome, but not with IE: Vereyon.WebBrowser
            sb.AppendLine("  <style>");
            var fontFamily = "";
            if (monospacedFont) fontFamily = $"font-family: {Const.htmlMonospacedFont};"; // courier or consolas
            sb.AppendLine("    table.SideBySide { border-collapse: collapse; border-spacing: 0; max-width: 100vw; table-layout: fixed; }");
            sb.AppendLine("    table.SideBySide th { text-align: left; border-style: none none solid none; border-width: 0px 0px 2px 0px; border-color: black; padding: 1px 10px 1px 10px; " + 
                fontFamily + "word-wrap: break-word; }");
            // Only for deleted text: text-decoration: line-through;
            sb.AppendLine("    table.SideBySide td { border-style: solid none none none; border-width: 1px 0px 0px 0px; border-color: black; padding: 1px 10px 1px 10px; " + 
                fontFamily + " word-wrap: break-word; }");
            if (colorizeDiff) sb.AppendLine("    span.diff { background-color: yellow; }");
            sb.AppendLine("  </style>");
            sb.AppendLine("  <table class='SideBySide'>");
            sb.AppendLine("    <tr><th>L</th><th>Left</th><th>Delta</th><th>Right</th><th>R</th></tr>");
            return sb;
        }

        public static StringBuilder GetInlineStyle(StringBuilder sb, bool monospacedFont)
        {
            // Only for deleted text: text-decoration: line-through;

            var fontFamily = $"font-family: {Const.htmlMonospacedFont}";
            if (!monospacedFont) fontFamily = "";

            sb.Append($@"
    <style>
    table.TextDiffInline {{
        border-collapse: collapse;
        border-spacing: 0;
        font-size: 11pt;
        border-style: none none none none;
        {fontFamily}
    }}
    table.TextDiffInline tr.greenClass {{
        background-color: rgb({HtmlColors.ToRgbString(Const.InsertBkGrndColor)});
    }}
    table.TextDiffInline tr.redClass {{
        background-color: rgb({HtmlColors.ToRgbString(Const.DeleteBkGrndColor)});
    }}
    table.TextDiffInline th, td {{
        text-align: left;
        padding: 1px 8px 1px 8px;
    }}
    table.TextDiffInline th.rb, td.rb {{
        border-style: none solid none none;
        border-width: 0px 1px 0px 0px;
    }}
    </style>
    <table class='TextDiffInline'>
    <thead><tr><th class='rb'>L</th><th class='rb'>R</th><th>D</th><th>Line</th></tr></thead><tbody>");

            return sb;
        }

        public static string HtmlEncode(string? text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            // For example, when embedded in a block of text, 
            //  the characters < and > are encoded as &lt; and &gt; for HTTP transmission.
            return System.Net.WebUtility.HtmlEncode(text);

            //if (string.IsNullOrEmpty(text)) return string.Empty;
            //// Do not replace space by non-breaking space, in order to keep column width size: word-wrap: break-word;
            ////return HttpUtility.HtmlEncode(text).Replace(" ", "&nbsp;");
            //return text;
        }

        public static string? RemoveAfterParenthesis(string? input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            int index = input.IndexOf('(');
            if (index >= 0) return input.Substring(0, index).Trim();

            return input;
        }
    }
}