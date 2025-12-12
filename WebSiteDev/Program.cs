using System;
using System.Windows.Forms;

namespace WebSiteDev
{
    internal static class Program
    {
        private static BlockForms blockForms;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Properties.Settings.Default.Reload();

            AuthForm authForm = new AuthForm();

            blockForms = new BlockForms(authForm);
            blockForms.OnInactivityDetected += BlockForms_OnInactivityDetected;

            Application.Run(authForm);
        }

        private static void BlockForms_OnInactivityDetected(object sender, EventArgs e)
        {
            blockForms.LockAllForms();
        }

        public static BlockForms GetBlockForms()
        {
            return blockForms;
        }
    }
}
