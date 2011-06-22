/*
using System;
using System.Collections.Generic;
using System.Text;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;
using System.Threading;

namespace MineWorld
{
    public class IClient : Player
    {
        public System.Collections.ObjectModel.Collection<qmsg> Que;
        private System.Collections.ObjectModel.Collection<qmsg> InternalQue;
        public NetConnection Connection;
        public bool Active = true;
        public EventWaitHandle WH;
        int msgs = 0;

        public IClient(NetConnection conn, Game gameInstance):base(conn,gameInstance)
        {
            Que = new System.Collections.ObjectModel.Collection<qmsg>();
            InternalQue = new System.Collections.ObjectModel.Collection<qmsg>();
            Connection = conn;
            WH = new AutoResetEvent(false);

        }

        public void start()
        {
            while(Active)
            {
                if (msgs > 0)
                {
                    if (Connection.Status == NetConnectionStatus.Connected)
                    {
                        qmsg[] temp;
                        lock (Que)
                        {
                            temp = new qmsg[Que.Count];
                            Que.CopyTo(temp, 0);
                            Que.Clear();
                            msgs = 0;
                        }
                        for (int i = 0; i < temp.GetLength(0); i++)
                        {

                            Connection.SendMsg(temp[i].Buffer, temp[i].Channel);

                        }
                    }
                    else
                    {
                        Active = false;
                    }
                }
                if (msgs == 0)
                {
                    WH.WaitOne();
                }
            }
        }

        public void AddQueMsg(NetBuffer buff, NetChannel chan)
        {
            lock (Que)
            {
                Que.Add(new qmsg(buff, chan));
                msgs++;
            }
            WH.Set();
        }

        public class qmsg
        {
            public NetBuffer Buffer;
            public NetChannel Channel;
            public qmsg(NetBuffer buff, NetChannel chan)
            {
                Buffer = buff;
                Channel = chan;
            }

        }
    } 
}
*/