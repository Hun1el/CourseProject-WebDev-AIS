using System;
using System.Windows.Forms;

namespace WebSiteDev
{
    public class Inactivity
    {
        public static void OnFormLoad(Form form)
        {
            BlockForms blockForms = Program.GetBlockForms();

            if (blockForms != null)
            {
                blockForms.RegisterForm(form);
                blockForms.Start();
            }
        }

        public static void OnFormClosing(Form form)
        {
            BlockForms blockForms = Program.GetBlockForms();

            if (blockForms != null)
            {
                blockForms.UnregisterForm(form);
            }
        }
    }
}
