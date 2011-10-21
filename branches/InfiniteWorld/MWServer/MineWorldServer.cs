using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MineWorld
{
    public partial class MineWorldServer
    {
        Random randomizer = new Random();

        MineWorldNetServer netServer = null;
        public DayManager dayManager = null;
        public LuaManager luaManager = null;
        public Dictionary<NetConnection, ServerPlayer> playerList = new Dictionary<NetConnection, ServerPlayer>();
        public List<NetConnection> toGreet = new List<NetConnection>();
        public List<string> admins = new List<string>(); //List of strings with all the admins
        public List<string> bannednames = new List<string>(); // List of strings with all names that cannot be chosen
        public List<string> banList = new List<string>(); // List of string with all thhe banned ip's
        List<MapSender> mapSendingProgress = new List<MapSender>();

        DateTime lasthearthbeatsend = DateTime.Now;
        DateTime lastServerListUpdate = DateTime.Now;
        DateTime lastMapBackup = DateTime.Now;
        DateTime lastKeyAvaible = DateTime.Now;

        public string serverIP;
        public string sessionid;
        int frameCount = 100;

        public bool StopFluids;

        bool keepRunning = true;

        // Server restarting variables.
        DateTime restartTime = DateTime.Now;
        bool restartTriggered = false;

        // Server shutdown variables.
        DateTime shutdownTime = DateTime.Now;
        bool shutdownTriggerd = false;

        public ServerSettings Ssettings = new ServerSettings();
        public ServerExtraSettings SAsettings = new ServerExtraSettings();
        public MapSettings Msettings = new MapSettings();

        //TODO Find a proper place
        // Fluids count
        int Totallavablockcount;
        int Totalwaterblockcount;

        public string GetExternalIp()
        {
            //TODO Remove hardcoded external ip
            string ip = "0.0.0.0";
            //string ip = HttpRequest.Get(Defines.MASTERSERVER_BASE_URL + "ip.php", null);
            return ip;
        }

        public bool Start()
        {
            //Find a better place for this
            luaManager = new LuaManager(this);

            //Display server version in console
            ConsoleWrite(Defines.MINEWORLDSERVER_VERSION, ConsoleColor.Cyan);

            // Load the directory's.
            LoadDirectorys();

            // Load the general-settings.
            LoadSettings();

            // Load extra settings.
            LoadExtraSettings();

            // Load the map-settings.
            LoadMapSettings();

            // Load the ban-list.
            banList = LoadBanList();

            // Load the admin-list.
            admins = LoadAdminList();

            // Load the bannednames-list.
            bannednames = LoadBannedNames();

            // Load scripts
            LoadScriptFiles();

            // Initialize the server.
            NetConfiguration netConfig = new NetConfiguration("MineWorldPlus");
            netConfig.MaxConnections = Ssettings.Maxplayers;
            netConfig.Port = 5565;
            netServer = new MineWorldNetServer(netConfig);
            netServer.SetMessageTypeEnabled(NetMessageType.ConnectionApproval, true);
            //netServer.SimulatedMinimumLatency = 0.1f;
            //netServer.SimulatedLatencyVariance = 0.05f;
            //netServer.SimulatedLoss = 0.1f;
            //netServer.SimulatedDuplicates = 0.05f;

            // Initialize the daymanager.
            dayManager = new DayManager(Ssettings.Lightsteps);

            // Store the last time that we did a physics calculation.
            DateTime lastCalc = DateTime.Now;

            //Display external IP
            //Dont bother if it isnt public
            if (Ssettings.Public == true)
            {
                serverIP = GetExternalIp();
                ConsoleWrite("Your external IP Adress: " + serverIP);
            }
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

            int tempblocks = Defines.MAPSIZE * Defines.MAPSIZE * Defines.MAPSIZE;
            ConsoleWrite("TOTAL BLOCKS = " + tempblocks.ToString("n0"));
            ConsoleWrite("TOTAL LAVA BLOCKS = " + Totallavablockcount);
            ConsoleWrite("TOTAL WATER BLOCKS = " + Totalwaterblockcount);

            lastMapBackup = DateTime.Now;
            ServerListener listener = new ServerListener(netServer, this);
            Thread listenerthread = new Thread(new ThreadStart(listener.start));
            Thread physicsthread = new Thread(new ThreadStart(DoPhysics));

            DateTime lastFPScheck = DateTime.Now;
            double frameRate = 0;

            //Start netserver
            netServer.Start();

            //Start listernerthread
            listenerthread.Start();

            //Start physicsthread
            physicsthread.Start();

            // Main server loop!
            ConsoleWriteSucces("SERVER READY");

            while (keepRunning)
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
                frameCount = frameCount + 1;
                if (lastFPScheck <= DateTime.Now - TimeSpan.FromMilliseconds(1000))
                {
                    lastFPScheck = DateTime.Now;
                    frameRate = frameCount;
                    if (frameCount <= 20)
                    {
                        ConsoleWrite("Heavy load: " + frameCount + " FPS", ConsoleColor.Yellow);
                    }
                    frameCount = 0;
                }

                // Dont send hearthbeats too fast
                TimeSpan updatehearthbeat = DateTime.Now - lasthearthbeatsend;
                if (updatehearthbeat.TotalMilliseconds > 1000)
                {
                    SendHearthBeat();
                    lasthearthbeatsend = DateTime.Now;
                }

                //Check the time
                dayManager.Update();

                // Look if the time is changed so that we tell the clients
                if (dayManager.Timechanged())
                {
                    SendDayTimeUpdate(dayManager.Light);
                }

                //Time to backup map?
                // If Ssettings.autosavetimer is 0 then autosave is disabled
                if (Ssettings.Autosavetimer != 0)
                {
                    TimeSpan mapUpdateTimeSpan = DateTime.Now - lastMapBackup;
                    if (mapUpdateTimeSpan.TotalMinutes > Ssettings.Autosavetimer)
                    {
                        ConsoleWrite("BACK-UP STARTED");
                        Thread backupthread = new Thread(new ThreadStart(BackupLevel));
                        backupthread.Start();
                        lastMapBackup = DateTime.Now;
                        ConsoleWriteSucces("BACK-UP DONE");
                    }
                }

                //Time to terminate finished map sending threads?
                TerminateFinishedThreads();

                //Update our lua files
                luaManager.Update();

                // Handle console keypresses.
                while (Console.KeyAvailable)
                {
                    // What if there is constant keyavaible ?
                    // This code makes sure the rest of the program can also run
                    TimeSpan timeSpanLastKeyAvaible = DateTime.Now - lastKeyAvaible;
                    if (timeSpanLastKeyAvaible.Milliseconds < 2000)
                    {
                        string input = Console.ReadLine();
                        ConsoleProcessInput(input);
                        lastKeyAvaible = DateTime.Now;
                        break;
                    }
                }

                // Restart the server?
                if (restartTriggered && DateTime.Now > restartTime)
                {
                    DisconnectAllPlayers();
                    netServer.Shutdown("serverrestart");
                    BackupLevel();
                    return true;
                }

                if (shutdownTriggerd && DateTime.Now > shutdownTime)
                {
                    DisconnectAllPlayers();
                    netServer.Shutdown("servershutdown");
                    BackupLevel();
                    return false;
                }

                // Pass control over to waiting threads.
                Thread.Sleep(1);
            }
            return false;
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

            /*
            int temp;

            if (dataFile.Data.ContainsKey("mapsize"))
            {
                temp = int.Parse(dataFile.Data["mapsize"]);
                switch (temp)
                {
                    case 1:
                        {
                            Msettings.Mapsize = Mapsize.Small;
                            break;
                        }
                    case 2:
                        {
                            Msettings.Mapsize = Mapsize.Normal;
                            break;
                        }
                    case 3:
                        {
                            Msettings.Mapsize = Mapsize.Large;
                            break;
                        }
                }
            }
            else
            {
                Msettings.Mapsize = Mapsize.Normal;
                ConsoleWrite("Couldnt find mapsize settings so we use the default ((2)normal)");
            }

            if (!(Msettings.Mapsize == Mapsize.Small || Msettings.Mapsize == Mapsize.Normal || Msettings.Mapsize == Mapsize.Large))
            {
                Msettings.Mapsize = Mapsize.Normal;
                ConsoleWrite("Invalid number in mapsize settings so we use the default ((2)normal)");
            }*/

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

            luaManager.LoadScriptFiles(Ssettings.ScriptsDir);

            ConsoleWriteSucces("SCRIPTS LOADED");
        }
    }
}