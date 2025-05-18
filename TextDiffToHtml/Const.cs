
namespace TextDiffToHtml
{
    internal class Const
    {
        public const string appTitle = "TextDiffToHtml";
        public const string dateVersion = "18/05/2025";

        public const string sideBySideFile = "SideBySide.html";
        public const string inlineFile = "Inline.html";
        public const string compactFile = "Compact.html";
        public const string trackChanges = "TrackChanges.html";

        public const string outputFilename = "TextDiffToHtml.html";
        
        public static readonly Color InsertBkGrndColor = HtmlColors.VeryLightGreen;
        public static readonly Color DeleteBkGrndColor = HtmlColors.VeryLightRed;

        public static readonly Color InsertColorCharLevel = HtmlColors.Green;
        public static readonly Color DeleteColorCharLevel = HtmlColors.Red;

        public static readonly Color UpdateCharLevelLeft = HtmlColors.Red;
        public static readonly Color UpdateCharLevelRight = HtmlColors.Green;

        // TC = TrackChanges mode
        public static readonly Color DeleteBkGrndColorTC = HtmlColors.Red;
        public static readonly Color InsertBkGrndColorTC = HtmlColors.Green;

        public const int pageCode = 1252;

        public const string newline = "\n"; // Line Feed
        public const string newlineCRLF = "\r\n"; // Carriage Return + Line Feed: Environment.NewLine
        public const char newlineChar = '\n';

        public const string htmlSpace = "\x00a0";
        public const string htmlNewline = "<br>"; // HTML5
        //public const string htmlNewline = "<br/>"; // XHTML or XML compatibility

        public const string htmlCharset = @"<meta charset = ""UTF-8"">";

        // Note: style tag should be in the head section of the HTML document, but it is accepted in some browsers
        // (for example font style may be ignored in somes browsers)
        public const string htmlStart =
@"<!DOCTYPE html>
<html lang=""fr"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>TextDiffToHtmlSamples</title>
</head>
<body>
";
        public const string htmlEnd =
@"</body>
</html>";

        public const string htmlPilcrowSign = "&para;"; // Glyph used to identify a paragraph
        //public const string htmlMonospacedFont = "courier"; // "Courier New"; 
        public const string htmlMonospacedFont = "consolas"; // monospace
    }

    public static class HtmlColors
    {
        //public static readonly Color Green = Color.LightGreen; // Exists
        //public static readonly Color Red = Color.LightRed; // Does not exist
        public static readonly Color DarkGreen = Color.FromArgb(163, 209, 163); // #a3d1a3
        public static readonly Color DarkRed = Color.FromArgb(255, 128, 128); // #ff8080
        public static readonly Color Green = Color.FromArgb(172, 242, 189); // "#acf2bd"; 
        public static readonly Color Red = Color.FromArgb(253, 184, 192); // "#fdb8c0";
        public static readonly Color LightRed = Color.FromArgb(255, 204, 204); // #ffcccc
        public static readonly Color LightGreen = Color.FromArgb(204, 255, 204); // #ccffcc
        public static readonly Color VeryLightRed = Color.FromArgb(255, 220, 224); // #ffdce0
        public static readonly Color VeryLightGreen = Color.FromArgb(220, 245, 227); // "#dcf5e3";
        //public static readonly Color VeryLightGreen = Color.FromArgb(230, 255, 237); // "#e6ffed";

        public static string ToHexString(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        public static string ToRgbString(Color color) => $"{color.R},{color.G},{color.B}";
    }
}