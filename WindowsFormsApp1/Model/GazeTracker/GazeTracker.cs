using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.Win32;
using System.Windows.Forms;




using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using EyeGaze.EyeTracking;
using System.Runtime.InteropServices;

namespace EyeGaze.GazeTracker
{
    class GazeTracker : InterfaceGazeToCoords
    {
        //[DllImport("user32.dll")]
        //private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int x, int y);

        EyeGaze.EyeTracking.MousePoint m = new MousePoint();

        static GazeTracker GT;

        public (double,double) position;
        TcpClient gp3_client;
        NetworkStream data_feed;
        StreamWriter data_write;    
        String incoming_data = "";
        public Boolean initialized = false;

        public GazeTracker()
        {

        }
        public void connect()
        {
            const int ServerPort = 4242;
            const string ServerAddr = "127.0.0.1";
            ConsoleKeyInfo keybinput;

            // Try to create client object, return if no server found
            try
            {
                gp3_client = new TcpClient(ServerAddr, ServerPort);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to connect with error: {0}", e);
                return;
            }
            initialized = true;

        }

        public void disconnect()
        {
            data_write.Close();
            data_feed.Close();
            gp3_client.Close();
        }

        public void listen()
        {
            Thread t = new Thread(() =>
            {
                ConvertGazeToCoords();
            });
            t.Start();

        }

        public void ConvertGazeToCoords()
        {
            //EyeGaze.EyeTracking.debugger l = new debugger();
            //l.Show();
            //l.Refresh();

            bool exit_state = false;
            int startindex, endindex;

            // Load the read and write streams
            data_feed = gp3_client.GetStream();
            data_write = new StreamWriter(data_feed);

            // Setup the data records
            data_write.Write("<SET ID=\"ENABLE_SEND_POG_FIX\" STATE=\"1\" />\r\n");
            data_write.Write("<SET ID=\"ENABLE_SEND_TIME\" STATE=\"1\" />\r\n");
            data_write.Write("<SET ID=\"ENABLE_SEND_DATA\" STATE=\"1\" />\r\n");

            // Flush the buffer out the socket
            data_write.Flush();

            //List<Tuple<double, double>> coordinate_list = new List<Tuple<double, double>>();
            //var coordinate_list = new List<(double x, double t)>();
            double timeSum = 0;
            DateTime start = DateTime.UtcNow;
            DateTime end = DateTime.UtcNow;
            double xAvg;
            double yAvg;
            double xStd;
            double yStd;
            List<double> xlist = new List<double>();
            List<double> ylist = new List<double>();
            int ch=0;

            while (true)
            {
                if(timeSum >= 0.8 && xlist.Count > 0)
                {

                    xAvg = xlist.Average();
                    yAvg = ylist.Average();
                    //Console.WriteLine(xAvg);
                    //Console.WriteLine(yAvg);
                    xStd = GetStandardDeviation(xlist);
                    yStd = GetStandardDeviation(ylist);
                    double std_avarage = (xStd + yStd) / 2;
                    if (std_avarage < 0.05)
                    {
                        position = (xAvg, yAvg);
                    }
                    timeSum = 0;
                    xlist = new List<double>();
                    ylist = new List<double>();
                }
                //start= DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                start = DateTime.UtcNow;

                while((char)ch!='\n')
                {
                    ch = data_feed.ReadByte();
                    incoming_data += (char)ch;
                }
                ch = 0;
                if (ch != -1)
                {
                    //incoming_data += (char)ch;

                    // find string terminator ("\r\n") 
                    if (incoming_data.IndexOf("\r\n") != -1)
                    {
                        // only process DATA RECORDS, ie <REC .... />
                        if (incoming_data.IndexOf("<REC") != -1)
                        {
                            double time_val;
                            double fpogx;
                            double fpogy;
                            int fpog_valid;

                            // Process incoming_data string to extract FPOGX, FPOGY, etc...

                            startindex = incoming_data.IndexOf("FPOGX=\"") + "FPOGX=\"".Length;
                            endindex = incoming_data.IndexOf("\"", startindex);
                            fpogx = Double.Parse(incoming_data.Substring(startindex, endindex - startindex));

                            startindex = incoming_data.IndexOf("FPOGY=\"") + "FPOGY=\"".Length;
                            endindex = incoming_data.IndexOf("\"", startindex);
                            fpogy = Double.Parse(incoming_data.Substring(startindex, endindex - startindex));

                            //Point p = new Point(fpogx, fpogy);
                            xlist.Add(fpogx);
                            ylist.Add(fpogy);

                            //------ paint ----------
                            Point screen = ScreenSize();
                            float scale = getWindowScale();
                            //Console.WriteLine("X: " + (int)((screen.X * fpogx)) + "Y: " + (int)((screen.Y * fpogy)));
                            //Console.WriteLine("MouseX: " + m.GetEyeGazePosition().X + " mouseY: " + m.GetEyeGazePosition().Y);
                            //l.MoveCircle((int)((screen.X * fpogx)), (int)((screen.Y * fpogy)));


                            //position = currPos;

                            //Console.WriteLine("Raw data: {0}", incoming_data);
                            //Console.WriteLine("Processed data: Gaze ({0},{1})", fpogx, fpogy);
                        }

                        incoming_data = "";
                    }
                }
                //end = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                end = DateTime.UtcNow;
                //timeSum += (int)(end - start)/1000;
                timeSum += ((end - start).TotalMilliseconds)/1000;
            }
        }

