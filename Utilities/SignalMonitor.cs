using System;

namespace MissionPlanner.Utilities
{
    /// <summary>
    /// Radio link and failsafe monitoring with audio alerts.
    /// MONITORING ONLY — does NOT send any commands.
    /// </summary>
    public static class SignalMonitor
    {
        private static DateTime _lastPacketTime = DateTime.Now;
        private static int _noPacketSeconds = 0;

        // Configurable thresholds
        public static int RSSIWarningLevel { get; set; } = 50;
        public static int RSSICriticalLevel { get; set; } = 20;
        public static int NoPacketWarningSeconds { get; set; } = 3;

        public enum SignalState { Good, Warning, Critical, Lost }

        public static SignalState CurrentState { get; private set; } = SignalState.Good;
        public static string StatusText { get; private set; } = "";

        public static SignalState Update(int rssi, int remrssi, bool connected)
        {
            if (!connected)
            {
                CurrentState = SignalState.Lost;
                StatusText = "НЕТ СВЯЗИ";
                return CurrentState;
            }

            // Check RSSI levels
            int effectiveRssi = Math.Max(rssi, remrssi);

            if (effectiveRssi > 0 && effectiveRssi < RSSICriticalLevel)
            {
                CurrentState = SignalState.Critical;
                StatusText = string.Format("КРИТИЧНО: RSSI {0}", effectiveRssi);
            }
            else if (effectiveRssi > 0 && effectiveRssi < RSSIWarningLevel)
            {
                CurrentState = SignalState.Warning;
                StatusText = string.Format("ВНИМАНИЕ: RSSI {0}", effectiveRssi);
            }
            else
            {
                CurrentState = SignalState.Good;
                StatusText = string.Format("Связь: OK (RSSI {0})", effectiveRssi);
            }

            return CurrentState;
        }
    }
}
