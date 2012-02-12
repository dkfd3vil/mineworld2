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
        public int ID;
        public string Name;
        public string Ip = "";
        public NetConnection NetConn;
        public Vector3 Position = Vector3.Zero;
        public Vector3 Heading = Vector3.Zero;

        public ServerPlayer(NetConnection netcon)
        {
            NetConn = netcon;
            ID = GetId();
            Ip = netcon.RemoteEndpoint.Address.ToString();
        }

        public static int id = 0;
        public static int GetId()
        {
            id = id + 1;
            return id;
        }
    }
}
