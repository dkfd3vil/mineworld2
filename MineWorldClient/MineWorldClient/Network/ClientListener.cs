using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Lidgren.Network.Xna;
using MineWorldData;
using Microsoft.Xna.Framework;

namespace MineWorld
{
    public class ClientListener
    {
        PropertyBag Pbag;
        NetClient Client;
        NetIncomingMessage _msgBuffer;

        public ClientListener(NetClient netc, PropertyBag pb)
        {
            Client = netc;
            Pbag = pb;
        }

        public void Update()
        {
            // Recieve messages from the server.
            while ((_msgBuffer = Client.ReadMessage()) != null)
            {
                NetIncomingMessageType msgType = _msgBuffer.MessageType;

                switch (msgType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        {
                            NetConnectionStatus status = (NetConnectionStatus)_msgBuffer.ReadByte();

                            if (status == NetConnectionStatus.Disconnected)
                            {
                                string reason = _msgBuffer.ReadString();
                                switch (reason)
                                {
                                    case "kicked":
                                        {
                                            Pbag.GameManager.SetErrorState(ErrorMsg.Kicked);
                                            break;
                                        }
                                    case "banned":
                                        {
                                            Pbag.GameManager.SetErrorState(ErrorMsg.Banned);
                                            break;
                                        }
                                    case "serverfull":
                                        {
                                            Pbag.GameManager.SetErrorState(ErrorMsg.ServerFull);
                                            break;
                                        }
                                    case "versionmismatch":
                                        {
                                            Pbag.GameManager.SetErrorState(ErrorMsg.VersionMismatch);
                                            break;
                                        }
                                    case "shutdown":
                                        {
                                            Pbag.GameManager.SetErrorState(ErrorMsg.ServerShutdown);
                                            break;
                                        }
                                    case "exit":
                                        {
                                            //Todo need to find more info about this case in the disconnect switch
                                            break;
                                        }
                                    default:
                                        {
                                            Pbag.GameManager.SetErrorState(ErrorMsg.Unkown);
                                            break;
                                        }
                                }
                            }
                            break;
                        }
                    case NetIncomingMessageType.DiscoveryResponse:
                        {
                            ServerInformation newserver = new ServerInformation(_msgBuffer.ReadString(), _msgBuffer.SenderEndpoint.Address.ToString());
                            Pbag.GameManager.AddServer(newserver);
                            break;
                        }
                    case NetIncomingMessageType.ConnectionApproval:
                        {
                            break;
                        }
                    case NetIncomingMessageType.Data:
                        {
                            PacketType dataType = (PacketType)_msgBuffer.ReadByte();
                            switch (dataType)
                            {
                                case PacketType.WorldMapSize:
                                    {
                                        Pbag.WorldManager.Mapsize = _msgBuffer.ReadInt32();
                                        Pbag.WorldManager.Start();
                                        break;
                                    }
                                case PacketType.PlayerInitialUpdate:
                                    {
                                        Pbag.Player.Myid = _msgBuffer.ReadInt32();
                                        Pbag.Player.Name = _msgBuffer.ReadString();
                                        Pbag.Player.Position = _msgBuffer.ReadVector3();
                                        break;
                                    }
                                case PacketType.PlayerJoined:
                                    {
                                        ClientPlayer dummy = new ClientPlayer(Pbag.GameManager.conmanager);
                                        dummy.ID = _msgBuffer.ReadInt32();
                                        dummy.name = _msgBuffer.ReadString();
                                        dummy.position = _msgBuffer.ReadVector3();
                                        dummy.heading = _msgBuffer.ReadVector3();
                                        Pbag.WorldManager.playerlist.Add(dummy.ID, dummy);
                                        break;
                                    }
                                case PacketType.PlayerLeft:
                                    {
                                        int id = _msgBuffer.ReadInt32();
                                        Pbag.WorldManager.playerlist.Remove(id);
                                        break;
                                    }
                                case PacketType.PlayerMovementUpdate:
                                    {
                                        int id = _msgBuffer.ReadInt32();
                                        if (Pbag.WorldManager.playerlist.ContainsKey(id))
                                        {
                                            Pbag.WorldManager.playerlist[id].position = _msgBuffer.ReadVector3();
                                        }
                                        break;
                                    }
                                case PacketType.PlayerNameSet:
                                    {
                                        int id = _msgBuffer.ReadInt32();
                                        //Lets see if its my id or someones else id
                                        if (Pbag.Player.Myid == id)
                                        {
                                            Pbag.Player.Name = _msgBuffer.ReadString();
                                        }
                                        else
                                        {
                                            //Then its someones else its id
                                            if (Pbag.WorldManager.playerlist.ContainsKey(id))
                                            {
                                                Pbag.WorldManager.playerlist[id].position = _msgBuffer.ReadVector3();
                                            }
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                        }
                        break;
                }
            }
        }
    }
}
