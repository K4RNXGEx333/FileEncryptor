using System;
using System.Data.SqlTypes;
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
        private static readonly byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

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
                var files = Directory.GetFiles(folderPath, "*.txt");
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
            var files = Directory.GetFiles(folderPath, "*.txt");

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
                // Compute file hash before encryption
                string hashBeforeEncryption = ComputeFileHash(filePath);

                byte[] bytesToBeEncrypted = File.ReadAllBytes(filePath);
                IntPtr passwordBSTR = Marshal.SecureStringToBSTR(password);
                passwordBytes = Encoding.UTF8.GetBytes(Marshal.PtrToStringBSTR(passwordBSTR));
                Marshal.ZeroFreeBSTR(passwordBSTR);
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);
                File.WriteAllBytes(filePath, bytesEncrypted);

                // Save hash alongside the encrypted file
                File.WriteAllText(filePath + ".hash", hashBeforeEncryption);

                // Set the file as read-only
                File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.ReadOnly);
            }
            catch (CryptographicException ex)
            {
                MessageBox.Show($"Encryption error: {ex.Message}");
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
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] bytesDecrypted = Array.Empty<byte>();

            try
            {
                // Remove read-only attribute
                File.SetAttributes(filePath, File.GetAttributes(filePath) & ~FileAttributes.ReadOnly);

                // Read encrypted bytes
                byte[] bytesToBeDecrypted = File.ReadAllBytes(filePath);
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                // Decrypt bytes
                bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

                // Write decrypted bytes to a temporary file
                string tempFilePath = filePath + ".temp";
                File.WriteAllBytes(tempFilePath, bytesDecrypted);

                // Replace original file with the decrypted file
                File.Replace(tempFilePath, filePath, null);

                // Verify file integrity
                string hashFilePath = filePath + ".hash";
                if (File.Exists(hashFilePath))
                {
                    string originalHash = File.ReadAllText(hashFilePath);
                    string hashAfterDecryption = ComputeFileHash(filePath);

                    if (originalHash != hashAfterDecryption)
                    {
                        MessageBox.Show($"File integrity check failed for: {filePath}");
                    }
                    else
                    {
                        File.Delete(hashFilePath); // Remove hash file if the integrity is valid
                    }
                }
            }
            catch (CryptographicException ex)
            {
                MessageBox.Show($"Decryption error: {ex.Message}");
            }
            finally
            {
                if (passwordBytes != null)
                    Array.Clear(passwordBytes, 0, passwordBytes.Length);
                if (bytesDecrypted != null)
                    Array.Clear(bytesDecrypted, 0, bytesDecrypted.Length);
            }
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
            byte[] encryptedBytes;

            using (var aes = Aes.Create())
            {
                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 10000, HashAlgorithmName.SHA256);

                aes.Key = key.GetBytes(32);
                aes.IV = key.GetBytes(16);

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                            cs.FlushFinalBlock();
                        }

                        encryptedBytes = ms.ToArray();
                    }
                }
            }

            return encryptedBytes;
        }

        private byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes;

            using (var aes = Aes.Create())
            {
                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 10000, HashAlgorithmName.SHA256);

                aes.Key = key.GetBytes(32);
                aes.IV = key.GetBytes(16);

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    using (var ms = new MemoryStream(bytesToBeDecrypted))
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (var sr = new MemoryStream())
                            {
                                cs.CopyTo(sr);
                                decryptedBytes = sr.ToArray();
                            }
                        }
                    }
                }
            }

            return decryptedBytes;
        }

        private string ComputeFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
        }
    }
}
