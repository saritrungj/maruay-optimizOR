using System;
using System.Security.Principal;
using System.Windows.Forms;
using optimizOR.UI;

namespace optimizOR
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!IsAdministrator())
            {
                MessageBox.Show(
                    "This application must be run as Administrator.",
                    "Access Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            Application.Run(new Form1());
        }

        public static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}

