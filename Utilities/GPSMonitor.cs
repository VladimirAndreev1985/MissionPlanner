using System;

namespace MissionPlanner.Utilities
{
    /// <summary>
    /// GPS spoofing detection and signal quality monitoring.
    /// MONITORING ONLY — does NOT send any commands to the drone.
    /// </summary>
    public static class GPSMonitor
    {
        // Previous position for jump detection
        private static double _prevLat, _prevLng;
        private static DateTime _prevTime = DateTime.MinValue;
        private static bool _initialized = false;

        // Alert state
        public static bool SpoofingAlert { get; private set; }
        public static string AlertMessage { get; private set; } = "";
        public static DateTime LastAlertTime { get; private set; }

        // Thresholds (configurable)
        public static double MaxJumpMeters { get; set; } = 500;  // Max position jump in 1 second
        public static double MaxSpeedMps { get; set; } = 100;    // Max realistic speed m/s (~360 km/h)
        public static int MinSatCount { get; set; } = 6;         // Minimum satellites for reliable fix
        public static double MaxHDOP { get; set; } = 3.0;        // Maximum acceptable HDOP

        /// <summary>
        /// Called every telemetry update cycle. Checks for anomalies.
        /// Returns true if alert triggered.
        /// </summary>
        public static bool Update(double lat, double lng, double groundspeed,
                                   int satcount, double hdop, int gpsstatus)
        {
            SpoofingAlert = false;
            AlertMessage = "";

            if (!_initialized)
            {
                _prevLat = lat;
                _prevLng = lng;
                _prevTime = DateTime.Now;
                _initialized = true;
                return false;
            }

            var now = DateTime.Now;
            var dt = (now - _prevTime).TotalSeconds;
            if (dt < 0.1) return false; // Too fast, skip

            // 1. Position jump detection
            double dlat = (lat - _prevLat) * 111320;
            double dlng = (lng - _prevLng) * 111320 * Math.Cos(lat * Math.PI / 180);
            double jumpM = Math.Sqrt(dlat * dlat + dlng * dlng);
            double impliedSpeed = jumpM / dt;

            if (jumpM > MaxJumpMeters && dt < 2.0)
            {
                SpoofingAlert = true;
                AlertMessage = string.Format("СПУФИНГ? Прыжок {0:F0}м за {1:F1}с", jumpM, dt);
                LastAlertTime = now;
            }

            // 2. Unrealistic speed (GPS says slow but jump says fast, or vice versa)
            if (impliedSpeed > MaxSpeedMps && groundspeed < impliedSpeed * 0.3)
            {
                SpoofingAlert = true;
                AlertMessage = string.Format("СПУФИНГ? Скорость расч. {0:F0} м/с, GPS {1:F0} м/с",
                    impliedSpeed, groundspeed);
                LastAlertTime = now;
            }

            // 3. Satellite count dropped suddenly
            // (just warning, not necessarily spoofing)

            // 4. HDOP spike
            if (hdop > MaxHDOP && gpsstatus >= 3) // 3 = 3D fix
            {
                // High HDOP with 3D fix is suspicious
                AlertMessage = string.Format("GPS: HDOP {0:F1} (порог {1:F1})", hdop, MaxHDOP);
            }

            _prevLat = lat;
            _prevLng = lng;
            _prevTime = now;

            return SpoofingAlert;
        }

        /// <summary>
        /// Get signal quality summary string for status display
        /// </summary>
        public static string GetSignalStatus(int satcount, double hdop,
                                              int rssi, int remrssi, int linkquality)
        {
            var sb = new System.Text.StringBuilder();

            // GPS quality
            if (satcount < MinSatCount)
                sb.AppendFormat("GPS:{0}! ", satcount);
            else
                sb.AppendFormat("GPS:{0} ", satcount);

            sb.AppendFormat("HDOP:{0:F1} ", hdop);

            // Radio link
            if (rssi > 0)
                sb.AppendFormat("RSSI:{0} ", rssi);
            if (remrssi > 0)
                sb.AppendFormat("RemRSSI:{0} ", remrssi);
            if (linkquality > 0)
                sb.AppendFormat("LQ:{0}% ", linkquality);

            return sb.ToString();
        }

        public static void Reset()
        {
            _initialized = false;
            SpoofingAlert = false;
            AlertMessage = "";
        }
    }
}
