using System;
using Auxilium.Forms;
using System.Windows.Forms;

namespace Auxilium.Classes
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //bool mutex;
            //Mutex m = new Mutex(true, "Auxilium", out mutex);

            //if (!mutex)
            //{
            //    //TODO: Activate main instance of our program.
            //    return;
            //}

            //GC.KeepAlive(m);
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new frmMain());
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
    }
}
