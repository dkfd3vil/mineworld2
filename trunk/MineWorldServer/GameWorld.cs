using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineWorldData;
using System.IO;

namespace MineWorldServer
{
    public class GameWorld
    {
        MineWorldServer mineserver;
        public GameWorld(MineWorldServer mines)
        {
            mineserver = mines;
        }

        public void GenerateSimpleMap(BlockTypes type)
        {
            for (int xi = 0; xi < mineserver.WorldMapSize.X; xi++)
            {
                for (int zi = 0; zi < mineserver.WorldMapSize.Z; zi++)
                {
                    for (int yi = 0; yi < (mineserver.WorldMapSize.Y / 2); yi++)
                    {
                        mineserver.WorldMap[xi, yi, zi] = type;
                    }
                }
            }
        }

        public void KickPlayerByName(string name)
        {
            foreach (ServerPlayer player in mineserver.PlayerList.Values)
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
            foreach (ServerPlayer player in mineserver.PlayerList.Values)
            {
                KickPlayer(player);
            }
        }

        public byte[] CurrentWorldMapToBytes()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    for (int xi = 0; xi < mineserver.WorldMapSize.X; xi++)
                    {
                        for (int zi = 0; zi < mineserver.WorldMapSize.Z; zi++)
                        {
                            for (int yi = 0; yi < (mineserver.WorldMapSize.Z / 2); yi++)
                            {
                                writer.Write((byte)mineserver.WorldMap[xi,yi,zi]);
                            }
                        }
                    }
                }
                return m.ToArray();
            }
        }
    }
}
