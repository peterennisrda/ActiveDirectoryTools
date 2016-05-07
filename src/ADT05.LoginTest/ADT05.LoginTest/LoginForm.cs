using System;
using System.Windows.Forms;

namespace ADT05.LoginTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void ClearForm1()
        {
            txtSAMAccountName.Text = "";
            txtPassword.Text = "";
        }

        private void btnValidate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSAMAccountName.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("User Name and Password is required.");
                return;
            }
            try
            {
                bool IsAuthenticated = false;
                //Validation v = new Validation();

                IsAuthenticated = Validation.IsUserValidated(txtSAMAccountName.Text, "rda.local", txtPassword.Text);

                if (txtSAMAccountName.Text == "peter")
                {
                    // The login is authenticated
                    MessageBox.Show("Login Authenticated!");
                }
                else
                {
                    MessageBox.Show("Login Failed!");
                }

                if (txtPassword.Text == "pan")
                {
                    // The application is authorized
                    MessageBox.Show("Application Authorized!");
                }
                else
                {
                    MessageBox.Show("Login Failed!");
                }
                ClearForm1();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void lblUserName_Click(object sender, EventArgs e)
        {

        }
    }
}
