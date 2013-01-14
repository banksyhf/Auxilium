using System;
using System.Windows.Forms;

namespace Auxilium.Forms
{
    public partial class InputBox : Form
    {
        public string Result { get; set; }
        public InputBox()
        {
            InitializeComponent();
            tbInput.Select();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbInput.Text.Trim()))
            {
                MessageBox.Show("Please input something!");
                return;
            }
            Result = tbInput.Text;
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}
