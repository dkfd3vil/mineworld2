using System.Net;
using Lidgren.Network;

namespace MineWorld
{
    public class ServerInformation
    {
        public string Gametag;
        public IPEndPoint IpEndPoint;
        public bool LanServer;
        public string MaxPlayers;
        public string NumPlayers;
        public string ServerExtra;
        public string ServerName;

        public ServerInformation(NetBuffer netBuffer)
        {
            IpEndPoint = netBuffer.ReadIPEndPoint();
            ServerName = IpEndPoint.Address.ToString();
            LanServer = true;
        }

        public ServerInformation(IPAddress ip, string name, string gametag, string numPlayers, string maxPlayers,
                                 string extra)
        {
            ServerName = name;
            IpEndPoint = new IPEndPoint(ip, 5565);
            Gametag = gametag;
            NumPlayers = numPlayers;
            MaxPlayers = maxPlayers;
            ServerExtra = extra;
            LanServer = false;
        }

        public string GetServerDesc()
        {
            string serverDesc;

            if (LanServer)
            {
                serverDesc = ServerName.Trim() + " ( LAN SERVER )";
            }
            else
            {
                serverDesc = ServerName.Trim() + " ( " + NumPlayers.Trim() + " / " + MaxPlayers.Trim() + " )";
                if (ServerExtra.Trim() != "")
                    serverDesc += " - " + ServerExtra.Trim();
            }

            return serverDesc;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            ServerInformation serverInfo = obj as ServerInformation;

            if (!IpEndPoint.Equals(serverInfo.IpEndPoint))
                return false;

            return true;
        }
    }
}