using System;
using System.Windows.Forms;

namespace Woodpecker
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Application.Run(new mainForm());
            }
            catch
            {
                // Catches ALL uncaught exceptions
            }
        }
    }
}
