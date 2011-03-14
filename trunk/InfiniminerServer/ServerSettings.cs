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
            public bool AutoAnnouce;
            public string MOTD;
            public int Autosavetimer;
            public bool Logs;

            //Not used by configs
            public string SettingsDir;
            public string LogsDir;
            public string BackupDir;
        }
    }
}
