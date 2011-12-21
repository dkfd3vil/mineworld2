using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;

namespace MineWorld
{
    internal class ServerListener
    {
        private readonly MineWorldServer _server;
        private readonly NetServer _netServer;
        private NetConnection _msgSender;
        private NetIncomingMessageType _msgType;
        //private List<Thread> senderThreads

        public ServerListener(NetServer serv, MineWorldServer iserv)
        {
            _netServer = serv;
            _server = iserv;
            // Initialize variables we'll use.
        }

        public void Start()
        {
            NetIncomingMessage msgBuffer = _netServer.CreateBuffer();
            int duplicateNameCount = 0;

            while (true)
            {
                while (_netServer.ReadMessage(msgBuffer, out _msgType, out _msgSender))
                {
                    switch (_msgType)
                    {
                        case NetMessageType.ConnectionApproval:
                            {
                                bool error = false;
                                int authcode = msgBuffer.ReadInt32();
                                string handle = msgBuffer.ReadString().Trim();

                                if (authcode != Defines.MineworldBuild)
                                {
                                    _server.ConsoleWriteError("CONNECTION REJECTED: " + handle + " (VERSION WRONG)");
                                    _msgSender.Disapprove("versionwrong");
                                    error = true;
                                }
                                else if (_server.BanList.Contains(_msgSender.RemoteEndpoint.Address.ToString()))
                                {
                                    _server.ConsoleWriteError("CONNECTION REJECTED: " + handle + " (IP BANNED)");
                                    _msgSender.Disapprove("banned");
                                    error = true;
                                }
                                else if (_server.PlayerList.Count == _server.Ssettings.Maxplayers)
                                {
                                    _server.ConsoleWriteError("CONNECTION REJECTED: " + handle + " (SERVER FULL)");
                                    _msgSender.Disapprove("serverfull");
                                    error = true;
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
                                            error = true;
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
                                                    error = true;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _server.ConsoleWriteError("CONNECTION REJECTED: (NO NAME)");
                                        _msgSender.Disapprove("noname");
                                        error = true;
                                    }
                                }

                                //if we got a error then dont create a player
                                if (!error)
                                {
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
                                }
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
                                    case MineWorldMessage.ChatMessage:
                                        {
                                            // Read the data from the packet.
                                            string chatString = msgBuffer.ReadString();
                                            string author = msgBuffer.ReadString();

                                            //Lets see if its a command or actual message
                                            if (chatString.StartsWith("/"))
                                            {
                                                //Its a command
                                                _server.SendPlayerCommandAnswer(player, chatString);
                                            }
                                            else
                                            {
                                                //Its a normal message
                                                _server.SendPlayerMessageToPlayers(chatString, author);
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
            }
        }
    }
}