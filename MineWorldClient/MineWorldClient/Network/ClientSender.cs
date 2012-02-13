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
    public class ClientSender
    {
        PropertyBag Pbag;
        NetClient Client;
        NetOutgoingMessage outmsg;

        public ClientSender(NetClient netc, PropertyBag pb)
        {
            Client = netc;
            Pbag = pb;
        }

        public void SendJoinGame(string ip)
        {
            //IPEndPoint serverEndPoint
            // Create our connect message.
            outmsg = Client.CreateMessage();
            outmsg.Write(Constants.MINEWORLDCLIENT_VERSION);
            outmsg.Write(Pbag.Player.Name);
            Client.Connect(ip, Constants.MINEWORLD_PORT, outmsg);
        }

        public void SendPlayerInWorld()
        {
            outmsg = Client.CreateMessage();
            outmsg.Write((byte)PacketType.PlayerInWorld);
            outmsg.Write(true);
            Client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
        }

        public void SendMovementUpdate()
        {
            outmsg = Client.CreateMessage();
            outmsg.Write((byte)PacketType.PlayerMovementUpdate);
            outmsg.Write(Pbag.Player.Position);
            Client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
        }

        public void SendBlockSet(BlockTypes type,Vector3 pos)
        {
            outmsg = Client.CreateMessage();
            outmsg.Write((byte)PacketType.PlayerBlockSet);
            outmsg.Write(Pbag.Player.Myid);
            outmsg.Write(pos);
            outmsg.Write((byte)type);
        }

        public void DiscoverLocalServers(int port)
        {
            Client.DiscoverLocalPeers(port);
        }
    }
}
