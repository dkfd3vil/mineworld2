namespace MineWorld
{
    public class MapGenerator
    {
        private readonly int _mapX;
        private readonly int _mapY;
        private readonly int _mapZ;
        private readonly int _seed;

        //TODO Fix MapGenerator
        public MapGenerator(int seed, int x, int y, int z)
        {
            _seed = seed;
            _mapX = x;
            _mapY = y;
            _mapZ = z;
        }

        public BlockType[,,] GenerateSimpleCube()
        {
            BlockType[,,] caveData = new BlockType[_mapX,_mapY,_mapZ];

            FillMapPlaceholder(caveData);

            for (int x = 0; x < _mapX; x++)
            {
                for (int y = 0; y < _mapY/2; y++)
                {
                    for (int z = 0; z < _mapZ; z++)
                    {
                        caveData[x, y, z] = BlockType.Dirt;
                    }
                }
            }
            return caveData;
        }

        private void FillMapPlaceholder(BlockType[,,] caveData)
        {
            for (int x = 0; x < _mapX; x++)
            {
                for (int y = 0; y < _mapY; y++)
                {
                    for (int z = 0; z < _mapZ; z++)
                    {
                        caveData[x, y, z] = BlockType.None;
                    }
                }
            }
        }
    }
}