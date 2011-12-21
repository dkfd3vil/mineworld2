using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace MineWorld
{
    public class ServerPlayer
    {
        private static int _uniqueId;
        public bool Alive = true;
        public bool Godmode;
        public Vector3 Heading = Vector3.Zero;
        public int Health;
        public int HealthMax = 100;
        public int ID;
        public string Ip = "";
        public bool Kicked;
        public string Name = "";
        public NetConnection NetConn;
        public Vector3 Position = Vector3.Zero;

        public ServerPlayer(NetConnection netcon)
        {
            NetConn = netcon;
            ID = GetUniqueId();
            Ip = netcon.RemoteEndpoint.Address.ToString();
        }

        public static int GetUniqueId()
        {
            _uniqueId += 1;
            return _uniqueId;
        }
    }
}