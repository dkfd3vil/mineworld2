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
        private MineWorldNetServer netServer;
        private MineWorldServer IServer;
        private NetMessageType msgType;
        private NetConnection msgSender;

        public ServerListener(MineWorldNetServer serv,MineWorldServer iserv)
        {
            netServer = serv;
            IServer = iserv;
            // Initialize variables we'll use.
        }

        public void start()
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            int duplicateNameCount = 0;

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
                                        IServer.ConsoleWriteError("CONNECTION REJECTED: " + temphandle + " (VERSION WRONG)");
                                        msgSender.Disapprove("versionwrong");
                                    }
                                    else if (IServer.banList.Contains(msgSender.RemoteEndpoint.Address.ToString()))
                                    {
                                        IServer.ConsoleWriteError("CONNECTION REJECTED: " + temphandle + " (IP BANNED)");
                                        msgSender.Disapprove("banned");
                                    }
                                    else if (IServer.playerList.Count == IServer.Ssettings.Maxplayers)
                                    {
                                        IServer.ConsoleWriteError("CONNECTION REJECTED: " + temphandle + " (SERVER FULL)");
                                        msgSender.Disapprove("serverfull");
                                    }
                                    else
                                    {
                                        if (temphandle.Length != 0)
                                        {
                                            if (temphandle.ToLower() == "player")
                                            {
                                                IServer.ConsoleWriteError("CONNECTION REJECTED: " + temphandle + " (CHANGE NAME)");
                                                msgSender.Disapprove("changename");
                                            }
                                            else
                                            {
                                                foreach(string name in IServer.bannednames)
                                                {
                                                    if (name.ToLower() == temphandle.ToLower())
                                                    {
                                                        IServer.ConsoleWriteError("CONNECTION REJECTED: " + temphandle + " (BANNED NAME)");
                                                        msgSender.Disapprove("bannedname");
                                                    }
                                                }
                                                    }
                                                }
                                        else
                                        {
                                            IServer.ConsoleWriteError("CONNECTION REJECTED: (NO NAME)");
                                            msgSender.Disapprove("noname");
                                        }

                                        IClient newPlayer = new IClient(msgSender, null);

                                        foreach (Player oldPlayer in IServer.playerList.Values)
                                        {
                                            if (temphandle.ToLower() == oldPlayer.Name.ToLower())
                                            {
                                                duplicateNameCount++;
                                                temphandle += "." + duplicateNameCount.ToString();
                                                break;
                                            }
                                        }

                                        newPlayer.Name = temphandle;
                                        IServer.playerList[msgSender] = newPlayer;
                                        Thread SenderThread = new Thread(new ThreadStart(newPlayer.start));
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
                                        IServer.ConsoleWrite("CONNECT: " + IServer.playerList[msgSender].Name + " ( " + IServer.playerList[msgSender].IP + " )");
                                        IServer.SendCurrentMap(msgSender);
                                        IServer.SendPlayerJoined(player);
                                    }
                                    else if (msgSender.Status == NetConnectionStatus.Disconnected)
                                    {
                                        IServer.ConsoleWrite("DISCONNECT: " + IServer.playerList[msgSender].Name);
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
                                                    string[] splitted = commandstring.Split(new char[] { ' ' });
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
                                                                    foreach (IClient dummy in IServer.playerList.Values)
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
                                                                    foreach (IClient dummy in IServer.playerList.Values)
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
                                                foreach (IClient p in IServer.playerList.Values)
                                                {
                                                    p.AddQueMsg(chatPacket, NetChannel.ReliableInOrder3);
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
                                                    if (IServer.Ssettings.MOTD != "")
                                                    {
                                                        string greeting = IServer.Ssettings.MOTD;
                                                        greeting = greeting.Replace("[name]", IServer.playerList[msgSender].Name);
                                                        NetBuffer greetBuffer = netServer.CreateBuffer();
                                                        greetBuffer.Write((byte)MineWorldMessage.ChatMessage);
                                                        greetBuffer.Write((byte)ChatMessageType.SayServer);
                                                        greetBuffer.Write(Defines.Sanitize(greeting));
                                                        netServer.SendMessage(greetBuffer, msgSender, NetChannel.ReliableInOrder3);
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
                                                    IServer.SendHealthUpdate(player);
                                                    IServer.KillPlayerSpecific(player);
                                                }
                                                else
                                                {
                                                    // Let the client know what his new health is
                                                    IServer.SendHealthUpdate(player);
                                                }
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
