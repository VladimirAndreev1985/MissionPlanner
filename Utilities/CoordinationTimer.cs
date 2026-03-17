using System;
using System.Windows.Forms;
using System.Drawing;

namespace MissionPlanner.Utilities
{
    /// <summary>
    /// Countdown timer for coordinating multi-operator actions
    /// </summary>
    public class CoordinationTimer : Form
    {
        private Label lblTimer;
        private Timer timer;
        private DateTime targetTime;
        private bool expired = false;

        public CoordinationTimer(int seconds)
        {
            this.Text = "Таймер координации";
            this.Size = new Size(400, 200);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.BackColor = Color.Black;

            lblTimer = new Label();
            lblTimer.Font = new Font("Consolas", 48, FontStyle.Bold);
            lblTimer.ForeColor = Color.Lime;
            lblTimer.BackColor = Color.Black;
            lblTimer.TextAlign = ContentAlignment.MiddleCenter;
            lblTimer.Dock = DockStyle.Fill;
            this.Controls.Add(lblTimer);

            targetTime = DateTime.Now.AddSeconds(seconds);

            timer = new Timer();
            timer.Interval = 100;
            timer.Tick += (s, e) =>
            {
                var remaining = targetTime - DateTime.Now;
                if (remaining.TotalSeconds <= 0)
                {
                    if (!expired)
                    {
                        expired = true;
                        lblTimer.Text = "ПУСК!";
                        lblTimer.ForeColor = Color.Red;
                        // Beep alert
                        try
                        {
                            Console.Beep(2000, 1000);
                        }
                        catch { }
                    }
                }
                else
                {
                    lblTimer.Text = $"{(int)remaining.TotalMinutes:D2}:{remaining.Seconds:D2}.{remaining.Milliseconds / 100}";
                }
            };
            timer.Start();
        }

        public static void ShowTimer()
        {
            string input = "180";
            if (InputBox.Show("Таймер координации", "Секунд до действия:", ref input) == DialogResult.OK)
            {
                if (int.TryParse(input, out int seconds) && seconds > 0)
                {
                    new CoordinationTimer(seconds).Show();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) timer?.Dispose();
            base.Dispose(disposing);
        }
    }
}
