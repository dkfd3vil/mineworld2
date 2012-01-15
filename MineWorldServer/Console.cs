using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MineWorldServer
{
    public class MineWorldConsole
    {
        MineWorldServer mineserver;

        public MineWorldConsole(MineWorldServer mines)
        {
            mineserver = mines;
        }

        public void ProcessInput(string input)
        {
            string[] inputsplitted = input.Split(new[] { ' ' });

            switch (inputsplitted[0].ToLower())
            {
                case "kick":
                    {
                        if (inputsplitted.Length == 2)
                        {
                            mineserver.PlayerManager.KickPlayerByName(inputsplitted[1]);
                        }
                        else
                        {
                            ConsoleWrite("Specify player to be kicked");
                        }
                        break;
                    }
                case "exit":
                    {
                        mineserver.Server.Shutdown("shutdown");
                        mineserver.KeepServerRunning = false;
                        mineserver.RestartServer = false;
                        break;
                    }
                case "restart":
                    {
                        mineserver.Server.Shutdown("restart");
                        mineserver.KeepServerRunning = false;
                        mineserver.RestartServer = true;
                        break;
                    }
                default:
                    {
                        WriteError("Cant process input");
                        break;
                    }
            }
        }

        public void WriteError(string error)
        {
            ConsoleWrite(error, ConsoleColor.Red);
        }

        public void ConsoleWrite(string text)
        {
            Console.WriteLine(text);
        }

        public void ConsoleWrite(string text, ConsoleColor color)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = oldColor;
        }

        public void SetTitle(string title)
        {
            Console.Title = title;
        }

        public string ReadLine()
        {
            return Console.ReadLine();
        }

        public bool KeyAvailable()
        {
            return Console.KeyAvailable;
        }
    }
}
