using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EyeGaze
{
    public partial class PopUp : Form
    {
        public PopUp()
        {
            InitializeComponent();
        }

        public void changeLabel(string message)
        {
            this.popUpmessageLable.Text = message;
        }


        private void OKButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
