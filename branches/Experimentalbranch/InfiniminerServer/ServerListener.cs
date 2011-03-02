using System;
using System.Collections.Generic;
using System.Text;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;

namespace MineWorld
{
    class ServerListener
    {
        private MineWorldNetServer netServer;
        private MineWorldServer IServer;
        //private NetBuffer msgBuffer;
        private NetMessageType msgType;
        private NetConnection msgSender;
        uint duplicateNameCount = 0;

        public ServerListener(MineWorldNetServer serv,MineWorldServer iserv)
        {
            netServer = serv;
            IServer = iserv;
            // Initialize variables we'll use.
        }

        public void start()
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            while (true)
            {
                while (netServer.ReadMessage(msgBuffer, out msgType, out msgSender))
                {
                    try
                    {
                        switch (msgType)
                        {
                            case NetMessageType.ConnectionApproval:
                                {
                                    string temphandle = Defines.Sanitize(msgBuffer.ReadString()).Trim();
                                    double authcode = msgBuffer.ReadDouble();

                                    if (authcode != Defines.MINEWORLD_VER)
                                    {
                                        IServer.ConsoleWrite("CONNECTION REJECTED: " + temphandle + " (VERSION WRONG)");
                                        msgSender.Disapprove("versionwrong");
                                    }
                                    else if (IServer.banList.Contains(msgSender.RemoteEndpoint.Address.ToString()))
                                    {
                                        IServer.ConsoleWrite("CONNECTION REJECTED: " + temphandle + " (IP BANNED)");
                                        msgSender.Disapprove("banned");
                                    }
                                    else if (IServer.playerList.Count == IServer.Ssettings.Maxplayers)
                                    {
                                        IServer.ConsoleWrite("CONNECTION REJECTED: " + temphandle + " (SERVER FULL)");
                                        msgSender.Disapprove("serverfull");
                                    }
                                    else
                                    {
                                        if (temphandle.Length != 0)
                                        {
                                            if (temphandle.ToLower() == "player")
                                            {
                                                IServer.ConsoleWrite("CONNECTION REJECTED: " + temphandle + " (CHANGE NAME)");
                                                msgSender.Disapprove("changename");
                                            }
                                            else
                                            {
                                                foreach(string name in IServer.bannednames)
                                                {
                                                    if (name.ToLower() == temphandle.ToLower())
                                                    {
                                                        IServer.ConsoleWrite("CONNECTION REJECTED: " + temphandle + " (BANNED NAME)");
                                                        msgSender.Disapprove("bannedname");
                                                    }
                                                }
                                                    }
                                                }
                                        else
                                        {
                                            IServer.ConsoleWrite("CONNECTION REJECTED: (NO NAME)");
                                            msgSender.Disapprove("noname");
                                        }

                                        IClient newPlayer = new IClient(msgSender, null);

                                        foreach (Player oldPlayer in IServer.playerList.Values)
                                        {
                                            if (newPlayer.Handle.ToLower() == oldPlayer.Handle.ToLower())
                                            {
                                                duplicateNameCount++;
                                                newPlayer.Handle += "." + duplicateNameCount.ToString();
                                                break;
                                            }
                                        }

                                        newPlayer.Handle = temphandle;
                                        IServer.playerList[msgSender] = newPlayer;
                                        System.Threading.Thread SenderThread = new System.Threading.Thread(new System.Threading.ThreadStart(newPlayer.start));
                                        SenderThread.Start();
                                        IServer.toGreet.Add(msgSender);
                                        this.netServer.SanityCheck(msgSender);
                                        msgSender.Approve();
                                        // Dont bother if the server isnt public
                                        if(IServer.Ssettings.Public == true)
                                        {
                                            IServer.updateMasterServer();
                                        }
                                    }
                                }
                                break;

                            case NetMessageType.StatusChanged:
                                {
                                    if (!IServer.playerList.ContainsKey(msgSender))
                                    {
                                        break;
                                    }

                                    IClient player = IServer.playerList[msgSender];

                                    if (msgSender.Status == NetConnectionStatus.Connected)
                                    {
                                        IServer.ConsoleWrite("CONNECT: " + IServer.playerList[msgSender].Handle + " ( " + IServer.playerList[msgSender].IP + " )");
                                        IServer.SendCurrentMap(msgSender);
                                        IServer.SendPlayerJoined(player);
                                    }
                                    else if (msgSender.Status == NetConnectionStatus.Disconnected)
                                    {
                                        IServer.ConsoleWrite("DISCONNECT: " + IServer.playerList[msgSender].Handle);
                                        IServer.SendPlayerLeft(player, player.Kicked ? "WAS KICKED FROM THE GAME!" : "HAS ABANDONED THEIR DUTIES!");

                                        if (IServer.playerList.ContainsKey(msgSender))
                                        {
                                            IServer.playerList.Remove(msgSender);
                                        }
                                    }
                                    if (IServer.Ssettings.Public == true)
                                    {
                                        IServer.updateMasterServer();
                                    }
                                }
                                break;

                            case NetMessageType.Data:
                                {
                                    if (!IServer.playerList.ContainsKey(msgSender))
                                    {
                                        break;
                                    }

                                    IClient player = IServer.playerList[msgSender];

                                    // If kicked we dont care anymore what he sends
                                    if (player.Kicked == true)
                                    {
                                        break;
                                    }

                                    MineWorldMessage dataType = (MineWorldMessage)msgBuffer.ReadByte();
                                    switch (dataType)
                                    {
                                        case MineWorldMessage.PlayerCommand:
                                            {
                                                PlayerCommands command = PlayerCommands.None;
                                                UInt32 argplayer = 0;

                                                if(IServer.GetAdmin(player.IP))
                                                {
                                                    string commandstring = Defines.Sanitize(msgBuffer.ReadString());
                                                    String[] splitted = commandstring.Split(new char[] { ' ' });
                                                    splitted[0] = splitted[0].ToLower();

                                                    switch (splitted[0])
                                                    {
                                                        case "/godmode":
                                                            {
                                                                //Redo this :S
                                                                command = PlayerCommands.Godmode;
                                                                break;
                                                            }
                                                        case "/stopfluids":
                                                            {
                                                                command = PlayerCommands.Stopfluids;
                                                                IServer.Ssettings.StopFluids = true;
                                                                break;
                                                            }
                                                        case "/startfluids":
                                                            {
                                                                command = PlayerCommands.Startfluids;
                                                                IServer.Ssettings.StopFluids = false;
                                                                break;
                                                            }
                                                        case "/nocost":
                                                            {
                                                                command = PlayerCommands.Nocost;
                                                                if (player.nocost == false)
                                                                {
                                                                    player.nocost = true;
                                                                }
                                                                else
                                                                {
                                                                    player.nocost = false;
                                                                }
                                                                break;
                                                            }
                                                        case "/teleportto":
                                                        case "/tpt":
                                                            {
                                                                if (splitted.Length > 1)
                                                                {
                                                                    command = PlayerCommands.Teleportto;

                                                                    foreach (IClient dummy in IServer.playerList.Values)
                                                                    {
                                                                        if (dummy.Handle.ToLower() == splitted[1])
                                                                        {
                                                                            //We found the player woot
                                                                            argplayer = dummy.ID;
                                                                            break;
                                                                        }
                                                                    }
                                                                }
                                                                break;
                                                            }
                                                        case "/announce":
                                                            {
                                                                command = PlayerCommands.Announce;
                                                                IServer.ProcessCommand(commandstring, true);
                                                                break;
                                                            }
                                                        case "/restart":
                                                            {
                                                                command = PlayerCommands.Restart;
                                                                IServer.ProcessCommand(commandstring,true);
                                                                break;
                                                            }
                                                        default:
                                                            {
                                                                command = PlayerCommands.None;
                                                                break;
                                                            }
                                                    }
                                                    IServer.ConsoleWrite("COMMAND: (" + commandstring + ") has been used by (" + player.Handle + ")"); 
                                                }
                                                else
                                                {
                                                    command = PlayerCommands.Noadmin;
                                                }

                                                NetBuffer chatPacket = netServer.CreateBuffer();
                                                chatPacket.Write((byte)MineWorldMessage.PlayerCommandEnable);
                                                chatPacket.Write((byte)command);
                                                chatPacket.Write(argplayer);
                                                player.AddQueMsg(chatPacket, NetChannel.ReliableInOrder6);
                                                break;
                                            }
                                        case MineWorldMessage.ChatMessage:
                                            {
                                                // Read the data from the packet.
                                                ChatMessageType chatType = (ChatMessageType)msgBuffer.ReadByte();
                                                string chatString = Defines.Sanitize(msgBuffer.ReadString());
                                                //IServer.ConsoleWrite("CHAT: (" + player.Handle + ") " + chatString);

                                                // Append identifier information.
                                                if (chatType == ChatMessageType.SayAll)
                                                    chatString = player.Handle + " (ALL): " + chatString;
                                                else
                                                    chatString = player.Handle + " (TEAM): " + chatString;

                                                // Construct the message packet.
                                                NetBuffer chatPacket = netServer.CreateBuffer();
                                                chatPacket.Write((byte)MineWorldMessage.ChatMessage);
                                                chatPacket.Write((byte)((player.Team == PlayerTeam.Red) ? ChatMessageType.SayRedTeam : ChatMessageType.SayBlueTeam));
                                                chatPacket.Write(chatString);

                                                // Send the packet to people who should recieve it.
                                                foreach (IClient p in IServer.playerList.Values)
                                                {
                                                    if (chatType == ChatMessageType.SayAll ||
                                                    chatType == ChatMessageType.SayBlueTeam && p.Team == PlayerTeam.Blue ||
                                                    chatType == ChatMessageType.SayRedTeam && p.Team == PlayerTeam.Red)
                                                    //if (p.NetConn.Status == NetConnectionStatus.Connected)
                                                    p.AddQueMsg(chatPacket, NetChannel.ReliableInOrder3);
                                                }
                                            }
                                            break;

                                        case MineWorldMessage.UseTool:
                                            {
                                                Vector3 playerPosition = msgBuffer.ReadVector3();
                                                Vector3 playerHeading = msgBuffer.ReadVector3();
                                                PlayerTools playerTool = (PlayerTools)msgBuffer.ReadByte();
                                                BlockType blockType = (BlockType)msgBuffer.ReadByte();
                                                switch (playerTool)
                                                {
                                                    case PlayerTools.Pickaxe:
                                                        IServer.UsePickaxe(player, playerPosition, playerHeading);
                                                        break;
                                                    case PlayerTools.ConstructionGun:
                                                        IServer.UseConstructionGun(player, playerPosition, playerHeading, blockType);
                                                        break;
                                                    case PlayerTools.DeconstructionGun:
                                                        IServer.UseDeconstructionGun(player, playerPosition, playerHeading);
                                                        break;
                                                    case PlayerTools.ProspectingRadar:
                                                        IServer.UseSignPainter(player, playerPosition, playerHeading);
                                                        break;
                                                    case PlayerTools.Detonator:
                                                        IServer.UseDetonator(player);
                                                        break;
                                                }
                                            }
                                            break;

                                        case MineWorldMessage.SelectClass:
                                            {
                                                PlayerClass playerClass = (PlayerClass)msgBuffer.ReadByte();
                                                IServer.ConsoleWrite("SELECT_CLASS: " + player.Handle + ", " + playerClass.ToString());
                                                switch (playerClass)
                                                {
                                                    case PlayerClass.Engineer:
                                                        player.OreMax = 350;
                                                        player.WeightMax = 4;
                                                        break;
                                                    case PlayerClass.Miner:
                                                        player.OreMax = 200;
                                                        player.WeightMax = 8;
                                                        break;
                                                    case PlayerClass.Prospector:
                                                        player.OreMax = 200;
                                                        player.WeightMax = 4;
                                                        break;
                                                    case PlayerClass.Sapper:
                                                        player.OreMax = 200;
                                                        player.WeightMax = 4;
                                                        break;
                                                }
                                                IServer.SendResourceUpdate(player);
                                            }
                                            break;

                                        case MineWorldMessage.PlayerSetTeam:
                                            {
                                                PlayerTeam playerTeam = (PlayerTeam)msgBuffer.ReadByte();
                                                IServer.ConsoleWrite("SELECT_TEAM: " + player.Handle + ", " + playerTeam.ToString());
                                                player.Team = playerTeam;
                                                IServer.SendResourceUpdate(player);
                                                IServer.SendPlayerSetTeam(player);
                                            }
                                            break;

                                        case MineWorldMessage.PlayerDead:
                                            {
                                                IServer.ConsoleWrite("PLAYER_DEAD: " + player.Handle);
                                                player.Ore = 0;
                                                player.Cash = 0;
                                                player.Weight = 0;
                                                player.Alive = false;
                                                IServer.SendResourceUpdate(player);
                                                IServer.SendPlayerDead(player);

                                                string deathMessage = msgBuffer.ReadString();
                                                if (deathMessage != "")
                                                {
                                                    msgBuffer = netServer.CreateBuffer();
                                                    msgBuffer.Write((byte)MineWorldMessage.ChatMessage);
                                                    msgBuffer.Write((byte)(player.Team == PlayerTeam.Red ? ChatMessageType.SayRedTeam : ChatMessageType.SayBlueTeam));
                                                    msgBuffer.Write(player.Handle + " " + deathMessage);
                                                    foreach (IClient iplayer in IServer.playerList.Values)
                                                        //if (netConn.Status == NetConnectionStatus.Connected)
                                                        iplayer.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder3);
                                                }
                                            }
                                            break;

                                        case MineWorldMessage.PlayerAlive:
                                            {
                                                if (IServer.toGreet.Contains(msgSender))
                                                {
                                                    //string greeting = "Hello [name] welcome";
                                                    if (IServer.Ssettings.MOTD != "")
                                                    {
                                                        string greeting = IServer.Ssettings.MOTD;
                                                        greeting = greeting.Replace("[name]", IServer.playerList[msgSender].Handle);
                                                        NetBuffer greetBuffer = netServer.CreateBuffer();
                                                        greetBuffer.Write((byte)MineWorldMessage.ChatMessage);
                                                        greetBuffer.Write((byte)ChatMessageType.SayAll);
                                                        greetBuffer.Write(Defines.Sanitize(greeting));
                                                        netServer.SendMessage(greetBuffer, msgSender, NetChannel.ReliableInOrder3);
                                                    }
                                                    IServer.toGreet.Remove(msgSender);
                                                }
                                                IServer.ConsoleWrite("PLAYER_ALIVE: " + player.Handle);
                                                player.Ore = 0;
                                                player.Cash = 0;
                                                player.Weight = 0;
                                                player.Alive = true;
                                                IServer.SendResourceUpdate(player);
                                                IServer.SendPlayerAlive(player);
                                            }
                                            break;

                                        case MineWorldMessage.PlayerUpdate:
                                            {
                                                player.Position = msgBuffer.ReadVector3();
                                                player.Heading = msgBuffer.ReadVector3();
                                                player.Tool = (PlayerTools)msgBuffer.ReadByte();
                                                player.UsingTool = msgBuffer.ReadBoolean();
                                                IServer.SendPlayerUpdate(player);
                                            }
                                            break;

                                        case MineWorldMessage.DepositOre:
                                            {
                                                IServer.DepositOre(player);
                                                foreach (IClient p in IServer.playerList.Values)
                                                    IServer.SendResourceUpdate(p);
                                            }
                                            break;

                                        case MineWorldMessage.DepositCash:
                                            {
                                                //IServer.DepositCash(player);
                                                IServer.DepositForPlayers();
                                                foreach (IClient p in IServer.playerList.Values)
                                                    IServer.SendResourceUpdate(p);
                                            }
                                            break;

                                        case MineWorldMessage.WithdrawOre:
                                            {
                                                IServer.WithdrawOre(player);
                                                foreach (IClient p in IServer.playerList.Values)
                                                    IServer.SendResourceUpdate(p);
                                            }
                                            break;

                                        case MineWorldMessage.PlayerPing:
                                            {
                                                IServer.SendPlayerPing((uint)msgBuffer.ReadInt32());
                                            }
                                            break;

                                        case MineWorldMessage.PlaySound:
                                            {
                                                MineWorldSound sound = (MineWorldSound)msgBuffer.ReadByte();
                                                Vector3 position = msgBuffer.ReadVector3();
                                                IServer.PlaySound(sound, position);
                                            }
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                    catch { }
                }
                System.Threading.Thread.Sleep(25);
            }
        }

    }
}
