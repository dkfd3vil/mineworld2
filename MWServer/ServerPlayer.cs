using System;
using System.Collections.Generic;
using System.Text;
using Lidgren.Network;
using Lidgren.Network.Xna;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace MineWorld
{
    public class ServerPlayer
    {
        public bool Kicked = false;
        public bool Godmode = false;
        public string Name = "";
        // DJ NOT NICE
        // TODO This needs to be done proper
        public int Health = 0;
        public int HealthMax = 100;
        public bool Alive = false;
        public int ID;
        public Vector3 Heading = Vector3.Zero;
        public Vector3 Position = Vector3.Zero;
        public NetConnection NetConn;
        public string IP = "";
        public EventWaitHandle WH;

        public bool Canhealthregen;

        public ServerPlayer(NetConnection netcon)
        {
            this.NetConn = netcon;

            if (netcon != null)
            {
                this.IP = netcon.RemoteEndpoint.Address.ToString();
            }
            else
            {
                throw new Exception("SERVERPLAYER NETCONNECTION IS NULL");
            }

            WH = new AutoResetEvent(false);
        }

        public void Start()
        {
            while (true)
            {
                WH.WaitOne();
            }
        }
    }
}
