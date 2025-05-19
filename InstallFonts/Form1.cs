using System.IO.Compression;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic.Logging;
using Microsoft.Win32;

namespace InstallFonts
{
    public partial class Form1 : Form
    {
        private string[] selectedZipFiles = Array.Empty<string>(); // Initialize to avoid CS8618

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadDrives();
        }

        private void LoadDrives()
        {
            treeViewFolders.Nodes.Clear();

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    TreeNode driveNode = new TreeNode(drive.Name);
                    driveNode.Nodes.Add("Loading...");
                    treeViewFolders.Nodes.Add(driveNode);
                }
            }

            treeViewFolders.BeforeExpand += treeViewFolders_BeforeExpand;
        }

        private void treeViewFolders_BeforeExpand(object? sender, TreeViewCancelEventArgs e)
        {
            TreeNode node = e.Node;

            if (node.Nodes.Count == 1 && node.Nodes[0].Text == "Loading...")
            {
                node.Nodes.Clear();

                try
                {
                    string path = node.Tag?.ToString() ?? string.Empty;
                    string[] dirs = Directory.GetDirectories(path);
                    foreach (string dir in dirs)
                    {
                        TreeNode subNode = new TreeNode(Path.GetFileName(dir))
                        {
                            Tag = dir
                        };
                        subNode.Nodes.Add("Loading...");
                        node.Nodes.Add(subNode);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Handle access denied exceptions if needed
                    node.Nodes.Add("Access Denied");
                }
            }
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "ZIP files (*.zip)|*.zip";
            openFileDialog1.Multiselect = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                selectedZipFiles = openFileDialog1.FileNames;
                Log($"Selected {selectedZipFiles.Length} zip files.");
            }
        }

        private void btnInstallFonts_Click(object sender, EventArgs e)
        {
            if (selectedZipFiles == null || selectedZipFiles.Length == 0)
            {
                MessageBox.Show("Please select one or more .zip files first.");
                return;
            }

            DialogResult result = MessageBox.Show(
                "Do you want to install fonts for all users (requires administrator privileges)?\n" +
                "Click No to install for the current user only.",
                "Choose Installation Scope",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Cancel)
            {
                Log("❌ Installation cancelled by user.");
                return;
            }

            bool installSystemWide = (result == DialogResult.Yes);

            try
            {
                Log("📦 Unzipping .zip files...");
                foreach (string zipPath in selectedZipFiles)
                {
                    Log($"🗂️ Extracting: {Path.GetFileName(zipPath)}");

                    string extractDir = Path.Combine(Path.GetTempPath(), "FontInstaller", Path.GetFileNameWithoutExtension(zipPath));
                    Directory.CreateDirectory(extractDir);

                    try
                    {
                        ZipFile.ExtractToDirectory(zipPath, extractDir, true);
                    }
                    catch (Exception ex)
                    {
                        Log($"❌ Failed to unzip {zipPath}: {ex.Message}");
                        continue;
                    }

                    string[] fontExtensions = { "*.ttf", "*.otf" };
                    foreach (string ext in fontExtensions)
                    {
                        foreach (string fontPath in Directory.GetFiles(extractDir, ext, SearchOption.AllDirectories))
                        {
                            InstallFont(fontPath, installSystemWide);
                        }
                    }
                }


                Log("\r\n✅ All fonts installed. You may need to restart for changes to take effect.");
            }
            catch (Exception ex)
            {
                Log($"🔥 Unexpected error: {ex.Message}");
            }



        }


        private void InstallFont(string fontPath, bool systemWide)
        {
            try
            {
                string fontsDir = systemWide
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts")
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\Windows\\Fonts");

                string destFontPath = Path.Combine(fontsDir, Path.GetFileName(fontPath));

                // Create font directory if needed
                Directory.CreateDirectory(fontsDir);
                File.Copy(fontPath, destFontPath, true);

                Log($"✔ Installed: {Path.GetFileName(fontPath)}");

                // Register font (if system-wide, use HKLM; else use HKCU)
                string regRoot = systemWide ? "HKEY_LOCAL_MACHINE" : "HKEY_CURRENT_USER";
                string regPath = @"Software\Microsoft\Windows NT\CurrentVersion\Fonts";
                string regName = Path.GetFileName(fontPath);
                string regValue = Path.GetFileName(fontPath);

                Microsoft.Win32.Registry.SetValue($"{regRoot}\\{regPath}", regName, regValue, Microsoft.Win32.RegistryValueKind.String);
            }
            catch (Exception ex)
            {
                Log($"❌ Failed to install {Path.GetFileName(fontPath)}: {ex.Message}");
            }
        }


        //private void chkSystemWide_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (chkSystemWide.Checked)
        //    {
        //        MessageBox.Show("Installing fonts system-wide requires administrator privileges.",
        //            "Permission Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    }
        //}


        private void Log(string message)
        {
            txtLog.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\r\n");
        }
    }
}
