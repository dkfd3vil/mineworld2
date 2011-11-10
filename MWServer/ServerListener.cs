using System;
using System.Threading;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;

namespace MineWorld
{
    internal class ServerListener
    {
        private readonly MineWorldServer _server;
        private readonly MineWorldNetServer _netServer;
        private NetConnection _msgSender;
        private NetMessageType _msgType;
        //private List<Thread> senderThreads

        public ServerListener(MineWorldNetServer serv, MineWorldServer iserv)
        {
            _netServer = serv;
            _server = iserv;
            // Initialize variables we'll use.
        }

        public void Start()
        {
            NetBuffer msgBuffer = _netServer.CreateBuffer();
            int duplicateNameCount = 0;

            while (true)
            {
                while (_netServer.ReadMessage(msgBuffer, out _msgType, out _msgSender))
                {
                    try
                    {
                        switch (_msgType)
                        {
                            case NetMessageType.ConnectionApproval:
                                {
                                    double authcode = msgBuffer.ReadDouble();
                                    string handle = msgBuffer.ReadString().Trim();

                                    if (authcode != Defines.MineworldBuild)
                                    {
                                        _server.ConsoleWriteError("CONNECTION REJECTED: " + handle + " (VERSION WRONG)");
                                        _msgSender.Disapprove("versionwrong");
                                    }
                                    else if (_server.BanList.Contains(_msgSender.RemoteEndpoint.Address.ToString()))
                                    {
                                        _server.ConsoleWriteError("CONNECTION REJECTED: " + handle + " (IP BANNED)");
                                        _msgSender.Disapprove("banned");
                                    }
                                    else if (_server.PlayerList.Count == _server.Ssettings.Maxplayers)
                                    {
                                        _server.ConsoleWriteError("CONNECTION REJECTED: " + handle + " (SERVER FULL)");
                                        _msgSender.Disapprove("serverfull");
                                    }
                                    else
                                    {
                                        if (handle.Length != 0)
                                        {
                                            if (handle.ToLower() == "player")
                                            {
                                                _server.ConsoleWriteError("CONNECTION REJECTED: " + handle +
                                                                          " (CHANGE NAME)");
                                                _msgSender.Disapprove("changename");
                                            }
                                            else
                                            {
                                                foreach (string name in _server.Bannednames)
                                                {
                                                    if (name.ToLower() == handle.ToLower())
                                                    {
                                                        _server.ConsoleWriteError("CONNECTION REJECTED: " + handle +
                                                                                  " (BANNED NAME)");
                                                        _msgSender.Disapprove("bannedname");
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            _server.ConsoleWriteError("CONNECTION REJECTED: (NO NAME)");
                                            _msgSender.Disapprove("noname");
                                        }
                                    }

                                    ServerPlayer newPlayer = new ServerPlayer(_msgSender);

                                    foreach (ServerPlayer oldPlayer in _server.PlayerList.Values)
                                    {
                                        if (handle.ToLower() == oldPlayer.Name.ToLower())
                                        {
                                            duplicateNameCount++;
                                            handle += "." + duplicateNameCount.ToString();
                                            break;
                                        }
                                    }

                                    newPlayer.Name = handle;
                                    _server.PlayerList[_msgSender] = newPlayer;
                                    _server.ToGreet.Add(_msgSender);
                                    _netServer.SanityCheck(_msgSender);
                                    _msgSender.Approve();
                                    _server.ConsoleWrite("CONNECT: " + _server.PlayerList[_msgSender].Name + " ( " +
                                                         _server.PlayerList[_msgSender].Ip + " )");
                                    _server.SendPlayerCurrentMap(_msgSender);
                                    _server.SendPlayerJoined(newPlayer);
                                    break;
                                }

                            case NetMessageType.StatusChanged:
                                {
                                    if (!_server.PlayerList.ContainsKey(_msgSender))
                                    {
                                        break;
                                    }

                                    ServerPlayer player = _server.PlayerList[_msgSender];

                                    // We disconnected from the server
                                    if (_msgSender.Status == NetConnectionStatus.Disconnected)
                                    {
                                        _server.ConsoleWrite("DISCONNECT: " + _server.PlayerList[_msgSender].Name);
                                        _server.SendPlayerLeft(player,
                                                               player.Kicked
                                                                   ? "WAS KICKED FROM THE GAME!"
                                                                   : "HAS ABANDONED THEIR DUTIES!");

                                        if (_server.PlayerList.ContainsKey(_msgSender))
                                        {
                                            _server.PlayerList.Remove(_msgSender);
                                        }
                                    }
                                }
                                break;

                            case NetMessageType.Data:
                                {
                                    if (!_server.PlayerList.ContainsKey(_msgSender))
                                    {
                                        break;
                                    }

                                    ServerPlayer player = _server.PlayerList[_msgSender];

                                    // If kicked we dont care anymore what he sends
                                    if (player.Kicked)
                                    {
                                        break;
                                    }

                                    MineWorldMessage dataType = (MineWorldMessage) msgBuffer.ReadByte();
                                    switch (dataType)
                                    {
                                        case MineWorldMessage.PlayerCommand:
                                            {
                                                string answer = "";

                                                if (_server.GetAdmin(player.Ip))
                                                {
                                                    string commandstring = msgBuffer.ReadString();
                                                    string[] splitted = commandstring.Split(new[] {' '});
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
                                                                _server.StopFluids = true;
                                                                answer = "Stopfluids enabled";
                                                                break;
                                                            }
                                                        case "/startfluids":
                                                            {
                                                                _server.StopFluids = false;
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
                                                                    foreach (
                                                                        ServerPlayer dummy in _server.PlayerList.Values)
                                                                    {
                                                                        if (dummy.Name.ToLower() == splitted[1])
                                                                        {
                                                                            playerfound = true;
                                                                            if (!dummy.Alive)
                                                                            {
                                                                                answer =
                                                                                    "Cant teleport to a dead player";
                                                                                break;
                                                                            }
                                                                            player.Position = dummy.Position;
                                                                            _server.SendPlayerPosition(player);
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
                                                                    foreach (
                                                                        ServerPlayer dummy in _server.PlayerList.Values)
                                                                    {
                                                                        if (dummy.Name.ToLower() == splitted[1])
                                                                        {
                                                                            playerfound = true;
                                                                            if (!dummy.Alive)
                                                                            {
                                                                                answer = "Player is already dead";
                                                                                break;
                                                                            }
                                                                            _server.SendPlayerHealthUpdate(dummy);
                                                                            _server.KillPlayerSpecific(dummy);
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
                                                                _server.DayManager.SetDay();
                                                                answer = "Time changed to day";
                                                                break;
                                                            }
                                                        case "/setnight":
                                                            {
                                                                _server.DayManager.SetNight();
                                                                answer = "Time changed to night";
                                                                break;
                                                            }
                                                        case "/announce":
                                                            {
                                                                _server.SendServerWideMessage(commandstring);
                                                                break;
                                                            }
                                                        case "/restart":
                                                            {
                                                                _server.RestartServer();
                                                                break;
                                                            }
                                                        case "/shutdown":
                                                            {
                                                                _server.ShutdownServer();
                                                                break;
                                                            }
                                                        default:
                                                            {
                                                                answer = "Command not regonized";
                                                                break;
                                                            }
                                                    }
                                                    _server.ConsoleWrite("COMMAND: (" + commandstring +
                                                                         ") has been used by (" + player.Name + ")");
                                                }
                                                else
                                                {
                                                    answer = "You arent a admin";
                                                }
                                                _server.SendServerMessageToPlayer(answer, player);
                                                break;
                                            }
                                        case MineWorldMessage.ChatMessage:
                                            {
                                                // Read the data from the packet.
                                                string chatString = msgBuffer.ReadString();
                                                string author = msgBuffer.ReadString();

                                                // Construct the message packet.
                                                NetBuffer chatPacket = _netServer.CreateBuffer();
                                                chatPacket.Write((byte) MineWorldMessage.ChatMessage);
                                                chatPacket.Write((byte) ChatMessageType.PlayerSay);
                                                chatPacket.Write(chatString);
                                                chatPacket.Write(author);

                                                // Send the packet to people who should recieve it.
                                                foreach (ServerPlayer p in _server.PlayerList.Values)
                                                {
                                                    _netServer.SendMsg(chatPacket, p.NetConn, NetChannel.ReliableInOrder3);
                                                }
                                            }
                                            break;

                                        case MineWorldMessage.UseTool:
                                            {
                                                CustomMouseButtons key = (CustomMouseButtons) msgBuffer.ReadByte();
                                                Vector3 playerPosition = msgBuffer.ReadVector3();
                                                Vector3 playerHeading = msgBuffer.ReadVector3();
                                                BlockType blockType = (BlockType) msgBuffer.ReadByte();
                                                switch (key)
                                                {
                                                    case CustomMouseButtons.AltFire:
                                                        _server.RemoveBlock(player, playerPosition, playerHeading);
                                                        break;
                                                    case CustomMouseButtons.Fire:
                                                        _server.PlaceBlock(player, playerPosition, playerHeading,
                                                                           blockType);
                                                        break;
                                                }
                                            }
                                            break;

                                        case MineWorldMessage.PlayerAlive:
                                            {
                                                if (_server.ToGreet.Contains(_msgSender))
                                                {
                                                    if (_server.Ssettings.MOTD != "")
                                                    {
                                                        string greeting = _server.Ssettings.MOTD;
                                                        greeting = greeting.Replace("[name]",
                                                                                    _server.PlayerList[_msgSender].Name);
                                                        NetBuffer greetBuffer = _netServer.CreateBuffer();
                                                        greetBuffer.Write((byte) MineWorldMessage.ChatMessage);
                                                        greetBuffer.Write((byte) ChatMessageType.Server);
                                                        greetBuffer.Write(greeting);
                                                        _netServer.SendMsg(greetBuffer, _msgSender,
                                                                          NetChannel.ReliableInOrder3);
                                                    }
                                                    _server.ToGreet.Remove(_msgSender);
                                                }
                                                _server.ConsoleWrite("PLAYER_ALIVE: " + player.Name);
                                                player.Health = player.HealthMax;
                                                player.Alive = true;
                                                _server.SendPlayerHealthUpdate(player);
                                                _server.SendPlayerAlive(player);
                                            }
                                            break;

                                        case MineWorldMessage.PlayerRequest:
                                            {
                                                PlayerRequests request = (PlayerRequests) msgBuffer.ReadByte();
                                                switch (request)
                                                {
                                                    case PlayerRequests.Respawn:
                                                        {
                                                            _server.SendPlayerAlive(player);
                                                            break;
                                                        }
                                                    default:
                                                        {
                                                            //Invalid request
                                                            break;
                                                        }
                                                }
                                            }
                                            break;

                                        case MineWorldMessage.PlayerUpdate:
                                            {
                                                player.Position = _server.AuthPosition(msgBuffer.ReadVector3(), player);
                                                player.Heading = _server.AuthHeading(msgBuffer.ReadVector3(), player);
                                                _server.SendPlayerPositionUpdate(player);
                                            }
                                            break;

                                        case MineWorldMessage.PlayerUpdate1: //minus position
                                            {
                                                player.Heading = _server.AuthHeading(msgBuffer.ReadVector3(), player);
                                                _server.SendPlayerPositionUpdate(player);
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
                                                else //Then its in procents
                                                {
                                                    if (damage > 100)
                                                    {
                                                        damage = 100;
                                                    }
                                                    player.Health -= (player.HealthMax/100)*damage;
                                                }
                                                if (player.Health <= 0)
                                                {
                                                    _server.KillPlayerSpecific(player);
                                                }
                                                break;
                                            }

                                        case MineWorldMessage.PlaySound:
                                            {
                                                MineWorldSound sound = (MineWorldSound) msgBuffer.ReadByte();
                                                Vector3 position = msgBuffer.ReadVector3();
                                                _server.SendPlaySound(sound, position);
                                            }
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                Thread.Sleep(25);
            }
        }
    }
}