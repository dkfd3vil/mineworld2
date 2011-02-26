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
        MineWorldNetServer netServer = null;
        public Dictionary<NetConnection, IClient> playerList = new Dictionary<NetConnection, IClient>();
        public List<NetConnection> toGreet = new List<NetConnection>();
        public List<string> admins = new List<string>(); //List of strings with all the admins
        public List<string> bannednames = new List<string>(); // List of strings with all names that cannot be chosen

        DateTime lasthearthbeatsend = DateTime.Now;
        DateTime lastServerListUpdate = DateTime.Now;
        DateTime lastMapBackup = DateTime.Now;
        public List<string> banList = null;

        public String serverIP;

        bool keepRunning = true;

        // Server restarting variables.
        DateTime restartTime = DateTime.Now;
        bool restartTriggered = false;

        // Server shutdown variables.
        DateTime shutdownTime = DateTime.Now;
        bool shutdownTriggerd = false;

        public ServerSettings Ssettings = new ServerSettings();

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
            // Load the general-settings.
            LoadSettings();

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
            netServer.Start();

            // Store the last time that we did a flow calculation.
            DateTime lastCalc = DateTime.Now;

            //Display external IP
            //Dont bother if it isnt public
            if (Ssettings.Public == true)
            {
                serverIP = GetExternalIp();
                ConsoleWrite("Your external IP Adress: " + serverIP);
            }

            //Check if we should autoload a level
            if (Ssettings.Autoload)
            {
                blockList = new BlockType[Defines.MAPSIZE, Defines.MAPSIZE, Defines.MAPSIZE];
                blockCreatorTeam = new PlayerTeam[Defines.MAPSIZE, Defines.MAPSIZE, Defines.MAPSIZE];
                LoadLevel(Ssettings.LevelName);
            }
            else
            {
                // Calculate initial lava flows.
                ConsoleWrite("CALCULATING INITIAL LAVA FLOWS");
                ConsoleWrite("TOTAL LAVA BLOCKS = " + newMap());
                ConsoleWrite("TOTAL BLOCKS = " + Defines.MAPSIZE * Defines.MAPSIZE * Defines.MAPSIZE);
            }
            lastMapBackup = DateTime.Now;
            ServerListener listener = new ServerListener(netServer,this);
            System.Threading.Thread listenerthread = new System.Threading.Thread(new ThreadStart(listener.start));
            listenerthread.Start();


            // Main server loop!
            ConsoleWrite("SERVER READY");
            Random randomizer = new Random(56235676);

            //If public, announce server to public tracker
            if (Ssettings.Public)
            {
                updateMasterServer();
            }


            while (keepRunning)
            {
                // Dont send hearthbeats too fast
                TimeSpan updatehearthbeat = DateTime.Now - lasthearthbeatsend;
                if (updatehearthbeat.TotalMilliseconds > 1000)
                {
                    Sendhearthbeat();
                    lasthearthbeatsend = DateTime.Now;
                }

                //Time to backup map?
                // If Ssettings.autosavetimer is 0 then autosave is disabled
                if (Ssettings.Autosavetimer == 0)
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

                //Do some world calculation
                DoPhysics();

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

                // Is the game over?
                if (winningTeam != PlayerTeam.None && !restartTriggered)
                {
                    BroadcastGameOver();
                    restartTriggered = true;
                    restartTime = DateTime.Now.AddSeconds(10);
                }

                // Restart the server?
                if (restartTriggered && DateTime.Now > restartTime)
                {
                    SaveLevel("autosave_" + (UInt64)DateTime.Now.ToBinary() + ".lvl");
                    //netServer.Shutdown("The server is restarting.");
                    return true;
                }

                if (shutdownTriggerd && DateTime.Now > shutdownTime)
                {
                    SaveLevel("autosave_" + (UInt64)DateTime.Now.ToBinary() + ".lvl");
                    return false;
                }

                // Pass control over to waiting threads.
                Thread.Sleep(1);
            }
            return false;
        }

        public void LoadSettings()
        {
            ConsoleWrite("LOADING SETTINGS");
            //TODO: Load settings from file
            //For now we hardcode them
            Ssettings.StopFluids = false;
            Ssettings.Directory = "ServerConfigs";

            // Read in from the config file.
            Datafile dataFile = new Datafile(Ssettings.Directory + "/server.config.txt");
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

            if (dataFile.Data.ContainsKey("includelava"))
                Ssettings.Includelava = bool.Parse(dataFile.Data["includelava"]);
            else
            {
                Ssettings.Includelava = true;
                ConsoleWrite("Couldnt find includelava setting so we use the default (true)");
            }

            if (dataFile.Data.ContainsKey("autosave"))
                Ssettings.Autosavetimer = int.Parse(dataFile.Data["autosave"]);
            else
            {
                Ssettings.Autosavetimer = 5;
                ConsoleWrite("Couldnt find autosave setting so we use the default (5)");
            }

            if (!(Ssettings.Maxplayers > 1 && Ssettings.Maxplayers <= 16))
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

        public void Shutdownserver()
        {
            SendServerMessage("Server is shutting down");
            disconnectAll();
            shutdownTriggerd = true;
            shutdownTime = DateTime.Now;
            netServer.Shutdown("servershutdown");
        }

        public void Restartserver()
        {
            SendServerMessage("Server is restarting");
            disconnectAll();
            restartTriggered = true;
            restartTime = DateTime.Now;
            netServer.Shutdown("serverrestart");
        }

        public void SendServerMessageToPlayer(string message, NetConnection conn)
        {
            if (conn.Status == NetConnectionStatus.Connected)
            {
                NetBuffer msgBuffer = netServer.CreateBuffer();
                msgBuffer = netServer.CreateBuffer();
                msgBuffer.Write((byte)MineWorldMessage.ChatMessage);
                msgBuffer.Write((byte)ChatMessageType.SayAll);
                msgBuffer.Write(Defines.Sanitize(message));
                
                playerList[conn].AddQueMsg(msgBuffer, NetChannel.ReliableInOrder3);
            }
        }

        public void SendServerMessage(string message)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.ChatMessage);
            msgBuffer.Write((byte)ChatMessageType.SayAll);
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
            msgBuffer.Write((uint)player.Ore);
            msgBuffer.Write((uint)player.Cash);
            msgBuffer.Write((uint)player.Weight);
            msgBuffer.Write((uint)player.OreMax);
            msgBuffer.Write((uint)player.WeightMax);
            msgBuffer.Write((uint)(player.Team == PlayerTeam.Red ? teamOreRed : teamOreBlue));
            msgBuffer.Write((uint)teamCashRed);
            msgBuffer.Write((uint)teamCashBlue);
            player.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder1);
        }

        List<MapSender> mapSendingProgress = new List<MapSender>();

        public void TerminateFinishedThreads()
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
            MapSender ms = new MapSender(client, this, netServer/*,Defines.MAPSIZE/*,playerList[client].compression*/);
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

            msgBuffer.Write((ushort)player.Score / 100);

            foreach (IClient iplayer in playerList.Values)
                //if (netConn.Status == NetConnectionStatus.Connected)
                iplayer.AddQueMsg(msgBuffer, NetChannel.UnreliableInOrder1);
        }

        public void SendSetBeacon(Vector3 position, string text, PlayerTeam team)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.SetBeacon);
            msgBuffer.Write(position);
            msgBuffer.Write(text);
            msgBuffer.Write((byte)team);
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
                msgBuffer.Write((byte)MineWorldMessage.PlayerSetTeam);
                msgBuffer.Write((uint)p.ID);
                msgBuffer.Write((byte)p.Team);
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
                msgBuffer.Write((byte)bPair.Value.Team);
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

            // Send this out just incase someone is joining at the last minute.
            if (winningTeam != PlayerTeam.None)
                BroadcastGameOver();

            // Send out a chat message.
            msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.ChatMessage);
            msgBuffer.Write((byte)ChatMessageType.SayAll);
            msgBuffer.Write(player.Handle + " HAS JOINED THE ADVENTURE!");
            foreach (IClient iplayer in playerList.Values)
                //if (netConn.Status == NetConnectionStatus.Connected)
                iplayer.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder3);
        }

        public void BroadcastGameOver()
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.GameOver);
            msgBuffer.Write((byte)winningTeam);
            foreach (IClient player in playerList.Values)
                //if (netConn.Status == NetConnectionStatus.Connected)
                player.AddQueMsg(msgBuffer, NetChannel.ReliableUnordered);    
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
            msgBuffer.Write((byte)ChatMessageType.SayAll);
            msgBuffer.Write(player.Handle + " " + reason);
            foreach (IClient iplayer in playerList.Values)
                //if (netConn.Status == NetConnectionStatus.Connected)
                iplayer.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder3);
        }

        public void SendPlayerSetTeam(IClient player)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.PlayerSetTeam);
            msgBuffer.Write((uint)player.ID);
            msgBuffer.Write((byte)player.Team);
            foreach (IClient iplayer in playerList.Values)
                //if (netConn.Status == NetConnectionStatus.Connected)
                iplayer.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder2);
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
    }
}
