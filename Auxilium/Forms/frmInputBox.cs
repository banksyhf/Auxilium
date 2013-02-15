using System;
using System.Windows.Forms;

namespace Auxilium.Forms
{
    public partial class frmInputBox : Form
    {
        public string Result { get; set; }
        public frmInputBox()
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
            DialogResult = DialogResult.OK;
            Close();
        }

        private void tbInput_TextChanged(object sender, EventArgs e)
        {

        }

        private void lblText_Click(object sender, EventArgs e)
        {

        }
    }
}
