using System;
using System.Collections.Generic;
using System.Threading;
using Lidgren.Network;
using System.Net;

namespace MineWorld
{
    public partial class MineWorldServer
    {
        public MapSettings Msettings;
        public ServerExtraSettings SAsettings;
        public ServerSettings Ssettings;
        public bool StopFluids;
        public List<string> Admins = new List<string>(); //List of strings with all the admins
        public List<string> BanList = new List<string>(); // List of string with all thhe banned ip's
        public List<string> Bannednames = new List<string>(); // List of strings with all names that cannot be chosen
        public DayManager DayManager;

        private int _frameCount = 100;

        private DateTime _lastKeyAvaible = DateTime.Now;
        private DateTime _lastMapBackup = DateTime.Now;
        private DateTime _lasthearthbeatsend = DateTime.Now;
        public LuaManager LuaManager;
        private MineWorldNetServer _netServer;
        public Dictionary<NetConnection, ServerPlayer> PlayerList = new Dictionary<NetConnection, ServerPlayer>();

        // Server restarting variables.
        private DateTime _restartTime = DateTime.Now;
        private bool _restartTriggered;
        public string ServerIp;
        public string Sessionid;

        // Server shutdown variables.
        private DateTime _shutdownTime = DateTime.Now;
        private bool _shutdownTriggerd;
        public List<NetConnection> ToGreet = new List<NetConnection>();

        public string GetExternalIp()
        {
            WebClient client = new WebClient();
            client.Proxy = null;
            return client.DownloadString("http://whatismyip.org/");
        }

