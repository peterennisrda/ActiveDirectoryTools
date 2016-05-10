using System;
using System.Security.Authentication;
using System.Windows.Forms;

namespace ADT06.ADAAL
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
                txtOutput.Text += "User Name and Password is required." + " \r\n";
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
                txtOutput.Text += "dnsHostName: " + theDnsHostName + " \r\n";
                txtOutput.Text += "Application Group: " + txtAppGroup.Text + " \r\n";
                txtOutput.Text += "SAM Account Name: " + txtSAMAccountName.Text + " \r\n";
                theRootDSE = subStrings[1];

                IsUserAuthenticated = ADValidation.IsUserValidated(txtSAMAccountName.Text, theDnsHostName, txtPassword.Text);
                //MessageBox.Show("IsAuthenticated = " + IsUserAuthenticated);

                if (IsUserAuthenticated)
                {
                    // The login is authenticated
                    txtOutput.Text += string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + " " + txtSAMAccountName.Text + " Login Authenticated!" + " \r\n";
                    MessageBox.Show("Login Authenticated!");
                }
                else
                {
                    txtOutput.Text += string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + " " + txtSAMAccountName.Text + " Login Failed!" + " \r\n";
                    MessageBox.Show("Login Failed!");
                    ClearForm1();
                    throw new InvalidCredentialException();
                }

                IsUserAuthorized = ADValidation.IsUserInGroup(txtSAMAccountName.Text, theDnsHostName, txtAppGroup.Text);
                //MessageBox.Show("IsUserAuthorized = " + IsUserAuthorized);

                if (IsUserAuthorized)
                {
                    // The application is authorized for the user
                    txtOutput.Text += string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + " " + txtSAMAccountName.Text + " Application Authorized!" + " \r\n";
                    MessageBox.Show("Application Authorized!");
                }
                else
                {
                    txtOutput.Text += string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + " " + txtSAMAccountName.Text + " Authorization Failed!" + " \r\n";
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
