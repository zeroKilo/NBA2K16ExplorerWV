using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SevenZip;

namespace NBA2K16ExplorerWV
{
    public class Archive
    {
        public SevenZipExtractor ex;
        public Archive(string path)
        {
            string p = Path.GetDirectoryName(Application.ExecutablePath) + "\\7z.dll";
            SevenZipExtractor.SetLibraryPath(p);
            ex = new SevenZipExtractor(path);
        }
        public string[] getFiles()
        {
            List<string> result = new List<string>();
            result.AddRange(ex.ArchiveFileNames);
            result.Sort();
            return result.ToArray();
        }
        public byte[] getFileData(string name)
        {
            MemoryStream m = new MemoryStream();
            for (int i = 0; i < ex.ArchiveFileNames.Count; i++)
                if (ex.ArchiveFileNames[i] == name)
                {
                    ex.ExtractFile(i, m);
                    break;
                }
            return m.ToArray();
        }
    }
}
