using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace LoginApplication
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }
        //Connection String
        string cs = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=|DataDirectory|\MyDatabase.mdf;Integrated Security=True;";
        //btn_Submit Click event
        private void button1_Click(object sender, EventArgs e)
        {
            if(txt_UserName.Text=="" || txt_Password.Text=="")
            {
                MessageBox.Show("Please provide UserName and Password");
                return;
            }
            try
            {
                //Create SqlConnection
                SqlConnection con = new SqlConnection(cs);
                SqlCommand cmd = new SqlCommand("Select * from tbl_Login where UserName=@username and Password=@password",con);
                cmd.Parameters.AddWithValue("@username",txt_UserName.Text);
                cmd.Parameters.AddWithValue("@password", txt_Password.Text);
                con.Open();
                SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapt.Fill(ds);
                con.Close();
                int count = ds.Tables[0].Rows.Count;
                //If count is equal to 1, than show frmMain form
                if (count == 1)
                {
                    MessageBox.Show("Login Successful!");
                    this.Hide();
                    frmMain fm = new frmMain();
                    fm.Show();
                }
                else
                {
                    MessageBox.Show("Login Failed!");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