        public bool Start()
        {
            //Find a better place for this
            LuaManager = new LuaManager(this);

            //Display server version in console
            ConsoleWrite(Defines.MineworldserverVersion, ConsoleColor.Cyan);

            // Load the directory's.
            LoadDirectorys();

            // Load the general-settings.
            LoadSettings();

            // Load extra settings.
            LoadExtraSettings();

            // Load the map-settings.
            LoadMapSettings();

            // Load the ban-list.
            BanList = LoadBanList();

            // Load the admin-list.
            Admins = LoadAdminList();

            // Load the bannednames-list.
            Bannednames = LoadBannedNames();

            // Load scripts
            LoadScriptFiles();

            // Initialize the server.
            NetConfiguration netConfig = new NetConfiguration("MineWorld")
                                             {MaxConnections = Ssettings.Maxplayers, Port = 5565};
            _netServer = new MineWorldNetServer(netConfig);
            _netServer.SetMessageTypeEnabled(NetMessageType.ConnectionApproval, true);
            //netServer.SimulatedMinimumLatency = 0.1f;
            //netServer.SimulatedLatencyVariance = 0.05f;
            //netServer.SimulatedLoss = 0.1f;
            //netServer.SimulatedDuplicates = 0.05f;

            // Initialize the daymanager.
            DayManager = new DayManager(Ssettings.Lightsteps);

            //Display external IP
            //Dont bother if it isnt public
            if (Ssettings.Public)
            {
                ServerIp = GetExternalIp();
                ConsoleWrite("Your external IP Adress: " + ServerIp);
            }

            /*
            bool loaded = false;
            //Check if we should autoload a level
            if (Ssettings.LevelName != "")
            {
                ConsoleWrite("AUTOLOAD MAP");
                blockList = new BlockType[Defines.MAPSIZE, Defines.MAPSIZE, Defines.MAPSIZE];
                loaded = LoadLevel(Ssettings.LevelName);
                if (loaded == false)
                {
                    ConsoleWriteError("AUTOLOAD FAILED");
                }
                else
                {
                    ConsoleWriteSucces("AUTLOAD SUCCESFULL");
                }
            }
            if (loaded == false)
            {
                ConsoleWrite("GENERATING NEW MAP");
                GenerateNewMap();
                ConsoleWriteSucces("NEW MAP GENERATED");
                ConsoleWrite("MAPSIZE = [" + Defines.MAPSIZE + "] [" + Defines.MAPSIZE + "] [" + Defines.MAPSIZE + "]");
            }
             */

            ConsoleWrite("GENERATING NEW MAP WITH SEED(" + Msettings.Mapseed + ")");
            GenerateNewMap();
            ConsoleWriteSucces("NEW MAP GENERATED");

            _lastMapBackup = DateTime.Now;
            ServerListener listener = new ServerListener(_netServer, this);
            Thread listenerthread = new Thread(listener.Start);
            Thread physicsthread = new Thread(DoPhysics);

            DateTime lastFpsCheck = DateTime.Now;

            //Start netserver
            _netServer.Start();

            //Start listernerthread
            listenerthread.Start();

            //Start physicsthread
            physicsthread.Start();

            // Main server loop!
            ConsoleWriteSucces("SERVER READY");

            while (true)
            {
                // Check the state of our core threads
                if (!listenerthread.IsAlive)
                {
                    ConsoleWriteError("Listenerthread died");
                    ConsoleWriteError("Server is shutting down");
                    return false;
                }
                if (!physicsthread.IsAlive)
                {
                    ConsoleWriteError("Physicsthread died");
                    ConsoleWriteError("Server is shutting down");
                    return false;
                }

                // Fps for the server
                _frameCount = _frameCount + 1;
                if (lastFpsCheck <= DateTime.Now - TimeSpan.FromMilliseconds(1000))
                {
                    lastFpsCheck = DateTime.Now;
                    if (_frameCount <= 20)
                    {
                        ConsoleWrite("Heavy load: " + _frameCount + " FPS", ConsoleColor.Yellow);
                    }
                    _frameCount = 0;
                }

                // Dont send hearthbeats too fast
                TimeSpan updatehearthbeat = DateTime.Now - _lasthearthbeatsend;
                if (updatehearthbeat.TotalMilliseconds > 1000)
                {
                    SendHearthBeat();
                    _lasthearthbeatsend = DateTime.Now;
                }

                //Check the time
                DayManager.Update();
                SendDayTimeUpdate(DayManager.Light);

                //Time to backup map?
                // If Ssettings.autosavetimer is 0 then autosave is disabled
                if (Ssettings.Autosavetimer != 0)
                {
                    TimeSpan mapUpdateTimeSpan = DateTime.Now - _lastMapBackup;
                    if (mapUpdateTimeSpan.TotalMinutes > Ssettings.Autosavetimer)
                    {
                        ConsoleWrite("BACK-UP STARTED");
                        //Thread backupthread = new Thread(new ThreadStart(BackupLevel));
                        //backupthread.Start();
                        _lastMapBackup = DateTime.Now;
                        ConsoleWriteSucces("BACK-UP DONE");
                    }
                }

                //Update our lua files
                LuaManager.Update();

                // Handle console keypresses.
                while (Console.KeyAvailable)
                {
                    // What if there is constant keyavaible ?
                    // This code makes sure the rest of the program can also run
                    TimeSpan timeSpanLastKeyAvaible = DateTime.Now - _lastKeyAvaible;
                    if (timeSpanLastKeyAvaible.Milliseconds < 2000)
                    {
                        string input = Console.ReadLine();
                        ConsoleProcessInput(input);
                        _lastKeyAvaible = DateTime.Now;
                        break;
                    }
                }

                // Restart the server?
                if (_restartTriggered && DateTime.Now > _restartTime)
                {
                    DisconnectAllPlayers();
                    _netServer.Shutdown("serverrestart");
                    //BackupLevel();
                    return true;
                }

                if (_shutdownTriggerd && DateTime.Now > _shutdownTime)
                {
                    DisconnectAllPlayers();
                    _netServer.Shutdown("servershutdown");
                    //BackupLevel();
                    return false;
                }

                // Pass control over to waiting threads.
                Thread.Sleep(1);
            }
        }

