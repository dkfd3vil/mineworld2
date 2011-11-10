using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace MineWorld
{
    internal class Program
    {
        private static void RunServer()
        {
            bool restartServer = true;
            while (restartServer)
            {
                MineWorldServer mineWorldServer = new MineWorldServer();
                restartServer = mineWorldServer.Start();
                GC.Collect();
            }
        }

        private static void Main()
        {
            if (Debugger.IsAttached)
            {
                RunServer();
            }
            else
            {
                try
                {
                    RunServer();
                }
                catch (Exception e)
                {
                    string logtext = "";
                    if (File.Exists("Servercrashlog.log"))
                    {
                        logtext = File.ReadAllText("Servercrashlog.log");
                    }
                    File.WriteAllText("Servercrashlog.log", logtext + "\r\n" + e.Message + "\r\n\r\n" + e.StackTrace);
                    MessageBox.Show("The game has crashed. The crash info has been written to the crashlog.",
                                    "Crash and Burn", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                    MessageBoxDefaultButton.Button1);
                }
            }
            Environment.Exit(0);
        }
    }
}