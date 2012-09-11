using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;

namespace Auxilium
{
    static class Program
    {
        static bool mutex = false;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Mutex mTex = new Mutex(true, "Auxilium", out mutex);
            if (!mutex) Environment.Exit(0);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
