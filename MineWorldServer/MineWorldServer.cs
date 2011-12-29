using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace MineWorldServer
{
    public class MineWorldServer
    {
        NetServer Server;

        public MineWorldServer()
        {
            NetPeerConfiguration netConfig = new NetPeerConfiguration("MineWorld");
            Server = new NetServer(netConfig);
        }

        public bool Start()
        {
            Server.Start();

            while (true)
            {

            }
        }
    }
}
