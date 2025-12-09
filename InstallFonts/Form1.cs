using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace InstallFonts
{
    public partial class Form1 : Form
    {
        private TreeView treeViewFolders;
        private ListView listViewFiles;
        private TextBox txtLog;
        private Button btnSelectFolder;
        private Button btnInstall;
        private Button btnClearSelection;
        private Button btnSelectAll;
        private Label lblSelectedCount;
        private ImageList imageList;

        private string currentPath = "";
        private List<string> selectedZipFiles = new List<string>();

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
            LoadDrives();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form
            this.ClientSize = new Size(900, 600);
            this.Text = "Font Installer";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 500);
            this.BackColor = Color.FromArgb(245, 245, 245);

            this.ResumeLayout(false);
        }

        private void InitializeCustomComponents()
        {
            // ImageList for icons
            imageList = new ImageList();
            imageList.ImageSize = new Size(16, 16);
            imageList.ColorDepth = ColorDepth.Depth32Bit;

            // TreeView - File/Folder Browser
            treeViewFolders = new TreeView
            {
                Location = new Point(20, 20),
                Size = new Size(280, 350),
                Font = new Font("Segoe UI", 9F),
                BorderStyle = BorderStyle.FixedSingle,
                ImageList = imageList
            };
            treeViewFolders.AfterSelect += TreeViewFolders_AfterSelect;
            treeViewFolders.BeforeExpand += TreeViewFolders_BeforeExpand;

            // ListView - File List
            listViewFiles = new ListView
            {
                Location = new Point(320, 20),
                Size = new Size(560, 350),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                CheckBoxes = true,
                Font = new Font("Segoe UI", 9F),
                BorderStyle = BorderStyle.FixedSingle,
                SmallImageList = imageList
            };
            listViewFiles.Columns.Add("Name", 300);
            listViewFiles.Columns.Add("Size", 100);
            listViewFiles.Columns.Add("Type", 150);
            listViewFiles.ItemChecked += ListViewFiles_ItemChecked;

            // Selected Count Label
            lblSelectedCount = new Label
            {
                Location = new Point(460, 380),
                Size = new Size(250, 25),
                Text = "Selected: 0 ZIP files",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(64, 64, 64)
            };

            // Select Folder Button
            btnSelectFolder = new Button
            {
                Location = new Point(20, 380),
                Size = new Size(130, 35),
                Text = "Select Folder",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(64, 64, 64),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnSelectFolder.FlatAppearance.BorderSize = 0;
            btnSelectFolder.Click += BtnSelectFolder_Click;

            // Select All Button
            btnSelectAll = new Button
            {
                Location = new Point(160, 380),
                Size = new Size(140, 35),
                Text = "Select All",
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(225, 225, 225),
                ForeColor = Color.Black,
                Cursor = Cursors.Hand
            };
            btnSelectAll.FlatAppearance.BorderSize = 0;
            btnSelectAll.Click += BtnSelectAll_Click;

            // Clear Selection Button
            btnClearSelection = new Button
            {
                Location = new Point(310, 380),
                Size = new Size(140, 35),
                Text = "Clear Selection",
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(225, 225, 225),
                ForeColor = Color.Black,
                Cursor = Cursors.Hand
            };
            btnClearSelection.FlatAppearance.BorderSize = 0;
            btnClearSelection.Click += BtnClearSelection_Click;

            // Install Button
            btnInstall = new Button
            {
                Location = new Point(740, 380),
                Size = new Size(140, 35),
                Text = "Install Fonts",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Black,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Enabled = true
            };
            btnInstall.FlatAppearance.BorderSize = 0;
            btnInstall.Click += BtnInstall_Click;

            // Log TextBox
            txtLog = new TextBox
            {
                Location = new Point(20, 430),
                Size = new Size(860, 140),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9F),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                treeViewFolders,
                listViewFiles,
                lblSelectedCount,
                btnSelectFolder,
                btnSelectAll,
                btnClearSelection,
                btnInstall,
                txtLog
            });
        }

        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            if (listViewFiles.Items.Count == 0) return;

            foreach (ListViewItem item in listViewFiles.Items)
            {
                item.Checked = true;
            }
            Log("🔄 All ZIP files selected");
        }

        private void LoadDrives()
        {
            treeViewFolders.Nodes.Clear();

            // Add folder icon
            Icon folderIcon = GetFolderIcon();
            imageList.Images.Add("folder", folderIcon);

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    TreeNode driveNode = new TreeNode(drive.Name)
                    {
                        Tag = drive.Name,
                        ImageKey = "folder",
                        SelectedImageKey = "folder"
                    };
                    driveNode.Nodes.Add(""); // Dummy node for expansion
                    treeViewFolders.Nodes.Add(driveNode);
                }
            }
        }

        private void TreeViewFolders_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode node = e.Node;

            if (node.Nodes.Count == 1 && string.IsNullOrEmpty(node.Nodes[0].Text))
            {
                node.Nodes.Clear();
                string path = node.Tag as string;

                try
                {
                    foreach (string dir in Directory.GetDirectories(path))
                    {
                        DirectoryInfo di = new DirectoryInfo(dir);
                        if ((di.Attributes & FileAttributes.Hidden) == 0 &&
                            (di.Attributes & FileAttributes.System) == 0)
                        {
                            TreeNode subNode = new TreeNode(di.Name)
                            {
                                Tag = di.FullName,
                                ImageKey = "folder",
                                SelectedImageKey = "folder"
                            };
                            subNode.Nodes.Add(""); // Dummy node
                            node.Nodes.Add(subNode);
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    node.Nodes.Add("Access Denied");
                }
            }
        }

        private void TreeViewFolders_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string path = e.Node.Tag as string;
            if (!string.IsNullOrEmpty(path))
            {
                currentPath = path;
                LoadFilesInListView(path);
            }
        }

        private void LoadFilesInListView(string path)
        {
            listViewFiles.Items.Clear();
            selectedZipFiles.Clear();
            UpdateSelectedCount();

            try
            {
                // Add ZIP icon if not exists
                if (!imageList.Images.ContainsKey(".zip"))
                {
                    Icon zipIcon = GetFileIcon(".zip");
                    imageList.Images.Add(".zip", zipIcon);
                }

                foreach (string file in Directory.GetFiles(path, "*.zip"))
                {
                    FileInfo fi = new FileInfo(file);
                    ListViewItem item = new ListViewItem(fi.Name)
                    {
                        Tag = fi.FullName,
                        ImageKey = ".zip"
                    };
                    item.SubItems.Add(FormatFileSize(fi.Length));
                    item.SubItems.Add("ZIP Archive");
                    listViewFiles.Items.Add(item);
                }

                Log($"📂 Loaded folder: {path}");
                Log($"   Found {listViewFiles.Items.Count} ZIP file(s)");
            }
            catch (Exception ex)
            {
                Log($"❌ Error loading folder: {ex.Message}");
            }
        }

        private void ListViewFiles_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            string filePath = e.Item.Tag as string;

            if (e.Item.Checked && !selectedZipFiles.Contains(filePath))
            {
                selectedZipFiles.Add(filePath);
            }
            else if (!e.Item.Checked && selectedZipFiles.Contains(filePath))
            {
                selectedZipFiles.Remove(filePath);
            }

            UpdateSelectedCount();
        }

        private void UpdateSelectedCount()
        {
            lblSelectedCount.Text = $"Selected: {selectedZipFiles.Count} ZIP file(s)";
            btnInstall.Enabled = selectedZipFiles.Count > 0;
        }

        private void BtnSelectFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select folder containing ZIP font files";
                dialog.ShowNewFolderButton = false;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    currentPath = dialog.SelectedPath;
                    LoadFilesInListView(currentPath);
                }
            }
        }

        private void BtnClearSelection_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFiles.Items)
            {
                item.Checked = false;
            }
            selectedZipFiles.Clear();
            UpdateSelectedCount();
            Log("🔄 Selection cleared");
        }

        private void BtnInstall_Click(object sender, EventArgs e)
        {
            if (selectedZipFiles.Count == 0)
            {
                MessageBox.Show("Please select at least one ZIP file.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Install fonts from {selectedZipFiles.Count} ZIP file(s)?\n\n" +
                "• Click YES to install for all users (requires admin rights)\n" +
                "• Click NO to install for current user only\n" +
                "• Click CANCEL to abort",
                "Choose Installation Scope",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Cancel)
            {
                Log("❌ Installation cancelled");
                return;
            }

            bool installSystemWide = (result == DialogResult.Yes);

            btnInstall.Enabled = false;
            btnSelectFolder.Enabled = false;
            btnClearSelection.Enabled = false;

            try
            {
                Log("═══════════════════════════════════════════════════");
                Log($"🚀 Starting font installation ({(installSystemWide ? "System-wide" : "User only")})");
                Log($"📦 Processing {selectedZipFiles.Count} ZIP file(s)...");
                Log("═══════════════════════════════════════════════════");

                int totalInstalled = 0;
                int totalFailed = 0;

                foreach (string zipPath in selectedZipFiles)
                {
                    Log($"\n📦 Extracting: {Path.GetFileName(zipPath)}");

                    string extractDir = Path.Combine(Path.GetTempPath(), "FontInstaller",
                        Path.GetFileNameWithoutExtension(zipPath));

                    try
                    {
                        if (Directory.Exists(extractDir))
                            Directory.Delete(extractDir, true);

                        Directory.CreateDirectory(extractDir);
                        ZipFile.ExtractToDirectory(zipPath, extractDir);

                        string[] fontFiles = Directory.GetFiles(extractDir, "*.*", SearchOption.AllDirectories)
                            .Where(f => f.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
                                       f.EndsWith(".otf", StringComparison.OrdinalIgnoreCase))
                            .ToArray();

                        Log($"   Found {fontFiles.Length} font file(s)");

                        foreach (string fontPath in fontFiles)
                        {
                            if (InstallFont(fontPath, installSystemWide))
                                totalInstalled++;
                            else
                                totalFailed++;
                        }

                        // Cleanup
                        Directory.Delete(extractDir, true);
                    }
                    catch (Exception ex)
                    {
                        Log($"❌ Failed to process {Path.GetFileName(zipPath)}: {ex.Message}");
                        totalFailed++;
                    }
                }

                Log("\n═══════════════════════════════════════════════════");
                Log("✅ Installation complete!");
                Log($"   Successfully installed: {totalInstalled} font(s)");
                if (totalFailed > 0)
                    Log($"   Failed: {totalFailed} font(s)");
                Log("   ℹ️  Restart applications to see new fonts");
                Log("═══════════════════════════════════════════════════\n");

                MessageBox.Show(
                    $"Installation Complete!\n\n" +
                    $"✓ Installed: {totalInstalled} fonts\n" +
                    $"{(totalFailed > 0 ? $"✗ Failed: {totalFailed} fonts\n" : "")}" +
                    $"\nRestart applications to use the new fonts.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                Log($"🔥 Critical error: {ex.Message}");
                MessageBox.Show($"An error occurred:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnInstall.Enabled = true;
                btnSelectFolder.Enabled = true;
                btnClearSelection.Enabled = true;
            }
        }

        private bool InstallFont(string fontPath, bool systemWide)
        {
            try
            {
                string fontsDir = systemWide
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts")
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Microsoft", "Windows", "Fonts");

                Directory.CreateDirectory(fontsDir);
                string destPath = Path.Combine(fontsDir, Path.GetFileName(fontPath));

                File.Copy(fontPath, destPath, true);

                // Register in registry
                string regRoot = systemWide ? "HKEY_LOCAL_MACHINE" : "HKEY_CURRENT_USER";
                string regPath = @"Software\Microsoft\Windows NT\CurrentVersion\Fonts";
                string fontName = Path.GetFileName(fontPath);
                string fontValue = systemWide ? Path.GetFileName(fontPath) : destPath;

                Registry.SetValue($"{regRoot}\\{regPath}", fontName, fontValue, RegistryValueKind.String);

                Log($"   ✓ {Path.GetFileName(fontPath)}");
                return true;
            }
            catch (Exception ex)
            {
                Log($"   ✗ {Path.GetFileName(fontPath)}: {ex.Message}");
                return false;
            }
        }

        private void Log(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(Log), message);
            }
            else
            {
                txtLog.AppendText($"{message}\r\n");
                txtLog.SelectionStart = txtLog.Text.Length;
                txtLog.ScrollToCaret();
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                len /= 1024;
                order++;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        // P/Invoke for getting file icons
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, 
            ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_SMALLICON = 0x1;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x10;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;

        private Icon GetFolderIcon()
        {
            SHFILEINFO shfi = new SHFILEINFO();
            SHGetFileInfo("folder", FILE_ATTRIBUTE_DIRECTORY, ref shfi,
                (uint)Marshal.SizeOf(shfi), SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES);
            return Icon.FromHandle(shfi.hIcon);
        }

        private Icon GetFileIcon(string extension)
        {
            SHFILEINFO shfi = new SHFILEINFO();
            SHGetFileInfo(extension, FILE_ATTRIBUTE_NORMAL, ref shfi,
                (uint)Marshal.SizeOf(shfi), SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES);
            return Icon.FromHandle(shfi.hIcon);
        }
    }
}
