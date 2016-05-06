using System;
using System.Windows.Forms;

namespace LoginApplication
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }
        //btn_LogOut Click Event
        private void btn_LogOut_Click(object sender, EventArgs e)
        {
            this.Hide();
            frmLogin fl = new frmLogin();
            fl.Show();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

    }
}
