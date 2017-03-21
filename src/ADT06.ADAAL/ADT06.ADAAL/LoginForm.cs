using RDA.Auth;
using RDA.Auth.Providers;
using System;
using System.Security.Authentication;
using System.Windows.Forms;

namespace ADT06.ADAAL
{
    public partial class Form1 : Form
    {
        AuthorizeCOM com = new AuthorizeCOM();
        public Form1()
        {
            InitializeComponent();
            txtAppGroup.Text = "SARAnet Users";
            txtSAMAccountName.Text = "aconnolly";
            txtPassword.Text = "VBNazx7*9";
        }

        private void ClearForm1()
        {
            txtSAMAccountName.Text = "";
            txtPassword.Text = "";
        }

        private void btnValidate_Click(object sender, EventArgs e)
        {

            // ann is commenting
            if (string.IsNullOrWhiteSpace(txtAppGroup.Text) ||
                string.IsNullOrWhiteSpace(txtSAMAccountName.Text) || 
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Application Group, User Name, and Password is required.");
                txtOutput.Text += "Application Group, User Name, and Password is required." + " \r\n";
                return;
            }
            try
            {
              //  string result = COMTest.ValidateCredentialsTLS(theUserName, theUserDomainName, theUserPassword, out serverName);
                
              // txtOutput.Text = result;
                
                bool IsUserAuthenticated = false;
                bool IsUserAuthorized = false;
                ADValidation ad = new ADValidation();

                string theDnsHostNameRootDSE = "";
                string theDnsHostName = "";
                string theRootDSE = "";
                string serverName;

                theDnsHostNameRootDSE = ad.RetrieveDnsHostNameRootDseDefaultNamingContext();
                string[] subStrings = theDnsHostNameRootDSE.Split('|');
                theDnsHostName = subStrings[0];
                txtOutput.Text += "dnsHostName: " + theDnsHostName + " \r\n";
                txtOutput.Text += "Application Group: " + txtAppGroup.Text + " \r\n";
                txtOutput.Text += "SAM Account Name: " + txtSAMAccountName.Text + " \r\n";
                theRootDSE = subStrings[1];


                IsUserAuthenticated = ADValidation.IsUserValidated(txtSAMAccountName.Text, theDnsHostName, txtPassword.Text);

                bool result = com.Authenticate(txtSAMAccountName.Text, txtPassword.Text, theDnsHostName);
                
                //MessageBox.Show("IsAuthenticated = " + IsUserAuthenticated);

                if (IsUserAuthenticated)
                {
                    // The login is authenticated
                    txtOutput.Text += string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + " " + txtAppGroup.Text + " " + txtSAMAccountName.Text + " Login Authenticated!" + " \r\n";
                    MessageBox.Show("Login Authenticated!");
                }
                else
                {
                    txtOutput.Text += string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + " " + txtAppGroup.Text + " " + txtSAMAccountName.Text + " Login Failed!" + " \r\n";
                    MessageBox.Show("Login Failed!");
                    ClearForm1();
                    throw new InvalidCredentialException();
                }

                IsUserAuthorized = ADValidation.IsUserInGroup(txtSAMAccountName.Text, theDnsHostName, txtAppGroup.Text);
                result = com.Authorize(txtSAMAccountName.Text, theDnsHostName, txtAppGroup.Text);
                //MessageBox.Show("IsUserAuthorized = " + IsUserAuthorized);

                if (IsUserAuthorized)
                {
                    // The application is authorized for the user
                    txtOutput.Text += string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + " " + txtAppGroup.Text + " " + txtSAMAccountName.Text + " Application Authorized!" + " \r\n";
                    MessageBox.Show("Application Authorized!");
                }
                else
                {
                    txtOutput.Text += string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + " " + txtAppGroup.Text + " " + txtSAMAccountName.Text + " Authorization Failed!" + " \r\n";
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
