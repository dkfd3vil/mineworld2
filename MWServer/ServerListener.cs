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
        //private NetMessageType msgType;
        //private NetConnection msgSender;
        //private NetConnection msgSender;

        public ServerListener(NetServer serv,MineWorldServer iserv)
        {
            netServer = serv;
            IServer = iserv;
            // Initialize variables we'll use.
        }

        public void start()
        {
            //NetBuffer msgBuffer = netServer.CreateBuffer();
            int duplicateNameCount = 0;

            while (true)
            {
                //netServer.ReadMessage(
                //netServer.ReadMessage(msgBuffer, out msgType, out msgSender)
                while ((msg = netServer.ReadMessage()) != null)
                {
                    try
                    {
                        switch (msg.MessageType)
                        {
                            //case NetMessageType.ConnectionApproval:
                            case NetIncomingMessageType.ConnectionApproval:
                                {
                                    string temphandle = Defines.Sanitize(msg.ReadString()).Trim();
                                    double authcode = msg.ReadDouble();

                                    if (authcode != Defines.MINEWORLD_VER)
                                    {
                                        IServer.ConsoleWriteError("CONNECTION REJECTED: " + temphandle + " (VERSION WRONG)");
                                        msg.SenderConnection.Deny("versionwrong");
                                        //msgSender.Disapprove("versionwrong");
                                    }
                                    else if (IServer.banList.Contains(msgSender.RemoteEndpoint.Address.ToString()))
                                    {
                                        IServer.ConsoleWriteError("CONNECTION REJECTED: " + temphandle + " (IP BANNED)");
                                        msg.SenderConnection.Deny("banned");
                                        //msgSender.Disapprove("banned");
                                    }
                                    else if (IServer.playerList.Count == IServer.Ssettings.Maxplayers)
                                    {
                                        IServer.ConsoleWriteError("CONNECTION REJECTED: " + temphandle + " (SERVER FULL)");
                                        msg.SenderConnection.Deny("serverfull");
                                        //msgSender.Disapprove("serverfull");
                                    }
                                    else
                                    {
                                        if (temphandle.Length != 0)
                                        {
                                            if (temphandle.ToLower() == "player")
                                            {
                                                IServer.ConsoleWriteError("CONNECTION REJECTED: " + temphandle + " (CHANGE NAME)");
                                                msg.SenderConnection.Deny("changename");
                                                //msgSender.Disapprove("changename");
                                            }
                                            else
                                            {
                                                foreach(string name in IServer.bannednames)
                                                {
                                                    if (name.ToLower() == temphandle.ToLower())
                                                    {
                                                        IServer.ConsoleWriteError("CONNECTION REJECTED: " + temphandle + " (BANNED NAME)");
                                                        msg.SenderConnection.Deny("bannedname");
                                                        //msgSender.Disapprove("bannedname");
                                                    }
                                                }
                                                    }
                                                }
                                        else
                                        {
                                            IServer.ConsoleWriteError("CONNECTION REJECTED: (NO NAME)");
                                            msg.SenderConnection.Deny("noname");
                                            //msgSender.Disapprove("noname");
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
                                        //netServer.SanityCheck(msg.SenderConnection);
                                        msg.SenderConnection.Approve();
                                        //msgSender.Approve();
                                        // Dont bother if the server isnt public
                                        if(IServer.Ssettings.Public == true)
                                        {
                                            IServer.updateMasterServer();
                                        }
                                    }
                                }
                                break;
                            case NetIncomingMessageType.StatusChanged:
                                {
                                    //Todo need this or ?
                                    NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                                    if (!IServer.playerList.ContainsKey(msg.SenderConnection))
                                    {
                                        break;
                                    }

                                    ServerPlayer player = IServer.playerList[msgSender];

                                    //IClient player = IServer.playerList[msgSender];

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
                                                ChatMessageType chatType = (ChatMessageType)msgBuffer.ReadByte();
                                                string chatString = Defines.Sanitize(msgBuffer.ReadString());
                                                string author = Defines.Sanitize(msgBuffer.ReadString());

                                                // Construct the message packet.
                                                NetBuffer chatPacket = netServer.CreateBuffer();
                                                chatPacket.Write((byte)MineWorldMessage.ChatMessage);
                                                chatPacket.Write((byte)ChatMessageType.Say);
                                                chatPacket.Write(chatString);
                                                chatPacket.Write(author);

                                                // Send the packet to people who should recieve it.
                                                foreach (ServerPlayer p in IServer.playerList.Values)
                                                {
                                                    netServer.SendMsg(chatPacket, p.NetConn, NetChannel.ReliableInOrder3);
                                                }
                                            }
                                            break;

                                        case MineWorldMessage.UseTool:
                                            {
                                                KeyBoardButtons key = (KeyBoardButtons)msgBuffer.ReadByte();
                                                Vector3 playerPosition = msgBuffer.ReadVector3();
                                                Vector3 playerHeading = msgBuffer.ReadVector3();
                                                BlockType blockType = (BlockType)msgBuffer.ReadByte();

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
                                                string deathMessage = msgBuffer.ReadString();

                                                IServer.SendHealthUpdate(player);
                                                IServer.SendPlayerDead(player);

                                                if (deathMessage != "")
                                                {
                                                    msgBuffer = netServer.CreateBuffer();
                                                    msgBuffer.Write((byte)MineWorldMessage.ChatMessage);
                                                    msgBuffer.Write((byte)ChatMessageType.Say);
                                                    msgBuffer.Write(player.Name + " " + deathMessage);
                                                    foreach (ServerPlayer iplayer in IServer.playerList.Values)
                                                    {
                                                        netServer.SendMsg(msgBuffer, iplayer.NetConn, NetChannel.ReliableInOrder3);
                                                    }
                                                }
                                                IServer.SendPlayerRespawn(player);//allow this player to instantly respawn
                                            }
                                            break;

                                        case MineWorldMessage.PlayerAlive:
                                            {
                                                if (IServer.toGreet.Contains(msgSender))
                                                {
                                                    if (IServer.Ssettings.MOTD != "")
                                                    {
                                                        string greeting = IServer.Ssettings.MOTD;
                                                        greeting = greeting.Replace("[name]", IServer.playerList[msgSender].Name);
                                                        NetBuffer greetBuffer = netServer.CreateBuffer();
                                                        greetBuffer.Write((byte)MineWorldMessage.ChatMessage);
                                                        greetBuffer.Write((byte)ChatMessageType.SayServer);
                                                        greetBuffer.Write(Defines.Sanitize(greeting));
                                                        netServer.SendMsg(greetBuffer, msgSender, NetChannel.ReliableInOrder3);
                                                    }
                                                    IServer.toGreet.Remove(msgSender);
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
                                                player.Position = IServer.Auth_Position(msgBuffer.ReadVector3(), player);
                                                player.Heading = IServer.Auth_Heading(msgBuffer.ReadVector3());
                                                IServer.SendPlayerUpdate(player);
                                            }
                                            break;

                                        case MineWorldMessage.PlayerUpdate1://minus position
                                            {
                                                player.Heading = IServer.Auth_Heading(msgBuffer.ReadVector3());
                                                IServer.SendPlayerUpdate(player);
                                                break;
                                            }

                                        case MineWorldMessage.PlayerHurt:
                                            {
                                                int damage = msgBuffer.ReadInt32();
                                                bool flatdamage = msgBuffer.ReadBoolean();

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

                                                /*
                                                if (player.Health <= 0)
                                                {
                                                    // Reset it back to zero or else the client sees weird stuff
                                                    // Player is dead stop health regen
                                                    player.Health = 0;
                                                    IServer.SendHealthUpdate(player);
                                                    IServer.KillPlayerSpecific(player);
                                                }
                                                else
                                                {
                                                    // Let the client know what his new health is
                                                    IServer.SendHealthUpdate(player);
                                                }
                                                 */
                                                break;
                                            }

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
                Thread.Sleep(25);
            }
        }

    }
}
