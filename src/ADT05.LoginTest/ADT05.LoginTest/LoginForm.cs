using System;
using System.Security.Authentication;
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
                bool IsUserAuthenticated = false;
                bool IsUserAuthorized = false;

                string theDnsHostNameRootDSE = "";
                string theDnsHostName = "";
                string theRootDSE = "";

                theDnsHostNameRootDSE = ADValidation.RetrieveDnsHostNameRootDseDefaultNamingContext();
                string[] subStrings = theDnsHostNameRootDSE.Split('|');
                theDnsHostName = subStrings[0];
                theRootDSE = subStrings[1];

                IsUserAuthenticated = ADValidation.IsUserValidated(txtSAMAccountName.Text, theDnsHostName, txtPassword.Text);
                //MessageBox.Show("IsAuthenticated = " + IsUserAuthenticated);

                if (IsUserAuthenticated)
                {
                    // The login is authenticated
                    MessageBox.Show("Login Authenticated!");
                }
                else
                {
                    MessageBox.Show("Login Failed!");
                    ClearForm1();
                    throw new InvalidCredentialException();
                }

                IsUserAuthorized = ADValidation.IsUserInGroup(txtSAMAccountName.Text, theDnsHostName, txtAppGroup.Text);
                //MessageBox.Show("IsUserAuthorized = " + IsUserAuthorized);

                if (IsUserAuthorized)
                {
                    // The application is authorized for the user
                    MessageBox.Show("Application Authorized!");
                }
                else
                {
                    MessageBox.Show("Authorization Failed!");
                    ClearForm1();
                    throw new InvalidCredentialException();
                }
                ClearForm1();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
