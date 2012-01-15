using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;
using MineWorldData;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

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

        public void SendInitialUpdate(ServerPlayer player)
        {
            outmsg = netserver.CreateMessage();
            outmsg.Write((byte)PacketType.InitialUpdate);
            outmsg.Write(player.Position);
            netserver.SendMessage(outmsg, player.NetConn, NetDeliveryMethod.ReliableOrdered);
        }

        public void SendCurrentWorld(ServerPlayer player)
        {
            outmsg = netserver.CreateMessage();
            outmsg.Write((byte)PacketType.WorldMapTransfer);
            outmsg.Write(mineserver.MapManager.WorldMapSize);
            netserver.SendMessage(outmsg, player.NetConn, NetDeliveryMethod.ReliableOrdered);
        }

        public void SendTerrainTextureData(ServerPlayer player)
        {
            FileStream opener = new FileStream("Data/Terrain.png", FileMode.Open);
            mineserver.Terrain = new byte[opener.Length];
            opener.Read(mineserver.Terrain, 0, (int)opener.Length);
            outmsg = netserver.CreateMessage();
            outmsg.Write(mineserver.Terrain.Length);
            outmsg.Write(mineserver.Terrain);
            netserver.SendMessage(outmsg, player.NetConn, NetDeliveryMethod.ReliableOrdered);
        }
    }
}
