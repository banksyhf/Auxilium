using System;
using System.Collections.Generic;
using System.Linq;
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
        public static string Hash(string input)
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
        public static bool ApplicationIsActivated()
        {
            IntPtr activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
                return false;
            int procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

    }
}
namespace Extensions
{
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}