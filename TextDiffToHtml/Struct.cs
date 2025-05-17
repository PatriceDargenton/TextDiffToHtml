
using static TextDiffToHtml.TextDiffToHtmlEnums;

namespace TextDiffToHtml
{
    public class Parameter
    {
        public string LeftText { get; set; } = string.Empty;
        public string RightText { get; set; } = string.Empty;
        public LibraryEnum Library { get; set; }
        public DisplayModeEnum DisplayMode { get; set; }
        public bool ShowIdenticalLines { get; set; }
        public bool ShowIdenticalParts { get; set; }
        public bool LineThrough { get; set; }
        public bool CharLevel { get; set; }
        public bool MonospacedFont { get; set; }
        public long averageLength { get; set; }
    }

    public class Record
    {
        public int? L { get; set; } // Left line number
        public int? R { get; set; } // Right line number
        public string Delta { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
