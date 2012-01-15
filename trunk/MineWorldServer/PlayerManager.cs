﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace MineWorldServer
{
    public class PlayerManager
    {
        public Dictionary<NetConnection, ServerPlayer> PlayerList = new Dictionary<NetConnection, ServerPlayer>();

        public PlayerManager()
        {
        }

        public void AddPlayer(NetConnection con)
        {
            ServerPlayer player = new ServerPlayer(con);
            PlayerList[con] = player;
        }

        public void RemovePlayer(ServerPlayer play)
        {
            foreach (ServerPlayer dummy in PlayerList.Values)
            {
                if (play == dummy)
                {
                    PlayerList.Remove(dummy.NetConn);
                }
            }
        }

        public ServerPlayer GetPlayerByConnection(NetConnection conn)
        {
            foreach (ServerPlayer dummy in PlayerList.Values)
            {
                if (dummy.NetConn == conn)
                {
                    return dummy;
                }
            }

            return null;
        }

        public NetConnection GetConnectionByPlayer(ServerPlayer play)
        {
            foreach (ServerPlayer dummy in PlayerList.Values)
            {
                if (play == dummy)
                {
                    return dummy.NetConn;
                }
            }

            return null;
        }

        public void KickPlayerByName(string name)
        {
            foreach (ServerPlayer player in PlayerList.Values)
            {
                if (player.Name.ToLower() == name.ToLower())
                {
                    KickPlayer(player);
                }
            }
        }

        public void KickPlayer(ServerPlayer player)
        {
            player.NetConn.Disconnect("kicked");
        }

        public void KickAllPlayers()
        {
            foreach (ServerPlayer player in PlayerList.Values)
            {
                KickPlayer(player);
            }
        }
    }
}
