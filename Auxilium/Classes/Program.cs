using System;
using System.Threading;
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
            bool mutex;
            Mutex m = new Mutex(true, "Auxilium" + Application.ProductVersion, out mutex);

            if (!mutex)
            {
                Environment.Exit(0);
            }

            GC.KeepAlive(m);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
