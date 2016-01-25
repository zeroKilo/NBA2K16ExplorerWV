using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace NBA2K16ExplorerWV
{
    public static class Log
    {
        private static readonly object _sync = new object();
        private static RichTextBox rtb;

        public static void setBox(RichTextBox r)
        {
            rtb = r;
        }

        public static void Clear()
        {
            lock (_sync)
            {
                rtb.Invoke((MethodInvoker)delegate { rtb.Text = ""; });
                Application.DoEvents();
            }
        }

        public static void PrintLn(string s)
        {
            lock (_sync)
            {
                rtb.Invoke((MethodInvoker)delegate { 
                    rtb.AppendText(s + "\n");
                    rtb.SelectionStart = rtb.Text.Length;
                    rtb.ScrollToCaret();
                });
                Application.DoEvents();
            }
        }
    }
}
