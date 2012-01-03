using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Lidgren.Network;

namespace MineWorldServer
{
    public class ServerPlayer
    {
        public long ID;
        public string Name;
        public string Ip = "";
        public NetConnection NetConn;
        public Vector3 Position = Vector3.Zero;
        public Vector3 Heading = Vector3.Zero;

        public ServerPlayer(NetConnection netcon)
        {
            NetConn = netcon;
            ID = netcon.RemoteUniqueIdentifier;
            Ip = netcon.RemoteEndpoint.Address.ToString();
        }
    }
}
