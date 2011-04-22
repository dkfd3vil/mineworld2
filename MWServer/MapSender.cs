using System;
using System.Collections.Generic;
using System.Text;
using Lidgren.Network;
using System.Threading;

namespace MineWorld
{
    class MapSender
    {
        NetConnection client;
        Mapsize mapsize;
        Thread conn;
        MineWorldServer infs;
        MineWorldNetServer infsN;
        public bool finished
        {
            get
            {
                return !conn.IsAlive;
            }
        }

        public MapSender(NetConnection nClient, MineWorldServer nInfs, MineWorldNetServer nInfsN,Mapsize mpsize)
        {
            client = nClient;
            infs = nInfs;
            infsN = nInfsN;
            mapsize = mpsize;
            conn = new Thread(new ThreadStart(this.start));
            conn.Start();
            DateTime started = DateTime.Now;
            TimeSpan diff = DateTime.Now - started;
            while (!conn.IsAlive&&diff.Milliseconds<250) //Hold execution until it starts
            {
                diff = DateTime.Now - started;
            }
        }

        private void start()
        {
            for (byte x = 0; x < Defines.MAPSIZE; x++)
                for (byte y = 0; y < Defines.MAPSIZE; y += Defines.PACKETSIZE)
                {
                    NetBuffer msgBuffer = infsN.CreateBuffer();
                    msgBuffer.Write((byte)MineWorldMessage.BlockBulkTransfer);
                    msgBuffer.Write(x);
                    msgBuffer.Write(y);
                    for (byte dy = 0; dy < Defines.PACKETSIZE; dy++)
                    {
                        for (byte z = 0; z < Defines.MAPSIZE; z++)
                        {
                            msgBuffer.Write((byte)(infs.blockList[x, y + dy, z]));
                        }
                    }
                    if (client.Status == NetConnectionStatus.Connected)
                    {
                        infsN.SendMessage(msgBuffer, client, NetChannel.ReliableUnordered);
                    }
                }
            conn.Abort();
        }

        public void stop()
        {
            conn.Abort();
        }
    }
}