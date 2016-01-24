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

namespace NBA2K16ExplorerWV
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public struct TOCEntry
        {
            public string Container;
            public string Name;
            public long Offset;
            public long Size;
        }

        public string basepath;
        public List<TOCEntry> TOCList;
        public string CurrentContainer;
        public string CurrentFileName;

        private void openNBA2K16exeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "NBA2K16.exe|NBA2K16.exe";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                basepath = Path.GetDirectoryName(d.FileName) + "\\";
                LoadData();
            }
        }

        void LoadData()
        {
            LoadTOC();
            LoadContainer();
        }

        void LoadTOC()
        {            
            listBox1.Items.Clear();
            string[] lines = File.ReadAllLines(basepath + "manifest");
            TOCList = new List<TOCEntry>();
            foreach (string line in lines)
            {
                TOCEntry e = new TOCEntry();
                string[] p1 = line.Split('\t');
                e.Container = p1[0];
                string[] p2 = p1[1].Trim().Split(' ');
                e.Name = p2[0];
                e.Offset = Convert.ToInt64(p2[1]);
                e.Size = Convert.ToInt64(p2[2]);
                TOCList.Add(e);
            }
            vScrollBar1.Maximum = lines.Length - 100;
            RefreshTOC();
        }

        void RefreshTOC()
        {
            listBox1.Items.Clear();
            int pos = vScrollBar1.Value;
            for (int i = pos; i < pos + 100 && i < TOCList.Count; i++)
            {
                TOCEntry e = TOCList[i];
                listBox1.Items.Add(i + " : Container='" + e.Container + "' Name='" + e.Name + "' Size=" + e.Size + " bytes Offset=0x" + e.Offset.ToString("X"));
            }
        }

        void LoadContainer()
        {
            List<string> Container = new List<string>();
            bool found;
            foreach (TOCEntry e in TOCList)
            {
                found = false;
                foreach(string c in Container)
                    if (c == e.Container)
                    {
                        found = true;
                        break;
                    }
                if (!found)
                    Container.Add(e.Container);
            }
            Container.Sort();
            listBox2.Items.Clear();
            foreach (string c in Container)
                listBox2.Items.Add(c);
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            RefreshTOC();
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox3.Items.Clear();
            int n = listBox2.SelectedIndex;
            if (n == -1)
                return;
            CurrentContainer = listBox2.Items[n].ToString();
            foreach (TOCEntry te in TOCList)
                if (te.Container == CurrentContainer)
                    listBox3.Items.Add("@" + te.Offset.ToString("X8") + " : " + te.Name);
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox3.SelectedIndex;
            if (n == -1)
                return;
            int count = 0;
            foreach (TOCEntry te in TOCList)
                if (te.Container == CurrentContainer && (count++) == n)
                {
                    CurrentFileName = te.Name + ".zip";
                    MemoryStream m = new MemoryStream();
                    FileStream fs = new FileStream(basepath + CurrentContainer, FileMode.Open, FileAccess.Read);
                    fs.Seek(te.Offset, 0);                    
                    hb1.ByteProvider = new Be.Windows.Forms.DynamicByteProvider(ReadLZMA(fs, te.Size));
                    break;
                }
        }

        byte[] ReadLZMA(Stream input, long size)
        {
            MemoryStream output = new MemoryStream();
            int b, count = 0;
            while ((b = input.ReadByte()) != -1 && (count++) < size)
                output.WriteByte((byte)b);
            return output.ToArray();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = CurrentFileName + "|" + CurrentFileName;
            d.FileName = CurrentFileName;
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MemoryStream m = new MemoryStream();
                for (long i = 0; i < hb1.ByteProvider.Length; i++)
                    m.WriteByte(hb1.ByteProvider.ReadByte(i));
                File.WriteAllBytes(d.FileName, m.ToArray());
                MessageBox.Show("Done.");
            }
        }
    }
}
