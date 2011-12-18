using System;
namespace MineWorld
{
    public class MapGenerator
    {
        private readonly int _mapX;
        private readonly int _mapY;
        private readonly int _mapZ;
        private readonly int _seed;

        public BlockType[, ,] mapData;

        //TODO Fix MapGenerator
        public MapGenerator(int seed, int x, int y, int z)
        {
            _seed = seed;
            _mapX = x;
            _mapY = y;
            _mapZ = z;

            //Init nice and empty map in the constructor 
            mapData = new BlockType[_mapX, _mapY, _mapZ];
            for (int fx = 0; fx < _mapX; fx++)
            {
                for (int fy = 0; fy < _mapY; fy++)
                {
                    for (int fz = 0; fz < _mapZ; fz++)
                    {
                        mapData[fx, fy, fz] = BlockType.None;
                    }
                }
            }

        }

        //draw 3D cube in map filled with brush
        public void drawCube(int x1, int y1, int z1, int x2, int y2, int z2, BlockType brush)
        {
            int cx1 = Math.Min(x1, x2);
            int cx2 = Math.Max(x1, x2);
            int cy1 = Math.Min(y1, y2);
            int cy2 = Math.Max(y1, y2);
            int cz1 = Math.Min(z1, z2);
            int cz2 = Math.Max(z1, z2);

            for (int x = cx1; x < cx2; x++)
            {
                for (int y = cy1; y < cy2 / 2; y++)
                {
                    for (int z = cz1; z < cz2; z++)
                    {
                        mapData[x, y, z] = brush;
                    }
                }
            }
        }
    }
}