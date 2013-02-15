using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Auxilium.Classes;

namespace Auxilium.Forms
{
    public partial class frmUpdate : Form
    {
        public frmUpdate(string version, string url)
        {
            InitializeComponent();
            pictureInfo.Image = SystemIcons.Information.ToBitmap();

            labelInfo.Text = string.Format("An update was found!\nUpdating to version {0}", version);

            Updater updater = new Updater(url);
            updater.ProgressChanged += percentage => progressUpdateBar.Value = percentage;
            updater.Initialize();
        }
    }
}
