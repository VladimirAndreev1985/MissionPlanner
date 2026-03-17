using System;
using System.Drawing;
using System.Windows.Forms;
using MissionPlanner.Utilities;

namespace MissionPlanner.Utilities
{
    public static class TabletMode
    {
        private const string SettingKey = "tablet_mode";

        public static bool Enabled
        {
            get => Settings.Instance.GetBoolean(SettingKey, false);
            set => Settings.Instance[SettingKey] = value.ToString();
        }

        // Scale factor for tablet mode (1.5x default)
        public static float ScaleFactor => Enabled ? 1.5f : 1.0f;

        /// <summary>
        /// Apply tablet mode scaling to a form and all child controls recursively
        /// </summary>
        public static void ApplyToForm(Form form)
        {
            if (!Enabled) return;

            ApplyToControl(form);
        }

        private static void ApplyToControl(Control control)
        {
            // Increase font size
            if (control.Font != null)
            {
                control.Font = new Font(control.Font.FontFamily,
                    control.Font.Size * ScaleFactor,
                    control.Font.Style);
            }

            // Increase button minimum size
            if (control is Button btn)
            {
                btn.MinimumSize = new Size(
                    Math.Max(btn.MinimumSize.Width, (int)(60 * ScaleFactor)),
                    Math.Max(btn.MinimumSize.Height, (int)(40 * ScaleFactor)));
                btn.Padding = new Padding(8);
            }

            // Recurse into child controls
            foreach (Control child in control.Controls)
            {
                ApplyToControl(child);
            }
        }
    }
}
