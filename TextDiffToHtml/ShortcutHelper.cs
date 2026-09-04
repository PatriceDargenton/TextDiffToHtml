
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
                workingDirectory = Path.GetDirectoryName(targetPath) ?? string.Empty;

            if (string.IsNullOrEmpty(iconPath))
                iconPath = targetPath;

            var wshType = System.Type.GetTypeFromProgID("WScript.Shell");
            if (wshType is null)
            {
                MessageBox.Show(
                    "WScript.Shell not found.",
                    "Error creating shortcut",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            dynamic? wshShell = Activator.CreateInstance(wshType);
            if (wshShell is null)
            {
                MessageBox.Show(
                    "Can't create WScript.Shell.",
                    "Error creating shortcut",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            dynamic? shortcut = wshShell.CreateShortcut(shortcutPath);
            if (shortcut is null)
            {
                MessageBox.Show(
                    "Can't create the shortcut.",
                    "Error creating shortcut",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

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