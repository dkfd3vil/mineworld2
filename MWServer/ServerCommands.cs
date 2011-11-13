using System;
using System.Collections.Generic;
using System.IO;

//Contains commands, mainly console related (but not dealing with the console itself)

namespace MineWorld
{
    public partial class MineWorldServer
    {
        public List<string> LoadAdminList()
        {
            ConsoleWrite("LOADING ADMINLIST");
            List<string> temp = new List<string>();

            try
            {
                if (!File.Exists(Ssettings.SettingsDir + "/admins.txt"))
                {
                    FileStream fs = File.Create(Ssettings.SettingsDir + "/admins.txt");
                    StreamWriter sr = new StreamWriter(fs);
                    sr.WriteLine("#A list of all admins - just add one ip per line");
                    sr.Close();
                    fs.Close();
                }
                else
                {
                    FileStream file = new FileStream(Ssettings.SettingsDir + "/admins.txt", FileMode.Open,
                                                     FileAccess.Read);
                    StreamReader sr = new StreamReader(file);
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        if (line.Trim().Length != 0 && line.Trim().ToCharArray()[0] != '#')
                            temp.Add(line.Trim());
                        line = sr.ReadLine();
                    }
                    sr.Close();
                    file.Close();
                }
            }
            catch
            {
                ConsoleWriteError("Unable to load admin list.");
            }
            ConsoleWriteSucces(temp.Count + " ADMINS LOADED");
            return temp;
        }

        public bool SaveAdminList()
        {
            try
            {
                FileStream file = new FileStream(Ssettings.SettingsDir + "/admins.txt", FileMode.Create,
                                                 FileAccess.Write);
                StreamWriter sw = new StreamWriter(file);
                sw.WriteLine("#A list of all admins - just add one ip per line\n");
                foreach (string ip in BanList)
                    sw.WriteLine(ip);
                sw.Close();
                file.Close();
                return true;
            }
            catch (Exception)
            {
            }
            return false;
        }

