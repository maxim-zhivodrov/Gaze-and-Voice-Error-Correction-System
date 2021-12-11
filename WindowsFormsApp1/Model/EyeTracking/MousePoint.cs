using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EyeGaze.EyeTracking
{
    class MousePoint : EyeGazeInterface
    {
      

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public override Point GetEyeGazePosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            Console.WriteLine(lpPoint);

            return lpPoint;
        }
    }
}