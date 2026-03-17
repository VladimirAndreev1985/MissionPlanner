using System;

namespace MissionPlanner.Utilities
{
    /// <summary>
    /// Estimates whether drone has enough battery to return home.
    /// MONITORING ONLY — alerts operator, does NOT send commands.
    /// </summary>
    public static class PointOfNoReturn
    {
        public static double SafetyMarginPercent { get; set; } = 20; // 20% reserve

        public enum ReturnState { Safe, Warning, Critical }

        public static ReturnState CurrentState { get; private set; }
        public static string StatusText { get; private set; } = "";
        public static double EstimatedReturnTimeSec { get; private set; }
        public static double EstimatedBatteryOnReturn { get; private set; }

        /// <summary>
        /// Update calculation. Call on each telemetry cycle.
        /// </summary>
        public static ReturnState Update(double distToHomeM, double groundspeedMs,
            double batteryRemaining, double currentAmps, double batteryCapacityMah)
        {
            // Estimate time to reach home
            if (groundspeedMs < 1) groundspeedMs = 5; // assume minimum 5 m/s
            EstimatedReturnTimeSec = distToHomeM / groundspeedMs;

            // Estimate battery consumption during return (mAh)
            double consumptionPerSec = (currentAmps * 1000) / 3600; // mAh/s
            double returnConsumptionMah = consumptionPerSec * EstimatedReturnTimeSec;
            double returnConsumptionPercent = (batteryCapacityMah > 0)
                ? (returnConsumptionMah / batteryCapacityMah) * 100
                : EstimatedReturnTimeSec * 0.5; // rough estimate: 0.5%/sec

            EstimatedBatteryOnReturn = batteryRemaining - returnConsumptionPercent;

            if (EstimatedBatteryOnReturn < SafetyMarginPercent / 2)
            {
                CurrentState = ReturnState.Critical;
                StatusText = $"КРИТИЧНО! Батарея при возврате: {EstimatedBatteryOnReturn:F0}%";
            }
            else if (EstimatedBatteryOnReturn < SafetyMarginPercent)
            {
                CurrentState = ReturnState.Warning;
                StatusText = $"ВНИМАНИЕ: батарея при возврате ~{EstimatedBatteryOnReturn:F0}%";
            }
            else
            {
                CurrentState = ReturnState.Safe;
                StatusText = $"Возврат: ~{EstimatedReturnTimeSec:F0}с, остаток ~{EstimatedBatteryOnReturn:F0}%";
            }

            return CurrentState;
        }
    }
}
