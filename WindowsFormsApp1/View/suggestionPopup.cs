using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Drawing.Drawing2D;

namespace EyeGaze
{
    partial class suggestionPopup : Form
    {
        List<Label> labels;
         String chosenWord;

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

        public suggestionPopup(int x, int y, List<String> suggestions)
        {
            InitializeComponent();

            labels = new List<Label>();

            int formLength = 30;
            for (int i = 0; i < Math.Min(suggestions.Count,5); i++)
            {
                Label l = new Label();
                l.Font = new Font("Arial", 18, FontStyle.Bold);

                if (i == 0)
                {
                    l.Location = new Point(16,12);
                    l.Text = i + 1 + ") " + suggestions[i];
                }
                else
                {
                    Label prevLabel = labels[labels.Count - 1];
                    float prevWidth = CreateGraphics().MeasureString(prevLabel.Text, prevLabel.Font, 495).Width;
                   
                    l.Location = new Point(prevLabel.Location.X + (int)prevWidth+50, prevLabel.Location.Y);
                    l.Text = (i + 1) + ") " + suggestions[i];
                }
                Size textSize = TextRenderer.MeasureText(l.Text, l.Font);
                l.Width = textSize.Width;
                l.Size = textSize;
                //formLength += textSize.Width;
                formLength += l.Size.Width;



                l.Click+= new EventHandler(LB_Click);
                l.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, l.Width+50, l.Height, 10, 10));
                l.TextAlign = ContentAlignment.MiddleCenter;
                l.Refresh();

                this.Controls.Add(l);
                labels.Add(l);
            }
            this.Size = new Size(formLength, this.Size.Height);

            // change form location
            this.StartPosition = FormStartPosition.Manual;
            this.Left = x;
            this.Top = y;

            // round edges
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));


        }

        public void chooseWord(int pos)
        {
            foreach(Label label in labels)
            {
                label.BackColor = SystemColors.GradientActiveCaption;
            }
            Label chosenLabel = labels[pos - 1];
            chosenLabel.BackColor = Color.FromArgb(235, 108, 108);
            chosenWord = chosenLabel.Text;
            Application.DoEvents();
        }

        protected void LB_Click(object sender, EventArgs e)
        {
            //attempt to cast the sender as a label
            Label lbl = sender as Label;
            foreach (Label label in labels)
            {
                label.BackColor = SystemColors.GradientActiveCaption;
            }
            lbl.BackColor = Color.FromArgb(235, 108, 108);

            Application.DoEvents();

        }


        private void suggestionPopup_Load(object sender, EventArgs e)
        {

        }

    }
}
