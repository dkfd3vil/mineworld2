using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Lidgren.Network.Xna;
using MineWorldData;
using System.IO;
using Microsoft.Xna.Framework;

namespace MineWorld
{
    public class PropertyBag
    {
        public MineWorldClient Game;
        public GameStateManager GameManager;
        public WorldManager WorldManager;
        public Player Player;
        public NetClient Client;
        public NetIncomingMessage _msgBuffer;
        public Dictionary<int, ClientPlayer> ClientPlayers;
        public BlockTypes[, ,] Tempblockmap;

        public PropertyBag(MineWorldClient Gamein,GameStateManager GameManagerin)
        {
            Game = Gamein;
            GameManager = GameManagerin;
            NetPeerConfiguration netconfig = new NetPeerConfiguration("MineWorld");
            Client = new NetClient(netconfig);
            Client.Start();
            Player = new Player(this);
            WorldManager = new WorldManager(GameManager, Player);
        }

        public void JoinGame(string ip)
        {
            //IPEndPoint serverEndPoint
            // Create our connect message.
            NetOutgoingMessage connectBuffer = Client.CreateMessage();
            connectBuffer.Write(Constants.MINEWORLDCLIENT_VERSION);
            Client.Connect(ip, Constants.MINEWORLD_PORT, connectBuffer);
        }

        public void ReceiveMessages()
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
                                        WorldManager.Mapsize = _msgBuffer.ReadVector3();
                                        WorldManager.Start();
                                        break;
                                    }
                                case PacketType.InitialUpdate:
                                    {
                                        Player.Position = _msgBuffer.ReadVector3();
                                        break;
                                    }
                                case PacketType.TerrainTextureData:
                                    {
                                        int byteslength = _msgBuffer.ReadInt32();
                                        byte[] tempdata = _msgBuffer.ReadBytes(byteslength);
                                        WorldManager.SetTerrainData(tempdata);
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

        public void SendMessage()
        {
            NetOutgoingMessage msg;
        }
    }
}
