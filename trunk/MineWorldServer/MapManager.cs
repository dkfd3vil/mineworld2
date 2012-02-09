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
        public int Mapsize;
        public int ChunkHeight;
        public int ChuckSize;

        public MapManager()
        {
            ChunkHeight = 128;
            ChuckSize = 16;
        }

        public void SetMapSize(int size)
        {
            Mapsize = size;
        }

        public void GenerateCubeMap(BlockTypes type)
        {
            WorldMap = new BlockTypes[Mapsize, ChunkHeight, Mapsize];
            EmptyMap();

            for (int xi = 0; xi < Mapsize; xi++)
            {
                for (int zi = 0; zi < Mapsize; zi++)
                {
                    for (int yi = 0; yi < (ChunkHeight / 2); yi++)
                    {
                        WorldMap[xi, yi, zi] = type;
                    }
                }
            }
        }

        public void EmptyMap()
        {
            for (int xi = 0; xi < Mapsize; xi++)
            {
                for (int zi = 0; zi < Mapsize; zi++)
                {
                    for (int yi = 0; yi < ChunkHeight; yi++)
                    {
                        WorldMap[xi, yi, zi] = BlockTypes.Air;
                    }
                }
            }
        }

        public Vector3 GenerateSpawnPosition()
        {
            Vector3 pos;
            pos.X = Utils.RandGen.Next(0, Mapsize);
            pos.Z = Utils.RandGen.Next(0, Mapsize);
            pos.Y = ChunkHeight + 1;

            return pos;
        }
    }
}
