using System;
using System.Collections.Generic;
using System.Text;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;
using System.Threading;

namespace MineWorld
{
    class ServerListener
    {
        private NetServer netServer;
        private MineWorldServer IServer;
        private NetIncomingMessage msg;

        public ServerListener(NetServer serv,MineWorldServer iserv)
        {
            netServer = serv;
            IServer = iserv;
        }

        public void start()
        {
            int duplicateNameCount = 0;

            while (true)
            {
                while ((msg = netServer.ReadMessage()) != null)
                {
                    try
                    {
                        switch (msg.MessageType)
                        {
                            case NetIncomingMessageType.ConnectionApproval:
                                {
                                    string temphandle = Defines.Sanitize(msg.ReadString()).Trim();
                                    double authcode = msg.ReadDouble();

                                    if (authcode != Defines.MINEWORLD_VER)
                                    {
                                        IServer.ConsoleWriteError("CONNECTION REJECTED: " + temphandle + " (VERSION WRONG)");
                                        msg.SenderConnection.Deny("versionwrong");
                                    }
                                    else if (IServer.bannedips.Contains(msg.SenderEndpoint.ToString()))
                                    {
                                        IServer.ConsoleWriteError("CONNECTION REJECTED: " + temphandle + " (IP BANNED)");
                                        msg.SenderConnection.Deny("banned");
                                    }
                                    else if (IServer.playerList.Count == IServer.Ssettings.Maxplayers)
                                    {
                                        IServer.ConsoleWriteError("CONNECTION REJECTED: " + temphandle + " (SERVER FULL)");
                                        msg.SenderConnection.Deny("serverfull");
                                    }
                                    else
                                    {
                                        if (temphandle.Length != 0)
                                        {
                                            if (temphandle.ToLower() == "player")
                                            {
                                                IServer.ConsoleWriteError("CONNECTION REJECTED: " + temphandle + " (CHANGE NAME)");
                                                msg.SenderConnection.Deny("changename");
                                            }
                                            else
                                            {
                                                foreach(string name in IServer.bannednames)
                                                {
                                                    if (name.ToLower() == temphandle.ToLower())
                                                    {
                                                        IServer.ConsoleWriteError("CONNECTION REJECTED: " + temphandle + " (BANNED NAME)");
                                                        msg.SenderConnection.Deny("bannedname");
                                                    }
                                                }
                                                    }
                                                }
                                        else
                                        {
                                            IServer.ConsoleWriteError("CONNECTION REJECTED: (NO NAME)");
                                            msg.SenderConnection.Deny("noname");
                                        }

                                        ServerPlayer newPlayer = new ServerPlayer(msg.SenderConnection);

                                        foreach (ServerPlayer oldPlayer in IServer.playerList.Values)
                                        {
                                            if (temphandle.ToLower() == oldPlayer.Name.ToLower())
                                            {
                                                duplicateNameCount++;
                                                temphandle += "." + duplicateNameCount.ToString();
                                                break;
                                            }
                                        }

                                        newPlayer.Name = temphandle;
                                        IServer.playerList[msg.SenderConnection] = newPlayer;
                                        Thread SenderThread = new Thread(new ThreadStart(newPlayer.Start));
                                        SenderThread.Start();
                                        IServer.toGreet.Add(msg.SenderConnection);
                                        msg.SenderConnection.Approve();
                                        // Dont bother if the server isnt public
                                        if(IServer.Ssettings.Public == true)
                                        {
                                            IServer.updateMasterServer();
                                        }
                                    }
                                }
                                break;
                            case NetIncomingMessageType.DiscoveryRequest:
                                break;
                            case NetIncomingMessageType.StatusChanged:
                                {
                                    NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                                    if (!IServer.playerList.ContainsKey(msg.SenderConnection))
                                    {
                                        break;
                                    }

                                    ServerPlayer player = IServer.playerList[msg.SenderConnection];

                                    if (status == NetConnectionStatus.Connected)
                                    {
                                        IServer.ConsoleWrite("CONNECT: " + IServer.playerList[msg.SenderConnection].Name + " ( " + IServer.playerList[msg.SenderConnection].IP + " )");
                                        IServer.SendCurrentMap(msg.SenderConnection);
                                        IServer.SendPlayerJoined(player);
                                    }
                                    else if (status == NetConnectionStatus.Disconnected)
                                    {
                                        IServer.ConsoleWrite("DISCONNECT: " + IServer.playerList[msg.SenderConnection].Name);
                                        IServer.SendPlayerLeft(player, player.Kicked ? "WAS KICKED FROM THE GAME!" : "HAS ABANDONED THEIR DUTIES!");

                                        if (IServer.playerList.ContainsKey(msg.SenderConnection))
                                        {
                                            IServer.playerList.Remove(msg.SenderConnection);
                                        }
                                    }
                                    if (IServer.Ssettings.Public == true)
                                    {
                                        IServer.updateMasterServer();
                                    }
                                }
                                break;

                            case NetIncomingMessageType.Data:
                                {
                                    if (!IServer.playerList.ContainsKey(msg.SenderConnection))
                                    {
                                        break;
                                    }

                                    ServerPlayer player = IServer.playerList[msg.SenderConnection];

                                    // If kicked we dont care anymore what he sends
                                    if (player.Kicked == true)
                                    {
                                        break;
                                    }

                                    MineWorldMessage dataType = (MineWorldMessage)msg.ReadByte();
                                    switch (dataType)
                                    {
                                        case MineWorldMessage.PlayerCommand:
                                            {
                                                string answer = "";

                                                if(IServer.GetAdmin(player.IP))
                                                {
                                                    string commandstring = Defines.Sanitize(msg.ReadString());
                                                    string[] splitted = commandstring.Split(new char[] { ' ' });
                                                    splitted[0] = splitted[0].ToLower();

                                                    switch (splitted[0])
                                                    {
                                                        case "/godmode":
                                                            {
                                                                if (player.Godmode == false)
                                                                {
                                                                    player.Godmode = true;
                                                                    answer = "Godmode enabled";
                                                                }
                                                                else
                                                                {
                                                                    player.Godmode = false;
                                                                    answer = "Godmode disabled";
                                                                }
                                                                break;
                                                            }
                                                        case "/stopfluids":
                                                            {
                                                                IServer.SEsettings.Stopfluids = true;
                                                                answer = "Stopfluids enabled";
                                                                break;
                                                            }
                                                        case "/startfluids":
                                                            {
                                                                IServer.SEsettings.Stopfluids = false;
                                                                answer = "Stopfluids disabled";
                                                                break;
                                                            }
                                                        case "/teleportto":
                                                        case "/tpt":
                                                            {
                                                                bool playerfound = false;

                                                                if (splitted.Length > 1)
                                                                {
                                                                    splitted[1] = splitted[1].ToLower();

                                                                    if (player.Name.ToLower() == splitted[1])
                                                                    {
                                                                        answer = "Cant teleport to yourself";
                                                                        break;
                                                                    }
                                                                    foreach (ServerPlayer dummy in IServer.playerList.Values)
                                                                    {
                                                                        if (dummy.Name.ToLower() == splitted[1])
                                                                        {
                                                                            playerfound = true;
                                                                            if (!dummy.Alive)
                                                                            {
                                                                                answer = "Cant teleport to a dead player";
                                                                                break;
                                                                            }
                                                                            player.Position = dummy.Position;
                                                                            IServer.SendPlayerPosition(player);
                                                                            answer = "Teleporting you to " + dummy.Name;
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

                                                                    if (player.Name.ToLower() == splitted[1])
                                                                    {
                                                                        answer = "Cant kill yourself";
                                                                        break;
                                                                    }
                                                                    foreach (ServerPlayer dummy in IServer.playerList.Values)
                                                                    {
                                                                        if (dummy.Name.ToLower() == splitted[1])
                                                                        {
                                                                            playerfound = true;
                                                                            if (!dummy.Alive)
                                                                            {
                                                                                answer = "Player is already dead";
                                                                                break;
                                                                            }
                                                                            IServer.SendHealthUpdate(dummy);
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
                                                                IServer.dayManager.SetDay();
                                                                answer = "Time changed to day";
                                                                break;
                                                            }
                                                        case "/setnight":
                                                            {
                                                                IServer.dayManager.SetNight();
                                                                answer = "Time changed to night";
                                                                break;
                                                            }
                                                        case "/announce":
                                                            {
                                                                IServer.SendServerWideMessage(commandstring);
                                                                break;
                                                            }
                                                        case "/restart":
                                                            {
                                                                if (splitted.Length > 1)
                                                                {
                                                                    splitted[1] = splitted[1].ToLower();
                                                                    IServer.Restartserver(int.Parse(splitted[1]));
                                                                }
                                                                else
                                                                {
                                                                    IServer.Restartserver(0);
                                                                }
                                                                break;
                                                            }
                                                        case "/shutdown":
                                                            {
                                                                if (splitted.Length > 1)
                                                                {
                                                                    splitted[1] = splitted[1].ToLower();
                                                                    IServer.Shutdownserver(int.Parse(splitted[1]));
                                                                }
                                                                else
                                                                {
                                                                    IServer.Shutdownserver(0);
                                                                }
                                                                break;
                                                            }
                                                        default:
                                                            {
                                                                answer = "Command not regonized";
                                                                break;
                                                            }
                                                    }
                                                    IServer.ConsoleWrite("COMMAND: (" + commandstring + ") has been used by (" + player.Name + ")"); 
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
                                                ChatMessageType chatType = (ChatMessageType)msg.ReadByte();
                                                string chatString = Defines.Sanitize(msg.ReadString());
                                                string author = Defines.Sanitize(msg.ReadString());

                                                // Construct the message packet.
                                                NetOutgoingMessage chat = netServer.CreateMessage();
                                                chat.Write((byte)MineWorldMessage.ChatMessage);
                                                chat.Write((byte)ChatMessageType.Say);
                                                chat.Write(chatString);
                                                chat.Write(author);

                                                // Send the packet to people who should recieve it.
                                                foreach (ServerPlayer p in IServer.playerList.Values)
                                                {
                                                    netServer.SendMessage(chat, p.NetConn, NetDeliveryMethod.ReliableOrdered);
                                                }
                                            }
                                            break;

                                        case MineWorldMessage.UseTool:
                                            {
                                                KeyBoardButtons key = (KeyBoardButtons)msg.ReadByte();
                                                Vector3 playerPosition = msg.ReadVector3();
                                                Vector3 playerHeading = msg.ReadVector3();
                                                BlockType blockType = (BlockType)msg.ReadByte();

                                                switch (key)
                                                {
                                                    case KeyBoardButtons.AltFire:
                                                        IServer.RemoveBlock(player, playerPosition, playerHeading);
                                                        break;
                                                    case KeyBoardButtons.Fire:
                                                        IServer.PlaceBlock(player, playerPosition, playerHeading, blockType);
                                                        break;
                                                }
                                            }
                                            break;

                                        case MineWorldMessage.PlayerDead:
                                            {
                                                IServer.ConsoleWrite("PLAYER_DEAD: " + player.Name);
                                                player.Alive = false;
                                                string deathMessage = msg.ReadString();

                                                IServer.SendHealthUpdate(player);
                                                IServer.SendPlayerDead(player);

                                                if (deathMessage != "")
                                                {
                                                    NetOutgoingMessage packet = netServer.CreateMessage();
                                                    packet.Write((byte)MineWorldMessage.ChatMessage);
                                                    packet.Write((byte)ChatMessageType.Say);
                                                    packet.Write(player.Name + " " + deathMessage);
                                                    foreach (ServerPlayer iplayer in IServer.playerList.Values)
                                                    {
                                                        netServer.SendMessage(packet, iplayer.NetConn, NetDeliveryMethod.ReliableOrdered);
                                                    }
                                                }
                                                IServer.SendPlayerRespawn(player);//allow this player to instantly respawn
                                            }
                                            break;

                                        case MineWorldMessage.PlayerAlive:
                                            {
                                                if (IServer.toGreet.Contains(msg.SenderConnection))
                                                {
                                                    if (IServer.SEsettings.MOTD != "")
                                                    {
                                                        string greeting = IServer.SEsettings.MOTD;
                                                        greeting = greeting.Replace("[name]", IServer.playerList[msg.SenderConnection].Name);
                                                        NetOutgoingMessage greet = netServer.CreateMessage();
                                                        greet.Write((byte)MineWorldMessage.ChatMessage);
                                                        greet.Write((byte)ChatMessageType.SayServer);
                                                        greet.Write(Defines.Sanitize(greeting));
                                                        netServer.SendMessage(greet, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                                    }
                                                    IServer.toGreet.Remove(msg.SenderConnection);
                                                }
                                                IServer.ConsoleWrite("PLAYER_ALIVE: " + player.Name);
                                                player.Health = player.HealthMax;
                                                player.Alive = true;
                                                IServer.SendHealthUpdate(player);
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
                                                player.Position = IServer.Auth_Position(msg.ReadVector3(), player);
                                                player.Heading = IServer.Auth_Heading(msg.ReadVector3());
                                                IServer.SendPlayerUpdate(player);
                                            }
                                            break;

                                        case MineWorldMessage.PlayerUpdate1://minus position
                                            {
                                                player.Heading = IServer.Auth_Heading(msg.ReadVector3());
                                                IServer.SendPlayerUpdate(player);
                                                break;
                                            }

                                        case MineWorldMessage.PlayerHurt:
                                            {
                                                int damage = msg.ReadInt32();
                                                bool flatdamage = msg.ReadBoolean();

                                                //If the player has godmode then ignore
                                                if (player.Godmode)
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
                                                break;
                                            }

                                        case MineWorldMessage.PlaySound:
                                            {
                                                MineWorldSound sound = (MineWorldSound)msg.ReadByte();
                                                Vector3 position = msg.ReadVector3();
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
                //Dont recycle a null message
                if (msg != null)
                {
                    netServer.Recycle(msg);
                }
                Thread.Sleep(25);
            }
        }

    }
}
