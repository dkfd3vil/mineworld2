﻿using System;
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
                                    int authcode = msgBuffer.ReadInt32();
                                    if (authcode != Defines.MINEWORLD_AUTH)
                                    {
                                        msgSender.Disapprove("VER;" + Defines.MINEWORLDSERVER_VERSION + " Required consider updating MineWorld");
                                    }
                                    else if (IServer.banList.Contains(msgSender.RemoteEndpoint.Address.ToString()))
                                    {
                                        msgSender.Disapprove("You are banned from this server");
                                    }
                                    else if (IServer.playerList.Count == IServer.Ssettings.Maxplayers)
                                    {
                                        msgSender.Disapprove("The server is full");
                                    }
                                    else
                                    {
                                        IClient newPlayer = new IClient(msgSender, null);
                                        newPlayer.Handle = temphandle;
                                        if (newPlayer.Handle.Length == 0 || newPlayer.Handle.ToLower() == "player")
                                        {
                                            //newPlayer.Handle = "";
                                            msgSender.Disapprove("You didnt changed you name in the client.config");
                                        }

                                        if (IServer.admins.ContainsKey(newPlayer.IP))
                                            newPlayer.admin = IServer.admins[newPlayer.IP];
                                        IServer.playerList[msgSender] = newPlayer;
                                        //Check if we should compress the map for the client
                                        try
                                        {
                                            bool compression = msgBuffer.ReadBoolean();
                                            if (compression)
                                                IServer.playerList[msgSender].compression = true;
                                        }
                                        catch { }
                                        System.Threading.Thread SenderThread = new System.Threading.Thread(new System.Threading.ThreadStart(newPlayer.start));
                                        SenderThread.Start();
                                        IServer.toGreet.Add(msgSender);
                                        this.netServer.SanityCheck(msgSender);
                                        msgSender.Approve();
                                        //IServer.PublicServerListUpdate(true);
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
                                        //IServer.PublicServerListUpdate();
                                    }

                                    else if (msgSender.Status == NetConnectionStatus.Disconnected)
                                    {
                                        IServer.ConsoleWrite("DISCONNECT: " + IServer.playerList[msgSender].Handle);
                                        IServer.SendPlayerLeft(player, player.Kicked ? "WAS KICKED FROM THE GAME!" : "HAS ABANDONED THEIR DUTIES!");
                                        if (IServer.playerList.ContainsKey(msgSender))
                                            IServer.playerList.Remove(msgSender);
                                        //IServer.PublicServerListUpdate();
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
                                    MineWorldMessage dataType = (MineWorldMessage)msgBuffer.ReadByte();
                                    switch (dataType)
                                    {
                                        case MineWorldMessage.PlayerCommand:
                                            {
                                                PlayerCommands command = PlayerCommands.None;
                                                //string chatstring;
                                                //if(IServer.admins.ContainsKey(player.IP))
                                                if(!player.IsAdmin)
                                                {
                                                    string commandstring = Defines.Sanitize(msgBuffer.ReadString());
                                                    commandstring = commandstring.ToLower();
                                                    switch (commandstring)
                                                    {
                                                        case "/godmode":
                                                            {
                                                                command = PlayerCommands.Godmode;
                                                                //if (player.godmode == false)
                                                                //{
                                                                    //player.godmode = true;
                                                                    //chatstring = "Godmode enabled";
                                                                    //command = PlayerCommands.Godmode;
                                                                //}
                                                                //else
                                                                //{
                                                                    //player.godmode = false;
                                                                    //chatstring = "Godmode disabled";
                                                                //}
                                                                break;
                                                            }
                                                        default:
                                                            {
                                                                command = PlayerCommands.None;
                                                                //chatstring = "Command not reconized";
                                                                break;
                                                            }
                                                    }
                                                    IServer.ConsoleWrite("COMMAND: (" + commandstring + ") has been used by (" + player.Handle); 
                                                }
                                                else
                                                {
                                                    //chatstring = "You are not a admin";
                                                }
                                                NetBuffer chatPacket = netServer.CreateBuffer();
                                                //chatPacket.Write((byte)MineWorldMessage.ChatMessage);
                                                //chatPacket.Write((byte)(ChatMessageType.SayAll));
                                                //chatPacket.Write(chatstring);
                                                chatPacket.Write((byte)MineWorldMessage.PlayerCommandEnable);
                                                chatPacket.Write((byte)command);
                                                player.AddQueMsg(chatPacket, NetChannel.ReliableInOrder6);
                                                //chatstring = "";
                                                break;
                                            }
                                        case MineWorldMessage.ChatMessage:
                                            {
                                                // Read the data from the packet.
                                                ChatMessageType chatType = (ChatMessageType)msgBuffer.ReadByte();
                                                string chatString = Defines.Sanitize(msgBuffer.ReadString());
                                                if (!IServer.ProcessCommand(chatString, IServer.GetAdmin(IServer.playerList[msgSender].IP), IServer.playerList[msgSender]))
                                                {
                                                    IServer.ConsoleWrite("CHAT: (" + player.Handle + ") " + chatString);

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
                                                            p.AddQueMsg(chatPacket,  NetChannel.ReliableInOrder3);
                                                    }
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
                                                    string greeting = "Hello [name] welcome";
                                                    greeting = greeting.Replace("[name]", IServer.playerList[msgSender].Handle);
                                                    if (greeting != "")
                                                    {
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
