using System;
using System.Windows.Forms;

namespace Eplicta.Conserver.Cli
{
    internal static class Program
    {
        [STAThread]   // ← EXTREMT VIKTIG
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AppForm());
        }
    }
}
