using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace EyeGaze.EyeTracking
{
    class debugger : Form
    {
        // Location for the circle
        public int x = 0;
        public int y = 0;

        public debugger()
        {
            InitializeComponent();
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;
            this.TransparencyKey = Color.White;
            this.Bounds = Screen.PrimaryScreen.Bounds;

            this.Paint += Layer_Paint;
        }

        private void Layer_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (Pen pen = new Pen(Color.Blue))
            {
                var screen = Screen.PrimaryScreen.Bounds;
                // Set format of string.
                StringFormat drawFormat = new StringFormat();
                drawFormat.LineAlignment = StringAlignment.Center;
                drawFormat.Alignment = StringAlignment.Center;
                pen.Width = 2.0F;
                //g.DrawString("1", new Font("Arial", 250), new SolidBrush(Color.FromArgb(255, 0, 0, 255)), screen.Width / 2, screen.Height / 2, drawFormat);
                //g.DrawEllipse(pen, screen.Width * 3 / 8, screen.Height * 2 / 8, 400, 400);
                g.DrawEllipse(pen, new Rectangle(this.x, this.y, 30, 30));
            }
        }

        public void MoveCircle(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.Refresh();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Layer
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "Layer";
            this.Load += new System.EventHandler(this.Layer_Load);
            this.ResumeLayout(false);

        }

        private void Layer_Load(object sender, EventArgs e)
        {

        }
    }
}
