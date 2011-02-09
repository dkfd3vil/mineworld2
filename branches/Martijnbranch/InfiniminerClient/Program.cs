using System;
using System.IO;
using System.Windows.Forms;

namespace MineWorld
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            using (MineWorldGame game = new MineWorldGame(args))
            {
                try
                {
                    game.Run();
                }
                catch (Exception e)
                {
                    String logtext = "";
                    if (File.Exists("crashlog.log"))
                    {
                        logtext = File.ReadAllText("crashlog.log");
                    }
                    File.WriteAllText("crashlog.log", logtext + "\r\n" + e.Message + "\r\n\r\n" + e.StackTrace);
                    MessageBox.Show("The game has crashed. The crash info has been written to the crashlog.", "Crash and Burn", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }
        }
    }
}

