using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using MineWorldData;

namespace MineWorldServer
{
    public class ServerSender
    {
        MineWorldServer mineserver;
        NetServer netserver;
        NetOutgoingMessage outmsg;

        public ServerSender(NetServer nets,MineWorldServer mines)
        {
            mineserver = mines;
            netserver = nets;
        }

        public void SendCurrentWorld(ServerPlayer player)
        {
            outmsg = netserver.CreateMessage();
            outmsg.Write((byte)PacketTypes.WorldMapTransfer);
            outmsg.WritePadBits();
            outmsg.Write(mineserver.GameWorld.CurrentWorldMapToBytes());
            netserver.SendMessage(outmsg, player.NetConn, NetDeliveryMethod.ReliableOrdered);
        }
    }
}
