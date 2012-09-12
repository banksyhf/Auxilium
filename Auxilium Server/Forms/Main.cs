using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace Auxilium_Server
{
    public partial class Main : Form
    {
        #region Global Variables
        private Nexus.Server server = new Nexus.Server();
        private object[] MOTD = new object[] { Convert.ToBoolean(File.ReadAllText("motd.txt").Split('~')[0]), File.ReadAllText("motd.txt").Split('~')[1] };
        MySqlConnection connection = new MySqlConnection();
        MySqlDataAdapter adapter;
        #endregion
        #region Delegates
        private delegate void UpdateListDelegate(ListView list, object text, Nexus.Client c = null);
        private delegate void RemoveDelegate(ListView list, Nexus.Client c);
        private delegate void SendUsersDelegate(int channel, string[] moveEvent = null);
        #endregion
        public Main()
        {
            try
            {
                connection.ConnectionString = "server=localhost;user id=auxilium;password=123456;database=Auxilium";
                connection.Open();
                server.Incoming += server_Incoming;
                server.Listen(3357);
                InitializeComponent();
            }
            catch (Exception ex)
            {
                File.AppendAllText("aux errors.txt", ex.ToString() + Environment.NewLine);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            GetQuery("select * from users");
        }
        #region SQL Query
        private delegate void GetQueryDelegate(string query);
        private void GetQuery(string query)
        {
            if (InvokeRequired)
            {
                Invoke(new GetQueryDelegate(GetQuery), query);
                return;
            }
            adapter = new MySqlDataAdapter(query, connection);
            DataSet DS = new DataSet();
            adapter.Fill(DS);
            dataGridView1.DataSource = DS.Tables[0];
        }
        private void InsertUser(string user, string pass)
        {
            MySqlCommand cmd = new MySqlCommand("", connection);
            cmd.CommandText = "INSERT INTO users VALUES (@username,@password);";
            cmd.Parameters.AddWithValue("@username", user);
            cmd.Parameters.AddWithValue("@password", pass);
            cmd.ExecuteNonQuery();
            GetQuery("select * from users");
        }
        private bool CheckLogin(string user, string pass)
        {
            foreach (DataGridViewRow dg in dataGridView1.Rows)
            {
                if (dg.Cells[0].Value.ToString().ToLower().Trim() == user && (string)dg.Cells[1].Value == pass)
                    return true;
            }
            return false;
        }
        private bool CheckRegister(string user)
        {
            foreach (DataGridViewRow dg in dataGridView1.Rows)
            {
                if (dg.Cells[0].Value.ToString().ToLower().Trim() == user.Trim().ToLower())
                    return true;
            }
            return false;
        }
        #endregion
        void server_Incoming(Nexus.Client c, byte[] d)
        {
            try
            {
                object[] data = (object[])Nexus.Deserialize(d);
                switch ((int)data[0])
                {
                    case (int)Nexus.Headers.SendLogin:
                        GetQuery("select * from users");
                        c.LoggedIn = CheckLogin(data[1].ToString().ToLower().Trim(), data[2].ToString());
                        if (c.LoggedIn)
                        {
                            c.Username = data[1].ToString();
                            c.Send(Nexus.Serialize(new object[] { (int)Nexus.Headers.LoginSuccess }));
                            c.Status += c_Status;
                            UpdateList(lvUsers, data[1].ToString(), c);
                            c.Channel = (int)Nexus.Channels.Lounge;
                            SendUsers(c.Channel);
                            if ((bool)MOTD[0])
                                c.Send(Nexus.Serialize(new object[] { 2, MOTD[1] }));
                        }
                        else
                        {
                            c.Send(Nexus.Serialize(new object[] { (int)Nexus.Headers.LoginFail }));
                        }
                        break;
                    case (int)Nexus.Headers.Message:
                        if (c.LoggedIn)
                        {
                            if (!(string.IsNullOrEmpty(data[1].ToString().Trim())))
                            {
                                UpdateList(lvMessages, new string[] { c.Username, data[1].ToString() });
                                foreach (Nexus.Client client in server.Connections)
                                    if (client != c && client.Channel == c.Channel)
                                        client.Send(Nexus.Serialize(new object[] { 1, c.Username, data[1].ToString() }));
                            }
                        }
                        break;
                    case (int)Nexus.Headers.SendRegister:
                        if (!CheckRegister(data[1].ToString()) && data[1].ToString().Length <= 24)
                        {
                            c.Send(Nexus.Serialize(new object[] { (int)Nexus.Headers.RegisterSuccess }));
                            InsertUser(data[1].ToString(), data[2].ToString());
                        }
                        else
                            c.Send(Nexus.Serialize(new object[] { (int)Nexus.Headers.RegisterFail }));
                        break;
                    case (int)Nexus.Headers.Channels:
                        int[] channels = (int[])Enum.GetValues(typeof(Nexus.Channels));
                        Dictionary<string, int> chans = new Dictionary<string, int>();
                        foreach (int str in channels)
                            chans.Add(Enum.GetName(typeof(Nexus.Channels), str).Replace("VB_NET", "VB.NET").Replace("CSharp", "C#"), str);
                        c.Send(Nexus.Serialize(new object[] { (int)Nexus.Headers.Channels, chans }));
                        break;
                    case (int)Nexus.Headers.SelectChannel:
                        int oldChannel = c.Channel;
                        string chanName = Enum.GetName(typeof(Nexus.Channels), (int)data[1]).Replace("VB_NET", "VB.NET").Replace("CSharp", "C#");
                        c.Channel = (int)data[1];
                        SendUsers(c.Channel, new string[] { c.Username, "has joined this channel." });
                        SendUsers(oldChannel, new string[] { c.Username, "has moved to " + chanName });
                        break;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); c.Disconnect(); /* Get the fuck out. */ }
        }
        void c_Status(Nexus.Client c, Nexus.State state)
        {
            switch (state)
            {
                case Nexus.State.Disconnected:
                    Remove(lvUsers, c);
                    break;
            }
        }
        private void UpdateList(ListView list, object text, Nexus.Client c = null)
        {
            if (InvokeRequired)
            {
                Invoke(new UpdateListDelegate(UpdateList), list, text, c);
                return;
            }
            if (c != null)
            {
                ListViewItem LVI = new ListViewItem((string)text);
                LVI.Tag = c;
                list.Items.Add(LVI);
            }
            else
            {
                string[] shit = (string[])text;
                ListViewItem lvi = new ListViewItem(shit[0]);
                lvi.SubItems.Add(shit[1]);
                list.Items.Add(lvi);
            }
        }
        private void Remove(ListView list, Nexus.Client c)
        {
            if (InvokeRequired)
            {
                Invoke(new RemoveDelegate(Remove), list, c);
                return;
            }
            foreach (ListViewItem item in list.Items)
                if (((Nexus.Client)item.Tag) == c)
                    list.Items.Remove(item);
            SendUsers(c.Channel, new string[] {c.Username, "has disconnected."});
        }
        private void SendUsers(int channel, string[] moveEvent = null)
        {
            if (InvokeRequired)
            {
                Invoke(new SendUsersDelegate(SendUsers), channel, moveEvent);
                return;
            }
            List<string> connected = new List<string>();
            foreach (ListViewItem item in lvUsers.Items)
                if (((Nexus.Client)item.Tag).Channel == channel)
                    connected.Add(item.SubItems[0].Text);
            foreach (ListViewItem item in lvUsers.Items)
                if (((Nexus.Client)item.Tag).Channel == channel)
                    if (!(moveEvent == null) && !(((Nexus.Client)item.Tag).Username == moveEvent[0])) {
                        ((Nexus.Client)item.Tag).Send(Nexus.Serialize(new object[] { (int)Nexus.Headers.UserChannelEvent, moveEvent}));
                    } else {
                        ((Nexus.Client)item.Tag).Send(Nexus.Serialize(new object[] { (int)Nexus.Headers.Users, connected.ToArray() }));
                    }
        }
        private void copyAllMessagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder messages = new StringBuilder();
            foreach (ListViewItem lvi in lvMessages.Items)
                messages.Append(string.Format("{0}: {1}{2}", lvi.Text, lvi.SubItems[1].Text, Environment.NewLine));
            Clipboard.SetText(messages.ToString().Remove(messages.ToString().Length - 2));
        }
        private void copySelectedMessagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder messages = new StringBuilder();
            foreach (ListViewItem lvi in lvMessages.SelectedItems)
                messages.Append(string.Format("{0}: {1}{2}", lvi.SubItems[0].Text, lvi.SubItems[1].Text, Environment.NewLine));
            Clipboard.SetText(messages.ToString().Remove(messages.ToString().Length - 2));
        }
    }
}
