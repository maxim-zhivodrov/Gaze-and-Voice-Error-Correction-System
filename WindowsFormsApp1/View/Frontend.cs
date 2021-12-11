using EyeGaze.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Windows.Forms;

namespace EyeGaze
{
    public partial class Frontend : Form
    {
        Panel[] panels = new Panel[2];
        (Panel panel,int index) active_panel;
        private List<PopUpDisappear> popupDisappearQueue;
        Controller controller;
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();



        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
           int nLeftRect,     // x-coordinate of upper-left corner
           int nTopRect,      // y-coordinate of upper-left corner
           int nRightRect,    // x-coordinate of lower-right corner
           int nBottomRect,   // y-coordinate of lower-right corner
           int nWidthEllipse, // height of ellipse
           int nHeightEllipse // width of ellipse
        );

        public Frontend(Controller c)
        {
            InitializeComponent();
            this.controller = c;
            popupDisappearQueue = new List<PopUpDisappear>();
            this.speechToTextCombox.SelectedIndex = 0;
            this.spellCheckerComboBox.SelectedIndex = 0;

        }

        public void handlerMessageFromEngine(object sender, MessageEvent e)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {                        //change back to main thread
                    if (e.type == MessageEvent.messageType.WrongAuthentication)
                    {
                        this.TopMost = true;
                        showPopUp(e.message);
                        start_button.Enabled = true;
                        //if (this.thread != null)
                        //    this.thread.Abort();
                    }
                    if (e.type == MessageEvent.messageType.ConnectionFail)
                    {
                        PopTimer pt = new PopTimer(e.message);
                    }
                    if (e.type == MessageEvent.messageType.TriggerWord)
                    {
                        PopTimer pt = new PopTimer(e.message);
                    }
                    if (e.type == MessageEvent.messageType.closeFile)
                    {
                        this.start_button.Enabled = true;
                    }
                });
                return;
            }
        }

        private void showPopUp(string message)
        {
            PopUp popup = new PopUp();
            popup.changeLabel(message);
            popup.TopMost = true;
            popup.Show();
        }


        void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                PopUpDisappear p = popupDisappearQueue.First();
                popupDisappearQueue.RemoveAt(0);
                p.Close();
                p.SendToBack();
                p.TopMost = false;
            }
            catch (Exception)
            {

            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Frontend_Load(object sender, EventArgs e)
        {
            //pictureBox2.Image = SetImageOpacity(pictureBox2.Image, (float)0.5);
            //pictureBox2.Image.RotateFlip(RotateFlipType.Rotate180FlipY);
            pictureBox3.Image.RotateFlip(RotateFlipType.Rotate180FlipX);
            calibrationButton.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, calibrationButton.Width, calibrationButton.Height, 70, 70));
            calibrationButton.TabStop = false;
            calibrationButton.FlatStyle = FlatStyle.Flat;
            calibrationButton.FlatAppearance.BorderSize = 0;
            active_panel = (starterpanel,-1);
            panels[0] = filepanel;
            panels[1] = calibrationpanel;
            //panels[2] = settingspanel;
            for(int i=1;i<panels.Length;i++)
            {
                //panels[i].panel.SendToBack();
                bunifuTransition2.HideSync(panels[i], animation: BunifuAnimatorNS.Animation.Mosaic);
            }
            starterpanel.BringToFront();
            bunifuElipse1.ApplyElipse(start_button);
            bunifuElipse1.ApplyElipse(starterpanel);
            bunifuTransition2.HideSync(settingspanel);

            //pictureBox3.SendToBack();



        }

        public Image SetImageOpacity(Image image, float opacity)
        {
            Bitmap bmp = new Bitmap(image.Width, image.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix33 = opacity;
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default,
                                                  ColorAdjustType.Bitmap);
                g.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height),
                                   0, 0, image.Width, image.Height,
                                   GraphicsUnit.Pixel, attributes);
            }
            return bmp;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //filepanel.Visible = false;
            //calibrationpanel.Visible = true;
            bunifuTransition2.HideSync(filepanel, animation: BunifuAnimatorNS.Animation.Mosaic);
            //filepanel.Visible = false;

            bunifuTransition1.ShowSync(calibrationpanel, animation: BunifuAnimatorNS.Animation.HorizSlide);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //calibrationpanel.Visible = false;
            //filepanel.Visible = true;
            bunifuTransition2.HideSync(calibrationpanel, animation:BunifuAnimatorNS.Animation.Mosaic);
            //calibrationpanel.Visible = false;
            bunifuTransition1.ShowSync(filepanel, animation: BunifuAnimatorNS.Animation.HorizSlide);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //foreach (Form form in Application.OpenForms)
            //{
            //    form.WindowState = FormWindowState.Minimized;
            //}
            Application.Exit();
        }
        
        private void button7_Click(object sender, EventArgs e)
        {
            bunifuTransition3.Hide(starterpanel, animation: BunifuAnimatorNS.Animation.Mosaic);
            //left_arrow.Visible = true;
            //right_arrow.Visible = true;
            active_panel = (panels[0], 0);
            //starterpanel.Visible = false;
            //bunifuTransition1.ShowSync(filepanel);

        }

        private void right_arrow_Click(object sender, EventArgs e)
        {
            left_arrow.Visible = true;
            if (active_panel.panel == starterpanel)
            {
                active_panel = (panels[0], 0);
                activatePanel(0);
            }
            if (active_panel.index + 1 == panels.Length - 1)
            {
                right_arrow.Visible = false;
                start_button.Visible = true;
            }
            activatePanel(active_panel.index + 1);
        }

        private void left_arrow_Click(object sender, EventArgs e)
        {
            right_arrow.Visible = true;
            if (active_panel.panel == starterpanel)
            {
                active_panel = (panels[0], 0);
                activatePanel(0);
            }
            if (active_panel.index - 1 == 0)
                left_arrow.Visible = false;
            activatePanel(active_panel.index - 1);
        }

        private void activatePanel(int index)
        {
            bunifuTransition2.HideSync(active_panel.panel, animation: BunifuAnimatorNS.Animation.Mosaic);
            active_panel = (panels[index], index);
            bunifuTransition1.ShowSync(active_panel.panel, animation: BunifuAnimatorNS.Animation.HorizSlide);
            
        }

        private void srLabel_Click(object sender, EventArgs e)
        {

        }

        private void scLabel_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void speechToTextCombox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string chosenSpeechToText = speechToTextCombox.SelectedItem.ToString();
            switch (chosenSpeechToText)
            {
                case "Microsoft Azure Cloud (Recommended)":
                    controller.key = "69a12462814f4df1a7b1d38c67963adf";
                    controller.region = "westeurope";
                    controller.speechToText = "EyeGaze.SpeechToText.MicrosoftCloudSpeechToText";
                    break;
                case "IBM Watson Cloud":
                    controller.key = "y6vLeWANkSwvbLXmqRihAMudCQS_r7zuOQUDR28O6AaB";
                    controller.region = "wss://api.eu-gb.speech-to-text.watson.cloud.ibm.com/instances/6ca2c958-936a-4b8a-a41c-76011cc0a451/v1/recognize";
                    controller.speechToText = "EyeGaze.SpeechToText.IBMCloudSpeechToText";
                    break;
                case "System Lib (Offline)":
                    controller.speechToText = "EyeGaze.SpeechToText.SystemLibSpeechToText";
                    break;
            }
        }

        private void start_button_Click(object sender, EventArgs e)
        {
            if (controller.path.Equals(""))
            {
                showPopUp("Please choose a file to work with.");
            }
            else if (speechToTextCombox.SelectedItem.ToString().Equals(""))
            {
                showPopUp("Please choose speech to text to work with.");
            }
            else if (speechToTextCombox.SelectedItem.ToString() != "System Lib (Offline)" && (controller.key.Equals("") || controller.region.Equals("")))
            {
                showPopUp("Please enter key and info of the cloud.\n    Go to \"Credentials\"");
                //TODO: check if there are more endings for a word file
            }
            else if (!controller.path.EndsWith(".docx"))
            {
                showPopUp("The text editor that is chosen is Word. Please choose a Word file to work with. It should have a .docx ending");
            }
            else
            {
                start_button.Enabled = false;
                this.TopMost = false;
                string spellChecker = controller.getSpellChecker(spellCheckerComboBox.SelectedItem.ToString());
                controller.StartProgram(spellChecker, controller.speechToText, "VoiceGaze");

            }
        }

        private void openFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Word Documents|*.docx";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                controller.path = ofd.FileName;
                WorkspaceText.Text = controller.path;
            }
            WorkspaceText.Visible = true;
            Application.DoEvents();
            Thread.Sleep(300);
            right_arrow_Click(null,null);
        }

        private void Frontend_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void settings_button_Click(object sender, EventArgs e)
        {
            if (this.Controls.GetChildIndex(settingspanel) == 0)
            {
                settings_button.BackColor = Color.White;

                bunifuTransition2.HideSync(settingspanel, animation: BunifuAnimatorNS.Animation.VertSlide);
                settingspanel.SendToBack();

            }
            else
            {
                settings_button.BackColor = Color.Gainsboro;
                settingspanel.BringToFront();
                bunifuTransition2.ShowSync(settingspanel, animation: BunifuAnimatorNS.Animation.VertSlide);
            }
            

        }

        private void calibrationButton_Click(object sender, EventArgs e)
        {
            GazeTracker.GazeTracker GT = GazeTracker.GazeTracker.getInstance();
            GT.connect();
            GT.CalibrateEyes();
            //GT.disconnect();
            //GT.connect();
            //Thread.Sleep(500);
            //GT.getCalibrationResults();
            GT.disconnect();
        }
    }
}
