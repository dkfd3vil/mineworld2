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
                                ServerPlayer newplayer = new ServerPlayer(_msgSender);
                                mineserver.PlayerList[_msgSender] = newplayer;
                                _msgSender.Approve();
                                mineserver.ServerSender.SendCurrentWorld(newplayer);
                                break;
                            }
                    }
                }
            }
        }
    }
}
