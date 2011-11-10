using System.Diagnostics;
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

            NetBuffer msgBuffer = _netServer.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.PlayerPosition);
            msgBuffer.Write(player.Position);
            _netServer.SendMsg(msgBuffer, player.NetConn, NetChannel.ReliableUnordered);
        }

        public void SendPlayerPositionUpdate(ServerPlayer player)
        {
            NetBuffer msgBuffer = _netServer.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.PlayerUpdate);
            msgBuffer.Write(player.ID);
            msgBuffer.Write(player.Position);
            msgBuffer.Write(player.Heading);

            foreach (ServerPlayer iplayer in PlayerList.Values)
            {
                _netServer.SendMsg(msgBuffer, iplayer.NetConn, NetChannel.UnreliableInOrder1);
            }
        }

        public void SendPlayerJoined(ServerPlayer player)
        {
            NetBuffer msgBuffer = _netServer.CreateBuffer();

            // Let other players know about this player.

            foreach (ServerPlayer iplayer in PlayerList.Values)
            {
                if (player.ID == iplayer.ID)
                {
                    //Dont send the joined message to ourself
                    break;
                }
                msgBuffer.Write((byte) MineWorldMessage.PlayerJoined);
                msgBuffer.Write(player.ID);
                msgBuffer.Write(player.Name);
                msgBuffer.Write(player.Alive);
                _netServer.SendMsg(msgBuffer, iplayer.NetConn, NetChannel.ReliableInOrder2);
            }

            // Send out a chat message.
            msgBuffer = _netServer.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.ChatMessage);
            msgBuffer.Write((byte) ChatMessageType.Server);
            msgBuffer.Write(player.Name + " HAS JOINED THE ADVENTURE!");
            foreach (ServerPlayer iplayer in PlayerList.Values)
            {
                // Dont send the joined message to ourself
                if (player.ID == iplayer.ID)
                {
                    break;
                }
                _netServer.SendMsg(msgBuffer, iplayer.NetConn, NetChannel.ReliableInOrder3);
            }

            msgBuffer = _netServer.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.PlayerId);
            msgBuffer.Write(player.ID);
            _netServer.SendMessage(msgBuffer, player.NetConn, NetChannel.ReliableInOrder13);


            player.Position = GenerateSpawnLocation();
            SendPlayerAlive(player);
            SendPlayerPosition(player);
        }

        public void SendPlayerLeft(ServerPlayer player, string reason)
        {
            NetBuffer msgBuffer = _netServer.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.PlayerLeft);
            msgBuffer.Write(player.ID);
            foreach (ServerPlayer iplayer in PlayerList.Values)
                if (player.NetConn != iplayer.NetConn)
                {
                    _netServer.SendMsg(msgBuffer, iplayer.NetConn, NetChannel.ReliableInOrder2);
                }

            // Send out a chat message.
            msgBuffer = _netServer.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.ChatMessage);
            msgBuffer.Write((byte) ChatMessageType.PlayerSay);
            msgBuffer.Write(player.Name + " " + reason);
            foreach (ServerPlayer iplayer in PlayerList.Values)
            {
                _netServer.SendMsg(msgBuffer, iplayer.NetConn, NetChannel.ReliableInOrder3);
            }
        }

        public void SendPlayerDead(ServerPlayer player)
        {
            NetBuffer msgBuffer = _netServer.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.PlayerDead);
            msgBuffer.Write(player.ID);
            foreach (ServerPlayer iplayer in PlayerList.Values)
            {
                _netServer.SendMsg(msgBuffer, iplayer.NetConn, NetChannel.ReliableInOrder2);
            }
        }

        public void SendPlayerAlive(ServerPlayer player)
        {
            NetBuffer msgBuffer = _netServer.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.PlayerAlive);
            msgBuffer.Write(player.ID);
            foreach (ServerPlayer iplayer in PlayerList.Values)
            {
                _netServer.SendMsg(msgBuffer, iplayer.NetConn, NetChannel.ReliableInOrder2);
            }
        }

        public void SendPlaySound(MineWorldSound sound, Vector3 position)
        {
            NetBuffer msgBuffer = _netServer.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.PlaySound);
            msgBuffer.Write((byte) sound);
            msgBuffer.Write(true);
            msgBuffer.Write(position);
            foreach (ServerPlayer player in PlayerList.Values)
            {
                _netServer.SendMsg(msgBuffer, player.NetConn, NetChannel.ReliableUnordered);
            }
        }

        public void SendHearthBeat()
        {
            NetBuffer msgBuffer = _netServer.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.Hearthbeat);
            foreach (ServerPlayer iplayer in PlayerList.Values)
            {
                _netServer.SendMsg(msgBuffer, iplayer.NetConn, NetChannel.UnreliableInOrder15);
            }
        }

        public void SendDayTimeUpdate(float time)
        {
            NetBuffer msgBuffer = _netServer.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.DayUpdate);
            msgBuffer.Write(time);
            foreach (ServerPlayer iplayer in PlayerList.Values)
            {
                _netServer.SendMsg(msgBuffer, iplayer.NetConn, NetChannel.UnreliableInOrder14);
            }
        }

        public void SendServerMessageToPlayer(string message, ServerPlayer player)
        {
            NetBuffer msgBuffer = _netServer.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.ChatMessage);
            msgBuffer.Write((byte) ChatMessageType.Server);
            msgBuffer.Write(message);
            _netServer.SendMsg(msgBuffer, player.NetConn, NetChannel.ReliableUnordered);
        }

        public void SendServerWideMessage(string message)
        {
            foreach (ServerPlayer player in PlayerList.Values)
            {
                SendServerMessageToPlayer(message, player);
            }
        }

        public void SendPlayerHealthUpdate(ServerPlayer player)
        {
            // Health, HealthMax both int
            NetBuffer msgBuffer = _netServer.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.HealthUpdate);
            msgBuffer.Write(player.Health);
            msgBuffer.Write(player.HealthMax);
            _netServer.SendMsg(msgBuffer, player.NetConn, NetChannel.ReliableInOrder15);
        }

        public void SendRemoveBlock(int x, int y, int z)
        {
            if (!SaneBlockPosition(x, y, z))
                return;

            BlockList[x, y, z] = BlockType.None;

            // x, y, z, type, all bytes
            NetBuffer msgBuffer = _netServer.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.BlockSet);
            msgBuffer.Write((byte) x);
            msgBuffer.Write((byte) y);
            msgBuffer.Write((byte) z);
            msgBuffer.Write((byte) BlockType.None);
            foreach (ServerPlayer player in PlayerList.Values)
            {
                _netServer.SendMsg(msgBuffer, player.NetConn, NetChannel.ReliableUnordered);
            }
        }

        public void SendSetBlock(int x, int y, int z, BlockType blockType)
        {
            Debug.Assert(blockType != BlockType.None, "Setblock used for removal",
                         "Block was sent " + blockType.ToString());

            if (!SaneBlockPosition(x, y, z))
                return;

            BlockList[x, y, z] = blockType;

            // x, y, z, type, all bytes
            NetBuffer msgBuffer = _netServer.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.BlockSet);
            msgBuffer.Write((byte) x);
            msgBuffer.Write((byte) y);
            msgBuffer.Write((byte) z);
            msgBuffer.Write((byte) blockType);
            foreach (ServerPlayer player in PlayerList.Values)
            {
                _netServer.SendMsg(msgBuffer, player.NetConn, NetChannel.ReliableUnordered);
            }
        }

        public void SendPlayerCurrentMap(NetConnection client)
        {
            //TODO Figure out why i have to hold this message, client is responding too slow?
            Thread.Sleep(2000);
            NetBuffer msgBuffer = _netServer.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.BlockBulkTransfer);
            msgBuffer.Write(Msettings.MapsizeX);
            msgBuffer.Write(Msettings.MapsizeY);
            msgBuffer.Write(Msettings.MapsizeZ);
            msgBuffer.Write(Msettings.Mapseed);
            _netServer.SendMsg(msgBuffer, client, NetChannel.ReliableInOrder15);
        }
    }
}