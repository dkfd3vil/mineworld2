//Contains assorted server settings
namespace MineWorld
{
    public partial class MineWorldServer
    {
        public struct ServerSettings
        {
            public string Servername;
            public int Maxplayers;
            public bool Public;
            public bool Proxy;
            public string LevelName;
            public bool Autoload;
            public bool AutoAnnouce;
            public string MOTD;
            public int Autosavetimer;
            public bool Logs;

            //Not used by configs
            public bool StopFluids;
            public string SettingsDir;
            public string LogsDir;
            public string BackupDir;
        }
        int lavaBlockCount = 0;
        uint oreFactor = 10;

        uint teamCashRed = 0;
        uint teamCashBlue = 0;
        uint teamOreRed = 0;
        uint teamOreBlue = 0;

        uint winningCashAmount = 10000;
        PlayerTeam winningTeam = PlayerTeam.None;
    }
}
