using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NinecraftApkLauncher
{
    public class MainForm : Form
    {
        private string apkDir = null;
        private const string ConfigFile = "config.cfg";
        private const string Executable = "ninecraft.exe";

        private ComboBox combo;
        private Button launchButton;
        private Button browseButton;

        public MainForm()
        {
            InitializeComponents();
            LoadConfig();
            RefreshFolders();
        }

        private void InitializeComponents()
        {
            this.Text = "Ninecraft APK Launcher";
            this.ClientSize = new System.Drawing.Size(400, 140);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            var label = new Label()
            {
                Text = "Select an APK folder:",
                AutoSize = true,
                Location = new System.Drawing.Point(20, 20)
            };
            this.Controls.Add(label);

            combo = new ComboBox()
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new System.Drawing.Point(20, 50),
                Width = 350
            };
            this.Controls.Add(combo);

            browseButton = new Button()
            {
                Text = "Browse...",
                Location = new System.Drawing.Point(130, 90),
                Width = 100
            };
            browseButton.Click += BrowseButton_Click;
            this.Controls.Add(browseButton);

            launchButton = new Button()
            {
                Text = "Launch",
                Location = new System.Drawing.Point(20, 90),
                Width = 100
            };
            launchButton.Click += LaunchButton_Click;
            this.Controls.Add(launchButton);
        }

        private void LoadConfig()
        {
            if (File.Exists(ConfigFile))
            {
                string path = File.ReadAllText(ConfigFile).Trim();
                if (Directory.Exists(path))
                {
                    apkDir = path;
                }
            }

            if (apkDir == null)
            {
                MessageBox.Show("Please select a folder containing APK subfolders.", "No folder selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SaveConfig()
        {
            if (!string.IsNullOrEmpty(apkDir))
            {
                File.WriteAllText(ConfigFile, apkDir);
            }
        }

        private void RefreshFolders()
        {
            combo.Items.Clear();

            if (string.IsNullOrEmpty(apkDir) || !Directory.Exists(apkDir))
                return;

            var directories = Directory.GetDirectories(apkDir)
                                       .Select(Path.GetFileName)
                                       .ToArray();

            combo.Items.AddRange(directories);

            if (combo.Items.Count > 0)
                combo.SelectedIndex = 0;
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select the folder containing extracted APK directories";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    apkDir = dialog.SelectedPath;
                    SaveConfig();
                    RefreshFolders();
                }
            }
        }

        private void LaunchButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(apkDir) || combo.SelectedItem == null)
            {
                MessageBox.Show("Please select a folder.", "No selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedFolder = combo.SelectedItem.ToString();
            string gamePath = Path.Combine(apkDir, selectedFolder);

            var startInfo = new ProcessStartInfo()
            {
                FileName = Executable,
                Arguments = $"--game \"{gamePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = true
            };

            try
            {
                Process proc = Process.Start(startInfo);
                proc.WaitForExit();
                if (proc.ExitCode != 0)
                {
                    MessageBox.Show($"Failed to run the command. Exit code: {proc.ExitCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show($"{Executable} not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to run the command:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
