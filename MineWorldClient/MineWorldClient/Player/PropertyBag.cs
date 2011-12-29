using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Lidgren.Network;
using MineWorldData;

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

        public PropertyBag(MineWorldClient Gamein,GameStateManager GameManagerin)
        {
            Game = Gamein;
            GameManager = GameManagerin;
            NetPeerConfiguration netconfig = new NetPeerConfiguration("MineWorld");
            Client = new NetClient(netconfig);
            Client.Start();
        }

        public void JoinGame()
        {
            GameManager.SwitchState(GameStates.LoadingState);
            //IPEndPoint serverEndPoint
            // Create our connect message.
            NetOutgoingMessage connectBuffer = Client.CreateMessage();
            connectBuffer.Write(Constants.MINEWORLDCLIENT_VERSION);

            // Connect to the server.
            //Client.Connect(serverEndPoint, connectBuffer);
            Client.Connect("127.0.0.1", Constants.MINEWORLD_PORT, connectBuffer);
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
