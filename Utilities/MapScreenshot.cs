using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using GMap.NET.WindowsForms;
using MissionPlanner.Controls;

namespace MissionPlanner.Utilities
{
    /// <summary>
    /// Capture map screenshot with drone position and timestamp overlay
    /// </summary>
    public static class MapScreenshot
    {
        public static string Capture(GMapControl mapControl)
        {
            if (mapControl == null) return null;

            // Capture map as bitmap
            var bmp = new Bitmap(mapControl.Width, mapControl.Height);
            mapControl.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));

            // Add overlay text
            using (var g = Graphics.FromImage(bmp))
            {
                var cs = MainV2.comPort.MAV.cs;

                // Build overlay text
                string overlay = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss} | " +
                                 $"{cs.lat:F6}, {cs.lng:F6} | " +
                                 $"Alt:{cs.altasl:F0}м | " +
                                 $"Spd:{cs.groundspeed:F0}м/с | " +
                                 $"Hdg:{cs.yaw:F0}°";

                var font = new Font("Consolas", 10, FontStyle.Bold);
                var textSize = g.MeasureString(overlay, font);

                // Draw semi-transparent black background strip at bottom
                g.FillRectangle(new SolidBrush(Color.FromArgb(180, 0, 0, 0)),
                    0, bmp.Height - textSize.Height - 8, bmp.Width, textSize.Height + 8);

                // Draw text in white
                g.DrawString(overlay, font, Brushes.White, 4, bmp.Height - textSize.Height - 4);

                font.Dispose();
            }

            // Save
            var filename = $"MapCapture_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var dir = Path.Combine(Settings.GetDataDirectory(), "screenshots");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, filename);
            bmp.Save(path, ImageFormat.Png);
            bmp.Dispose();

            return path;
        }

        public static void CaptureAndShow(GMapControl mapControl)
        {
            var path = Capture(mapControl);
            if (path != null)
            {
                CustomMessageBox.Show($"Скриншот сохранён:\n{path}", "Захват карты");
            }
            else
            {
                CustomMessageBox.Show("Ошибка захвата карты.", "Ошибка");
            }
        }
    }
}
