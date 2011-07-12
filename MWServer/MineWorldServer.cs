using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MineWorld
{
    public partial class MineWorldServer
    {
        Random randomizer = new Random();

        //NetServer netServer = null;
        NetServer netServer = null;
        public DayManager dayManager = null;
        public Dictionary<NetConnection, ServerPlayer> playerList = new Dictionary<NetConnection, ServerPlayer>();
        public List<NetConnection> toGreet = new List<NetConnection>();
        public List<string> admins = new List<string>(); //List of strings with all the admins
        public List<string> bannednames = new List<string>(); // List of strings with all names that cannot be chosen
        public List<string> banList = new List<string>(); // List of string with all thhe banned ip's

        DateTime lasthearthbeatsend = DateTime.Now;
        DateTime lastServerListUpdate = DateTime.Now;
        DateTime lastMapBackup = DateTime.Now;
        DateTime lastKeyAvaible = DateTime.Now;

        public string serverIP;
        public string sessionid;
        int frameCount = 100;

        public bool StopFluids;

        bool keepRunning = true;

        // Server restarting variables.
        DateTime restartTime = DateTime.Now;
        bool restartTriggered = false;

        // Server shutdown variables.
        DateTime shutdownTime = DateTime.Now;
        bool shutdownTriggerd = false;

        public ServerSettings Ssettings = new ServerSettings();
        public ServerExtraSettings SAsettings = new ServerExtraSettings();
        public MapSettings Msettings = new MapSettings();

        //TODO Find a proper place
        // Fluids count
        int Totallavablockcount;
        int Totalwaterblockcount;

        public MineWorldServer()
        {
        }

        public string GetExternalIp()
        {
            string ip = HttpRequest.Get(Defines.MASTERSERVER_BASE_URL + "ip.php", null);
            return ip;
        }

        public bool Start()
        {
            //Display server version in console
            ConsoleWrite(Defines.MINEWORLDSERVER_VERSION, ConsoleColor.Cyan);

            // Load the directory's.
            LoadDirectorys();

            // Load the general-settings.
            LoadSettings();

            // Load extra settings.
            LoadExtraSettings();

            // Load the map-settings.
            LoadMapSettings();

            // Load the ban-list.
            banList = LoadBanList();

            // Load the admin-list.
            admins = LoadAdminList();

            // Load the bannednames-list.
            bannednames = LoadBannedNames();

            // Initialize the server.
            NetPeerConfiguration config = new NetPeerConfiguration("MineWorld");
            //Todo put this in the config files
            config.Port = Defines.MINEWORLD_PORT;
            config.MaximumConnections = Ssettings.Maxplayers;
            //Todo check wich messages need to be enabled
            //config.EnableMessageType();
            netServer = new NetServer(config);

            // Initialize the daymanager.
            dayManager = new DayManager(Ssettings);

            // Store the last time that we did a physics calculation.
            DateTime lastCalc = DateTime.Now;

            //Display external IP
            //Dont bother if it isnt public
            if (Ssettings.Public == true)
            {
                serverIP = GetExternalIp();
                ConsoleWrite("Your external IP Adress: " + serverIP);
            }
            bool loaded = false;
            //Check if we should autoload a level
            if (Ssettings.LevelName != "")
            {
                ConsoleWrite("AUTOLOAD MAP");
                blockList = new BlockType[Defines.MAPSIZE, Defines.MAPSIZE, Defines.MAPSIZE];
                loaded = LoadLevel(Ssettings.LevelName);
                if (loaded == false)
                {
                    ConsoleWriteError("AUTOLOAD FAILED");
                }
                else
                {
                    ConsoleWriteSucces("AUTLOAD SUCCESFULL");
                }
            }
            if(loaded == false)
            {
                ConsoleWrite("GENERATING NEW MAP");
                GenerateNewMap();
                ConsoleWriteSucces("NEW MAP GENERATED");
                ConsoleWrite("MAPSIZE = [" + Defines.MAPSIZE + "] [" + Defines.MAPSIZE + "] [" + Defines.MAPSIZE + "]");
            }

            int tempblocks = Defines.MAPSIZE * Defines.MAPSIZE * Defines.MAPSIZE;
            ConsoleWrite("TOTAL BLOCKS = " + tempblocks.ToString("n0"));
            ConsoleWrite("TOTAL LAVA BLOCKS = " + Totallavablockcount);
            ConsoleWrite("TOTAL WATER BLOCKS = " + Totalwaterblockcount);

            lastMapBackup = DateTime.Now;
            ServerListener listener = new ServerListener(netServer,this);
            Thread listenerthread = new Thread(new ThreadStart(listener.start));
            Thread physicsthread = new Thread(new ThreadStart(DoPhysics));

            DateTime lastFPScheck = DateTime.Now;
            double frameRate = 0;

            //Start netserver
            netServer.Start();

            //Start listernerthread
            listenerthread.Start();

            //Start physicsthread
            physicsthread.Start();

            //If public, announce server to public tracker
            if (Ssettings.Public)
            {
                addtoMasterServer();
            }

            // Main server loop!
            ConsoleWriteSucces("SERVER READY");

            while (keepRunning)
            {
                // Check the state of our core threads
                if (!listenerthread.IsAlive)
                {
                    ConsoleWriteError("Listenerthread died");
                    ConsoleWriteError("Server is shutting down");
                    return false;
                }
                if (!physicsthread.IsAlive)
                {
                    ConsoleWriteError("Physicsthread died");
                    ConsoleWriteError("Server is shutting down");
                    return false;
                }

                // Fps for the server
                frameCount = frameCount + 1;
                if (lastFPScheck <= DateTime.Now - TimeSpan.FromMilliseconds(1000))
                {
                    lastFPScheck = DateTime.Now;
                    frameRate = frameCount;
                    if (frameCount <= 20)
                    {
                        ConsoleWrite("Heavy load: " + frameCount + " FPS", ConsoleColor.Yellow);
                    }
                    frameCount = 0;
                }

                // Dont send hearthbeats too fast
                TimeSpan updatehearthbeat = DateTime.Now - lasthearthbeatsend;
                if (updatehearthbeat.TotalMilliseconds > 1000)
                {
                    Sendhearthbeat();
                    lasthearthbeatsend = DateTime.Now;
                }

                //Check the time
                dayManager.Update();

                // Look if the time is changed so that wel tell the clients
                if (dayManager.Timechanged())
                {
                    Senddaytimeupdate(dayManager.Light);
                }

                //Time to backup map?
                // If Ssettings.autosavetimer is 0 then autosave is disabled
                if (Ssettings.Autosavetimer != 0)
                {
                    TimeSpan mapUpdateTimeSpan = DateTime.Now - lastMapBackup;
                    if (mapUpdateTimeSpan.TotalMinutes > Ssettings.Autosavetimer)
                    {
                        ConsoleWrite("BACK-UP STARTED");
                        Thread backupthread = new Thread(new ThreadStart(BackupLevel));
                        backupthread.Start();
                        lastMapBackup = DateTime.Now;
                        ConsoleWriteSucces("BACK-UP DONE");
                    }
                }

                //Time to terminate finished map sending threads?
                TerminateFinishedThreads();

                // Handle console keypresses.
                while (Console.KeyAvailable)
                {
                    // What if there is constant keyavaible ?
                    // This code makes sure the rest of the program can also run
                    TimeSpan timeSpanLastKeyAvaible = DateTime.Now - lastKeyAvaible;
                    if (timeSpanLastKeyAvaible.Milliseconds < 2000)
                    {
                        string input = Console.ReadLine();
                        ConsoleProcessInput(input);
                        lastKeyAvaible = DateTime.Now;
                        break;
                    }
                }

                // Restart the server?
                if (restartTriggered && DateTime.Now > restartTime)
                {
                    disconnectAll();
                    netServer.Shutdown("serverrestart");
                    BackupLevel();
                    removefromMasterServer();
                    return true;
                }

                if (shutdownTriggerd && DateTime.Now > shutdownTime)
                {
                    disconnectAll();
                    netServer.Shutdown("servershutdown");
                    BackupLevel();
                    removefromMasterServer();
                    return false;
                }

                // Pass control over to waiting threads.
                Thread.Sleep(1);
            }
            return false;
        }

        public void LoadSettings()
        {
            Datafile dataFile = new Datafile(Ssettings.SettingsDir + "/server.config.txt");

            if (dataFile.Data.ContainsKey("logs"))
                Ssettings.Logs = bool.Parse(dataFile.Data["logs"]);
            else
            {
                Ssettings.Logs = false;
                ConsoleWriteError("Couldnt find logs settings so we use the default (false)");
            }
            ConsoleWrite("LOADING SETTINGS");

            // Read in from the config file.
            if (dataFile.Data.ContainsKey("maxplayers"))
                Ssettings.Maxplayers = int.Parse(dataFile.Data["maxplayers"]);
            else
            {
                Ssettings.Maxplayers = 16;
                ConsoleWriteError("Couldnt find maxplayers setting so we use the default (16)");
            }

            if (dataFile.Data.ContainsKey("public"))
                Ssettings.Public = bool.Parse(dataFile.Data["public"]);
            else
            {
                Ssettings.Public = false;
                ConsoleWriteError("Couldnt find public setting so we use the default (FALSE)");
            }
            if (dataFile.Data.ContainsKey("proxy"))
                Ssettings.Proxy = bool.Parse(dataFile.Data["proxy"]);
            else
            {
                Ssettings.Proxy = false;
                ConsoleWriteError("Couldnt find proxy setting so we use the default (false)");
            }

            if (dataFile.Data.ContainsKey("servername"))
                Ssettings.Servername = dataFile.Data["servername"];
            else
            {
                Ssettings.Servername = "Default";
                ConsoleWriteError("Couldnt find servername setting so we use the default (Default)");
            }

            if (dataFile.Data.ContainsKey("levelname"))
                Ssettings.LevelName = dataFile.Data["levelname"];
            else
            {
                Ssettings.LevelName = "";
                ConsoleWriteError("Couldnt find levelname setting so we use the default ()");
            }

            if (dataFile.Data.ContainsKey("motd"))
                Ssettings.MOTD = dataFile.Data["motd"];
            else
            {
                Ssettings.MOTD = "Welcome";
                ConsoleWriteError("Couldnt find MOTD setting so we use the default (Welcome)");
            }

            if (dataFile.Data.ContainsKey("autoannounce"))
                Ssettings.AutoAnnouce = bool.Parse(dataFile.Data["autoannounce"]);
            else
            {
                Ssettings.AutoAnnouce = false;
                ConsoleWriteError("Couldnt find autoannounce setting so we use the default (false)");
            }

            if (dataFile.Data.ContainsKey("autosave"))
                Ssettings.Autosavetimer = int.Parse(dataFile.Data["autosave"]);
            else
            {
                Ssettings.Autosavetimer = 5;
                ConsoleWriteError("Couldnt find autosave setting so we use the default (5)");
            }

            if (dataFile.Data.ContainsKey("lightsteps"))
                Ssettings.Lightsteps = int.Parse(dataFile.Data["lightsteps"]);
            else
            {
                Ssettings.Lightsteps = 60;
                ConsoleWriteError("Couldnt find lightsteps setting so we use the default (60)");
            }

            if (!(Ssettings.Maxplayers >= 1 && Ssettings.Maxplayers <= 16))
            {
                Ssettings.Maxplayers = 16;
                ConsoleWriteError("The value of maxplayers must be between 1 and 16 for now");
                ConsoleWrite("Setting Maxplayers to 16");
            }

            if (!(Ssettings.Autosavetimer >= 0 && Ssettings.Autosavetimer <= 60))
            {
                Ssettings.Autosavetimer = 5;
                ConsoleWriteError("The value of autosave must be between 0 and 60 for now");
                ConsoleWrite("Setting autosave to 5");
            }
            ConsoleWriteSucces("SETTINGS LOADED");
        }

        public void LoadMapSettings()
        {
            ConsoleWrite("LOADING MAPSETTINGS");

            Datafile dataFile = new Datafile(Msettings.SettingsDir + "/map.config.txt");

            if (dataFile.Data.ContainsKey("includetrees"))
                Msettings.Includetrees = bool.Parse(dataFile.Data["includetrees"]);
            else
            {
                Msettings.Includetrees = true;
                ConsoleWriteError("Couldnt find includetrees setting so we use the default (true)");
            }

            if (dataFile.Data.ContainsKey("includelava"))
                Msettings.Includelava = bool.Parse(dataFile.Data["includelava"]);
            else
            {
                Msettings.Includelava = true;
                ConsoleWriteError("Couldnt find includelava setting so we use the default (true)");
            }

            if (dataFile.Data.ContainsKey("includeadminblocks"))
                Msettings.IncludeAdminblocks = bool.Parse(dataFile.Data["includeadminblocks"]);
            else
            {
                Msettings.IncludeAdminblocks = true;
                ConsoleWriteError("Couldnt find includeadminblocks setting so we use the default (true)");
            }

            if (dataFile.Data.ContainsKey("lavaspawns"))
                Msettings.Lavaspawns = int.Parse(dataFile.Data["lavaspawns"]);
            else
            {
                Msettings.Lavaspawns = 0;
                ConsoleWriteError("Couldnt find lavaspawns setting so we use the default (0)");
            }

            if (dataFile.Data.ContainsKey("treecount"))
                Msettings.Treecount = int.Parse(dataFile.Data["treecount"]);
            else
            {
                Msettings.Treecount = 0;
                ConsoleWriteError("Couldnt find treecount setting so we use the default (0)");
            }

            if (dataFile.Data.ContainsKey("orefactor"))
                Msettings.Orefactor = int.Parse(dataFile.Data["orefactor"]);
            else
            {
                Msettings.Orefactor = 0;
                ConsoleWriteError("Couldnt find orefactor setting so we use the default (0)");
            }

            if (dataFile.Data.ContainsKey("includewater"))
                Msettings.Includewater = bool.Parse(dataFile.Data["includewater"]);
            else
            {
                Msettings.Includewater = true;
                ConsoleWriteError("Couldnt find includewater setting so we use the default (true)");
            }

            if (dataFile.Data.ContainsKey("waterspawns"))
                Msettings.Waterspawns = int.Parse(dataFile.Data["waterspawns"]);
            else
            {
                Msettings.Waterspawns = 0;
                ConsoleWriteError("Couldnt find waterspawns setting so we use the default (0)");
            }

            /*
            int temp;

            if (dataFile.Data.ContainsKey("mapsize"))
            {
                temp = int.Parse(dataFile.Data["mapsize"]);
                switch (temp)
                {
                    case 1:
                        {
                            Msettings.Mapsize = Mapsize.Small;
                            break;
                        }
                    case 2:
                        {
                            Msettings.Mapsize = Mapsize.Normal;
                            break;
                        }
                    case 3:
                        {
                            Msettings.Mapsize = Mapsize.Large;
                            break;
                        }
                }
            }
            else
            {
                Msettings.Mapsize = Mapsize.Normal;
                ConsoleWrite("Couldnt find mapsize settings so we use the default ((2)normal)");
            }

            if (!(Msettings.Mapsize == Mapsize.Small || Msettings.Mapsize == Mapsize.Normal || Msettings.Mapsize == Mapsize.Large))
            {
                Msettings.Mapsize = Mapsize.Normal;
                ConsoleWrite("Invalid number in mapsize settings so we use the default ((2)normal)");
            }*/

            ConsoleWriteSucces("MAPSETTINGS LOADED");
        }

        public void LoadExtraSettings()
        {
            ConsoleWrite("LOADING EXTRASETTINGS");

            //ToDo Load them from the file
            //dont forget to use [name]
            //File is already inplace
            SAsettings.Deathbydrown = "WAS DROWNED!";
            SAsettings.Deathbyfall = "WAS KILLED BY GRAVITY!";
            SAsettings.Deathbylava = "WAS INCINERATED BY LAVA!";
            SAsettings.Deathbyoutofbounds = "WAS KILLED BY MISADVENTURE!";
            SAsettings.Deathbysuicide = "HAS COMMITED PIXELCIDE!";
            SAsettings.Deathbycrush = "WAS CRUSHED!";
            SAsettings.Deathbyexplosion = "WAS KILLED IN AN EXPLOSION!";

            SAsettings.Playerhealth = 100;
            SAsettings.Playerregenrate = 1;

            ConsoleWriteSucces("EXTRASETTINGS LOADED");
        }

        public void LoadDirectorys()
        {
            ConsoleWrite("LOADING DIRECTORYS");

            Ssettings.SettingsDir = "ServerConfigs";
            Ssettings.LogsDir = "Logs";
            Ssettings.BackupDir = "Backups";
            Msettings.SettingsDir = "ServerConfigs";

            ConsoleWriteSucces("DIRECTORYS LOADED");
        }

        public void Shutdownserver()
        {
            ConsoleWrite("Server is shutting down in 5 seconds.", ConsoleColor.Yellow);
            SendServerWideMessage("Server is shutting down in 5 seconds.");
            shutdownTriggerd = true;
            shutdownTime = DateTime.Now + TimeSpan.FromSeconds(5);
        }

        public void Restartserver()
        {
            ConsoleWrite("Server restarting in 5 seconds.", ConsoleColor.Yellow);
            SendServerWideMessage("Server restarting in 5 seconds.");
            restartTriggered = true;
            restartTime = DateTime.Now + TimeSpan.FromSeconds(5);
        }

        public void SendServerMessageToPlayer(string message, ServerPlayer player)
        {
            NetOutgoingMessage msgBuffer = netServer.CreateMessage();
            msgBuffer.Write((byte)MineWorldMessage.ChatMessage);
            msgBuffer.Write((byte)ChatMessageType.SayServer);
            msgBuffer.Write(Defines.Sanitize(message));
            netServer.SendMessage(msgBuffer, player.NetConn, NetDeliveryMethod.ReliableUnordered);
        }

        public void SendServerWideMessage(string message)
        {
            NetOutgoingMessage msgBuffer = netServer.CreateMessage();
            msgBuffer.Write((byte)MineWorldMessage.ChatMessage);
            msgBuffer.Write((byte)ChatMessageType.SayServer);
            msgBuffer.Write(Defines.Sanitize(message));
            foreach (ServerPlayer player in playerList.Values)
            {
                netServer.SendMessage(msgBuffer, player.NetConn, NetDeliveryMethod.ReliableUnordered);
            }
        }

        // Lets a player know about their resources.
        public void SendHealthUpdate(ServerPlayer player)
        {
            // Health, HealthMax both int
            NetOutgoingMessage msgBuffer = netServer.CreateMessage();
            msgBuffer.Write((byte)MineWorldMessage.HealthUpdate);
            msgBuffer.Write(player.Health);
            msgBuffer.Write(player.HealthMax);
            netServer.SendMessage(msgBuffer, player.NetConn, NetDeliveryMethod.ReliableOrdered);
        }

        List<MapSender> mapSendingProgress = new List<MapSender>();

        public void  TerminateFinishedThreads()
        {
            List<MapSender> mapSendersToRemove = new List<MapSender>();
            foreach (MapSender ms in mapSendingProgress)
            {
                if (ms.finished)
                {
                    ms.stop();
                    mapSendersToRemove.Add(ms);
                }
            }
            foreach (MapSender ms in mapSendersToRemove)
            {
                mapSendingProgress.Remove(ms);
            }
        }

        public void SendCurrentMap(NetConnection client)
        {
            MapSender ms = new MapSender(client, this, netServer,Msettings.Mapsize);
            mapSendingProgress.Add(ms);
        }

        public void SendInitialStats(NetConnection client)
        {

        }

        public void KillPlayerSpecific(ServerPlayer player)
        {
            // Put variables to zero
            player.Health = 0;
            player.Alive = false;

            // Kill the player specific
            NetOutgoingMessage msgBufferb = netServer.CreateMessage();
            msgBufferb.Write((byte)MineWorldMessage.Killed);
            //TODO IMPLENT DEATH MESSAGES
            msgBufferb.Write("");
            netServer.SendMessage(msgBufferb, player.NetConn, NetDeliveryMethod.ReliableOrdered);

            // Let all the other players know
            NetOutgoingMessage msgBuffer = netServer.CreateMessage();
            msgBuffer.Write((byte)MineWorldMessage.PlayerDead);
            msgBuffer.Write(player.ID);
            foreach (ServerPlayer iplayer in playerList.Values)
            {
                netServer.SendMessage(msgBuffer, player.NetConn, NetDeliveryMethod.ReliableOrdered);
            }
        }

        public void SendPlayerPosition(ServerPlayer player)
        {
            if (player.NetConn.Status != NetConnectionStatus.Connected)
                return;

            NetOutgoingMessage msgBuffer = netServer.CreateMessage();
            msgBuffer.Write((byte)MineWorldMessage.PlayerPosition);
            msgBuffer.Write(player.Position);
            netServer.SendMessage(msgBuffer, player.NetConn, NetDeliveryMethod.ReliableOrdered);
        }

        public void SendPlayerUpdate(ServerPlayer player)
        {
            NetOutgoingMessage msgBuffer = netServer.CreateMessage();
            msgBuffer.Write((byte)MineWorldMessage.PlayerUpdate);
            msgBuffer.Write(player.ID);
            msgBuffer.Write(player.Position);
            msgBuffer.Write(player.Heading);

            foreach (ServerPlayer iplayer in playerList.Values)
            {
                netServer.SendMessage(msgBuffer, iplayer.NetConn, NetDeliveryMethod.ReliableOrdered);
            }
        }

        public void SendPlayerJoined(ServerPlayer player)
        {
            NetOutgoingMessage msgBuffer;

            // Let this player know about other players.
            foreach (ServerPlayer p in playerList.Values)
            {
                msgBuffer = netServer.CreateMessage();
                msgBuffer.Write((byte)MineWorldMessage.PlayerJoined);
                msgBuffer.Write(p.ID);
                msgBuffer.Write(p.Name);
                msgBuffer.Write(p == player);
                msgBuffer.Write(p.Alive);
                netServer.SendMessage(msgBuffer, p.NetConn, NetDeliveryMethod.ReliableOrdered);
            }

            // Let other players know about this player.
            msgBuffer = netServer.CreateMessage();
            msgBuffer.Write((byte)MineWorldMessage.PlayerJoined);
            msgBuffer.Write(player.ID);
            msgBuffer.Write(player.Name);
            msgBuffer.Write(false);
            msgBuffer.Write(player.Alive);

            foreach (ServerPlayer iplayer in playerList.Values)
            {
                netServer.SendMessage(msgBuffer, iplayer.NetConn, NetDeliveryMethod.ReliableOrdered);
            }

            SendPlayerRespawn(player);

            // Send out a chat message.
            msgBuffer = netServer.CreateMessage();
            msgBuffer.Write((byte)MineWorldMessage.ChatMessage);
            msgBuffer.Write((byte)ChatMessageType.Say);
            msgBuffer.Write(player.Name + " HAS JOINED THE ADVENTURE!");
            foreach (ServerPlayer iplayer in playerList.Values)
            {
                //if (netConn.Status == NetConnectionStatus.Connected)
                // Dont send the joined message to ourself
                if (player.ID == iplayer.ID)
                {
                    break;
                }
                netServer.SendMessage(msgBuffer, iplayer.NetConn, NetDeliveryMethod.ReliableOrdered);
            }
        }

        public void SendPlayerLeft(ServerPlayer player, string reason)
        {
            NetOutgoingMessage msgBuffer = netServer.CreateMessage();
            msgBuffer.Write((byte)MineWorldMessage.PlayerLeft);
            msgBuffer.Write(player.ID);
            foreach (ServerPlayer iplayer in playerList.Values)
            {
                if (player.NetConn != iplayer.NetConn)
                {
                    netServer.SendMessage(msgBuffer, iplayer.NetConn, NetDeliveryMethod.ReliableOrdered);
                }
            }

            // Send out a chat message.
            msgBuffer = netServer.CreateMessage();
            msgBuffer.Write((byte)MineWorldMessage.ChatMessage);
            msgBuffer.Write((byte)ChatMessageType.Say);
            msgBuffer.Write(player.Name + " " + reason);
            foreach (ServerPlayer iplayer in playerList.Values)
            {
                netServer.SendMessage(msgBuffer, iplayer.NetConn, NetDeliveryMethod.ReliableOrdered);
            }
        }

        public void SendPlayerDead(ServerPlayer player)
        {
            NetOutgoingMessage msgBuffer = netServer.CreateMessage();
            msgBuffer.Write((byte)MineWorldMessage.PlayerDead);
            msgBuffer.Write(player.ID);
            foreach (ServerPlayer iplayer in playerList.Values)
            {
                netServer.SendMessage(msgBuffer, iplayer.NetConn, NetDeliveryMethod.ReliableOrdered);
            }
        }

        public void SendPlayerAlive(ServerPlayer player)
        {
            NetOutgoingMessage msg = netServer.CreateMessage();
            msg.Write((byte)MineWorldMessage.PlayerAlive);
            msg.Write(player.ID);
            foreach (ServerPlayer iplayer in playerList.Values)
            {
                netServer.SendMessage(msg, player.NetConn, NetDeliveryMethod.ReliableSequenced);
            }
        }

        public void SendPlayerRespawn(ServerPlayer player)
        {
            if (!player.Alive)
            {
                // Respawn a few blocks above a safe position above altitude 0.
                bool positionFound = false;

                // Try 20 times; use a potentially invalid position if we fail.
                for (int i = 0; i < 20; i++)
                {
                    // Pick a random starting point.
                    Vector3 startPos = new Vector3(randGen.Next(2, 62), 63, randGen.Next(2, 62));

                    // See if this is a safe place to drop.
                    for (startPos.Y = 63; startPos.Y >= 54; startPos.Y--)
                    {
                        BlockType blockType = BlockAtPoint(startPos);
                        if (blockType == BlockType.Lava)
                            break;
                        else if (blockType != BlockType.None)
                        {
                            // We have found a valid place to spawn, so spawn a few above it.
                            player.Position = startPos + Vector3.UnitY * 5;
                            positionFound = true;
                            break;
                        }
                    }

                    // If we found a position, no need to try anymore!
                    if (positionFound)
                        break;
                }

                // If we failed to find a spawn point, drop randomly.
                if (!positionFound)
                    player.Position = new Vector3(randGen.Next(2, 62), 66, randGen.Next(2, 62));

                // Drop the player on the middle of the block, not at the corner.
                player.Position += new Vector3(0.5f, 0, 0.5f);

                //NetOutgoingMessage msgBuffer = netServer.CreateMessage();
                NetOutgoingMessage msg = netServer.CreateMessage();
                msg.Write((byte)MineWorldMessage.PlayerRespawn);
                msg.Write(player.Position);
                netServer.SendMessage(msg, player.NetConn, NetDeliveryMethod.ReliableSequenced);
                //netServer.SendMsg(msgBuffer, player.NetConn, NetChannel.ReliableInOrder3);
            }
        }

        public void PlaySound(MineWorldSound sound, Vector3 position)
        {
            NetOutgoingMessage msg = netServer.CreateMessage();
            msg.Write((byte)MineWorldMessage.PlaySound);
            msg.Write((byte)sound);
            msg.Write(true);
            msg.Write(position);
            foreach (ServerPlayer player in playerList.Values)
            {
                netServer.SendMessage(msg, player.NetConn, NetDeliveryMethod.ReliableOrdered);
            }
        }

        public void addtoMasterServer()
        {
            updateMasterServer(true);
        }

        public void updateMasterServer()
        {
            updateMasterServer(false);
        }

        public void hearthbeatMasterServer()
        {
            Dictionary<string, string> session = new Dictionary<string, string>();
            session.Add("id", sessionid);
            HttpRequest.Get(Defines.MASTERSERVER_BASE_URL + "updateServer.php", session);
            ConsoleWrite("SENDING HEARTHBEAT TO MASTERSERVER");
        }

        public void removefromMasterServer()
        {
            Dictionary<string, string> session = new Dictionary<string, string>();
            session.Add("id", sessionid);
            HttpRequest.Get(Defines.MASTERSERVER_BASE_URL + "removeServer.php", session);
            ConsoleWrite("REMOVING SERVER FROM MASTERSERVER");
        }

        public void updateMasterServer(bool firsttime)
        {
            //TODO Serverinformation is hardcoded in the update function
            string temp;
            Dictionary<string, string> serverinfo = new Dictionary<string,string>();
            serverinfo.Add("ip", serverIP);
            serverinfo.Add("sn", Ssettings.Servername);
            serverinfo.Add("u" , playerList.Count.ToString());
            serverinfo.Add("mu", Ssettings.Maxplayers.ToString());
            serverinfo.Add("e", "InfiniteWorld");
            serverinfo.Add("t", "MineWorld");
            temp = HttpRequest.Get(Defines.MASTERSERVER_BASE_URL + "updateServer.php",serverinfo);
            if (firsttime)
            {
                sessionid = temp;
                ConsoleWrite("ADDING SERVER TO MASTERSERVER");
                ConsoleWrite("SESSION KEY: " + sessionid);
            }
            else
            {
                ConsoleWrite("UPDATING MASTERSERVER");
            }
        }

        public void Sendhearthbeat()
        {
            NetOutgoingMessage msg = netServer.CreateMessage();
            msg.Write((byte)MineWorldMessage.Hearthbeat);
            foreach (ServerPlayer iplayer in playerList.Values)
            {
                netServer.SendMessage(msg, iplayer.NetConn, NetDeliveryMethod.UnreliableSequenced);
            }
        }

        public void Senddaytimeupdate(float time)
        {
            NetOutgoingMessage msg = netServer.CreateMessage();
            msg.Write((byte)MineWorldMessage.DayUpdate);
            msg.Write(time);
            foreach (ServerPlayer iplayer in playerList.Values)
            {
                netServer.SendMessage(msg, iplayer.NetConn, NetDeliveryMethod.UnreliableSequenced);
            }
        }
    }
}