using System;
using System.Windows.Forms;

namespace MunApp.Win32
{
    public class CountDown
    {
        private Timer timer = new Timer();
        private int secs = 90;

        public CountDown()
        {
            timer.Interval = 1000;
            timer.Tick += timer_Tick;
            timer.Enabled = false;
        }

        public event EventHandler Tick;
        public event EventHandler Elapsed;

        public int Seconds { get; set; }
        public int Mins { get; set; }
        public string Time
        {
            get
            {
                string time = "";
                int mins = secs / 60;
                if (mins < 10)
                    time += "0";
                time += mins;
                time += ":";
                if (secs % 60 < 10)
                    time += "0";
                time += secs % 60;
                return time;
            }
        }

        public bool Running
        {
            get
            {
                return timer.Enabled;
            }
        }

        public void Start()
        {
            timer.Start();
        }

        public void Pause()
        {
            timer.Stop();
        }

        public void Stop()
        {
            secs = Mins * 60 + Seconds;
            timer.Stop();
        }

        public void Reset(int mins, int secs)
        {
            Mins = mins;
            Seconds = secs;
            Stop();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            secs--;
            if (Tick != null)
                Tick(this, EventArgs.Empty);
            if (secs <= 0)
            {
                secs = 0;
                Pause();
                if (Elapsed != null)
                    Elapsed(this, EventArgs.Empty);
            }
        }
    }
}
