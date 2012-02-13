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

        public ServerInformation(string name,string ip)
        {
            servername = name;
            ipaddress = ip;
        }
    }
}
