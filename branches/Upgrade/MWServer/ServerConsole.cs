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
        public void ConsoleWriteEmptyLine()
        {
            Console.Write("\n\r");
        }

        public void ConsoleWrite(string text)
        {
            if (Ssettings.Logs == true)
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
            if (Ssettings.Logs == true)
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
            for (int i = 0; i < 80 * 25; i++)
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

            string[] args = input.Split(new char[] { ' ' });

            switch (args[0].ToLower())
            {
                case "help":
                    {
                        ConsoleWrite("SERVER CONSOLE COMMANDS:");
                        ConsoleWrite(" announce");
                        ConsoleWrite(" status");
                        ConsoleWrite(" fps");
                        ConsoleWrite(" cls");
                        ConsoleWrite(" stop");
                        ConsoleWrite(" restart <seconds optional>");
                        ConsoleWrite(" shutdown <seconds optional>  ");
                        ConsoleWriteEmptyLine();
                        ConsoleWrite(" listplayers");
                        ConsoleWrite(" listadmins");
                        ConsoleWrite(" listbannedips");
                        ConsoleWrite(" listbannednames");
                        ConsoleWriteEmptyLine();
                        ConsoleWrite(" addadmin <ip>");
                        ConsoleWrite(" removeadmin <ip>");
                        ConsoleWriteEmptyLine();
                        ConsoleWrite(" kickip <ip>");
                        ConsoleWrite(" kickname <name>");
                        ConsoleWriteEmptyLine();
                        ConsoleWrite(" banip <ip>");
                        ConsoleWrite(" banname <name>");
                        ConsoleWriteEmptyLine();
                        ConsoleWrite(" addbannedname <name>");
                        ConsoleWrite(" removebannedname <name>");
                        ConsoleWriteEmptyLine();
                        ConsoleWrite(" save <mapfile>");
                        ConsoleWrite(" load <mapfile>");
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
                        ConsoleWrite("( " + playerList.Count + " / " + Ssettings.Maxplayers + " )");
                        foreach (ServerPlayer p in playerList.Values)
                        {
                            ConsoleWrite(p.Name + " - " + p.IP);
                        }
                        break;
                    }
                case "listadmins":
                    {
                        ConsoleWrite("Admin list:");
                        foreach (ServerPlayer p in playerList.Values)
                        {
                            if(GetAdmin(p.IP))
                            {
                                ConsoleWrite(p.Name + " - " + p.IP);
                            }
                        }
                        break;
                    }
                case "listbannedips":
                    {
                        ConsoleWrite("Banned ip list:");
                        foreach (string ip in bannedips)
                        {
                            ConsoleWrite(ip);
                        }
                        break;
                    }
                case "listbannednames":
                    {
                        ConsoleWrite("Banned names list:");
                        foreach (string name in bannednames)
                        {
                            ConsoleWrite(name);
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
                case "kickip":
                    {
                        if (args.Length == 2)
                        {
                            KickPlayer(args[1],false);
                        }
                        break;
                    }
                case "kickname":
                    {
                        if (args.Length == 2)
                        {
                            KickPlayer(args[1], true);
                        }
                        break;
                    }
                case "banip":
                    {
                        if (args.Length == 2)
                        {
                            BanPlayer(args[1],false);
                        }
                        break;
                    }
                case "banname":
                    {
                        if (args.Length == 2)
                        {
                            BanPlayer(args[1],true);
                        }
                        break;
                    }
                case "addbannedname":
                    {
                        if (args.Length == 2)
                        {
                            AddBannedName(args[1]);
                        }
                        break;
                    }
                case "removebannedname":
                    {
                        if (args.Length == 2)
                        {
                            RemoveBannedName(args[1]);
                        }
                        break;
                    }
                case "shutdown":
                    {
                        if (args.Length == 2)
                        {
                            Shutdownserver(int.Parse(args[1]));
                        }
                        else
                        {
                            Shutdownserver(0);
                        }
                        break;
                    }
                case "restart":
                    {
                        if (args.Length == 2)
                        {
                            Restartserver(int.Parse(args[1]));
                        }
                        else
                        {
                            Restartserver(0);
                        }
                        break;
                    }
                case "stop":
                    {
                        if (restartTriggered || shutdownTriggerd)
                        {
                            ConsoleWrite("Shutdown or restart stopped",ConsoleColor.Yellow);
                            restartTriggered = false;
                            shutdownTriggerd = false;
                        }
                        else
                        {
                            ConsoleWrite("Shutdown or restart wasnt started");
                        }
                        break;
                    }
                case "save":
                    {
                        if (args.Length == 2)
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
                        //ConsoleWrite(fpsCounter.GetFps());
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