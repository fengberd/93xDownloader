using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Downloader_93x
{
    static class Program
    {
        private static readonly string[] UNITS = new string[] { "B","KB","MB","GB","TB","PB","EB" };

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        public static string FormatSize(long bytes_)
        {
            if(bytes_ < 0)
            {
                return "UNKNOWN";
            }
            ulong bytes = Convert.ToUInt64(bytes_);
            int c = 0;
            for(c = 0;c < UNITS.Length;c++)
            {
                ulong m = (ulong)1 << ((c + 1) * 10);
                if(bytes < m)
                    break;
            }
            double n = bytes / (double)((ulong)1 << (c * 10));
            return string.Format("{0:0.##} {1}",n,UNITS[c]);
        }
    }
}
