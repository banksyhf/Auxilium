using System;
using System.Windows.Forms;

namespace Auxilium.Forms
{
    public partial class frmNews : Form
    {
        private string Value;
        public frmNews(string news)
        {
            Value = news;
            InitializeComponent();
        }

        private void News_Shown(object sender, EventArgs e)
        {
            tbNews.Text = Value;
        }
    }
}
