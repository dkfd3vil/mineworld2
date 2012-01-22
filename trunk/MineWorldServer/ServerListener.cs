using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Lidgren.Network.Xna;
using MineWorldData;

namespace MineWorldServer
{
    public class ServerListener
    {
        MineWorldServer mineserver;
        NetServer netserver;
        NetConnection _msgSender;

        public ServerListener(NetServer nets,MineWorldServer mines)
        {
            mineserver = mines;
            netserver = nets;
        }

        public void Start()
        {
            NetIncomingMessage packetin;
            while (true)
            {
                while ((packetin = netserver.ReadMessage()) != null)
                {
                    _msgSender = packetin.SenderConnection;
                    switch (packetin.MessageType)
                    {
                        case NetIncomingMessageType.StatusChanged:
                            {
                                //Player doesnt want to play anymore
                                if (_msgSender.Status == NetConnectionStatus.Disconnected)
                                {
                                    mineserver.ServerSender.SendPlayerLeft(mineserver.PlayerManager.GetPlayerByConnection(_msgSender));
                                    mineserver.PlayerManager.RemovePlayer(mineserver.PlayerManager.GetPlayerByConnection(_msgSender));
                                }
                                break;
                            }
                        case NetIncomingMessageType.ConnectionApproval:
                            {
                                int authcode = packetin.ReadInt32();
                                string name = packetin.ReadString();
                                mineserver.PlayerManager.AddPlayer(_msgSender);
                                _msgSender.Approve();
                                mineserver.GameWorld.GenerateSpawnPosition(mineserver.PlayerManager.GetPlayerByConnection(_msgSender));
                                mineserver.ServerSender.SendCurrentWorld(mineserver.PlayerManager.GetPlayerByConnection(_msgSender));
                                mineserver.ServerSender.SendInitialUpdate(mineserver.PlayerManager.GetPlayerByConnection(_msgSender));
                                mineserver.ServerSender.SendPlayerJoined(mineserver.PlayerManager.GetPlayerByConnection(_msgSender));
                                mineserver.console.ConsoleWrite(name + " Connected");
                                break;
                            }
                        case NetIncomingMessageType.Data:
                            {
                                PacketType dataType = (PacketType)packetin.ReadByte();

                                switch (dataType)
                                {
                                    case PacketType.PlayerMovementUpdate:
                                        {
                                            mineserver.PlayerManager.GetPlayerByConnection(_msgSender).Position = packetin.ReadVector3();
                                            mineserver.ServerSender.SendMovementUpdate(mineserver.PlayerManager.GetPlayerByConnection(_msgSender));
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
}