        public void LoadSettings()
        {
            Datafile dataFile = new Datafile(Ssettings.SettingsDir + "/server.config.txt");

            if (dataFile.Data.ContainsKey("logs"))
                Ssettings.Logs = bool.Parse(dataFile.Data["logs"]);
            else
            {
                Ssettings.Logs = false;
                ConsoleWriteError("Couldnt find logs settings so we use the default (false)");
            }
            ConsoleWrite("LOADING SETTINGS");

            // Read in from the config file.
            if (dataFile.Data.ContainsKey("maxplayers"))
                Ssettings.Maxplayers = int.Parse(dataFile.Data["maxplayers"]);
            else
            {
                Ssettings.Maxplayers = 16;
                ConsoleWriteError("Couldnt find maxplayers setting so we use the default (16)");
            }

            if (dataFile.Data.ContainsKey("public"))
                Ssettings.Public = bool.Parse(dataFile.Data["public"]);
            else
            {
                Ssettings.Public = false;
                ConsoleWriteError("Couldnt find public setting so we use the default (FALSE)");
            }
            if (dataFile.Data.ContainsKey("proxy"))
                Ssettings.Proxy = bool.Parse(dataFile.Data["proxy"]);
            else
            {
                Ssettings.Proxy = false;
                ConsoleWriteError("Couldnt find proxy setting so we use the default (false)");
            }

            if (dataFile.Data.ContainsKey("servername"))
                Ssettings.Servername = dataFile.Data["servername"];
            else
            {
                Ssettings.Servername = "Default";
                ConsoleWriteError("Couldnt find servername setting so we use the default (Default)");
            }

            if (dataFile.Data.ContainsKey("levelname"))
                Ssettings.LevelName = dataFile.Data["levelname"];
            else
            {
                Ssettings.LevelName = "";
                ConsoleWriteError("Couldnt find levelname setting so we use the default ()");
            }

            if (dataFile.Data.ContainsKey("motd"))
                Ssettings.MOTD = dataFile.Data["motd"];
            else
            {
                Ssettings.MOTD = "Welcome";
                ConsoleWriteError("Couldnt find MOTD setting so we use the default (Welcome)");
            }

            if (dataFile.Data.ContainsKey("autoannounce"))
                Ssettings.AutoAnnouce = bool.Parse(dataFile.Data["autoannounce"]);
            else
            {
                Ssettings.AutoAnnouce = false;
                ConsoleWriteError("Couldnt find autoannounce setting so we use the default (false)");
            }

            if (dataFile.Data.ContainsKey("autosave"))
                Ssettings.Autosavetimer = int.Parse(dataFile.Data["autosave"]);
            else
            {
                Ssettings.Autosavetimer = 5;
                ConsoleWriteError("Couldnt find autosave setting so we use the default (5)");
            }

            if (dataFile.Data.ContainsKey("lightsteps"))
                Ssettings.Lightsteps = int.Parse(dataFile.Data["lightsteps"]);
            else
            {
                Ssettings.Lightsteps = 60;
                ConsoleWriteError("Couldnt find lightsteps setting so we use the default (60)");
            }

            if (!(Ssettings.Maxplayers >= 1 && Ssettings.Maxplayers <= 16))
            {
                Ssettings.Maxplayers = 16;
                ConsoleWriteError("The value of maxplayers must be between 1 and 16 for now");
                ConsoleWrite("Setting Maxplayers to 16");
            }

            if (!(Ssettings.Autosavetimer >= 0 && Ssettings.Autosavetimer <= 60))
            {
                Ssettings.Autosavetimer = 5;
                ConsoleWriteError("The value of autosave must be between 0 and 60 for now");
                ConsoleWrite("Setting autosave to 5");
            }
            ConsoleWriteSucces("SETTINGS LOADED");
        }

