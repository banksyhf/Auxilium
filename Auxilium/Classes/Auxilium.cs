using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Auxilium
{
    static class Auxilium
    {
        public static string SHA1(string input)
        {
            SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashedBytes = sha.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "");
        }

        public static void Update(string szURL)
        {
            try
            {
                MessageBox.Show("Updating!", "Auxilium", MessageBoxButtons.OK, MessageBoxIcon.Information);
                new System.Threading.Thread(() =>
                    {
                        WebClient webClient = new WebClient();
                        if ((szURL.StartsWith("http://") || szURL.StartsWith("https://")))
                        {
                            string szDownloadPath = Application.StartupPath + "\\";
                            string szRandomFileName = szURL.Split('/')[szURL.Split('/').Length - 1];
                            webClient.DownloadFile(szURL, szDownloadPath + szRandomFileName);
                            if (File.Exists(szDownloadPath + szRandomFileName))
                            {
                                Process P = new Process();
                                P.StartInfo.FileName = szDownloadPath + szRandomFileName;
                                P.StartInfo.UseShellExecute = true;
                                P.Start();
                                if (Process.GetProcessById(P.Id) != null)
                                {
                                    ProcessStartInfo Info = new ProcessStartInfo();
                                    Info.Arguments = "/C ping 1.1.1.1 -n 1 -w 1000 > Nul & Del \"" + Application.ExecutablePath + "\"";
                                    Info.CreateNoWindow = true;
                                    Info.WindowStyle = ProcessWindowStyle.Hidden;
                                    Info.FileName = "cmd.exe";
                                    Process.Start(Info);
                                    Environment.Exit(0);
                                }
                            }
                        }
                    }).Start();
            }
            catch { }
        }

        #region APIs

        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern int SetForegroundWindow(IntPtr handle);

        [DllImport("user32.dll", EntryPoint = "FlashWindow")]
        public static extern bool FlashWindow(IntPtr handle, bool invert);

        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
        public static extern IntPtr GetForegroundWindow();

        #endregion
    }
}