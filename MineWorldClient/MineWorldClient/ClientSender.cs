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

        public void SendMovementUpdate()
        {

        }

        public void SendBlockSet(BlockTypes type,Vector3 pos)
        {
            outmsg = Client.CreateMessage();
            outmsg.Write((byte)PacketType.BlockSet);
            outmsg.Write(Pbag.Player.Myid);
            outmsg.Write(pos);
            outmsg.Write((byte)type);
        }
    }
}
