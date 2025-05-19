namespace InstallFonts
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            folderBrowserDialog1 = new FolderBrowserDialog();
            btnSelectFolder = new Button();
            txtLog = new TextBox();
            btnInstallFonts = new Button();
            openFileDialog1 = new OpenFileDialog();
            treeViewFolders = new TreeView();
            SuspendLayout();
            // 
            // btnSelectFolder
            // 
            btnSelectFolder.Location = new Point(364, 53);
            btnSelectFolder.Name = "btnSelectFolder";
            btnSelectFolder.Size = new Size(94, 23);
            btnSelectFolder.TabIndex = 0;
            btnSelectFolder.Text = "Select Folder";
            btnSelectFolder.UseVisualStyleBackColor = true;
            btnSelectFolder.Click += btnSelectFolder_Click;
            // 
            // txtLog
            // 
            txtLog.Location = new Point(76, 197);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.Size = new Size(382, 130);
            txtLog.TabIndex = 1;
            // 
            // btnInstallFonts
            // 
            btnInstallFonts.Location = new Point(373, 94);
            btnInstallFonts.Name = "btnInstallFonts";
            btnInstallFonts.Size = new Size(75, 23);
            btnInstallFonts.TabIndex = 3;
            btnInstallFonts.Text = "Install";
            btnInstallFonts.UseVisualStyleBackColor = true;
            btnInstallFonts.Click += btnInstallFonts_Click;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // treeViewFolders
            // 
            treeViewFolders.Location = new Point(76, 40);
            treeViewFolders.Name = "treeViewFolders";
            treeViewFolders.Size = new Size(256, 130);
            treeViewFolders.TabIndex = 4;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(531, 355);
            Controls.Add(treeViewFolders);
            Controls.Add(btnInstallFonts);
            Controls.Add(txtLog);
            Controls.Add(btnSelectFolder);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FolderBrowserDialog folderBrowserDialog1;
        private Button btnSelectFolder;
        private TextBox txtLog;
        private Button btnInstallFonts;
        private CheckBox chkSystemWide;
        private OpenFileDialog openFileDialog1;
        private TreeView treeViewFolders;
    }
}
