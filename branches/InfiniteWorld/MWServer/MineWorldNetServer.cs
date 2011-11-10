using Lidgren.Network;

namespace MineWorld
{
    public class MineWorldNetServer : NetServer
    {
        public MineWorldNetServer(NetConfiguration config) : base(config)
        {
        }

        /* crappy hack to fix duplicate key error crash in Lidgren, hopefully a new
         * version of Lidgren will fix this issue. */

        public bool SanityCheck(NetConnection connection)
        {
            if (m_connections.Contains(connection) == false)
            {
                if (m_connectionLookup.ContainsKey(connection.RemoteEndpoint))
                {
                    m_connectionLookup.Remove(connection.RemoteEndpoint);
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