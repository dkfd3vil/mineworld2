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
                                            if (temphandle.ToLower() == oldPlayer.Handle.ToLower())
                                            {
                                                duplicateNameCount++;
                                                temphandle += "." + duplicateNameCount.ToString();
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
                                                string answer = "";

                                                if(IServer.GetAdmin(player.IP))
                                                {
                                                    string commandstring = Defines.Sanitize(msgBuffer.ReadString());
                                                    String[] splitted = commandstring.Split(new char[] { ' ' });
                                                    splitted[0] = splitted[0].ToLower();

                                                    switch (splitted[0])
                                                    {
                                                        case "/godmode":
                                                            {
                                                                if (player.godmode == false)
                                                                {
                                                                    player.godmode = true;
                                                                    answer = "Godmode enabled";
                                                                }
                                                                else
                                                                {
                                                                    player.godmode = false;
                                                                    answer = "Godmode disabled";
                                                                }
                                                                break;
                                                            }
                                                        case "/stopfluids":
                                                            {
                                                                IServer.StopFluids = true;
                                                                answer = "Stopfluids enabled";
                                                                break;
                                                            }
                                                        case "/startfluids":
                                                            {
                                                                IServer.StopFluids = false;
                                                                answer = "Stopfluids disabled";
                                                                break;
                                                            }
                                                        case "/nocost":
                                                            {
                                                                if (player.nocost == false)
                                                                {
                                                                    player.nocost = true;
                                                                    answer = "Nocost enabled";
                                                                }
                                                                else
                                                                {
                                                                    player.nocost = false;
                                                                    answer = "Nocost disabled";
                                                                }
                                                                break;
                                                            }
                                                        case "/teleportto":
                                                        case "/tpt":
                                                            {
                                                                bool playerfound = false;

                                                                if (splitted.Length > 1)
                                                                {
                                                                    splitted[1] = splitted[1].ToLower();

                                                                    if (player.Handle.ToLower() == splitted[1])
                                                                    {
                                                                        answer = "Cant teleport to yourself";
                                                                        break;
                                                                    }
                                                                    foreach (IClient dummy in IServer.playerList.Values)
                                                                    {
                                                                        if (dummy.Handle.ToLower() == splitted[1])
                                                                        {
                                                                            playerfound = true;
                                                                            if (!dummy.Alive)
                                                                            {
                                                                                answer = "Cant teleport to a dead player";
                                                                                break;
                                                                            }
                                                                            player.Position = dummy.Position;
                                                                            IServer.SendPlayerPosition(player);
                                                                            answer = "Teleporting you to " + dummy.Handle;
                                                                            break;
                                                                        }
                                                                    }
                                                                    if (!playerfound)
                                                                    {
                                                                        answer = "Didnt find the player";
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    answer = "You didnt enter a name";
                                                                }
                                                                break;
                                                            }
                                                        case "/kill":
                                                            {
                                                                bool playerfound = false;

                                                                if (splitted.Length > 1)
                                                                {
                                                                    splitted[1] = splitted[1].ToLower();

                                                                    if (player.Handle.ToLower() == splitted[1])
                                                                    {
                                                                        answer = "Cant kill yourself";
                                                                        break;
                                                                    }
                                                                    foreach (IClient dummy in IServer.playerList.Values)
                                                                    {
                                                                        if (dummy.Handle.ToLower() == splitted[1])
                                                                        {
                                                                            playerfound = true;
                                                                            if (!dummy.Alive)
                                                                            {
                                                                                answer = "Player is already dead";
                                                                                break;
                                                                            }
                                                                            IServer.SendResourceUpdate(dummy);
                                                                            IServer.KillPlayerSpecific(dummy);
                                                                            break;
                                                                        }
                                                                    }
                                                                    if (!playerfound)
                                                                    {
                                                                        answer = "Didnt find the player";
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    answer = "You didnt enter a name";
                                                                }
                                                                break;
                                                            }
                                                        case "/setday":
                                                            {
                                                                IServer.dayManager.Light = 1.0f;
                                                                answer = "Time changed to day";
                                                                break;
                                                            }
                                                        case "/setnight":
                                                            {
                                                                IServer.dayManager.Light = 0.0f;
                                                                answer = "Time changed to night";
                                                                break;
                                                            }
                                                        case "/announce":
                                                            {
                                                                IServer.SendServerMessage(commandstring);
                                                                break;
                                                            }
                                                        case "/restart":
                                                            {
                                                                IServer.Restartserver();
                                                                break;
                                                            }
                                                        case "/shutdown":
                                                            {
                                                                IServer.Shutdownserver();
                                                                break;
                                                            }
                                                        default:
                                                            {
                                                                answer = "Command not regonized";
                                                                break;
                                                            }
                                                    }
                                                    IServer.ConsoleWrite("COMMAND: (" + commandstring + ") has been used by (" + player.Handle + ")"); 
                                                }
                                                else
                                                {
                                                    answer = "You arent a admin";
                                                }

                                                IServer.SendServerMessageToPlayer(answer, player);
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
                                                player.Alive = false;
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
                                                player.HealthMax = 400;
                                                player.Health = player.HealthMax;
                                                IServer.SendResourceUpdate(player);
                                            }
                                            break;

                                        case MineWorldMessage.PlayerSetTeam:
                                            {
                                                PlayerTeam playerTeam = (PlayerTeam)msgBuffer.ReadByte();
                                                IServer.ConsoleWrite("SELECT_TEAM: " + player.Handle + ", " + playerTeam.ToString());
                                                player.Team = playerTeam;
                                                player.Health = 0;
                                                player.Alive = false;
                                                player.Health = 0;
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
                                                string deathMessage = msgBuffer.ReadString();

                                                IServer.SendResourceUpdate(player);
                                                IServer.SendPlayerDead(player);

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
                                                IServer.SendPlayerRespawn(player);//allow this player to instantly respawn
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
                                                        greeting = greeting.Replace("[team]", IServer.playerList[msgSender].Team.ToString());
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
                                                player.Health = player.HealthMax;
                                                player.Alive = true;
                                                IServer.SendResourceUpdate(player);
                                                IServer.SendPlayerAlive(player);
                                            }
                                            break;

                                        case MineWorldMessage.PlayerRespawn:
                                            {
                                                IServer.SendPlayerRespawn(player);//new respawn
                                                break;
                                            }

                                        case MineWorldMessage.PlayerUpdate:
                                            {
                                                player.Position = IServer.Auth_Position(msgBuffer.ReadVector3(), player);
                                                player.Heading = IServer.Auth_Heading(msgBuffer.ReadVector3());
                                                player.Tool = (PlayerTools)msgBuffer.ReadByte();
                                                player.UsingTool = msgBuffer.ReadBoolean();
                                                IServer.SendPlayerUpdate(player);
                                            }
                                            break;

                                        case MineWorldMessage.PlayerUpdate1://minus position
                                            {
                                                player.Heading = IServer.Auth_Heading(msgBuffer.ReadVector3());
                                                player.Tool = (PlayerTools)msgBuffer.ReadByte();
                                                player.UsingTool = msgBuffer.ReadBoolean();
                                                IServer.SendPlayerUpdate(player);
                                                break;
                                            }

                                        case MineWorldMessage.PlayerUpdate2://minus position and heading
                                            {
                                                player.Tool = (PlayerTools)msgBuffer.ReadByte();
                                                player.UsingTool = msgBuffer.ReadBoolean();
                                                IServer.SendPlayerUpdate(player);
                                                break;
                                            }

                                        case MineWorldMessage.PlayerHurt:
                                            {
                                                uint damage = msgBuffer.ReadUInt32();
                                                bool flatdamage = msgBuffer.ReadBoolean();

                                                //If the player has godmode then ignore
                                                if (player.godmode)
                                                {
                                                    break;
                                                }

                                                if (flatdamage)
                                                {
                                                    player.Health = player.Health - damage;
                                                }
                                                else//Then its in procents
                                                {
                                                    if (damage > 100)
                                                    {
                                                        damage = 100;
                                                    }
                                                    player.Health -= (player.HealthMax / 100) * damage;
                                                }

                                                if (player.Health <= 0)
                                                {
                                                    // Reset it back to zero or else the client sees weird stuff
                                                    player.Health = 0;
                                                    IServer.SendResourceUpdate(player);
                                                    IServer.KillPlayerSpecific(player);
                                                }
                                                else
                                                {
                                                    // Let the client know what his new health is
                                                    IServer.SendResourceUpdate(player);
                                                }
                                                break;
                                            }
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
