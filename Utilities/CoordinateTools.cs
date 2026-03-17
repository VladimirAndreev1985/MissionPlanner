using System;
using System.Text;
using System.Windows.Forms;
using GeoUtility.GeoSystem;
using MissionPlanner.Controls;
using MissionPlanner.MsgBox;

namespace MissionPlanner.Utilities
{
    /// <summary>
    /// Military coordinate tools: format conversion, range/bearing, point offset
    /// </summary>
    public static class CoordinateTools
    {
        /// <summary>
        /// Format coordinates in all military formats with copy button
        /// </summary>
        public static void ShowCoordinates(double lat, double lng, double alt = 0)
        {
            var sb = new StringBuilder();

            // 1. Decimal degrees
            sb.AppendLine($"WGS-84:  {lat:F7}, {lng:F7}");

            // 2. Degrees Minutes Seconds
            sb.AppendLine($"ГМС:     {ToDMS(lat, true)}, {ToDMS(lng, false)}");

            // 3. Degrees Decimal Minutes
            sb.AppendLine($"ГМ:      {ToDM(lat, true)}, {ToDM(lng, false)}");

            // 4. MGRS
            try {
                var geo = new Geographic(lng, lat);
                var mgrs = (MGRS)geo;
                sb.AppendLine($"MGRS:    {mgrs.ToLongString()}");
            } catch { sb.AppendLine("MGRS:    ошибка"); }

            // 5. UTM
            try {
                var geo = new Geographic(lng, lat);
                var utm = (UTM)geo;
                sb.AppendLine($"UTM:     {utm}");
            } catch { sb.AppendLine("UTM:     ошибка"); }

            // 6. SK-42
            try {
                var sk = SK42.FromWGS84(lat, lng);
                sb.AppendLine($"СК-42:   {sk}");
            } catch { sb.AppendLine("СК-42:   ошибка"); }

            if (alt != 0)
                sb.AppendLine($"Высота:  {alt:F1} м");

            // Always copy to clipboard
            try { Clipboard.SetText(sb.ToString()); } catch { }

            // Show dialog
            CustomMessageBox.Show(sb.ToString(),
                "Координаты точки (скопировано в буфер)", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        /// <summary>
        /// Calculate range (meters) and bearing (degrees) between two points
        /// </summary>
        public static (double range, double bearing) RangeAndBearing(
            double lat1, double lng1, double lat2, double lng2)
        {
            // Haversine formula for distance
            double R = 6371000; // Earth radius in meters
            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLng = (lng2 - lng1) * Math.PI / 180;
            double a = Math.Sin(dLat/2) * Math.Sin(dLat/2) +
                       Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                       Math.Sin(dLng/2) * Math.Sin(dLng/2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
            double range = R * c;

            // Bearing formula
            double lat1Rad = lat1 * Math.PI / 180;
            double lat2Rad = lat2 * Math.PI / 180;
            double dLngRad = dLng;
            double y = Math.Sin(dLngRad) * Math.Cos(lat2Rad);
            double x = Math.Cos(lat1Rad) * Math.Sin(lat2Rad) -
                       Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(dLngRad);
            double bearing = Math.Atan2(y, x) * 180 / Math.PI;
            bearing = (bearing + 360) % 360;

            return (range, bearing);
        }

        /// <summary>
        /// Show range/bearing dialog between two clicked points
        /// </summary>
        public static void ShowRangeAndBearing(double lat1, double lng1, double lat2, double lng2)
        {
            var (range, bearing) = RangeAndBearing(lat1, lng1, lat2, lng2);

            var sb = new StringBuilder();
            sb.AppendLine("── ДАЛЬНОСТЬ И АЗИМУТ ─────────────");
            sb.AppendLine($"Дальность: {range:F0} м ({range/1000:F2} км)");
            sb.AppendLine($"Азимут:    {bearing:F1}°");
            sb.AppendLine($"Обратный:  {(bearing + 180) % 360:F1}°");
            sb.AppendLine();
            sb.AppendLine($"Точка 1: {lat1:F7}, {lng1:F7}");
            sb.AppendLine($"Точка 2: {lat2:F7}, {lng2:F7}");

            try { Clipboard.SetText(sb.ToString()); } catch { }
            CustomMessageBox.Show(sb.ToString(), "Дальность и азимут (скопировано в буфер)");
        }

        /// <summary>
        /// Calculate offset point from known position
        /// </summary>
        public static (double lat, double lng) OffsetPoint(
            double lat, double lng, double bearingDeg, double distanceM)
        {
            double R = 6371000;
            double d = distanceM / R;
            double brng = bearingDeg * Math.PI / 180;
            double lat1 = lat * Math.PI / 180;
            double lng1 = lng * Math.PI / 180;

            double lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(d) +
                                     Math.Cos(lat1) * Math.Sin(d) * Math.Cos(brng));
            double lng2 = lng1 + Math.Atan2(Math.Sin(brng) * Math.Sin(d) * Math.Cos(lat1),
                                              Math.Cos(d) - Math.Sin(lat1) * Math.Sin(lat2));

            return (lat2 * 180 / Math.PI, lng2 * 180 / Math.PI);
        }

        /// <summary>
        /// Show dialog for offset point calculation
        /// </summary>
        public static void ShowOffsetPoint(double baseLat, double baseLng)
        {
            string bearingStr = "";
            if (DialogResult.Cancel == InputBox.Show("Вынос точки", "Азимут (градусы):", ref bearingStr))
                return;
            if (!double.TryParse(bearingStr, out double bearing)) return;

            string distStr = "";
            if (DialogResult.Cancel == InputBox.Show("Вынос точки", "Дальность (метры):", ref distStr))
                return;
            if (!double.TryParse(distStr, out double distance)) return;

            var (newLat, newLng) = OffsetPoint(baseLat, baseLng, bearing, distance);

            // Show coordinates of new point
            ShowCoordinates(newLat, newLng);
        }

        // Helpers for DMS format
        private static string ToDMS(double deg, bool isLat)
        {
            string dir = isLat ? (deg >= 0 ? "С" : "Ю") : (deg >= 0 ? "В" : "З");
            deg = Math.Abs(deg);
            int d = (int)deg;
            int m = (int)((deg - d) * 60);
            double s = (deg - d - m / 60.0) * 3600;
            return $"{d}°{m:D2}'{s:F2}\"{dir}";
        }

        private static string ToDM(double deg, bool isLat)
        {
            string dir = isLat ? (deg >= 0 ? "С" : "Ю") : (deg >= 0 ? "В" : "З");
            deg = Math.Abs(deg);
            int d = (int)deg;
            double m = (deg - d) * 60;
            return $"{d}°{m:F4}'{dir}";
        }
    }
}
