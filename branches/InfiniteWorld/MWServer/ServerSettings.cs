namespace MineWorld
{
    public struct ServerSettings
    {
        public bool AutoAnnouce;
        public int Autosavetimer;
        public string BackupDir;
        public string LevelName;
        public int Lightsteps;
        public bool Logs;

        //Not used by configs
        public string LogsDir;
        public string MOTD;
        public int Maxplayers;
        public bool Proxy;
        public bool Public;
        public string ScriptsDir;
        public string Servername;
        public string SettingsDir;
    }
}