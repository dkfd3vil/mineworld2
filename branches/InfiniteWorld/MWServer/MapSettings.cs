namespace MineWorld
{
    public partial class MineWorldServer
    {
        public struct MapSettings
        {
            public bool IncludeAdminblocks;
            public bool Includelava;
            public bool Includetrees;
            public bool Includewater;
            public int Lavaspawns;
            public int Mapseed;

            //Not used by configs
            public int MapsizeX;
            public int MapsizeY;
            public int MapsizeZ;
            public int Orefactor; // 10

            public string SettingsDir;
            public int Treecount;
            public int Waterspawns;
        }
    }
}