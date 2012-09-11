using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Microsoft.Win32;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Auxilium
{
    public partial class frmLogin : Form
    {
        private Nexus.Client Client;
        public string User = string.Empty;
        public frmLogin(Nexus.Client client)
        {
            InitializeComponent();
            Client = client;
            Client.Incoming += new Nexus.Client.IncomingEventHandler(Client_Incoming);
            Client.Status += new Nexus.Client.StatusEventHandler(Client_Status);
        }

        void Client_Status(Nexus.Client c, Nexus.State state)
        {

        }

        void Client_Incoming(Nexus.Client c, byte[] d)
        {
            object[] data = (object[])Nexus.Deserialize(d);
            switch ((int)data[0])
            {
                case (int)Nexus.Headers.LoginSuccess:
                    User = tbUser.Text;
                    this.DialogResult = DialogResult.OK;
                    //this.Close();
                    break;
                case (int)Nexus.Headers.LoginFail:
                    MessageBox.Show("Username or password is wrong, please try again.");
                    break;
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            new frmRegister(Client).ShowDialog();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (cbRemember.Checked) {
                Registry.CurrentUser.OpenSubKey("Software\\Auxilium", true).SetValue("name", tbUser.Text);
                Registry.CurrentUser.OpenSubKey("Software\\Auxilium", true).SetValue("pass", tbPass.Text);
            } else {
                try { Registry.CurrentUser.OpenSubKey("Software\\Auxilium", true).DeleteValue("pass"); Registry.CurrentUser.OpenSubKey("Software\\Auxilium", true).DeleteValue("name"); }
                catch { }
            }
            Client.Send(Nexus.Serialize(new object[] { (int)Nexus.Headers.SendLogin, tbUser.Text, Auxilium.Hash(tbPass.Text) }));
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            //registry = a bitch.
            string[] shit = Registry.CurrentUser.OpenSubKey("Software").GetSubKeyNames();
            if (shit.Contains("Auxilium"))
            {
                tbUser.Text = Registry.CurrentUser.OpenSubKey("Software\\Auxilium").GetValue("name", "").ToString();
                tbPass.Text = Registry.CurrentUser.OpenSubKey("Software\\Auxilium").GetValue("pass", "").ToString();
            } else {
                Registry.CurrentUser.CreateSubKey("Software\\Auxilium");
            }
            if (!(string.IsNullOrEmpty(tbUser.Text.Trim())) && !(string.IsNullOrEmpty(tbUser.Text.Trim())))
                cbRemember.Checked = true;
            tbUser.Select();
        }
    }
}
