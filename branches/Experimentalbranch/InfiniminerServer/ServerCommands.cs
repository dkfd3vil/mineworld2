﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;

//Contains commands, mainly console related (but not dealing with the console itself)

namespace MineWorld
{
    public partial class MineWorldServer
    {
        /*
        public void MessageAll(string text)
        {
            if (announceChanges)
                SendServerMessage(text);
            ConsoleWrite(text);
        }
         */


        public List<string> LoadAdminList()
        {
            List<string> temp = new List<string>();

            try
            {
                if (!File.Exists(Ssettings.Directory + "/admins.txt"))
                {
                    FileStream fs = File.Create(Ssettings.Directory + "/admins.txt");
                    StreamWriter sr = new StreamWriter(fs);
                    sr.WriteLine("#A list of all admins - just add one ip per line");
                    sr.Close();
                    fs.Close();
                }
                else
                {
                    FileStream file = new FileStream(Ssettings.Directory + "/admins.txt", FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(file);
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        if (line.Trim().Length != 0 && line.Trim().ToCharArray()[0] != '#')
                            temp.Add(line.Trim()); //This will be changed to note authority too
                        line = sr.ReadLine();
                    }
                    sr.Close();
                    file.Close();
                }
            }
            catch
            {
                ConsoleWrite("Unable to load admin list.");
            }

            return temp;
        }

        public bool SaveAdminList()
        {
            try
            {
                FileStream file = new FileStream(Ssettings.Directory + "/admins.txt", FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(file);
                sw.WriteLine("#A list of all admins - just add one ip per line\n");
                foreach (string ip in banList)
                    sw.WriteLine(ip);
                sw.Close();
                file.Close();
                return true;
            }
            catch { }
            return false;
        }

        public List<string> LoadBanList()
        {
            List<string> retList = new List<string>();

            try
            {
                if (!File.Exists(Ssettings.Directory + "/banlist.txt"))
                {
                    FileStream fs = File.Create(Ssettings.Directory + "/banlist.txt");
                    StreamWriter sr = new StreamWriter(fs);
                    sr.WriteLine("#A list of all banned people - just add one ip per line");
                    sr.Close();
                    fs.Close();
                }
                else
                {
                    FileStream file = new FileStream(Ssettings.Directory + "/banlist.txt", FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(file);
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        if (line.Trim().Length != 0 && line.Trim().ToCharArray()[0] != '#')
                            retList.Add(line.Trim());
                        line = sr.ReadLine();
                    }
                    sr.Close();
                    file.Close();

                }
            }
            catch
            {
                ConsoleWrite("Unable to load banlist");
            }

            return retList;
        }

        public void SaveBanList()
        {
            try
            {
                FileStream file = new FileStream(Ssettings.Directory + "/banlist.txt", FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(file);
                foreach (string ip in banList)
                    sw.WriteLine(ip);
                sw.Close();
                file.Close();
            }
            catch { }
        }

        public List<string> LoadBannedNames()
        {
            List<string> retList = new List<string>();

            try
            {
                if (!File.Exists(Ssettings.Directory + "/bannednames.txt"))
                {
                    FileStream fs = File.Create(Ssettings.Directory + "/bannednames.txt");
                    StreamWriter sr = new StreamWriter(fs);
                    sr.WriteLine("#A list of all banned names - just add one name per line");
                    sr.Close();
                    fs.Close();
                }
                else
                {
                    FileStream file = new FileStream(Ssettings.Directory + "/bannednames.txt", FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(file);
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        if (line.Trim().Length != 0 && line.Trim().ToCharArray()[0] != '#')
                            retList.Add(line.Trim());
                        line = sr.ReadLine();
                    }
                    sr.Close();
                    file.Close();

                }
            }
            catch
            {
                ConsoleWrite("Unable to load bannednames");
            }

            return retList;
        }

        public void KickPlayer(string ip)
        {
            KickPlayer(ip, false);
        }

        public void KickPlayer(string ip, bool name)
        {
            List<Player> playersToKick = new List<Player>();
            foreach (IClient p in playerList.Values)
            {
                if ((p.IP == ip && !name) || (p.Handle.ToLower().Contains(ip.ToLower()) && name))
                    playersToKick.Add(p);
            }
            foreach (IClient p in playersToKick)
            {
                p.NetConn.Disconnect("", 0);
                p.Kicked = true;
            }
        }

        public void BanPlayer(string ip)
        {
            if (!banList.Contains(ip))
            {
                banList.Add(ip);
                KickPlayer(ip);
                SaveBanList();
            }
        }

        public bool GetAdmin(string ip)
        {
            if (admins.Contains(ip))
            {
                return true;
            }
            return false;
        }

        public void AddAdmin(string ip)
        {
            if(!admins.Contains(ip))
            {
                admins.Add(ip);
                SaveAdminList();
            }
        }

        public void SaveLevel(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            //Create level header with team info
            sw.WriteLine(teamOreBlue+","+teamOreRed+","+teamCashBlue+","+teamCashRed);

            //Save block info
            for (int x = 0; x < MAPSIZE; x++)
                for (int y = 0; y < MAPSIZE; y++)
                    for (int z = 0; z < MAPSIZE; z++)
                        sw.WriteLine((byte)blockList[x, y, z] + "," + (byte)blockCreatorTeam[x, y, z]);
            sw.Close();
            fs.Close();
        }
        public void BackupLevel()
        {
            SaveLevel("autoBK.lvl");
        }

        public bool LoadLevel(string filename)
        {
            try
            {
                if (!File.Exists(filename))
                {
                    ConsoleWrite("Unable to load level - " + filename + " does not exist!");
                    return false;
                }
                SendServerMessage("Changing map to " + filename + "!");
                disconnectAll();

                FileStream fs = new FileStream(filename, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                //read level header
                String header = sr.ReadLine().Trim();
                String[] headerPart = header.Split(new char[] { ',' });
                teamOreBlue = (uint)Int32.Parse(headerPart[0]);
                teamOreRed = (uint)Int32.Parse(headerPart[1]);
                teamCashBlue = (uint)Int32.Parse(headerPart[2]);
                teamCashRed = (uint)Int32.Parse(headerPart[3]);

                //read block info
                for (int x = 0; x < MAPSIZE; x++)
                    for (int y = 0; y < MAPSIZE; y++)
                        for (int z = 0; z < MAPSIZE; z++)
                        {
                            string line = sr.ReadLine();
                            string[] fileArgs = line.Split(",".ToCharArray());
                            if (fileArgs.Length == 2)
                            {
                                blockList[x, y, z] = (BlockType)int.Parse(fileArgs[0], System.Globalization.CultureInfo.InvariantCulture);
                                blockCreatorTeam[x, y, z] = (PlayerTeam)int.Parse(fileArgs[1], System.Globalization.CultureInfo.InvariantCulture);
                            }
                        }
                sr.Close();
                fs.Close();
                ConsoleWrite("Level loaded successfully - now playing " + filename + "!");
                return true;
            }
            catch { }
            return false;
        }

        public void ResetLevel()
        {
            disconnectAll();
            newMap();
        }

        public void disconnectAll()
        {
            foreach (IClient p in playerList.Values)
            {
                p.NetConn.Disconnect("", 0);
            }
        }
        public void status()
        {
            ConsoleWrite(Ssettings.Servername);//serverName);
            ConsoleWrite(playerList.Count + " / " + Ssettings.Maxplayers + " players");
        }
    }
}