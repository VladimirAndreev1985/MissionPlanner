using System;
using System.Threading;
using System.Threading.Tasks;

namespace MissionPlanner.Utilities
{
    /// <summary>
    /// Simple voice/tone alerts using Console.Beep.
    /// MONITORING ONLY — audio alerts, no flight commands.
    /// </summary>
    public static class VoiceAlerts
    {
        private static DateTime _lastAlert = DateTime.MinValue;
        private static int _minIntervalSec = 5; // Don't spam alerts

        public static void AlertGPSSpoofing()
        {
            if ((DateTime.Now - _lastAlert).TotalSeconds < _minIntervalSec) return;
            _lastAlert = DateTime.Now;
            // Three rapid beeps for GPS alert
            Task.Run(() => {
                try {
                    Console.Beep(2000, 200);
                    Console.Beep(2000, 200);
                    Console.Beep(2000, 200);
                } catch {}
            });
        }

        public static void AlertSignalLost()
        {
            if ((DateTime.Now - _lastAlert).TotalSeconds < _minIntervalSec) return;
            _lastAlert = DateTime.Now;
            Task.Run(() => {
                try {
                    Console.Beep(800, 500);
                    Console.Beep(800, 500);
                } catch {}
            });
        }

        public static void AlertBatteryCritical()
        {
            if ((DateTime.Now - _lastAlert).TotalSeconds < _minIntervalSec) return;
            _lastAlert = DateTime.Now;
            Task.Run(() => {
                try {
                    Console.Beep(1500, 300);
                    Thread.Sleep(100);
                    Console.Beep(1000, 300);
                    Thread.Sleep(100);
                    Console.Beep(500, 500);
                } catch {}
            });
        }

        public static void AlertPointOfNoReturn()
        {
            if ((DateTime.Now - _lastAlert).TotalSeconds < _minIntervalSec) return;
            _lastAlert = DateTime.Now;
            Task.Run(() => {
                try {
                    for (int i = 0; i < 5; i++) {
                        Console.Beep(1200, 150);
                        Thread.Sleep(100);
                    }
                } catch {}
            });
        }
    }
}
