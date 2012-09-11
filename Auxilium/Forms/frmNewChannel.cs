using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Auxilium
{
    public partial class frmNewChannel : Form
    {
        private Nexus.Client Client;
        private Dictionary<string, int> channels;
        public string Channel;
        #region Delegates
        private delegate void AddChannelsDelegate(object data);
        #endregion
        public frmNewChannel(Nexus.Client client)
        {
            Client = client;
            Client.Incoming += new Nexus.Client.IncomingEventHandler(Client_Incoming);
            Client.Send(Nexus.Serialize(new object[] { (int)Nexus.Headers.Channels }));
            InitializeComponent();
        }

        void Client_Incoming(Nexus.Client c, byte[] d)
        {
            object[] data = (object[])Nexus.Deserialize(d);
            switch ((int)data[0])
            {
                case (int)Nexus.Headers.Channels:
                    System.Threading.Thread.Sleep(200);
                    AddChannels(channels = (Dictionary<string, int>)data[1]);
                    ChangeLabel();
                    break;
            }
        }
        private void AddChannels(object data)
        {
            if (InvokeRequired)
            {
                Invoke(new AddChannelsDelegate(AddChannels), data);
                return;
            }
            comboBox1.BeginUpdate();
            comboBox1.Items.Clear();
            foreach (string str in channels.Keys)
                comboBox1.Items.Add(str);
            comboBox1.Text = comboBox1.Items[0].ToString();
            comboBox1.EndUpdate();
        }
        private void ChangeLabel()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(ChangeLabel));
                return;
            }
            label1.Text = "Channels grabbed.";
        }
        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)) 
                e.SuppressKeyPress = true;
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            int outVal;
            channels.TryGetValue(comboBox1.Text, out outVal);
            Channel = comboBox1.Text;
            Client.Send(Nexus.Serialize(new object[] { (int)Nexus.Headers.SelectChannel, outVal }));
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }
    }
}