        public List<string> LoadBanList()
        {
            ConsoleWrite("LOADING BANLIST");
            List<string> retList = new List<string>();

            try
            {
                if (!File.Exists(Ssettings.SettingsDir + "/banlist.txt"))
                {
                    FileStream fs = File.Create(Ssettings.SettingsDir + "/banlist.txt");
                    StreamWriter sr = new StreamWriter(fs);
                    sr.WriteLine("#A list of all banned people - just add one ip per line");
                    sr.Close();
                    fs.Close();
                }
                else
                {
                    FileStream file = new FileStream(Ssettings.SettingsDir + "/banlist.txt", FileMode.Open,
                                                     FileAccess.Read);
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
                ConsoleWriteError("Unable to load banlist");
            }
            ConsoleWriteSucces(retList.Count + " BANNED IP's LOADED");
            return retList;
        }

        public void SaveBanList()
        {
            try
            {
                FileStream file = new FileStream(Ssettings.SettingsDir + "/banlist.txt", FileMode.Create,
                                                 FileAccess.Write);
                StreamWriter sw = new StreamWriter(file);
                foreach (string ip in BanList)
                    sw.WriteLine(ip);
                sw.Close();
                file.Close();
            }
            catch (Exception)
            {
            }
        }

        public List<string> LoadBannedNames()
        {
            ConsoleWrite("LOADING BANNEDNAMES");
            List<string> retList = new List<string>();

            try
            {
                if (!File.Exists(Ssettings.SettingsDir + "/bannednames.txt"))
                {
                    FileStream fs = File.Create(Ssettings.SettingsDir + "/bannednames.txt");
                    StreamWriter sr = new StreamWriter(fs);
                    sr.WriteLine("#A list of all banned names - just add one name per line");
                    sr.Close();
                    fs.Close();
                }
                else
                {
                    FileStream file = new FileStream(Ssettings.SettingsDir + "/bannednames.txt", FileMode.Open,
                                                     FileAccess.Read);
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
                ConsoleWriteError("Unable to load bannednames");
            }
            ConsoleWriteSucces(retList.Count + " BANNEDNAMES LOADED");
            return retList;
        }

        public void KickPlayer(string ip, bool name)
        {
            List<ServerPlayer> playersToKick = new List<ServerPlayer>();
            foreach (ServerPlayer p in PlayerList.Values)
            {
                if ((p.Ip == ip && !name) || (p.Name.ToLower().Contains(ip.ToLower()) && name))
                    playersToKick.Add(p);
            }
            foreach (ServerPlayer p in playersToKick)
            {
                p.NetConn.Disconnect("kicked", 0);
                p.Kicked = true;
            }
        }

        public void BanPlayer(string ip, bool name)
        {
            if (BanList.Contains(ip)) return;
            BanList.Add(ip);
            KickPlayer(ip, name);
            SaveBanList();
        }

        public bool GetAdmin(string ip)
        {
            return Admins.Contains(ip);
        }

        public void AddAdmin(string ip)
        {
            if (!Admins.Contains(ip))
            {
                Admins.Add(ip);
                SaveAdminList();
            }
        }

        public void RemoveAdmin(string ip)
        {
            if (Admins.Contains(ip))
            {
                Admins.Remove(ip);
                SaveAdminList();
            }
        }

        /*
        public void SaveLevel(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);

            //Save block info
            for (int x = 0; x < Defines.MAPSIZE; x++)
                for (int y = 0; y < Defines.MAPSIZE; y++)
                    for (int z = 0; z < Defines.MAPSIZE; z++)
                        sw.WriteLine((byte)blockList[x, y, z]);
            sw.Close();
            fs.Close();
        }
         */
        /*
        public void BackupLevel()
        {
            SaveLevel(Ssettings.BackupDir + "/backup" + GetTime(false) + ".lvl");
        }
         */

        /*
        public bool LoadLevel(string filename)
        {
            try
            {
                if (!File.Exists(filename))
                {
                    ConsoleWriteError("Unable to load level - " + filename + " does not exist!");
                    return false;
                }
                SendServerWideMessage("Changing map to " + filename + "!");
                DisconnectAllPlayers();

                FileStream fs = new FileStream(filename, FileMode.Open);
                StreamReader sr = new StreamReader(fs);

                //read block info
                for (int x = 0; x < Defines.MAPSIZE; x++)
                    for (int y = 0; y < Defines.MAPSIZE; y++)
                        for (int z = 0; z < Defines.MAPSIZE; z++)
                        {
                            string line = sr.ReadLine();
                            blockList[x, y, z] = (BlockType)int.Parse(line, System.Globalization.CultureInfo.InvariantCulture);
                        }
                sr.Close();
                fs.Close();
                ConsoleWriteSucces("Level loaded successfully - now playing " + filename + "!");
                return true;
            }
            catch { }
            return false;
        }
         */

        public void DisconnectAllPlayers()
        {
            foreach (ServerPlayer p in PlayerList.Values)
            {
                p.NetConn.Disconnect("disconnected", 0);
            }
        }

        public void ShutdownServer()
        {
            ConsoleWrite("Server is shutting down in 5 seconds.", ConsoleColor.Yellow);
            SendServerWideMessage("Server is shutting down in 5 seconds.");
            _shutdownTriggerd = true;
            _shutdownTime = DateTime.Now + TimeSpan.FromSeconds(5);
        }

        public void RestartServer()
        {
            ConsoleWrite("Server restarting in 5 seconds.", ConsoleColor.Yellow);
            SendServerWideMessage("Server restarting in 5 seconds.");
            _restartTriggered = true;
            _restartTime = DateTime.Now + TimeSpan.FromSeconds(5);
        }

        public void GetInfo()
        {
            ConsoleWrite("General information");
            ConsoleWrite("ServerName: " + Ssettings.Servername);
            ConsoleWrite(PlayerList.Count + " / " + Ssettings.Maxplayers + " players");
            ConsoleWrite("Public: " + Ssettings.Public.ToString());
            ConsoleWrite("Proxy: " + Ssettings.Proxy.ToString());
            ConsoleWrite("Logging: " + Ssettings.Logs.ToString());
            ConsoleWrite("MOTD: " + Ssettings.MOTD);
            ConsoleWrite("");
            ConsoleWrite("Extra information");
        }
    }
}