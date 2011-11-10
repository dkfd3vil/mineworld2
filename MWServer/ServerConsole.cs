using System;

//Contains core console code

namespace MineWorld
{
    public partial class MineWorldServer
    {
        public void ConsoleWrite(string text)
        {
            if (Ssettings.Logs)
            {
                LogWrite(text);
            }
            Console.WriteLine(text);
        }

        public void ConsoleWriteError(string text)
        {
            ConsoleWrite(text, ConsoleColor.Red);
        }

        public void ConsoleWriteSucces(string text)
        {
            ConsoleWrite(text, ConsoleColor.Green);
        }

        public void ConsoleWrite(string text, ConsoleColor color)
        {
            if (Ssettings.Logs)
            {
                LogWrite(text);
            }
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = oldColor;
        }

        public void ConsoleClearScreen()
        {
            //Console.Clear();
            //TODO: Replace this simple by Console.Clear()
            for (int i = 0; i < 80*25; i++)
            {
                Console.Write(" ");
            }
        }

        public void ConsoleProcessInput(string input)
        {
            ConsoleWrite("> " + input);
            ProcessCommand(input);
        }

        public void ProcessCommand(string input)
        {
            // if the input is empty then return silent
            if (input == "")
            {
                return;
            }

            string[] args = input.Split(new[] {' '});

            if (args[0].StartsWith("/") && args[0].Length > 1)
            {
                //Remove the forwardslash
                args[0] = args[0].Substring(1);
            }

            switch (args[0].ToLower())
            {
                case "help":
                    {
                        ConsoleWrite("SERVER CONSOLE COMMANDS:");
                        ConsoleWrite(" announce");
                        ConsoleWrite(" listadmins");
                        ConsoleWrite(" addadmin <ip>");
                        ConsoleWrite(" removeadmin <ip>");
                        ConsoleWrite(" listplayers");
                        ConsoleWrite(" kick <ip>");
                        ConsoleWrite(" kickn <name>");
                        ConsoleWrite(" ban <ip>");
                        ConsoleWrite(" bann <name>");
                        ConsoleWrite(" save <mapfile>");
                        ConsoleWrite(" load <mapfile>");
                        ConsoleWrite(" status");
                        ConsoleWrite(" fps");
                        ConsoleWrite(" cls");
                        ConsoleWrite(" restart");
                        ConsoleWrite(" quit");
                        break;
                    }
                case "announce":
                    {
                        if (args.Length >= 2)
                        {
                            SendServerWideMessage(args[1]);
                        }
                        break;
                    }
                case "listplayers":
                    {
                        ConsoleWrite("( " + PlayerList.Count + " / " + Ssettings.Maxplayers + " )");
                        foreach (ServerPlayer p in PlayerList.Values)
                        {
                            string teamIdent = "";
                            if (GetAdmin(p.Ip))
                                teamIdent += " (Admin)";
                            ConsoleWrite(p.Name + teamIdent);
                            ConsoleWrite(" - " + p.Ip);
                        }
                        break;
                    }
                case "listadmins":
                    {
                        ConsoleWrite("Admin list:");
                        foreach (ServerPlayer p in PlayerList.Values)
                        {
                            if (GetAdmin(p.Ip))
                            {
                                ConsoleWrite(p.Name + " - " + p.Ip);
                            }
                        }
                        break;
                    }
                case "addadmin":
                    {
                        if (args.Length == 2)
                        {
                            AddAdmin(args[1]);
                        }
                        break;
                    }
                case "removeadmin":
                    {
                        if (args.Length == 2)
                        {
                            RemoveAdmin(args[1]);
                        }
                        break;
                    }
                case "status":
                    {
                        GetInfo();
                        break;
                    }
                case "kick":
                    {
                        if (args.Length == 2)
                        {
                            KickPlayer(args[1], false);
                        }
                        break;
                    }
                case "kickn":
                    {
                        if (args.Length == 2)
                        {
                            KickPlayer(args[1], true);
                        }
                        break;
                    }
                case "ban":
                    {
                        if (args.Length == 2)
                        {
                            BanPlayer(args[1], false);
                        }
                        break;
                    }
                case "bann":
                    {
                        if (args.Length == 2)
                        {
                            BanPlayer(args[1], true);
                        }
                        break;
                    }
                case "quit":
                    {
                        ShutdownServer();
                        break;
                    }
                case "restart":
                    {
                        RestartServer();
                        break;
                    }
                case "save":
                    {
                        if (args.Length >= 2)
                        {
                            //SaveLevel(args[1]);
                        }
                        break;
                    }
                case "load":
                    {
                        if (args.Length >= 2)
                        {
                            //LoadLevel(args[1]);
                        }
                        else if (Ssettings.LevelName != "")
                        {
                            //LoadLevel(Ssettings.LevelName);
                        }
                        break;
                    }
                case "fps":
                    {
                        ConsoleWrite("Server FPS:" + _frameCount);
                        break;
                    }
                case "cls":
                    {
                        ConsoleClearScreen();
                        ConsoleWrite("Your screen has been cleared by the roundhouse kick of chuck norris");
                        break;
                    }
                default:
                    {
                        ConsoleWrite("Command not reconized " + args[0]);
                        break;
                    }
            }
        }
    }
}