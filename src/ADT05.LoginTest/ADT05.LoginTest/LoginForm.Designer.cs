namespace ADT05.LoginTest
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtSAMAccountName = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblUserName = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.btnValidate = new System.Windows.Forms.Button();
            this.txtAppGroup = new System.Windows.Forms.TextBox();
            this.lblAppGroup = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtSAMAccountName
            // 
            this.txtSAMAccountName.Location = new System.Drawing.Point(163, 101);
            this.txtSAMAccountName.Name = "txtSAMAccountName";
            this.txtSAMAccountName.Size = new System.Drawing.Size(131, 20);
            this.txtSAMAccountName.TabIndex = 1;
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(163, 162);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(131, 20);
            this.txtPassword.TabIndex = 2;
            // 
            // lblUserName
            // 
            this.lblUserName.AutoSize = true;
            this.lblUserName.Location = new System.Drawing.Point(46, 108);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(104, 13);
            this.lblUserName.TabIndex = 5;
            this.lblUserName.Text = "SAM Account Name";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(97, 169);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(53, 13);
            this.lblPassword.TabIndex = 6;
            this.lblPassword.Text = "Password";
            // 
            // btnValidate
            // 
            this.btnValidate.Location = new System.Drawing.Point(163, 230);
            this.btnValidate.Name = "btnValidate";
            this.btnValidate.Size = new System.Drawing.Size(75, 23);
            this.btnValidate.TabIndex = 3;
            this.btnValidate.Text = "Validate";
            this.btnValidate.UseVisualStyleBackColor = true;
            this.btnValidate.Click += new System.EventHandler(this.btnValidate_Click);
            // 
            // txtAppGroup
            // 
            this.txtAppGroup.Location = new System.Drawing.Point(163, 38);
            this.txtAppGroup.Name = "txtAppGroup";
            this.txtAppGroup.Size = new System.Drawing.Size(131, 20);
            this.txtAppGroup.TabIndex = 0;
            // 
            // lblAppGroup
            // 
            this.lblAppGroup.AutoSize = true;
            this.lblAppGroup.Location = new System.Drawing.Point(59, 45);
            this.lblAppGroup.Name = "lblAppGroup";
            this.lblAppGroup.Size = new System.Drawing.Size(91, 13);
            this.lblAppGroup.TabIndex = 4;
            this.lblAppGroup.Text = "Application Group";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 335);
            this.Controls.Add(this.lblAppGroup);
            this.Controls.Add(this.txtAppGroup);
            this.Controls.Add(this.btnValidate);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.lblUserName);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.txtSAMAccountName);
            this.Name = "Form1";
            this.Text = "Login Form";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtSAMAccountName;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Button btnValidate;
        private System.Windows.Forms.TextBox txtAppGroup;
        private System.Windows.Forms.Label lblAppGroup;
    }
}

