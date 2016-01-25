using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevIL;

namespace NBA2K16ExplorerWV
{
    public partial class Form1 : Form
    {
        public string currentPath;
        public string myPath = Path.GetDirectoryName(Application.ExecutablePath) + "\\";
        public Archive ar;

        public Form1()
        {
            InitializeComponent();
        }

        private void openNBA2K16exeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "NBA2K16.exe|NBA2K16.exe";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Log.Clear();
                FileSystem.Load(Path.GetDirectoryName(d.FileName) + "\\");
                setCurrentPath("");                
                Log.PrintLn("Done.");
            }
        }

        private void setCurrentPath(string p)
        {
            currentPath = p;
            label1.Text = "Path: /" + p;
            listBox1.Items.Clear();
            listBox1.Items.AddRange(FileSystem.getFoldersFromPath(currentPath));
            listBox2.Items.Clear();
            listBox2.Items.AddRange(FileSystem.getFilesFromPath(currentPath));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Log.setBox(rtb1);
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            listBox2.Items.Clear();
            string name = listBox1.Items[n].ToString();
            if (name == "..")
            {
                string[] parts = currentPath.Split('/');
                if (parts.Length == 2)
                    setCurrentPath("");
                else
                {
                    StringBuilder path = new StringBuilder();
                    for (int i = 0; i < parts.Length - 2; i++)
                        path.Append(parts[i] + "/");
                    setCurrentPath(path.ToString());
                }
            }
            else
                setCurrentPath(currentPath + name + "/");  
            if (currentPath.Length != 0)
                listBox1.Items.Insert(0, "..");
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox2.SelectedIndex;
            if (n == -1)
                return;
            string path = currentPath + listBox2.Items[n].ToString();
            FileSystem.TOCEntry toc = FileSystem.findTOCbyPath(path);
            hb1.ByteProvider = new Be.Windows.Forms.DynamicByteProvider(FileSystem.getDataByTOC(toc));
            rtb2.Text = "";
            rtb2.AppendText("Path = " + path + "\n");
            rtb2.AppendText("Size = 0x" + toc.Size.ToString("X") + " bytes\n");
            rtb2.AppendText("Offset = 0x" + toc.RealOffset.ToString("X") + "\n");
            rtb2.AppendText("Container = " + toc.Container + "\n");
            CheckArchive(toc);
        }

        private void CheckArchive(FileSystem.TOCEntry toc)
        {
            listBox3.Items.Clear();
            if (hb1.ByteProvider.Length > 2 && (char)hb1.ByteProvider.ReadByte(0) == 'P' && (char)hb1.ByteProvider.ReadByte(1) == 'K')
            {
                File.WriteAllBytes(myPath + "temp.zip", FileSystem.getDataByTOC(toc));
                ar = new Archive(myPath + "temp.zip");
                listBox3.Items.AddRange(ar.getFiles());
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listBox2.SelectedIndex;
            if (n == -1)
                return;
            string name = listBox2.Items[n].ToString();
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = name + "|" + name;
            d.FileName = name;
            if(d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MemoryStream m = new MemoryStream();
                for (long i = 0; i < hb1.ByteProvider.Length; i++)
                    m.WriteByte(hb1.ByteProvider.ReadByte(i));
                File.WriteAllBytes(d.FileName, m.ToArray());
                MessageBox.Show("Done.");
            }
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox3.SelectedIndex;
            if (n == -1)
                return;
            string name = listBox3.Items[n].ToString();
            byte[] data = ar.getFileData(name);
            hb2.ByteProvider = new Be.Windows.Forms.DynamicByteProvider(data);
            pb1.CreateGraphics().Clear(Color.White);
            if (name.ToLower().EndsWith(".dds"))
            {
                File.WriteAllBytes(myPath + "temp.dds", data);
                pb1.Image = DevIL.DevIL.LoadBitmap(myPath + "temp.dds");
            }
        }

        private void extractFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listBox3.SelectedIndex;
            if (n == -1)
                return;
            string name = listBox3.Items[n].ToString();
            byte[] data = ar.getFileData(name);
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = name + "|" + name;
            if(d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllBytes(d.FileName, data);
                MessageBox.Show("Done.");
            }
        }
    }
}
