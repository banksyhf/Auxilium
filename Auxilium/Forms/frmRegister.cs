using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace Auxilium
{
    public partial class frmRegister : Form
    {
        private Nexus.Client Client;
        public frmRegister(Nexus.Client client)
        {
            InitializeComponent();
            Client = client;
            Client.Incoming += new Nexus.Client.IncomingEventHandler(client_Incoming);
        }

        void client_Incoming(Nexus.Client c, byte[] d)
        {
            object[] data = (object[])Nexus.Deserialize(d);
            switch ((int)data[0])
            {
                case (int)Nexus.Headers.RegisterSuccess:
                    MessageBox.Show("Registered succesfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CloseMe();
                    break;
                case (int)Nexus.Headers.RegisterFail:
                    MessageBox.Show("Something went wrong durring registration. Please check to make sure everything is correct.");
                    break;
            }
        }
        private void CloseMe()
        {
            if (InvokeRequired)
                Invoke(new MethodInvoker(CloseMe));
            else
                this.Close();
        }
        private void btnRegister_Click(object sender, EventArgs e)
        {
            Client.Send(Nexus.Serialize(new object[] { (int)Nexus.Headers.SendRegister, tbUser.Text, Auxilium.Hash(tbPass.Text) }));
        }

        private void frmRegister_Load(object sender, EventArgs e)
        {
            tbUser.Select();
        }
    }
}
