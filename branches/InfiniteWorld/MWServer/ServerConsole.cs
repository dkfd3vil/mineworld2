using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;


//Contains core console code

namespace MineWorld
{
    public partial class MineWorldServer
    {

        const int CONSOLE_SIZE = 30;
        List<string> consoleText = new List<string>();
        string consoleInput = "";


        public void ConsoleWrite(string text)
        {
            if (Ssettings.Logs == true)
            {
                LogWrite(text);
            }
            consoleText.Add(text);
            if (consoleText.Count > CONSOLE_SIZE)
                consoleText.RemoveAt(0);
            ConsoleRedraw();
        }

        public void ConsoleClearScreen()
        {
            while (consoleText.Count > 0)
            {
                consoleText.RemoveAt(0);
            }
            ConsoleRedraw();
        }

        public void ConsoleProcessInput()
        {
            ConsoleWrite("> " + consoleInput);

            ProcessCommand(consoleInput,false);

            consoleInput = "";
            ConsoleRedraw();
        }

        public bool ProcessCommand(string input,bool clientcommand)
        {
            //string[] args = input.Split(' '.ToString().ToCharArray(), 2);
            String[] args = input.Split(new char[] { ' ' });
            if (clientcommand == true)
            {
                args[0] = args[0].Substring(1);
            }
            else if (args[0].StartsWith("\\") && args[0].Length > 1)
            {
                args[0] = args[0].Substring(1);
            }
            else if(args[0].StartsWith("/") && args[0].Length > 1)
            {
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
                            SendServerMessage(args[1]);
                        }
                        break;
                    }
                case "listplayers":
                    {
                        ConsoleWrite("( " + playerList.Count + " / " + Ssettings.Maxplayers + " )");
                        foreach (IClient p in playerList.Values)
                        {
                            string teamIdent = "";
                            if (p.Team == PlayerTeam.Red)
                                teamIdent = " (R)";
                            else if (p.Team == PlayerTeam.Blue)
                                teamIdent = " (B)";
                            if (GetAdmin(p.IP))
                                teamIdent += " (Admin)";
                            ConsoleWrite(p.Handle + teamIdent);
                            ConsoleWrite(" - " + p.IP);
                        }
                        break;
                    }
                case "listadmins":
                    {
                        ConsoleWrite("Admin list:");
                        foreach (IClient p in playerList.Values)
                        {
                            if(GetAdmin(p.IP))
                            {
                                ConsoleWrite(p.Handle + " - " + p.IP);
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
                        status();
                        break;
                    }
                case "kick":
                    {
                        if (args.Length == 2)
                        {
                            KickPlayer(args[1],false);
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
                            BanPlayer(args[1],false);
                        }
                        break;
                    }
                case "bann":
                    {
                        if (args.Length == 2)
                        {
                            BanPlayer(args[1],true);
                        }
                        break;
                    }
                case "quit":
                    {
                        Shutdownserver();
                        break;
                    }
                case "restart":
                    {
                        Restartserver();
                        break;
                    }
                case "save":
                    {
                        if (args.Length >= 2)
                        {
                            SaveLevel(args[1]);
                        }
                        break;
                    }
                case "load":
                    {
                        if (args.Length >= 2)
                        {
                            LoadLevel(args[1]);
                        }
                        else if (Ssettings.LevelName != "")
                        {
                            LoadLevel(Ssettings.LevelName);
                        }
                        break;
                    }
                case "fps":
                    {
                        ConsoleWrite("Server FPS:" + frameCount);
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
            return true;
        }
        public void ConsoleRedraw()
        {
            Console.Clear();
            ConsoleDrawCentered(Defines.MINEWORLDSERVER_VERSION + " SERVER", 0);
            ConsoleDraw("================================================================================", 0, 1);
            for (int i = 0; i < consoleText.Count; i++)
                ConsoleDraw(consoleText[i], 0, i + 2);
            ConsoleDraw("================================================================================", 0, CONSOLE_SIZE + 2);
            ConsoleDraw("> " + consoleInput, 0, CONSOLE_SIZE + 3);
        }

        public void ConsoleDraw(string text, int x, int y)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(text);
        }

        public void ConsoleDrawCentered(string text, int y)
        {
            Console.SetCursorPosition(40 - text.Length / 2, y);
            Console.Write(text);
        }
    }
 }
