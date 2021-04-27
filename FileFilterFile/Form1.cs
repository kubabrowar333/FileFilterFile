using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FileFilterFile
{
    public partial class Form1 : Form
    {
        public class FolderFilter
        {
            public string FolderName { get; set; }
            public Regex Filter { get; set; }
            public int FileCunut { get; set; }
            public long Size { get; set; }
            public FolderFilter(string folder, string regex)
            {
                FolderName = folder;
                Filter = new Regex(regex);
                FileCunut = 0;
                Size = 0;
            }
            public void Clear()
            {
                FileCunut = 0;
                Size = 0;
            }
        }

        HashSet<FolderFilter> DeflautFolderFilter = new HashSet<FolderFilter>()
        {
            new FolderFilter("Smartphone",@"^[0-9]{8}_[0-9]{6}(_[0-9]){0,3}\.(mp4|gif|jpg)$"),
            new FolderFilter("Screenshot",@"^Screenshot_[0-9]{8}-[0-9]{6}_.*\.jpg$"),
            new FolderFilter("Messenger",@"^received_[0-9]{15,16}\.(mp4|png|gif)$"),
            new FolderFilter("Facebook",@"^FB_IMG_[0-9]{13}\.(mp4|png|jpg)$"),
            new FolderFilter("Twitter",@"^IMG_[0-9]{8}_[0-9]{6}\.(mp4|png|jpg)$"),
            new FolderFilter("Reddit",@"^[a-z0-9]{7}(-[0-9]){0,1}\.(png|gif|jpg)$"),
            new FolderFilter("Discord",@"^JPEG_[0-9]{8}_[0-9]{6}\.jpg$"),
            new FolderFilter("WhatsApp",@"^[a-zA-Z]{3}(_|-)[0-9]{8}-WA[0-9]{4}\.(mp4|jpg)$"),
            new FolderFilter("Other",@"^.*\.(mp4|png|gif)$"),
        };
        public Form1()
        {
            InitializeComponent();
        }

        /*

        Photo:      20180905_104540.jpg
        Video:      20200714_235013.mp4
        Screenshot: Screenshot_20200714-163810_Call.jpg or Screenshot_20200604-191924_Drive.jpg
        Messengfer: received_234887250744340.png // .mp4
        Facebook:   FB_IMG_1557650834258.jpg
        Twitter:    IMG_20190503_205558.jpg 
        Redit:      75d0124.png or 76a722e-1.jpg  //.gif
        Discord:    JPEG_20200128_152815.jpg
        Whatsapp:   IMG_20191117-WA0007.jpg or VID_20191117-WA0007.mp4    
        Other:      

        */
        private void DisplayFolders(HashSet<FolderFilter> folders)
        {
            listView1.Items.Clear();
            foreach (var folder in folders)
            {
                ListViewItem item = new ListViewItem(folder.FolderName);
                item.SubItems.Add(folder.FileCunut.ToString());

                string size_s = "";
                long size = folder.Size; // kb
                if (size < 1024)size_s = size + " KB";
                else if(size < 1048576) size_s = size/1024 + " MB";
                else size_s = size/1204/1024 + " GB";
                item.SubItems.Add(size_s);
                listView1.Items.Add(item);
            }
        }

        private bool CreateFolders(string dir,HashSet<FolderFilter> folders)
        {
            try
            {
                foreach (var folder in folders)
                    Directory.CreateDirectory(Path.Combine(dir, folder.FolderName));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił nieoczekiwany błąd podczas tworzenia folderu:\n" + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private bool DeleteEmptyFolders(string dir, HashSet<FolderFilter> folders)
        {
            try
            {
                foreach (var folder in folders)
                    if (!Directory.EnumerateFiles(Path.Combine(dir, folder.FolderName)).Any())
                        Directory.Delete(Path.Combine(dir, folder.FolderName));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił nieoczekiwany błąd podczas ususwania folderu:\n" + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private void bt_dir_sorce_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Wskaż folder źródłowy.";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                tb_source.Text = folderBrowserDialog.SelectedPath;
                tb_target.Text = Path.Combine(folderBrowserDialog.SelectedPath,"_FileFiletLite");
            }
        }
        private void bt_dir_target_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Wskaż folder docelowy.";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                tb_target.Text = folderBrowserDialog.SelectedPath;
        }

        private void bt_check_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listView1.Items.Clear();
            foreach (var item in DeflautFolderFilter)
                item.Clear();
            ProcessCheckDirectory(tb_source.Text);
            DisplayFolders(DeflautFolderFilter);

        }

        private void ProcessCheckDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessCheckFile(fileName);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessCheckDirectory(subdirectory);
        }

        private void ProcessCheckFile(string fileName)
        {
            string name = Path.GetFileName(fileName);
            name = name.Substring(0, name.LastIndexOf('.') + 1) + name.Substring(name.LastIndexOf('.') + 1).ToLower();
            foreach (var filtr in DeflautFolderFilter)
            {
                if (filtr.Filter.IsMatch(name))
                {
                    filtr.FileCunut++;
                    filtr.Size += new FileInfo(fileName).Length/1024;
                    if (filtr.FolderName == "Other") listBox1.Items.Add(name);
                    break;
                }
            }
        }

        private void ProcessMoveDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessMoveFile(fileName);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessMoveDirectory(subdirectory);
        }

        private void ProcessMoveFile(string fileName)
        {
            string name = Path.GetFileName(fileName);
            name = name.Substring(0, name.LastIndexOf('.') + 1) + name.Substring(name.LastIndexOf('.') + 1).ToLower();
            foreach (var filtr in DeflautFolderFilter)
            {
                if (filtr.Filter.IsMatch(name))
                {
                    var TargetFile = Path.Combine(tb_target.Text, filtr.FolderName, name);
                    filtr.FileCunut++;
                    filtr.Size += new FileInfo(fileName).Length / 1024;
                    if (filtr.FolderName == "Other") listBox1.Items.Add(name);
                    if (!File.Exists(TargetFile)) 
                    {
                        if (cb_autoFill.Checked == true) File.Move(fileName, TargetFile);
                        else File.Copy(fileName, TargetFile);
                    }
                    break;
                }
            }
        }

        private void bt_execute_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listView1.Items.Clear();
            foreach (var item in DeflautFolderFilter)
                item.Clear();

            if (CreateFolders(tb_target.Text, DeflautFolderFilter))
            {
                ProcessMoveDirectory(tb_source.Text);
                DisplayFolders(DeflautFolderFilter);
                foreach (string subdirectory in Directory.GetDirectories(tb_target.Text))
                    if (Directory.GetFiles(subdirectory).Length == 0 && Directory.GetDirectories(subdirectory).Length == 0)
                        Directory.Delete(subdirectory);
            }
        }

        private string[] GetDirPaths()
        {
            string[] dirs =
            {
                tb_source.Text,
                tb_target.Text
            };
            foreach (string path in dirs)
            {
                if (!Directory.Exists(path))
                    if (MessageBox.Show($"Folder nie istnieje.\n{path}\nCzy chcesz stworzyć nowy folder?", "Folder", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        Directory.CreateDirectory(path);
            }
            return dirs;
        }

        private void bt_oepn_source_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", tb_source.Text);
        }

        private void bt_open_target_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", tb_target.Text);
        }

        private void tb_source_TextChanged(object sender, EventArgs e)
        {
            if (tb_source.Text.Length == 0) bt_oepn_source.Enabled = false;
            else bt_oepn_source.Enabled = true;
        }

        private void tb_target_TextChanged(object sender, EventArgs e)
        {
            if (tb_target.Text.Length == 0) bt_open_target.Enabled = false;
            else bt_open_target.Enabled = true;
        }

        private void bt_help_Click(object sender, EventArgs e)
        {
            string HelpMessage = "Aby uzyskać pomoc, zadzwoń na:\n123-456-789";
            MessageBox.Show(HelpMessage, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }


            
    }
}
