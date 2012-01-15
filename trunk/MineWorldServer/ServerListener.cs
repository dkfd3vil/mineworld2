using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

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
                        case NetIncomingMessageType.ConnectionApproval:
                            {
                                mineserver.console.ConsoleWrite("Newplayerconnect");
                                int authcode = packetin.ReadInt32();
                                mineserver.PlayerManager.AddPlayer(_msgSender);
                                _msgSender.Approve();
                                mineserver.GameWorld.GenerateSpawnPosition(mineserver.PlayerManager.GetPlayerByConnection(_msgSender));
                                mineserver.ServerSender.SendTerrainTextureData(mineserver.PlayerManager.GetPlayerByConnection(_msgSender));
                                mineserver.ServerSender.SendCurrentWorld(mineserver.PlayerManager.GetPlayerByConnection(_msgSender));
                                mineserver.ServerSender.SendInitialUpdate(mineserver.PlayerManager.GetPlayerByConnection(_msgSender));
                                break;
                            }
                    }
                }
            }
        }
    }
}
