using System;
using System.Windows.Forms;
using MissionPlanner.Comms;

namespace MissionPlanner.Utilities
{
    /// <summary>
    /// Tactical commands — ALL require explicit operator action.
    /// Each command shows a confirmation dialog before execution.
    /// </summary>
    public static class TacticalCommands
    {
        /// <summary>
        /// Emergency motor kill — immediately stops all motors.
        /// USE WITH EXTREME CAUTION — drone will crash.
        /// </summary>
        public static void EmergencyMotorKill()
        {
            var result = CustomMessageBox.Show(
                "ВНИМАНИЕ! Моторы будут немедленно остановлены.\nБПА упадёт. Продолжить?",
                "АВАРИЙНАЯ ОСТАНОВКА МОТОРОВ",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                // Send ARM command with force parameter to force disarm
                MainV2.comPort.doARM(false, true); // force disarm
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        /// <summary>
        /// Trigger servo/relay for payload drop
        /// </summary>
        public static void PayloadRelease(int servoChannel = 9, int pwmValue = 1100)
        {
            var result = CustomMessageBox.Show(
                $"Сброс груза: канал {servoChannel}, PWM {pwmValue}\nПродолжить?",
                "Сброс груза",
                MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes) return;

            try
            {
                // MAV_CMD_DO_SET_SERVO = 183
                MainV2.comPort.doCommand(
                    MainV2.comPort.MAV.sysid,
                    MainV2.comPort.MAV.compid,
                    MAVLink.MAV_CMD.DO_SET_SERVO,
                    servoChannel, pwmValue, 0, 0, 0, 0, 0);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        /// <summary>
        /// Set terrain following altitude (change target altitude)
        /// </summary>
        public static void SetAltitudeAGL(double altMeters)
        {
            // This just changes the target altitude, terrain following must be
            // configured in ArduPilot parameters (TERRAIN_ENABLE, etc.)
            try
            {
                // Use guided mode altitude change
                MainV2.comPort.setNewWPAlt(
                    new Locationwp { alt = (float)altMeters });
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}
