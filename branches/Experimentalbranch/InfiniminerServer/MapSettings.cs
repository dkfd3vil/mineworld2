namespace MineWorld
{
    public partial class MineWorldServer
    {
        public struct MapSettings
        {
            public bool Includewater;
            public bool Includelava;
            public bool Includetrees;
            public bool IncludeAdminblocks;
            public Mapsize Mapsize;
            public int Lavaspawns;
            public int Waterspawns;
            public int Treecount;
            public int Orefactor; // 10
            public int Winningcashamount;

            //Not used by configs
            public string SettingsDir;

            public int Totallavablockcount;
            public int Totalwaterblockcount;
        }
    }
}