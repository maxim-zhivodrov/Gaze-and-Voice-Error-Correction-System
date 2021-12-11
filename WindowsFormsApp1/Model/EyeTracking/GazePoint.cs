using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace EyeGaze.EyeTracking
{
    class GazePoint : EyeGazeInterface
    {
        public override Point GetEyeGazePosition()
        {
            //Point gazePoint = getGaze();
            Point screen = ScreenSize();
            float scale = getWindowScale();
            GazeTracker.GazeTracker gt = GazeTracker.GazeTracker.getInstance();
            (double, double) position = gt.position;
            Console.WriteLine("first "+position.Item1+" "+position.Item2);
            Point returnPoint = new Point((int)((screen.X * position.Item1) / scale), (int)((screen.Y * position.Item2) / scale));
            Console.WriteLine("second " + returnPoint.X + " " + returnPoint.Y);
            Console.WriteLine(returnPoint);
            return returnPoint;
        }

        private float getWindowScale()
        {
            float currentDPI = (int)Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Desktop\\WindowMetrics", "AppliedDPI", 96);
            float scale = currentDPI / 96;
            return scale;
        }

        public Point ScreenSize()
        {
            var screen = Screen.PrimaryScreen.Bounds;
            Point point = new Point();
            point.X = screen.Width;
            point.Y = screen.Height;
            return point;
        }
    }
}