        public double GetStandardDeviation(IEnumerable<double> values)
        {
            double standardDeviation = 0;
            double[] enumerable = values as double[] ?? values.ToArray();
            int count = enumerable.Count();
            if (count > 1)
            {
                double avg = enumerable.Average();
                double sum = enumerable.Sum(d => (d - avg) * (d - avg));
                standardDeviation = Math.Sqrt(sum / count);
            }
            return standardDeviation;
        }

        public static GazeTracker getInstance()
        {
            if(GT == null)
            {
                GT = new GazeTracker();
            }
            return GT;
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

        public void CalibrateEyes()
        {

            // Load the read and write streams
            data_feed = gp3_client.GetStream();
            data_write = new StreamWriter(data_feed);

            // Setup the data records
            //data_write.Write("<SET ID=\"CALIBRATE_DELAY\" VALUE=\"0.2\" />\r\n");
            data_write.Write("<SET ID=\"CALIBRATE_START\" STATE=\"1\" />\r\n");
            data_write.Write("<SET ID=\"CALIBRATE_SHOW\" STATE=\"1\" />\r\n");
            

            // Flush the buffer out the socket
            data_write.Flush();


            bool calibration_finished = false;
            while(!calibration_finished)
            {

                int ch = data_feed.ReadByte();
                if (ch != -1)
                {
                    incoming_data += (char)ch;

                    // find string terminator ("\r\n") 
                    if (incoming_data.IndexOf("\r\n") != -1)
                    {
                        // only process DATA RECORDS, ie <REC .... />
                        if (incoming_data.IndexOf("<CAL ID=\"CALIB_RESULT\"") != -1)
                        {
                            calibration_finished = true;
                            Console.WriteLine("Raw data: {0}", incoming_data);
                        }

                        incoming_data = "";
                    }
                }
            }
            MouseEvent();

            data_write.Write("<GET ID=\"CALIBRATE_RESULT_SUMMARY\"/>\r\n");
            data_write.Flush();
            while (true)
            {
                int ch = data_feed.ReadByte();
                if (ch != -1)
                {
                    incoming_data += (char)ch;

                    // find string terminator ("\r\n") 
                    if (incoming_data.IndexOf("\r\n") != -1)
                    {
                        if (incoming_data.IndexOf("<") != -1)
                        {
                            Console.WriteLine("Raw data: {0}", incoming_data);
                            break;
                        }

                        incoming_data = "";
                    }
                }
            }

        }

        public void getCalibrationResults()
        {

         
            data_feed = gp3_client.GetStream();
            data_write = new StreamWriter(data_feed);
            data_write.Write("<GET ID=\"CALIBRATE_RESULT_SUMMARY\"/>\r\n");
            data_write.Flush();

            while (true)
            {
                int ch = data_feed.ReadByte();
                if (ch != -1)
                {
                    incoming_data += (char)ch;

                    // find string terminator ("\r\n") 
                    if (incoming_data.IndexOf("\r\n") != -1)
                    {
                        if (incoming_data.IndexOf("<") != -1)
                        {
                            Console.WriteLine("Raw data: {0}", incoming_data);
                            break;
                        }

                        incoming_data = "";
                    }
                }
            }
        }

        public static void MouseEvent()
        {
            SetCursorPos(0, 0);
            uint X = (uint)Cursor.Position.X;
            uint Y = (uint)Cursor.Position.Y;
            
            mouse_event(0x02 | 0x04, X, Y, 0, 0);
        }


    }
}
