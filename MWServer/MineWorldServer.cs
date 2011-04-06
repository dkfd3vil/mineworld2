using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MineWorld
{
    public partial class MineWorldServer
    {
        Random randomizer = new Random();

        MineWorldNetServer netServer = null;
        public DayManager dayManager = null;
        public Dictionary<NetConnection, IClient> playerList = new Dictionary<NetConnection, IClient>();
        public List<NetConnection> toGreet = new List<NetConnection>();
        public List<string> admins = new List<string>(); //List of strings with all the admins
        public List<string> bannednames = new List<string>(); // List of strings with all names that cannot be chosen

        DateTime lasthearthbeatsend = DateTime.Now;
        DateTime lastServerListUpdate = DateTime.Now;
        DateTime lastMapBackup = DateTime.Now;
        public List<string> banList = null;

        public String serverIP;
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
        public MapSettings Msettings = new MapSettings();

        //TODO Find a proper place
        // Fluids count
        int Totallavablockcount;
        int Totalwaterblockcount;

        public MineWorldServer()
        {
            Console.SetWindowSize(1, 1);
            Console.SetBufferSize(80, CONSOLE_SIZE + 4);
            Console.SetWindowSize(80, CONSOLE_SIZE + 4);
        }
        public String GetExternalIp()
        {
            string whatIsMyIp = "http://www.whatismyip.com/automation/n09230945.asp";
            WebClient wc = new WebClient();
            if (Ssettings.Proxy != true)
            {
                wc.Proxy = null;
            }
            UTF8Encoding utf8 = new UTF8Encoding();
            string requestHtml = "";
            try
            {
                requestHtml = utf8.GetString(wc.DownloadData(whatIsMyIp));
            }
            catch (WebException we)
            {
                // do something with exception
                ConsoleWrite(we.ToString());
            }
            String externalIp = null;
            if (requestHtml!="")
            {
                externalIp = requestHtml;
            }
            return externalIp;
        }

        public bool Start()
        {
            // Load the directory's.
            LoadDirectorys();

            // Load the general-settings.
            LoadSettings();

            // Load the map-settings.
            LoadMapSettings();

            // Load the ban-list.
            banList = LoadBanList();

            // Load the admin-list.
            admins = LoadAdminList();

            // Load the bannednames-list.
            bannednames = LoadBannedNames();

            // Initialize the server.
            NetConfiguration netConfig = new NetConfiguration("MineWorldPlus");
            netConfig.MaxConnections = Ssettings.Maxplayers;
            netConfig.Port = 5565;
            netServer = new MineWorldNetServer(netConfig);
            netServer.SetMessageTypeEnabled(NetMessageType.ConnectionApproval, true);
            //netServer.SimulatedMinimumLatency = 0.1f;
            //netServer.SimulatedLatencyVariance = 0.05f;
            //netServer.SimulatedLoss = 0.1f;
            //netServer.SimulatedDuplicates = 0.05f;

            // Initialize the daymanager.
            dayManager = new DayManager();

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
                    ConsoleWrite("AUTOLOAD FAILED");
                }
                else
                {
                    ConsoleWrite("AUTLOAD SUCCESFULL");
                }
            }
            if(loaded == false)
            {
                ConsoleWrite("GENERATING NEW MAP");
                GenerateNewMap();
                ConsoleWrite("NEW MAP GENERATED");
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
                updateMasterServer();
            }

            // Main server loop!
            ConsoleWrite("SERVER READY");

            while (keepRunning)
            {
                // Check the state of our core threads
                if (!listenerthread.IsAlive)
                {
                    ConsoleWrite("Listenerthread died");
                    ConsoleWrite("Server is shutting down");
                    return false;
                }
                if (!physicsthread.IsAlive)
                {
                    ConsoleWrite("Physicsthread died");
                    ConsoleWrite("Server is shutting down");
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
                        ConsoleWrite("Heavy load: " + frameCount + " FPS");
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
                dayManager.Update(Ssettings.Lightsteps);

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
                        System.Threading.Thread backupthread = new System.Threading.Thread(new ThreadStart(BackupLevel));
                        backupthread.Start();
                        lastMapBackup = DateTime.Now;
                        ConsoleWrite("BACK-UP DONE");
                    }
                }
                //Time to terminate finished map sending threads?
                TerminateFinishedThreads();

                // Handle console keypresses.
                while (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey();
                    if (keyInfo.Key == ConsoleKey.Enter)
                        ConsoleProcessInput();
                    else if (keyInfo.Key == ConsoleKey.Backspace)
                    {
                        if (consoleInput.Length > 0)
                            consoleInput = consoleInput.Substring(0, consoleInput.Length - 1);
                        ConsoleRedraw();
                    }
                    else
                    {
                        consoleInput += keyInfo.KeyChar;
                        ConsoleRedraw();
                    }
                }

                // Restart the server?
                if (restartTriggered && DateTime.Now > restartTime)
                {
                    disconnectAll();
                    netServer.Shutdown("serverrestart");
                    BackupLevel();
                    return true;
                }

                if (shutdownTriggerd && DateTime.Now > shutdownTime)
                {
                    disconnectAll();
                    netServer.Shutdown("servershutdown");
                    BackupLevel();
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
                ConsoleWrite("Couldnt find logs settings so we use the default (false)");
            }
            ConsoleWrite("LOADING SETTINGS");

            // Read in from the config file.
            if (dataFile.Data.ContainsKey("maxplayers"))
                Ssettings.Maxplayers = int.Parse(dataFile.Data["maxplayers"]);
            else
            {
                Ssettings.Maxplayers = 16;
                ConsoleWrite("Couldnt find maxplayers setting so we use the default (16)");
            }

            if (dataFile.Data.ContainsKey("public"))
                Ssettings.Public = bool.Parse(dataFile.Data["public"]);
            else
            {
                Ssettings.Public = false;
                ConsoleWrite("Couldnt find public setting so we use the default (FALSE)");
            }
            if (dataFile.Data.ContainsKey("proxy"))
                Ssettings.Proxy = bool.Parse(dataFile.Data["proxy"]);
            else
            {
                Ssettings.Proxy = false;
                ConsoleWrite("Couldnt find proxy setting so we use the default (false)");
            }

            if (dataFile.Data.ContainsKey("servername"))
                Ssettings.Servername = dataFile.Data["servername"];
            else
            {
                Ssettings.Servername = "Default";
                ConsoleWrite("Couldnt find servername setting so we use the default (Default)");
            }

            if (dataFile.Data.ContainsKey("levelname"))
                Ssettings.LevelName = dataFile.Data["levelname"];
            else
            {
                Ssettings.LevelName = "";
                ConsoleWrite("Couldnt find levelname setting so we use the default ()");
            }

            if (dataFile.Data.ContainsKey("motd"))
                Ssettings.MOTD = dataFile.Data["motd"];
            else
            {
                Ssettings.MOTD = "Welcome";
                ConsoleWrite("Couldnt find MOTD setting so we use the default (Welcome)");
            }

            if (dataFile.Data.ContainsKey("autoannounce"))
                Ssettings.AutoAnnouce = bool.Parse(dataFile.Data["autoannounce"]);
            else
            {
                Ssettings.AutoAnnouce = false;
                ConsoleWrite("Couldnt find autoannounce setting so we use the default (false)");
            }

            if (dataFile.Data.ContainsKey("autosave"))
                Ssettings.Autosavetimer = int.Parse(dataFile.Data["autosave"]);
            else
            {
                Ssettings.Autosavetimer = 5;
                ConsoleWrite("Couldnt find autosave setting so we use the default (5)");
            }

            if (dataFile.Data.ContainsKey("lightsteps"))
                Ssettings.Lightsteps = int.Parse(dataFile.Data["lightsteps"]);
            else
            {
                Ssettings.Lightsteps = 60;
                ConsoleWrite("Couldnt find lightsteps setting so we use the default (60)");
            }

            if (!(Ssettings.Maxplayers >= 1 && Ssettings.Maxplayers <= 16))
            {
                Ssettings.Maxplayers = 16;
                ConsoleWrite("The value of maxplayers must be between 1 and 16 for now");
                ConsoleWrite("Setting Maxplayers to 16");
            }

            if (!(Ssettings.Autosavetimer >= 0 && Ssettings.Autosavetimer <= 60))
            {
                Ssettings.Autosavetimer = 5;
                ConsoleWrite("The value of autosave must be between 0 and 60 for now");
                ConsoleWrite("Setting autosave to 5");
            }
            ConsoleWrite("SETTINGS LOADED");
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
                ConsoleWrite("Couldnt find includetrees setting so we use the default (true)");
            }

            if (dataFile.Data.ContainsKey("includelava"))
                Msettings.Includelava = bool.Parse(dataFile.Data["includelava"]);
            else
            {
                Msettings.Includelava = true;
                ConsoleWrite("Couldnt find includelava setting so we use the default (true)");
            }

            if (dataFile.Data.ContainsKey("includeadminblocks"))
                Msettings.IncludeAdminblocks = bool.Parse(dataFile.Data["includeadminblocks"]);
            else
            {
                Msettings.IncludeAdminblocks = true;
                ConsoleWrite("Couldnt find includeadminblocks setting so we use the default (true)");
            }

            if (dataFile.Data.ContainsKey("lavaspawns"))
                Msettings.Lavaspawns = int.Parse(dataFile.Data["lavaspawns"]);
            else
            {
                Msettings.Lavaspawns = 0;
                ConsoleWrite("Couldnt find lavaspawns setting so we use the default (0)");
            }

            if (dataFile.Data.ContainsKey("treecount"))
                Msettings.Treecount = int.Parse(dataFile.Data["treecount"]);
            else
            {
                Msettings.Treecount = 0;
                ConsoleWrite("Couldnt find treecount setting so we use the default (0)");
            }

            if (dataFile.Data.ContainsKey("orefactor"))
                Msettings.Orefactor = int.Parse(dataFile.Data["orefactor"]);
            else
            {
                Msettings.Orefactor = 0;
                ConsoleWrite("Couldnt find orefactor setting so we use the default (0)");
            }

            if (dataFile.Data.ContainsKey("includewater"))
                Msettings.Includewater = bool.Parse(dataFile.Data["includewater"]);
            else
            {
                Msettings.Includewater = true;
                ConsoleWrite("Couldnt find includewater setting so we use the default (true)");
            }

            if (dataFile.Data.ContainsKey("waterspawns"))
                Msettings.Waterspawns = int.Parse(dataFile.Data["waterspawns"]);
            else
            {
                Msettings.Waterspawns = 0;
                ConsoleWrite("Couldnt find waterspawns setting so we use the default (0)");
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

            ConsoleWrite("MAPSETTINGS LOADED");
        }

        public void LoadDirectorys()
        {
            Ssettings.SettingsDir = "ServerConfigs";
            Ssettings.LogsDir = "Logs";
            Ssettings.BackupDir = "Backups";
            Msettings.SettingsDir = "ServerConfigs";
        }

        public void Shutdownserver()
        {
            ConsoleWrite("Server is shutting down in 5 seconds.");
            SendServerMessage("Server is shutting down in 5 seconds.");
            shutdownTriggerd = true;
            shutdownTime = DateTime.Now + TimeSpan.FromSeconds(5);
        }

        public void Restartserver()
        {
            ConsoleWrite("Server restarting in 5 seconds.");
            SendServerMessage("Server restarting in 5 seconds.");
            restartTriggered = true;
            restartTime = DateTime.Now + TimeSpan.FromSeconds(5);
        }

        public void SendServerMessageToPlayer(string message, Player player)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.ChatMessage);
            msgBuffer.Write((byte)ChatMessageType.SayServer);
            msgBuffer.Write(Defines.Sanitize(message));
            netServer.SendMessage(msgBuffer, player.NetConn, NetChannel.ReliableUnordered);
        }

        public void SendServerMessage(string message)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.ChatMessage);
            msgBuffer.Write((byte)ChatMessageType.SayServer);
            msgBuffer.Write(Defines.Sanitize(message));
            foreach (IClient player in playerList.Values)
                //if (netConn.Status == NetConnectionStatus.Connected)
                player.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder3);
        }

        // Lets a player know about their resources.
        public void SendResourceUpdate(IClient player)
        {
            if (player.NetConn.Status != NetConnectionStatus.Connected)
                return;

            // ore, cash, weight, max ore, max weight, team ore, red cash, blue cash, all uint
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.ResourceUpdate);
            msgBuffer.Write((uint)player.Health);
            msgBuffer.Write((uint)player.HealthMax);
            player.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder1);
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

        //TODO: SendcurrentMapB is not used maybe remove it?
        /*
        public void SendCurrentMapB(NetConnection client)
        {
            Debug.Assert(MAPSIZE == 64, "The BlockBulkTransfer message requires a map size of 64.");
            
            for (byte x = 0; x < MAPSIZE; x++)
                for (byte y=0; y<MAPSIZE; y+=16)
                {
                    NetBuffer msgBuffer = netServer.CreateBuffer();
                    msgBuffer.Write((byte)MineWorldMessage.BlockBulkTransfer);
                    msgBuffer.Write(x);
                    msgBuffer.Write(y);
                    for (byte dy=0; dy<16; dy++)
                        for (byte z = 0; z < MAPSIZE; z++)
                            msgBuffer.Write((byte)(blockList[x, y+dy, z]));
                    if (client.Status == NetConnectionStatus.Connected)
                        netServer.SendMessage(msgBuffer, client, NetChannel.ReliableUnordered);
                }
        }
         */

        public void SendPlayerPing(uint playerId)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.PlayerPing);
            msgBuffer.Write(playerId);

            foreach (IClient player in playerList.Values)
                //if (netConn.Status == NetConnectionStatus.Connected)
                player.AddQueMsg(msgBuffer, NetChannel.ReliableUnordered);
        }

        public void KillPlayerSpecific(Player player)
        {
            // Put variables to zero
            player.Health = 0;
            player.Alive = false;

            // Kill the player specific
            NetBuffer msgBufferb = netServer.CreateBuffer();
            msgBufferb.Write((byte)MineWorldMessage.Killed);
            //TODO IMPLENT DEATH MESSAGES
            msgBufferb.Write("");
            netServer.SendMessage(msgBufferb, player.NetConn, NetChannel.ReliableUnordered);

            // Let all the other players know
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.PlayerDead);
            msgBuffer.Write((uint)player.ID);
            foreach (IClient iplayer in playerList.Values)
                //if (netConn.Status == NetConnectionStatus.Connected)
                iplayer.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder2);
        }

        public void SendPlayerPosition(Player player)
        {
            if (player.NetConn.Status != NetConnectionStatus.Connected)
                return;

            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.PlayerPosition);
            msgBuffer.Write(player.Position);
            netServer.SendMessage(msgBuffer, player.NetConn, NetChannel.ReliableUnordered);
        }

        public void SendPlayerUpdate(IClient player)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.PlayerUpdate);
            msgBuffer.Write((uint)player.ID);
            msgBuffer.Write(player.Position);
            msgBuffer.Write(player.Heading);
            msgBuffer.Write((byte)player.Tool);

            if (player.QueueAnimationBreak)
            {
                player.QueueAnimationBreak = false;
                msgBuffer.Write(false);
            }
            else
                msgBuffer.Write(player.UsingTool);

            msgBuffer.Write((ushort)player.Health / 100);

            foreach (IClient iplayer in playerList.Values)
                //if (netConn.Status == NetConnectionStatus.Connected)
                iplayer.AddQueMsg(msgBuffer, NetChannel.UnreliableInOrder1);
        }

        public void SendSetBeacon(Vector3 position, string text)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.SetBeacon);
            msgBuffer.Write(position);
            msgBuffer.Write(text);
            foreach (IClient player in playerList.Values)
                //if (netConn.Status == NetConnectionStatus.Connected)
                player.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder2);
        }

        public void SendPlayerJoined(IClient player)
        {
            NetBuffer msgBuffer;

            // Let this player know about other players.
            foreach (IClient p in playerList.Values)
            {
                msgBuffer = netServer.CreateBuffer();
                msgBuffer.Write((byte)MineWorldMessage.PlayerJoined);
                msgBuffer.Write((uint)p.ID);
                msgBuffer.Write(p.Handle);
                msgBuffer.Write(p == player);
                msgBuffer.Write(p.Alive);
                //if (player.NetConn.Status == NetConnectionStatus.Connected)
                    player.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder2);

                msgBuffer = netServer.CreateBuffer();
                msgBuffer.Write((uint)p.ID);
                //if (player.NetConn.Status == NetConnectionStatus.Connected)
                    player.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder2);
            }

            // Let this player know about all placed beacons.
            foreach (KeyValuePair<Vector3, Beacon> bPair in beaconList)
            {
                Vector3 position = bPair.Key;
                position.Y += 1; // beacon is shown a block below its actually position to make altitude show up right
                msgBuffer = netServer.CreateBuffer();
                msgBuffer.Write((byte)MineWorldMessage.SetBeacon);
                msgBuffer.Write(position);
                msgBuffer.Write(bPair.Value.ID);
                //if (player.NetConn.Status == NetConnectionStatus.Connected)
                    player.AddQueMsg(msgBuffer,  NetChannel.ReliableInOrder2);
            }

            // Let other players know about this player.
            msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.PlayerJoined);
            msgBuffer.Write((uint)player.ID);
            msgBuffer.Write(player.Handle);
            msgBuffer.Write(false);
            msgBuffer.Write(player.Alive);

            foreach (IClient iplayer in playerList.Values)
                //if (netConn.Status == NetConnectionStatus.Connected)
                iplayer.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder2);

            SendPlayerRespawn(player);

            // Send out a chat message.
            msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.ChatMessage);
            msgBuffer.Write((byte)ChatMessageType.Say);
            msgBuffer.Write(player.Handle + " HAS JOINED THE ADVENTURE!");
            foreach (IClient iplayer in playerList.Values)
            {
                //if (netConn.Status == NetConnectionStatus.Connected)
                // Dont send the joined message to ourself
                if (player.ID == iplayer.ID)
                {
                    break;
                }
                iplayer.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder3);
            }
        }

        public void SendPlayerLeft(IClient player, string reason)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.PlayerLeft);
            msgBuffer.Write((uint)player.ID);
            foreach (IClient iplayer in playerList.Values)
                if (player.NetConn != iplayer.NetConn)
                iplayer.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder2);

            // Send out a chat message.
            msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.ChatMessage);
            msgBuffer.Write((byte)ChatMessageType.Say);
            msgBuffer.Write(player.Handle + " " + reason);
            foreach (IClient iplayer in playerList.Values)
                //if (netConn.Status == NetConnectionStatus.Connected)
                iplayer.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder3);
        }

        public void SendPlayerDead(IClient player)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.PlayerDead);
            msgBuffer.Write((uint)player.ID);
            foreach (IClient iplayer in playerList.Values)
                //if (netConn.Status == NetConnectionStatus.Connected)
                iplayer.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder2);
        }

        public void SendPlayerAlive(IClient player)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.PlayerAlive);
            msgBuffer.Write((uint)player.ID);
            foreach (IClient iplayer in playerList.Values)
                //if (netConn.Status == NetConnectionStatus.Connected)
                iplayer.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder2);
        }

        public void SendPlayerRespawn(Player player)
        {
            if (!player.Alive)
            {
                //create respawn script
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
                //

                NetBuffer msgBuffer = netServer.CreateBuffer();
                msgBuffer.Write((byte)MineWorldMessage.PlayerRespawn);
                msgBuffer.Write(player.Position);
                netServer.SendMessage(msgBuffer, player.NetConn, NetChannel.ReliableInOrder3);
            }
        }

        public void PlaySound(MineWorldSound sound, Vector3 position)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.PlaySound);
            msgBuffer.Write((byte)sound);
            msgBuffer.Write(true);
            msgBuffer.Write(position);
            foreach (IClient player in playerList.Values)
                //if (netConn.Status == NetConnectionStatus.Connected)
                player.AddQueMsg(msgBuffer, NetChannel.ReliableUnordered);
        }

        public void updateMasterServer()
        {
            //WebClient wc = new WebClient();
            //if (!Ssettings.Proxy)
            //{
                //wc.Proxy = null;
            //}
            // JOw martijn dit moet ff anders
            // Ik zal je een voorbeeldje laten zien ;)
            /*
             De variablen leest de client op deze volgorde en dat ging fout bij jou :S
             * IP
             * NAME
             * EXTRA INFO (ons geval null dacht ik weet niet zeker :P
             * PLAYERCOUNT
             * MAXPLAYERS
             */
            //wc.DownloadString("http://www.humorco.nl/mineworld/updateServer.php?sn=" + servername + "&ip=" + IP + "&u=" + currentUsers + "&mu=" + maxUsers);
            ConsoleWrite("UPDATING MASTERSERVER");
        }

        public void Sendhearthbeat()
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.Hearthbeat);
            foreach (IClient iplayer in playerList.Values)
                //if (netConn.Status == NetConnectionStatus.Connected)
                iplayer.AddQueMsg(msgBuffer, NetChannel.UnreliableInOrder15);
        }

        public void Senddaytimeupdate(float time)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.DayUpdate);
            msgBuffer.Write(time);
            foreach (IClient iplayer in playerList.Values)
                iplayer.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder14);
        }
    }
}
