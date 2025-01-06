using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;

namespace KVCOMSVCINSTALLER
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!IsAdministrator())
            {
                MessageBox.Show("This application needs to be run as an administrator. Please restart the application with elevated permissions.", "Administrator Privileges Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
                return;
            }

            CheckIfInstalled();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string exePath = textBox1.Text;

            if (string.IsNullOrEmpty(exePath))
            {
                MessageBox.Show("Please enter the path to KVCOMSVC.exe", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Path.IsPathRooted(exePath) || !File.Exists(exePath))
            {
                MessageBox.Show("Please enter a valid path to KVCOMSVC.exe", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            InstallExecute(exePath);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            BrowseExefile();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            UninstallExecute();
        }

        private void InstallExecute(string exePath)
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                registryKey.SetValue("KVCOMSVC", exePath);
                MessageBox.Show("KVCOMSVC has been added to startup.", "Installation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add KVCOMSVC to startup: {ex.Message}", "Installation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Application.Exit();
            }
        }

        private void UninstallExecute()
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                registryKey.DeleteValue("KVCOMSVC", false);
                MessageBox.Show("KVCOMSVC has been removed from startup.", "Uninstallation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove KVCOMSVC from startup: {ex.Message}", "Uninstallation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Application.Exit();
            }
        }

        private void BrowseExefile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";
                openFileDialog.Title = "Select KVCOMSVC Executable";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = openFileDialog.FileName;
                }
            }
        }

        private void CheckIfInstalled()
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (registryKey.GetValue("KVCOMSVC") != null)
                {
                    MessageBox.Show("KVCOMSVC is already set to run at startup.", "Already Installed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    button1.Enabled = false; // Disable the install button
                    button1.Visible = false; // Hide the install button
                    button4.Enabled = true; // Enable the uninstall button
                    button4.Visible = true; // Show the uninstall button
                }
                else
                {
                    MessageBox.Show("KVCOMSVC is not set to run at startup.", "Not Installed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    button1.Enabled = true; // Enable the install button
                    button1.Visible = true; // Show the install button
                    button4.Enabled = false; // Disable the uninstall button
                    button4.Visible = false; // Hide the uninstall button
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to check if KVCOMSVC is installed: {ex.Message}", "Check Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}
