// Made and programmed by K4rnxge

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace FileEncryptor
{
    public partial class File_Encryptor : Form
    {
        public File_Encryptor()
        {
            InitializeComponent();
            saveFileDialog = new SaveFileDialog();
            folderBrowserDialog = new FolderBrowserDialog();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                txtFolderPath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            string folderPath = txtFolderPath.Text;
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match");
                return;
            }

            if (Directory.Exists(folderPath) && !string.IsNullOrEmpty(password))
            {
                EncryptFolder(folderPath, password);

                // Show SaveFileDialog to let user specify backup location
                saveFileDialog.Filter = "Text Files (*.txt)|*.txt";
                saveFileDialog.Title = "Save Backup File";
                saveFileDialog.FileName = "backup.txt";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    CreateBackupFile(saveFileDialog.FileName, password);
                    MessageBox.Show("Files encrypted successfully and backup created.");
                }
                else
                {
                    MessageBox.Show("Backup was not saved.");
                }
            }
            else
            {
                MessageBox.Show("Please select a valid folder and enter a password.");
            }
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            string folderPath = txtFolderPath.Text;
            string password = txtPassword.Text;

            if (Directory.Exists(folderPath) && !string.IsNullOrEmpty(password))
            {
                try
                {
                    DecryptFolder(folderPath, password);
                    MessageBox.Show("Files decrypted successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please select a valid folder and enter a password.");
            }
        }

        private void btnVerifyIntegrity_Click(object sender, EventArgs e)
        {
            string folderPath = txtFolderPath.Text;
            string password = txtPassword.Text;

            if (Directory.Exists(folderPath) && !string.IsNullOrEmpty(password))
            {
                SecureString securePassword = ConvertToSecureString(password);

                // Verify integrity for all files in the folder
                var files = Directory.GetFiles(folderPath);
                bool allFilesValid = true;

                foreach (var file in files)
                {
                    if (!VerifyFileIntegrity(file, securePassword))
                    {
                        allFilesValid = false;
                        MessageBox.Show($"File integrity verification failed for: {file}");
                        break;
                    }
                }

                if (allFilesValid)
                {
                    MessageBox.Show("All files' integrity verified successfully.");
                }
            }
            else
            {
                MessageBox.Show("Please select a valid folder and enter a password.");
            }
        }

        private void EncryptFolder(string folderPath, string password)
        {
            var files = Directory.GetFiles(folderPath, "*.txt");
            SecureString securePassword = ConvertToSecureString(password);

            foreach (var file in files)
            {
                EncryptFile(file, securePassword);
            }
        }

        private void DecryptFolder(string folderPath, string password)
        {
            var files = Directory.GetFiles(folderPath);

            foreach (var file in files)
            {
                DecryptFile(file, password);
            }
        }

        private void EncryptFile(string filePath, SecureString password)
        {
            byte[] passwordBytes = Array.Empty<byte>();
            byte[] bytesEncrypted = Array.Empty<byte>();

            try
            {
                byte[] bytesToBeEncrypted = File.ReadAllBytes(filePath);
                IntPtr passwordBSTR = Marshal.SecureStringToBSTR(password);
                passwordBytes = Encoding.UTF8.GetBytes(Marshal.PtrToStringBSTR(passwordBSTR));
                Marshal.ZeroFreeBSTR(passwordBSTR);
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);
                File.WriteAllBytes(filePath, bytesEncrypted);
            }
            finally
            {
                if (passwordBytes != null)
                    Array.Clear(passwordBytes, 0, passwordBytes.Length);
                if (bytesEncrypted != null)
                    Array.Clear(bytesEncrypted, 0, bytesEncrypted.Length);
            }
        }

        private void DecryptFile(string filePath, string password)
        {
            byte[] bytesToBeDecrypted = File.ReadAllBytes(filePath);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            File.WriteAllBytes(filePath, bytesDecrypted);
        }

        private SecureString ConvertToSecureString(string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            var securePassword = new SecureString();
            foreach (char c in password)
                securePassword.AppendChar(c);

            securePassword.MakeReadOnly();
            return securePassword;
        }

        private bool VerifyFileIntegrity(string filePath, SecureString password)
        {
            byte[] passwordBytes = Array.Empty<byte>();
            byte[] bytesDecrypted = Array.Empty<byte>();

            try
            {
                byte[] bytesToBeDecrypted = File.ReadAllBytes(filePath);

                IntPtr passwordBSTR = Marshal.SecureStringToBSTR(password);
                passwordBytes = Encoding.UTF8.GetBytes(Marshal.PtrToStringBSTR(passwordBSTR));
                Marshal.ZeroFreeBSTR(passwordBSTR);
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);
                return true;
            }
            catch (CryptographicException)
            {
                return false; // Return false if decryption fails
            }
            finally
            {
                if (passwordBytes != null)
                    Array.Clear(passwordBytes, 0, passwordBytes.Length);
                if (bytesDecrypted != null)
                    Array.Clear(bytesDecrypted, 0, bytesDecrypted.Length);
            }
        }

        private void CreateBackupFile(string backupFilePath, string password)
        {
            File.WriteAllText(backupFilePath, password);
        }

        private byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = Array.Empty<byte>();

            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (Aes AES = Aes.Create())
                {
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 10000, HashAlgorithmName.SHA256);

                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CFB;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        private byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = Array.Empty<byte>();

            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (Aes AES = Aes.Create())
                {
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 10000, HashAlgorithmName.SHA256);

                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CFB;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }
    }
}

// Made and programmed by K4rnxge
