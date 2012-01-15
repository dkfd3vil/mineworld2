using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineWorldData;
using Microsoft.Xna.Framework;

namespace MineWorldServer
{
    public class MapManager
    {
        public BlockTypes[, ,] WorldMap;
        public Vector3 WorldMapSize;

        public MapManager()
        {

        }

        public void SetMapSize(int x, int y, int z)
        {
            WorldMapSize.X = x;
            WorldMapSize.Y = y;
            WorldMapSize.Z = z;
        }

        public void GenerateCubeMap(BlockTypes type)
        {
            EmptyMap();

            for (int xi = 0; xi < WorldMapSize.X; xi++)
            {
                for (int zi = 0; zi < WorldMapSize.Z; zi++)
                {
                    for (int yi = 0; yi < (WorldMapSize.Y / 2); yi++)
                    {
                        WorldMap[xi, yi, zi] = type;
                    }
                }
            }
        }

        public void EmptyMap()
        {
            for (int xi = 0; xi < WorldMapSize.X; xi++)
            {
                for (int zi = 0; zi < WorldMapSize.Z; zi++)
                {
                    for (int yi = 0; yi < (WorldMapSize.Y); yi++)
                    {
                        WorldMap[xi, yi, zi] = BlockTypes.Air;
                    }
                }
            }
        }

        public Vector3 GenerateSpawnPosition()
        {
            Vector3 pos;
            pos.X = Utils.RandGen.Next(0, (int)WorldMapSize.X);
            pos.Z = Utils.RandGen.Next(0, (int)WorldMapSize.Z);
            pos.Y = WorldMapSize.Y + 1;

            return pos;
        }
    }
}
