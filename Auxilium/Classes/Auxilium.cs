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

        internal static bool CheckBottom(RichTextBox rtb)
        {
            Scrollbarinfo info = new Scrollbarinfo();
            info.CbSize = Marshal.SizeOf(info);
            int res = GetScrollBarInfo(rtb.Handle, ObjidVscroll, ref info);
            return info.XyThumbBottom > (info.RcScrollBar.Bottom - info.RcScrollBar.Top - (info.DxyLineButton * 2));
        }

        #region APIs/Types

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr handle);

        [DllImport("user32.dll")]
        public static extern bool FlashWindow(IntPtr handle, bool invert);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetScrollBarInfo")]
        private static extern int GetScrollBarInfo(IntPtr hWnd, uint idObject, ref Scrollbarinfo psbi);

        public const uint ObjidVscroll = 0xFFFFFFFB;

        public struct Scrollbarinfo
        {
            public int CbSize;
            public Rect RcScrollBar;
            public int DxyLineButton;
            public int XyThumbTop;
            public int XyThumbBottom;
            public int Reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public int[] Rgstate;
        }

        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        #endregion
    }
    public enum MenuScreen
    {
        SignIn,
        Register,
        Chat,
        Reconnect,
        PrivateMessages
    }

    public enum ServerPacket : byte
    {
        SignIn,
        Register,
        UserList,
        UserJoin,
        UserLeave,
        ChannelList,
        MOTD,
        Chatter,
        GlobalMsg,
        BanList,
        PM,
        KeepAlive
    }

    public enum ClientPacket : byte
    {
        SignIn,
        Register,
        Channel,
        ChatMessage,
        PM,
        KeepAlive
    }

    public class User
    {
        public string Name;
        public ushort ID;
        public bool Admin;
        public User(ushort id, string name, bool admin)
        {
            Name = name;
            ID = id;
            Admin = admin;
        }
    }
}