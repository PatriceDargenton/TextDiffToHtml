
using System.ComponentModel;

namespace TextDiffToHtml
{
    public class TextDiffToHtmlEnums
    {
        [Description("Library used")]
        [DefaultValue(LibraryEnum.DiffPlex)]
        public enum LibraryEnum
        {
            [Description("Compare using DiffPlex")] // https://github.com/mmanela/diffplex
            DiffPlex,
            [Description("Compare using DiffLib")] // https://github.com/lassevk/DiffLib
            DiffLib,
            //[Description("Compare using DiffMatchPatch")] // https://github.com/google/diff-match-patch
            //DiffMatchPatch,
        }

        [Description("Display mode")]
        [DefaultValue(DisplayModeEnum.SideBySide)]
        public enum DisplayModeEnum
        {
            [Description("Side by side")]
            SideBySide,
            [Description("Inline (the other version below each line)")]
            Inline,
            [Description("Compact (only one combined line)")]
            Compact,
            [Description("Track changes (more compact: no signs of similarities or differences added at the beginning of each line)")]
            TrackChanges
        }

        [Description("Show or hide identical lines")]
        [DefaultValue(ShowIdenticalLinesEnum.ShowIdenticalLines)]
        public enum ShowIdenticalLinesEnum
        {
            [Description("Show identical lines")]
            ShowIdenticalLines,
            [Description("Hide identical lines")]
            HideIdenticalLines
        }
        
        [Description("Show or hide identical parts in a line")]
        [DefaultValue(ShowIdenticalPartsEnum.ShowIdenticalParts)]
        public enum ShowIdenticalPartsEnum
        {
            [Description("Show identical parts")]
            ShowIdenticalParts,
            [Description("Hide identical parts")]
            HideIdenticalParts
        }
        
        [Description("Use monospaced font or not to display a text")]
        [DefaultValue(MonospacedFontEnum.StandardFont)]
        public enum MonospacedFontEnum
        {
            [Description("Monospaced font")]
            MonospacedFont,
            [Description("Standard font")]
            StandardFont
        }
        
        [Description("Use char level or word level to display differences")]
        [DefaultValue(CharLevelEnum.CharLevel)]
        public enum CharLevelEnum
        {
            [Description("Char level")]
            CharLevel,
            [Description("Word level")]
            WordLevel
        }

        [Description("Strikethrough (or underline) text or not to show the differences")]
        [DefaultValue(LineThroughEnum.On)]
        public enum LineThroughEnum
        {
            [Description("Line through: on")]
            On,
            [Description("Line through: off")]
            Off
        }
        
        [Description("Swap Left and Right texts")]
        [DefaultValue(SwapLeftRightEnum.Off)]
        public enum SwapLeftRightEnum
        {
            [Description("Swap Left & Right texts: on")]
            On,
            [Description("Swap Left & Right texts: off")]
            Off
        }

        public static LibraryEnum LibraryFromValue(string value)
        {
            LibraryEnum x;
            try
            {
                x = EnumExtensions.GetValueFromValue<LibraryEnum>(value);
            }
            catch (Exception /* ex */)
            {
                x = LibraryDefault();
            }
            return x;
        }

        public static LibraryEnum LibraryDefault()
        {
            var x = TextDiffToHtml.EnumHelper.GetDefaultValue<LibraryEnum>();
            return x;
        }

        public static DisplayModeEnum DisplayModeFromValue(string displayModeValue)
        {
            DisplayModeEnum x;
            try
            {
                x = EnumExtensions.GetValueFromValue<DisplayModeEnum>(displayModeValue);
            }
            catch (Exception /* ex */)
            {
                x = DisplayModeDefault();
            }
            return x;
        }
        
        public static DisplayModeEnum DisplayModeFromDescription(string displayModeDescr)
        {
            DisplayModeEnum x;
            try
            {
                x = typeof(DisplayModeEnum).GetValueFromDescription<DisplayModeEnum>(displayModeDescr);
            }
            catch (Exception /* ex */)
            {
                x = DisplayModeDefault();
            }
            return x;
        }

        public static DisplayModeEnum DisplayModeDefault()
        {
            var x = TextDiffToHtml.EnumHelper.GetDefaultValue<DisplayModeEnum>();
            return x;
        }

        public static ShowIdenticalLinesEnum ShowHideIdenticalLinesFromValue(string value)
        {
            ShowIdenticalLinesEnum x;
            try
            {
                x = EnumExtensions.GetValueFromValue<ShowIdenticalLinesEnum>(value);
            }
            catch (Exception /* ex */)
            {
                x = ShowHideIdenticalLinesModeDefault();
            }
            return x;
        }

        public static ShowIdenticalLinesEnum ShowHideIdenticalLinesModeDefault()
        {
            var x = TextDiffToHtml.EnumHelper.GetDefaultValue<ShowIdenticalLinesEnum>();
            return x;
        }
    }
}