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
            consoleText.Add(text);
            if (consoleText.Count > CONSOLE_SIZE)
                consoleText.RemoveAt(0);
            ConsoleRedraw();
        }

        public void ConsoleProcessInput()
        {
            ConsoleWrite("> " + consoleInput);

            ProcessCommand(consoleInput);

            consoleInput = "";
            ConsoleRedraw();
        }

        public bool ProcessCommand(string input)
        {
            string[] args = input.Split(' '.ToString().ToCharArray(), 2);
            if (args[0].StartsWith("\\") && args[0].Length > 1)
                args[0] = args[0].Substring(1);
            switch (args[0].ToLower())
            {
                case "help":
                    {
                        ConsoleWrite("SERVER CONSOLE COMMANDS:");
                        ConsoleWrite(" announce");
                        ConsoleWrite(" admins");
                        ConsoleWrite(" admin <name>");
                        ConsoleWrite(" players");
                        ConsoleWrite(" kick <ip>");
                        ConsoleWrite(" kickn <name>");
                        ConsoleWrite(" ban <ip>");
                        ConsoleWrite(" bann <name>");
                        ConsoleWrite(" say <message>");
                        ConsoleWrite(" save <mapfile>");
                        ConsoleWrite(" load <mapfile>");
                        ConsoleWrite(" status");
                        ConsoleWrite(" restart");
                        ConsoleWrite(" quit");
                        break;
                    }
                case "announce":
                    {
                        if (args.Length == 2)
                        {
                            string message = "SERVER: " + args[1];
                            SendServerMessage(message);
                        }
                        break;
                    }
                case "players":
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
                            ConsoleWrite("  - " + p.IP);
                        }
                        break;
                    }
                case "admins":
                    {
                        ConsoleWrite("Admin list:");
                        //foreach (string ip in admins.ToString())
                            //ConsoleWrite(ip);
                        break;
                    }

                case "admin":
                    {
                        if (args.Length == 2)
                        {
                            AddAdmin(args[1]);
                        }
                        break;
                    }
                    /*
                case "adminn":
                    {
                        if (args.Length == 2)
                        {
                            AddAdmin(args[1]);
                        }
                    }
                    break;
                     */

                case "status":
                    {
                        status();
                        break;
                    }
                case "kick":
                    {
                        if (args.Length == 2)
                        {
                            KickPlayer(args[1]);
                        }
                        break;
                    }
                case "kickn":
                    {
                        if (args.Length == 2)
                        {
                            KickPlayer(args[1], true);
                        }
                    }
                    break;

                case "ban":
                    {
                        if (args.Length == 2)
                        {
                            BanPlayer(args[1]);
                        }
                    }
                    break;

                case "bann":
                    {
                        if (args.Length == 2)
                        {
                            BanPlayer(args[1]);
                        }
                        break;
                    }
                case "quit":
                    {
                        keepRunning = false;
                        break;
                    }
                case "restart":
                    {
                        disconnectAll();
                        restartTriggered = true;
                        restartTime = DateTime.Now;
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
            ConsoleDrawCentered("MineWorld SERVER " + Defines.MINEWORLDSERVER_VERSION, 0);
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
