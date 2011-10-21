using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;

namespace MineWorld
{
    public partial class MineWorldServer
    {
        public void SendPlayerPosition(ServerPlayer player)
        {
            if (player.NetConn.Status != NetConnectionStatus.Connected)
                return;

            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.PlayerPosition);
            msgBuffer.Write(player.Position);
            netServer.SendMsg(msgBuffer, player.NetConn, NetChannel.ReliableUnordered);
        }

        public void SendPlayerPositionUpdate(ServerPlayer player)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.PlayerUpdate);
            msgBuffer.Write(player.ID);
            msgBuffer.Write(player.Position);
            msgBuffer.Write(player.Heading);

            foreach (ServerPlayer iplayer in playerList.Values)
            {
                netServer.SendMsg(msgBuffer, iplayer.NetConn, NetChannel.UnreliableInOrder1);
            }
        }

        public void SendPlayerJoined(ServerPlayer player)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            bool thisisme = false;

            // Let other players know about this player.

            foreach (ServerPlayer iplayer in playerList.Values)
            {
                if (player.ID == iplayer.ID)
                {
                    thisisme = true;
                }
                else
                {
                    thisisme = false;
                }
                msgBuffer.Write((byte)MineWorldMessage.PlayerJoined);
                msgBuffer.Write(player.ID);
                msgBuffer.Write(player.Name);
                msgBuffer.Write(thisisme);
                msgBuffer.Write(player.Alive);
                netServer.SendMsg(msgBuffer, iplayer.NetConn, NetChannel.ReliableInOrder2);
            }

            // Send out a chat message.
            msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.ChatMessage);
            msgBuffer.Write((byte)ChatMessageType.PlayerSay);
            msgBuffer.Write(player.Name + " HAS JOINED THE ADVENTURE!");
            foreach (ServerPlayer iplayer in playerList.Values)
            {
                // Dont send the joined message to ourself
                if (player.ID == iplayer.ID)
                {
                    break;
                }
                netServer.SendMsg(msgBuffer, iplayer.NetConn, NetChannel.ReliableInOrder3);
            }
            player.Position = GenerateSpawnLocation();
            SendPlayerAlive(player);
            SendPlayerPosition(player);
        }

        public void SendPlayerLeft(ServerPlayer player, string reason)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.PlayerLeft);
            msgBuffer.Write(player.ID);
            foreach (ServerPlayer iplayer in playerList.Values)
                if (player.NetConn != iplayer.NetConn)
                {
                    netServer.SendMsg(msgBuffer, iplayer.NetConn, NetChannel.ReliableInOrder2);
                }

            // Send out a chat message.
            msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.ChatMessage);
            msgBuffer.Write((byte)ChatMessageType.PlayerSay);
            msgBuffer.Write(player.Name + " " + reason);
            foreach (ServerPlayer iplayer in playerList.Values)
            {
                netServer.SendMsg(msgBuffer, iplayer.NetConn, NetChannel.ReliableInOrder3);
            }
        }

        public void SendPlayerDead(ServerPlayer player)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.PlayerDead);
            msgBuffer.Write(player.ID);
            foreach (ServerPlayer iplayer in playerList.Values)
            {
                netServer.SendMsg(msgBuffer, iplayer.NetConn, NetChannel.ReliableInOrder2);
            }
        }

        public void SendPlayerAlive(ServerPlayer player)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.PlayerAlive);
            msgBuffer.Write(player.ID);
            foreach (ServerPlayer iplayer in playerList.Values)
            {
                netServer.SendMsg(msgBuffer, iplayer.NetConn, NetChannel.ReliableInOrder2);
            }
            player.Position = GenerateSpawnLocation();
            SendPlayerPosition(player);
        }

        public void SendPlaySound(MineWorldSound sound, Vector3 position)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.PlaySound);
            msgBuffer.Write((byte)sound);
            msgBuffer.Write(true);
            msgBuffer.Write(position);
            foreach (ServerPlayer player in playerList.Values)
            {
                netServer.SendMsg(msgBuffer, player.NetConn, NetChannel.ReliableUnordered);
            }
        }

        public void SendHearthBeat()
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.Hearthbeat);
            foreach (ServerPlayer iplayer in playerList.Values)
            {
                netServer.SendMsg(msgBuffer, iplayer.NetConn, NetChannel.UnreliableInOrder15);
            }
        }

        public void SendDayTimeUpdate(float time)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.DayUpdate);
            msgBuffer.Write(time);
            foreach (ServerPlayer iplayer in playerList.Values)
            {
                netServer.SendMsg(msgBuffer, iplayer.NetConn, NetChannel.UnreliableInOrder14);
            }
        }

        public void SendServerMessageToPlayer(string message, ServerPlayer player)
        {
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.ChatMessage);
            msgBuffer.Write((byte)ChatMessageType.Server);
            msgBuffer.Write(Defines.Sanitize(message));
            netServer.SendMsg(msgBuffer, player.NetConn, NetChannel.ReliableUnordered);
        }

        public void SendServerWideMessage(string message)
        {
            foreach (ServerPlayer player in playerList.Values)
            {
                SendServerMessageToPlayer(message, player);
            }
        }

        public void SendPlayerHealthUpdate(ServerPlayer player)
        {
            // Health, HealthMax both int
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.HealthUpdate);
            msgBuffer.Write(player.Health);
            msgBuffer.Write(player.HealthMax);
            netServer.SendMsg(msgBuffer, player.NetConn, NetChannel.ReliableInOrder15);
        }

        public void SendRemoveBlock(int x, int y, int z)
        {
            if (!SaneBlockPosition(x, y, z))
                return;

            blockList[x, y, z] = BlockType.None;

            // x, y, z, type, all bytes
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.BlockSet);
            msgBuffer.Write((byte)x);
            msgBuffer.Write((byte)y);
            msgBuffer.Write((byte)z);
            msgBuffer.Write((byte)BlockType.None);
            foreach (ServerPlayer player in playerList.Values)
            {
                netServer.SendMsg(msgBuffer, player.NetConn, NetChannel.ReliableUnordered);
            }
        }

        public void SendSetBlock(int x, int y, int z, BlockType blockType)
        {
            Debug.Assert(blockType != BlockType.None, "Setblock used for removal", "Block was sent " + blockType.ToString());

            if (!SaneBlockPosition(x, y, z))
                return;

            blockList[x, y, z] = blockType;

            // x, y, z, type, all bytes
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.BlockSet);
            msgBuffer.Write((byte)x);
            msgBuffer.Write((byte)y);
            msgBuffer.Write((byte)z);
            msgBuffer.Write((byte)blockType);
            foreach (ServerPlayer player in playerList.Values)
            {
                netServer.SendMsg(msgBuffer, player.NetConn, NetChannel.ReliableUnordered);
            }
        }

        public void SendPlayerCurrentMap(NetConnection client)
        {
            MapSender ms = new MapSender(client, this, netServer, Msettings.Mapsize);
            mapSendingProgress.Add(ms);
        }
    }
}
