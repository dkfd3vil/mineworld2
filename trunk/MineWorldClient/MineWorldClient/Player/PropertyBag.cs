using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Lidgren.Network;
using MineWorldData;
using System.IO;

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

        public void JoinGame()
        {
            //IPEndPoint serverEndPoint
            // Create our connect message.
            NetOutgoingMessage connectBuffer = Client.CreateMessage();
            connectBuffer.Write(Constants.MINEWORLDCLIENT_VERSION);

            // Connect to the server.
            //Client.Connect(serverEndPoint, connectBuffer);
            //Client.Connect("127.0.0.1", Constants.MINEWORLD_PORT, connectBuffer);
            Client.Connect("192.168.0.15", Constants.MINEWORLD_PORT, connectBuffer);
            GameManager.SwitchState(GameStates.LoadingState);
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
                            PacketTypes dataType = (PacketTypes)_msgBuffer.ReadByte();
                            switch (dataType)
                            {
                                case PacketTypes.WorldMapTransfer:
                                    {
                                        int x = _msgBuffer.ReadInt32();
                                        int y = _msgBuffer.ReadInt32();
                                        int z = _msgBuffer.ReadInt32();

                                        WorldManager.Mapsize.X = x;
                                        WorldManager.Mapsize.Y = y;
                                        WorldManager.Mapsize.Z = z;
                                        WorldManager.Start();
                                        GameManager.SwitchState(GameStates.MainGameState);
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
            // Make sure our network thread actually gets to run.
            //Thread.Sleep(1);
        }

        public void SendMessage()
        {
            NetOutgoingMessage msg;
        }
    }
}
