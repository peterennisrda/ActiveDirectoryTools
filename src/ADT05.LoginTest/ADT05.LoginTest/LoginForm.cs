using System;
using System.Security.Authentication;
using System.Windows.Forms;

namespace ADT05.LoginTest
{
    public partial class Form1 : Form
    {
        private string theDnsHostNameRootDSE = "";
        private string theDnsHostName;
        private string theRootDSE = "";

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

                theDnsHostNameRootDSE = "";
                theDnsHostName = "";
                theRootDSE = "";

                // Non-privileged local user account on a domain PC returns this error:
                // "The specified domain either does not exist or could not be contacted."
                //theDnsHostNameRootDSE = ADValidation.RetrieveDnsHostNameRootDseDefaultNamingContext();
                //MessageBox.Show("'" + theDnsHostNameRootDSE + "'");
                //string[] subStrings = theDnsHostNameRootDSE.Split('|');
                //theDnsHostName = subStrings[0];
                //theRootDSE = subStrings[1];

                //theDnsHostName = "RDADC.rda.local";     // Login Authenticated! - Authorization Failed!
                //MessageBox.Show("'" + theDnsHostName + "'");
                theDnsHostName = "rda.local";     // Login Authenticated! - Authorization Failed!
                MessageBox.Show("'" + theDnsHostName + "'");
                //theDnsHostName = "rda";     // Login Authenticated! - Authorization Failed!
                //MessageBox.Show("'" + theDnsHostName + "'");

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
                MessageBox.Show(ex.Message + "ADT05 Message!" + System.Environment.NewLine +
                    "'" + txtAppGroup.Text + "'" + System.Environment.NewLine +
                    "'" + txtSAMAccountName.Text + "'" + System.Environment.NewLine +
                    "'" + theDnsHostName + "'" + System.Environment.NewLine);
            }
        }
    }
}
