using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;
using IWshRuntimeLibrary;
using File = System.IO.File;

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
            /*
            if (!IsAdministrator())
            {
                PromptForAdminRights();
                return;
            }
            */
            //CheckIfInstalled();
            //LoadSettings();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadSettings();

            string exePath = textBox1.Text;
            string textBox2Value = textBox2.Text;
            string textBox3Value = textBox3.Text;

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

            if (string.IsNullOrEmpty(textBox2Value))
            {
                MessageBox.Show("Please set a value for textBox2", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(textBox3Value))
            {
                MessageBox.Show("Please set a value for textBox3", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            LoadSettings();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            UninstallExecute();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            WriteToSettingsFileLine1(textBox2.Text);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            WriteToSettingsFileLine2(textBox3.Text);
        }
        private void button7_Click(object sender, EventArgs e)
        {
            BrowseServerApp();
        }

        private void InstallExecute(string exePath)
        {
            try
            {
                string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcutPath = Path.Combine(startupFolderPath, "KVCOMSVC.lnk");

                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.Description = "KVCOMSVC";
                shortcut.TargetPath = exePath;
                shortcut.Save();

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
                string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcutPath = Path.Combine(startupFolderPath, "KVCOMSVC.lnk");

                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                    MessageBox.Show("KVCOMSVC has been removed from startup.", "Uninstallation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("KVCOMSVC shortcut not found in startup folder.", "Uninstallation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

        private void BrowseServerApp()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";
                openFileDialog.Title = "Select KVCOMSERVER Executable";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox2.Text = AddSlashToPath(openFileDialog.FileName);
                }
            }
        }

        private string AddSlashToPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            // Replace each single backslash with a double backslash
            return path.Replace("\\", "\\\\");
        }

        private void CheckIfInstalled()
        {
            try
            {
                string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcutPath = Path.Combine(startupFolderPath, "KVCOMSVC.lnk");

                if (File.Exists(shortcutPath))
                {
                    textBox1.Text = ((IWshShortcut)new WshShell().CreateShortcut(shortcutPath)).TargetPath;
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

        private void LoadSettings()
        {
            try
            {
                string exePath = textBox1.Text;
                if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
                {
                    MessageBox.Show("Please select the KVCOMSVC executable first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string directory = Path.GetDirectoryName(exePath);
                string settingsFilePath = Path.Combine(directory, "KVCOMSVC_SETTING");

                if (File.Exists(settingsFilePath))
                {
                    string[] settings = File.ReadAllLines(settingsFilePath);
                    textBox2.Text = settings[0];
                    if (settings.Length > 1 && settings[1].StartsWith("/checkinterval:"))
                    {
                        textBox3.Text = settings[1].Substring("/checkinterval:".Length);
                    }
                    else
                    {
                        textBox3.Text = string.Empty;
                    }
                    MessageBox.Show("Settings loaded successfully.", "Settings Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("KVCOMSVC_SETTING file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void WriteToSettingsFileLine1(string newValue)
        {
            try
            {
                string exePath = textBox1.Text;
                if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
                {
                    MessageBox.Show("Please select the KVCOMSVC executable first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string directory = Path.GetDirectoryName(exePath);
                string settingsFilePath = Path.Combine(directory, "KVCOMSVC_SETTING");

                if (File.Exists(settingsFilePath))
                {
                    string[] settings = File.ReadAllLines(settingsFilePath);
                    if (settings.Length > 0)
                    {
                        settings[0] = newValue; // Update the first line
                    }
                    else
                    {
                        Array.Resize(ref settings, 1);
                        settings[0] = newValue;
                    }
                    File.WriteAllLines(settingsFilePath, settings);
                    MessageBox.Show("Settings updated successfully.", "Settings Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("KVCOMSVC_SETTING file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void WriteToSettingsFileLine2(string newValue)
        {
            try
            {
                string exePath = textBox1.Text;
                if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
                {
                    MessageBox.Show("Please select the KVCOMSVC executable first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string directory = Path.GetDirectoryName(exePath);
                string settingsFilePath = Path.Combine(directory, "KVCOMSVC_SETTING");

                if (File.Exists(settingsFilePath))
                {
                    string[] settings = File.ReadAllLines(settingsFilePath);
                    if (settings.Length > 1)
                    {
                        settings[1] = $"/checkinterval:{newValue}"; // Update the second line
                    }
                    else
                    {
                        Array.Resize(ref settings, 2);
                        settings[1] = $"/checkinterval:{newValue}";
                    }
                    File.WriteAllLines(settingsFilePath, settings);
                    MessageBox.Show("Settings updated successfully.", "Settings Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("KVCOMSVC_SETTING file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void PromptForAdminRights()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Application.ExecutablePath,
                    Verb = "runas"
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to restart with administrative privileges: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Application.Exit();
            }
        }

    }
}
