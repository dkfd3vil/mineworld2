using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Lidgren.Network;

namespace MineWorld
{
    public class ServerInformation
    {
        public IPEndPoint ipEndPoint;
        public string serverName;
        public string gametag;
        public string serverExtra;
        public string numPlayers;
        public string maxPlayers;
        public bool lanServer;

        public ServerInformation(NetBuffer netBuffer)
        {
            ipEndPoint = netBuffer.ReadIPEndPoint();
            serverName = ipEndPoint.Address.ToString();
            lanServer = true;
        }

        public ServerInformation(IPAddress ip, string name,string gametag, string numPlayers, string maxPlayers,string extra)
        {
            serverName = name;
            ipEndPoint = new IPEndPoint(ip, 5565);
            this.gametag = gametag;
            this.numPlayers = numPlayers;
            this.maxPlayers = maxPlayers;
            serverExtra = extra;
            lanServer = false;
        }

        public string GetServerDesc()
        {
            string serverDesc = "";

            if (lanServer)
            {
                serverDesc = serverName.Trim() + " ( LAN SERVER )";
            }
            else
            {
                serverDesc = serverName.Trim() + " ( " + numPlayers.Trim() + " / " + maxPlayers.Trim() + " )";
                if (serverExtra.Trim() != "")
                    serverDesc += " - " + serverExtra.Trim();
            }
            
            return serverDesc;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (this.GetType() != obj.GetType())
                return false;

            ServerInformation serverInfo = obj as ServerInformation;

            if (!ipEndPoint.Equals(serverInfo.ipEndPoint))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
