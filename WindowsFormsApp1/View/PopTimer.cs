using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EyeGaze;
using EyeGaze.Engine;
using EyeGaze.SpeechToText;

namespace EyeGaze
{
    public class PopTimer
    {
        private PopUpDisappear popupDisappear;
        public PopTimer(String message)
        {
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 2000;
            timer.Tick += new EventHandler(timer_Tick);
            popupDisappear = new PopUpDisappear(message);
            popupDisappear.Show();
            popupDisappear.TopMost = true;
            timer.Start();
        }

        async void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                while (popupDisappear.Opacity > 0.0)
                {
                    await Task.Delay(50);
                    popupDisappear.Opacity -= 0.05;
                }
                popupDisappear.Close();
                popupDisappear.SendToBack();
                popupDisappear.TopMost = false;
            }
            catch (Exception)
            {

            }
        }
    }
}
