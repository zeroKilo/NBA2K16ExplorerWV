using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NBA2K16ExplorerWV
{
    public static class FileSystem
    {
        public struct TOCEntry
        {
            public string Container;
            public string Name;
            public long Offset;
            public long RealOffset;
            public long Size;
        }

        public struct Container
        {
            public string name;
            public long offset;
        }

        private static string basePath;
        private static List<Container> container;
        public static List<TOCEntry> TOCList;


        public static void Load(string path)
        {
            basePath = path;
            LoadTOC();
        }

        private static void LoadTOC()
        {
            Log.PrintLn("Loading manifest...");
            string[] lines = File.ReadAllLines(basePath + "manifest");
            TOCList = new List<TOCEntry>();
            List<string> containerNames = new List<string>();
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
                containerNames.Add(e.Container);
            }
            Log.PrintLn("Looking up Containers...");
            containerNames = containerNames.Distinct().ToList();
            containerNames.Sort();
            container = new List<Container>();
            long offset = 0;
            foreach (string c in containerNames)
            {
                FileStream fs = new FileStream(basePath + c, FileMode.Open, FileAccess.Read);
                fs.Seek(0, SeekOrigin.End);
                Container con = new Container();
                con.name = c;
                con.offset = offset;
                offset += fs.Position;
                container.Add(con);
                fs.Close();
            }
            Log.PrintLn("Updating TOC List...");
            for (int i = 0; i < TOCList.Count; i++)
            {
                TOCEntry e = TOCList[i];
                e.RealOffset = e.Offset - getContainerOffset(e.Container);
                TOCList[i] = e;
            }
        }

        public static string[] getFoldersFromPath(string path)
        {
            List<string> result = new List<string>();
            string[] parts = path.Split('/');
            foreach (TOCEntry e in TOCList)
            {
                if (!e.Name.Contains("/"))
                    continue;
                string[] p2 = e.Name.Split('/');
                if (p2.Length <= parts.Length)
                    continue;
                bool diff = false;
                for(int i=0;i<parts.Length - 1;i++)
                    if (parts[i] != p2[i])
                    {
                        diff = true;
                        break;
                    }
                if (!diff)
                    result.Add(p2[parts.Length - 1]);
            }
            result = result.Distinct().ToList();
            result.Sort();
            return result.ToArray();
        }

        public static string[] getFilesFromPath(string path)
        {
            List<string> result = new List<string>();
            string[] parts = path.Split('/');
            foreach (TOCEntry e in TOCList)
            {
                if (!path.Contains("/") && !e.Name.Contains("/"))
                {
                    result.Add(e.Name);
                    continue;
                }
                string[] p2 = e.Name.Split('/');
                if (p2.Length != parts.Length)
                    continue;
                bool diff = false;
                for (int i = 0; i < parts.Length - 1; i++)
                    if (parts[i] != p2[i])
                    {
                        diff = true;
                        break;
                    }
                if (!diff)
                    result.Add(p2[p2.Length - 1]);
            }
            result = result.Distinct().ToList();
            result.Sort();
            return result.ToArray();
        }

        public static TOCEntry findTOCbyPath(string path)
        {
            TOCEntry result = new TOCEntry();
            foreach (TOCEntry e in TOCList)
                if (e.Name == path)
                    return e;
            return result;
        }

        public static byte[] getDataByTOC(TOCEntry e)
        {
            byte[] data = new byte[e.Size];
            FileStream fs = new FileStream(basePath + e.Container, FileMode.Open, FileAccess.Read);
            fs.Seek(e.RealOffset, 0);
            for (long i = 0; i < e.Size; i++)
                data[i] = (byte)fs.ReadByte();
            fs.Close();
            return data;
        }

        private static TreeNode addPath(TreeNode t, string path, char splitter = '/')
        {
            string[] parts = path.Split(splitter);
            TreeNode f = null;
            foreach (TreeNode c in t.Nodes)
                if (c.Text == parts[0].ToLower())
                {
                    f = c;
                    break;
                }
            if (f == null)
            {
                f = new TreeNode(parts[0].ToLower());
                if (parts.Length == 1)
                    f.Name = "";
                else
                    f.Name = "";
                t.Nodes.Add(f);
            }
            if (parts.Length > 1)
            {
                string subpath = path.Substring(parts[0].Length + 1, path.Length - 1 - parts[0].Length);
                f = addPath(f, subpath, splitter);
            }
            return t;
        }

        private static long getContainerOffset(string con)
        {
            foreach (Container c in container)
                if (c.name == con)
                    return c.offset;
            return 0;
        }
    }
}
