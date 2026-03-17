using MissionPlanner.Utilities;
using System;
using System.Windows.Forms;

namespace MissionPlanner.Controls
{
    public class SB
    {
        public static void Show(string detectedvia)
        {
            // Disabled: no external connections in offline-hardened build
            // Was: CubePilot SmartBattery verification sending serial numbers to discuss.cubepilot.org
            return;
        }
    }
}
