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
            outmsg.Write((byte)PacketType.PlayerInitialUpdate);
            outmsg.Write(player.ID);
            outmsg.Write(player.Name);
            outmsg.Write(player.Position);
            netserver.SendMessage(outmsg, player.NetConn, NetDeliveryMethod.ReliableOrdered);
        }

        public void SendCurrentWorldSize(ServerPlayer player)
        {
            outmsg = netserver.CreateMessage();
            outmsg.Write((byte)PacketType.WorldMapSize);
            outmsg.Write(mineserver.MapManager.Mapsize);
            netserver.SendMessage(outmsg, player.NetConn, NetDeliveryMethod.ReliableOrdered);
        }

        public void SendPlayerJoined(ServerPlayer player)
        {
            outmsg = netserver.CreateMessage();
            outmsg.Write((byte)PacketType.PlayerJoined);
            outmsg.Write(player.ID);
            outmsg.Write(player.Name);
            outmsg.Write(player.Position);
            outmsg.Write(player.Heading);
            foreach (ServerPlayer dummy in mineserver.PlayerManager.PlayerList.Values)
            {
                if (player.ID != dummy.ID)
                {
                    netserver.SendMessage(outmsg, dummy.NetConn, NetDeliveryMethod.ReliableOrdered);
                }
            }
        }

        public void SendOtherPlayersInWorld(ServerPlayer player)
        {
            foreach (ServerPlayer dummy in mineserver.PlayerManager.PlayerList.Values)
            {
                if (player.ID != dummy.ID)
                {
                    outmsg = netserver.CreateMessage();
                    outmsg.Write((byte)PacketType.PlayerJoined);
                    outmsg.Write(dummy.ID);
                    outmsg.Write(dummy.Name);
                    outmsg.Write(dummy.Position);
                    outmsg.Write(dummy.Heading);
                    netserver.SendMessage(outmsg, player.NetConn, NetDeliveryMethod.ReliableOrdered);
                }
            }
        }

        public void SendPlayerLeft(ServerPlayer player)
        {
            outmsg = netserver.CreateMessage();
            outmsg.Write((byte)PacketType.PlayerLeft);
            outmsg.Write(player.ID);
            foreach (ServerPlayer dummy in mineserver.PlayerManager.PlayerList.Values)
            {
                if (player.ID != dummy.ID)
                {
                    netserver.SendMessage(outmsg, dummy.NetConn, NetDeliveryMethod.ReliableOrdered);
                }
            }
        }

        public void SendMovementUpdate(ServerPlayer player)
        {
            outmsg = netserver.CreateMessage();
            outmsg.Write((byte)PacketType.PlayerMovementUpdate);
            outmsg.Write(player.ID);
            outmsg.Write(player.Position);
            foreach (ServerPlayer dummy in mineserver.PlayerManager.PlayerList.Values)
            {
                if (player.ID != dummy.ID)
                {
                    netserver.SendMessage(outmsg, dummy.NetConn, NetDeliveryMethod.ReliableOrdered);
                }
            }
        }

        public void SendNameSet(ServerPlayer player)
        {
            outmsg = netserver.CreateMessage();
            outmsg.Write((byte)PacketType.PlayerMovementUpdate);
            outmsg.Write(player.ID);
            outmsg.Write(player.Name);
            foreach (ServerPlayer dummy in mineserver.PlayerManager.PlayerList.Values)
            {
                netserver.SendMessage(outmsg, dummy.NetConn, NetDeliveryMethod.ReliableOrdered);
            }
        }
    }
}
