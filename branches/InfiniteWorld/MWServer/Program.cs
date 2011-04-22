using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Text;

namespace MineWorld.Server
{
    class Program
    {
        static void RunServer()
        {
            bool restartServer = true;
            while (restartServer)
            {
                MineWorldServer MineWorldServer = new MineWorldServer();
                restartServer = MineWorldServer.Start();
                GC.Collect();
            }
            
        }

        static void Main(string[] args)
        {

            if (System.Diagnostics.Debugger.IsAttached)
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
                    String logtext = "";
                    if (File.Exists("Servercrashlog.log"))
                    {
                        logtext = File.ReadAllText("Servercrashlog.log");
                    }
                    File.WriteAllText("Servercrashlog.log", logtext + "\r\n" + e.Message + "\r\n\r\n" + e.StackTrace);
                    MessageBox.Show("The game has crashed. The crash info has been written to the crashlog.", "Crash and Burn", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }
            Environment.Exit(0);
        }
    }
}
