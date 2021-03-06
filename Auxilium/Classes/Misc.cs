﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace Auxilium
{
    static class Functions
    {
        public static string SHA1(string input)
        {
            SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();

            string salt = input[input[0] % input.Length] + "B4TH5ALTS" + input.Length;

            byte[] inputBytes = Encoding.UTF8.GetBytes(input + salt);
            byte[] hashedBytes = sha.ComputeHash(inputBytes);

            return BitConverter.ToString(hashedBytes).Replace("-", "");
        }

        //internal static bool CheckBottom(RichTextBox rtb)
        //{
        //    Scrollbarinfo info = new Scrollbarinfo();
        //    info.CbSize = Marshal.SizeOf(info);
        //    int res = GetScrollBarInfo(rtb.Handle, ObjidVscroll, ref info);
        //    return info.XyThumbBottom > (info.RcScrollBar.Bottom - info.RcScrollBar.Top - (info.DxyLineButton * 2));
        //}

        #region APIs/Types

        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", EntryPoint = "FlashWindow")]
        public static extern bool FlashWindow(IntPtr handle, bool invert);

        //[DllImport("user32.dll", EntryPoint="SetForegroundWindow")]
        //public static extern int SetForegroundWindow(IntPtr handle);

        //[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        //public static extern int GetWindowLong(IntPtr handle, int index);

        //[DllImport("user32.dll", EntryPoint = "GetScrollInfo")]
        //private static extern bool GetScrollInfo(IntPtr handle, int bar, ref SCROLLINFO info);

        //[DllImport("user32.dll", EntryPoint = "SetScrollInfo")]
        //private static extern int SetScrollInfo(IntPtr handle, int bar, ref SCROLLINFO info, bool redraw);

        //[StructLayout(LayoutKind.Sequential)]
        //public struct SCROLLINFO
        //{
        //    public uint size;
        //    public uint mask;
        //    public int min;
        //    public int max;
        //    public int page;
        //    public int position;
        //    public int trackPosition;
        //}

        //public static SCROLLINFO VScroll;
        //public static bool GetScroll(IntPtr handle)
        //{
        //    VScroll = new SCROLLINFO();
        //    VScroll.size = (uint)Marshal.SizeOf(VScroll);
        //    VScroll.mask = 7;

        //    return GetScrollInfo(handle, 1, ref VScroll);
        //}

        //public static void SetScroll(IntPtr handle, int position)
        //{
        //    if (GetScroll(handle))
        //    {
        //        VScroll.mask = 4;
        //        VScroll.position = position;

        //        SetScrollInfo(handle, 1, ref VScroll, true);
        //    }
        //}

        //public static bool VScrollVisible(IntPtr handle)
        //{
        //    int value = GetWindowLong(handle, -16);
        //    return (value & 0x00200000) != 0;
        //}

        #endregion
    }
    public enum MenuScreen
    {
        SignIn,
        Register,
        Chat,
        Reconnect,
        ViewProfile,
        EditProfile
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
        PMConfirm,
        KeepAlive,
        WakeUp,
        RecentMessages,
        News,
        ViewProfile,
        Profile,
        EditProfile,
        ClearChat,
        NotVerified,
        AuthResponse
    }

    public enum ClientPacket : byte
    {
        SignIn,
        Register,
        Channel,
        ChatMessage,
        PM,
        KeepAlive,
        News,
        ViewProfile,
        EditProfile,
        ResendVerification,
        AuthCode
    }

    public enum AuthType : byte
    {
        Unknown,
        AccountVerification
    }

    public enum SpecialRank : byte
    {
        Admin = 41
    }

    public class User
    {
        public string Name;
        public byte Rank;
        public bool Idle;

        public User(string name, byte rank, bool idle)
        {
            Name = name;
            Rank = rank;
            Idle = idle;
        }
    }

    struct ChatMessage
    {
        public Color Color;
        public string Value;

        public ChatMessage(Color color, string value)
        {
            Color = color;
            Value = value;
        }
    }
}