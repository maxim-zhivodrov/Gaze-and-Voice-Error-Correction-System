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
    public partial class PopUpDisappear : Form
    {
        public PopUpDisappear(string message)
        {
            InitializeComponent();
            this.label1.Text = message;
        }
    
        private void popUpmessageLable_TextChanged(object sender, EventArgs e)
        {
    
        }
    
        private void label1_Click(object sender, EventArgs e)
        {
    
        }
    
        private void PopUpDisappear_Load(object sender, EventArgs e)
        {
    
        }
    }
}
