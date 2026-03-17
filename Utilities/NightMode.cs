using System.Drawing;
using System.Windows.Forms;

namespace MissionPlanner.Utilities
{
    /// <summary>
    /// Minimal screen brightness mode for night operations.
    /// Red-on-black theme to preserve night vision.
    /// MONITORING ONLY — display change, no flight commands.
    /// </summary>
    public static class NightMode
    {
        private const string SettingKey = "night_mode";

        public static bool Enabled
        {
            get => Settings.Instance.GetBoolean(SettingKey, false);
            set { Settings.Instance[SettingKey] = value.ToString(); }
        }

        private static Color NightBG = Color.FromArgb(10, 0, 0);
        private static Color NightText = Color.FromArgb(80, 0, 0);
        private static Color NightAccent = Color.FromArgb(120, 0, 0);

        public static void Toggle(Form form)
        {
            Enabled = !Enabled;
            if (Enabled)
                ApplyNightTheme(form);
            else
                ThemeManager.ApplyThemeTo(form); // restore normal theme
        }

        public static void ApplyNightTheme(Control control)
        {
            control.BackColor = NightBG;
            control.ForeColor = NightText;

            foreach (Control child in control.Controls)
            {
                ApplyNightTheme(child);
            }
        }
    }
}
