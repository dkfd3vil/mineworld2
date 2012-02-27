using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MineWorld
{
    public class ServerInformation
    {
        public string servername;
        public string ipaddress;
        public int playercount;
        public int maxplayercount;
        public bool lan;

        public ServerInformation(string name,string ip,int playerc, int playermax,bool la)
        {
            servername = name;
            ipaddress = ip;
            playercount = playerc;
            maxplayercount = playermax;
            lan = la;
        }

        public string GetTag()
        {
            return servername + " " + playercount.ToString() + "/" + maxplayercount.ToString();
        }
    }
}
