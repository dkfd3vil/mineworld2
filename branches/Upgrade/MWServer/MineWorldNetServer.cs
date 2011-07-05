// With the new lidgren libary this is nog needed anymore
/*
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;

namespace MineWorld
{
    public class MineWorldNetServer : NetServer
    {
        public MineWorldNetServer(NetConfiguration config): base(config)
        {
        }

        crappy hack to fix duplicate key error crash in Lidgren, hopefully a new
        version of Lidgren will fix this issue. 
        public bool SanityCheck(NetConnection connection)
        {
            if (this.m_connections.Contains(connection) == false)
            {
                if (this.m_connectionLookup.ContainsKey(connection.RemoteEndpoint))
                {
                    this.m_connectionLookup.Remove(connection.RemoteEndpoint);
                    return true;
                }
            }

            return false;
        }

        public void SendMsg(NetBuffer data, NetConnection connection, NetChannel channel)
        {
            if (connection.Status != NetConnectionStatus.Connected)
            {
                return;
            }

            SendMessage(data, connection, channel);
        }
    }
}
*/