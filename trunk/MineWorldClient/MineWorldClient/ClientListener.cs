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
                        }
                        break;
                    case NetIncomingMessageType.ConnectionApproval:
                        {
                            break;
                        }
                    case NetIncomingMessageType.Data:
                        {
                            PacketType dataType = (PacketType)_msgBuffer.ReadByte();
                            switch (dataType)
                            {
                                case PacketType.WorldMapTransfer:
                                    {
                                        Pbag.WorldManager.Mapsize = _msgBuffer.ReadVector3();
                                        Pbag.WorldManager.Start();
                                        break;
                                    }
                                case PacketType.PlayerInitialUpdate:
                                    {
                                        Pbag.Player.Myid = _msgBuffer.ReadInt64();
                                        Pbag.Player.Position = _msgBuffer.ReadVector3();
                                        break;
                                    }
                                case PacketType.PlayerJoined:
                                    {
                                        ClientPlayer dummy = new ClientPlayer(Pbag.GameManager.conmanager);
                                        dummy.ID = _msgBuffer.ReadInt64();
                                        dummy.name = _msgBuffer.ReadString();
                                        dummy.position = _msgBuffer.ReadVector3();
                                        dummy.heading = _msgBuffer.ReadVector3();
                                        Pbag.WorldManager.playerlist.Add(dummy.ID, dummy);
                                        break;
                                    }
                                case PacketType.PlayerLeft:
                                    {
                                        long id = _msgBuffer.ReadInt64();
                                        Pbag.WorldManager.playerlist.Remove(id);
                                        break;
                                    }
                                case PacketType.PlayerMovementUpdate:
                                    {
                                        long id = _msgBuffer.ReadInt64();
                                        Pbag.WorldManager.playerlist[id].position = _msgBuffer.ReadVector3();
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
