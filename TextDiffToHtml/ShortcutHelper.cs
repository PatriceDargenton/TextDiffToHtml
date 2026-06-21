
using System.IO;

namespace Shortcut.Helper
{
    public static class ShortcutHelper
    {
        /// <summary>
        /// Creates a Windows shell shortcut (.lnk) using WScript.Shell via dynamic COM.
        /// </summary>
        public static void CreateShortcut(
            ref string shortcutPath,
            ref string targetPath,
            string workingDirectory = "",
            int windowStyle = 4,
            string iconPath = "",
            int iconIndex = 0,
            string arguments = "")
        {
            if (!shortcutPath.ToLower().EndsWith(".lnk"))
                shortcutPath += ".lnk";

            if (string.IsNullOrEmpty(workingDirectory))
                workingDirectory = Path.GetDirectoryName(targetPath);

            if (string.IsNullOrEmpty(iconPath))
                iconPath = targetPath;

            dynamic wshShell = System.Activator.CreateInstance(
                System.Type.GetTypeFromProgID("WScript.Shell"));

            dynamic shortcut = wshShell.CreateShortcut(shortcutPath);

            shortcut.TargetPath = targetPath;
            shortcut.Arguments = arguments;
            shortcut.WorkingDirectory = workingDirectory;
            shortcut.WindowStyle = windowStyle;
            shortcut.IconLocation =
                wshShell.ExpandEnvironmentStrings(iconPath + ", " + iconIndex);

            shortcut.Save();

            shortcut = null;
            wshShell = null;
        }
    }
}