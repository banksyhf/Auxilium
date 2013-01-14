using System;
using System.Windows.Forms;

namespace Auxilium.Forms
{
    public partial class News : Form
    {
        private string Value;
        public News(string news)
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
