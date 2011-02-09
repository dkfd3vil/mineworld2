using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

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
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message + "\r\n\r\n" + e.StackTrace);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
        }
    }
}
