namespace FileEncryptor
{// Made and programmed by K4rnxge
    partial class File_Encryptor
    {
        // Made and programmed by K4rnxge
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
            }// Made and programmed by K4rnxge
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtPassword = new TextBox();
            Password = new Label();
            btnEncrypt = new Button();
            btnDecrypt = new Button();
            txtFolderPath = new TextBox();
            folderBrowserDialog = new FolderBrowserDialog();
            btnBrowse = new Button();
            saveFileDialog = new SaveFileDialog();
            SuspendLayout();
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(172, 71);
            txtPassword.Name = "txtPassword";
            // Made and programmed by K4rnxge
            txtPassword.Size = new Size(147, 23);
            txtPassword.TabIndex = 0;
            // 
            // Password
            // 
            Password.AutoSize = true;
            Password.Location = new Point(94, 74);
            Password.Name = "Password";
            // Made and programmed by K4rnxge
            Password.Size = new Size(57, 15);
            Password.TabIndex = 1;
            Password.Text = "Password";
            // 
            // btnEncrypt
            // 
            btnEncrypt.Location = new Point(172, 136);
            btnEncrypt.Name = "btnEncrypt";
            btnEncrypt.Size = new Size(75, 23);
            btnEncrypt.TabIndex = 2;
            // Made and programmed by K4rnxge
            btnEncrypt.Text = "Lock";
            btnEncrypt.UseVisualStyleBackColor = true;
            btnEncrypt.Click += btnEncrypt_Click;
            // 
            // btnDecrypt
            // 
            btnDecrypt.Location = new Point(325, 136);
            btnDecrypt.Name = "btnDecrypt";
            btnDecrypt.Size = new Size(75, 23);
            btnDecrypt.TabIndex = 3;
            btnDecrypt.Text = "Unlock";
            btnDecrypt.UseVisualStyleBackColor = true;
            btnDecrypt.Click += btnDecrypt_Click;
            // 
            // txtFolderPath
            // 
            txtFolderPath.Location = new Point(172, 100);
            txtFolderPath.Name = "txtFolderPath";
            txtFolderPath.Size = new Size(228, 23);
            // Made and programmed by K4rnxge
            txtFolderPath.TabIndex = 5;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(325, 71);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(75, 23);
            btnBrowse.TabIndex = 6;
            btnBrowse.Text = "Browse";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // Folder_Locker
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(515, 254);
            Controls.Add(btnBrowse);
            Controls.Add(txtFolderPath);
            // Made and programmed by K4rnxge
            Controls.Add(btnDecrypt);
            Controls.Add(btnEncrypt);
            // Made and programmed by K4rnxge
            Controls.Add(Password);
            Controls.Add(txtPassword);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "Folder_Locker";
            // Made and programmed by K4rnxge
            Text = "Folder Locker";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtPassword;
        private Label Password;
        private Button btnEncrypt;
        private Button btnDecrypt;
        private TextBox txtFolderPath;
        private Button btnBrowse;
        private SaveFileDialog saveFileDialog;
        private FolderBrowserDialog folderBrowserDialog;
    }
}
// Made and programmed by K4rnxge