        public void LoadMapSettings()
        {
            ConsoleWrite("LOADING MAPSETTINGS");

            Datafile dataFile = new Datafile(Msettings.SettingsDir + "/map.config.txt");

            if (dataFile.Data.ContainsKey("includetrees"))
                Msettings.Includetrees = bool.Parse(dataFile.Data["includetrees"]);
            else
            {
                Msettings.Includetrees = true;
                ConsoleWriteError("Couldnt find includetrees setting so we use the default (true)");
            }

            if (dataFile.Data.ContainsKey("includelava"))
                Msettings.Includelava = bool.Parse(dataFile.Data["includelava"]);
            else
            {
                Msettings.Includelava = true;
                ConsoleWriteError("Couldnt find includelava setting so we use the default (true)");
            }

            if (dataFile.Data.ContainsKey("includeadminblocks"))
                Msettings.IncludeAdminblocks = bool.Parse(dataFile.Data["includeadminblocks"]);
            else
            {
                Msettings.IncludeAdminblocks = true;
                ConsoleWriteError("Couldnt find includeadminblocks setting so we use the default (true)");
            }

            if (dataFile.Data.ContainsKey("lavaspawns"))
                Msettings.Lavaspawns = int.Parse(dataFile.Data["lavaspawns"]);
            else
            {
                Msettings.Lavaspawns = 0;
                ConsoleWriteError("Couldnt find lavaspawns setting so we use the default (0)");
            }

            if (dataFile.Data.ContainsKey("treecount"))
                Msettings.Treecount = int.Parse(dataFile.Data["treecount"]);
            else
            {
                Msettings.Treecount = 0;
                ConsoleWriteError("Couldnt find treecount setting so we use the default (0)");
            }

            if (dataFile.Data.ContainsKey("orefactor"))
                Msettings.Orefactor = int.Parse(dataFile.Data["orefactor"]);
            else
            {
                Msettings.Orefactor = 0;
                ConsoleWriteError("Couldnt find orefactor setting so we use the default (0)");
            }

            if (dataFile.Data.ContainsKey("includewater"))
                Msettings.Includewater = bool.Parse(dataFile.Data["includewater"]);
            else
            {
                Msettings.Includewater = true;
                ConsoleWriteError("Couldnt find includewater setting so we use the default (true)");
            }

            if (dataFile.Data.ContainsKey("waterspawns"))
                Msettings.Waterspawns = int.Parse(dataFile.Data["waterspawns"]);
            else
            {
                Msettings.Waterspawns = 0;
                ConsoleWriteError("Couldnt find waterspawns setting so we use the default (0)");
            }

            Msettings.MapsizeX = 256;
            Msettings.MapsizeY = 256;
            Msettings.MapsizeZ = 256;

            ConsoleWriteSucces("MAPSETTINGS LOADED");
        }

        public void LoadExtraSettings()
        {
            ConsoleWrite("LOADING EXTRASETTINGS");

            //ToDo Load them from the file
            //File is already inplace
            SAsettings.Deathbydrowned = "[name] drowned";
            SAsettings.Deathbyfall = "[name] felt";
            SAsettings.Deathbylava = "[name] burned";
            SAsettings.Deathbyoutofbounds = "[name] felt of the map";
            SAsettings.Playerhealth = 100;
            SAsettings.Playerregenrate = 1;

            ConsoleWriteSucces("EXTRASETTINGS LOADED");
        }

        public void LoadDirectorys()
        {
            ConsoleWrite("LOADING DIRECTORYS");

            Ssettings.SettingsDir = "ServerConfigs";
            Ssettings.LogsDir = "Logs";
            Ssettings.BackupDir = "Backups";
            Ssettings.ScriptsDir = "Scripts";
            Msettings.SettingsDir = "ServerConfigs";

            ConsoleWriteSucces("DIRECTORYS LOADED");
        }

        public void LoadScriptFiles()
        {
            ConsoleWrite("LOADING SCRIPTS");

            LuaManager.LoadScriptFiles(Ssettings.ScriptsDir);

            ConsoleWriteSucces("SCRIPTS LOADED");
        }
    }
}