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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace MineWorld
{
    public class ServerPlayer : Player
    {
        public EventWaitHandle WH;

        public bool Canhealthregen;

        public ServerPlayer(NetConnection netcon)
        {
            this.NetConn = netcon;

            if (netcon != null)
                this.IP = netcon.RemoteEndpoint.Address.ToString();

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
