using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Lidgren.Network;
using Lidgren.Network.Xna;

namespace MineWorld
{
    public class MineWorldGame : StateMasher.StateMachine
    {
        public ClientSettings Csettings = new ClientSettings();
        double timeSinceLastUpdate = 0;
        NetBuffer msgBuffer = null;
        Song songTitle = null;

        public KeyBindHandler keyBinds = new KeyBindHandler();

        public bool anyPacketsReceived = false;

        public IPAddress IPargument = null;

        public MineWorldGame(string[] args)
        {
            if (args.Length > 0)
            {
                IPAddress.TryParse(args[0], out IPargument);
            }
        }

        public void JoinGame(IPEndPoint serverEndPoint)
        {
            anyPacketsReceived = false;
            // Clear out the map load progress indicator.
            propertyBag.mapLoadProgress = new bool[Defines.MAPSIZE,Defines.MAPSIZE];
            for (int i = 0; i < Defines.MAPSIZE; i++)
                for (int j=0; j< Defines.MAPSIZE; j++)
                    propertyBag.mapLoadProgress[i,j] = false;

            // Create our connect message.
            NetBuffer connectBuffer = propertyBag.netClient.CreateBuffer();
            connectBuffer.Write(propertyBag.playerHandle);
            connectBuffer.Write(Defines.MINEWORLD_VER);

            // Connect to the server.
            propertyBag.netClient.Connect(serverEndPoint, connectBuffer.ToArray());
        }

        public List<ServerInformation> EnumerateServers(float discoveryTime)
        {
            List<ServerInformation> serverList = new List<ServerInformation>();

            // Discover local servers.
            propertyBag.netClient.DiscoverLocalServers(5565);
            NetBuffer msgBuffer = propertyBag.netClient.CreateBuffer();
            NetMessageType msgType;
            float timeTaken = 0;
            while (timeTaken < discoveryTime)
            {
                while (propertyBag.netClient.ReadMessage(msgBuffer, out msgType))
                {
                    if (msgType == NetMessageType.ServerDiscovered)
                    {
                        bool serverFound = false;
                        ServerInformation serverInfo = new ServerInformation(msgBuffer);
                        foreach (ServerInformation si in serverList)
                            if (si.Equals(serverInfo))
                                serverFound = true;
                        if (!serverFound)
                            serverList.Add(serverInfo);
                    }
                }

                timeTaken += 0.1f;
                Thread.Sleep(100);
            }

            // Discover remote servers.
            try
            {
                string publicList = HttpRequest.Get(Defines.MASTERSERVER_BASE_URL + "servers.php", null);
                foreach (string s in publicList.Split("\r\n".ToCharArray()))
                {
                    string[] args = s.Split(";".ToCharArray());
                    if (args.Length == 6)
                    {
                        IPAddress serverIp;
                        if (IPAddress.TryParse(args[1], out serverIp))
                        {
                            ServerInformation serverInfo = new ServerInformation(serverIp, args[0], args[2], args[3], args[4],args[5]);
                            serverList.Add(serverInfo);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return serverList;
        }

        public void UpdateNetwork(GameTime gameTime)
        {
            // Update the server with our status.
            timeSinceLastUpdate += gameTime.ElapsedGameTime.TotalSeconds;
            if (timeSinceLastUpdate > 0.05)
            {
                timeSinceLastUpdate = 0;
                if (CurrentStateType == "MineWorld.States.MainGameState")
                    propertyBag.SendPlayerUpdate();
            }

            // Recieve messages from the server.
            NetMessageType msgType;
            while (propertyBag.netClient.ReadMessage(msgBuffer, out msgType))
            {
                switch (msgType)
                {
                    case NetMessageType.StatusChanged:
                        {
                            if (propertyBag.netClient.Status == NetConnectionStatus.Disconnected)
                                ChangeState("MineWorld.States.ServerBrowserState");
                        }
                        break;
                    case NetMessageType.ConnectionApproval:
                        anyPacketsReceived = true;
                        break;
                    case NetMessageType.ConnectionRejected:
                        {
                            anyPacketsReceived = false;
                            try
                            {
                                string reason = msgBuffer.ReadString();
                                if (reason.Length != 0)
                                {
                                    switch (reason)
                                    {
                                        case "bannedname":
                                            {
                                                propertyBag.connectionerror = "Error: The name you choosed is banned from the server";
                                                propertyBag.connectionerrornewstate = "MineWorld.States.SettingsState";
                                                break;
                                            }
                                        case "noname":
                                            {
                                                propertyBag.connectionerror = "Error: You didnt choose a name";
                                                propertyBag.connectionerrornewstate = "MineWorld.States.SettingsState";
                                                break;
                                            }
                                        case "changename":
                                            {
                                                propertyBag.connectionerror = "Error: You need to change your name you cannot choose name (player)";
                                                propertyBag.connectionerrornewstate = "MineWorld.States.SettingsState";
                                                break;
                                            }
                                        case "versionwrong":
                                            {
                                                propertyBag.connectionerror = "Error: Your client is out of date consider updating";
                                                propertyBag.connectionerrornewstate = "MineWorld.States.ServerBrowserState";
                                                break;
                                            }
                                        case "banned":
                                            {
                                                propertyBag.connectionerror = "Error: You are banned from this server";
                                                propertyBag.connectionerrornewstate = "MineWorld.States.ServerBrowserState";
                                                break;
                                            }
                                        case "serverisfull":
                                            {
                                                propertyBag.connectionerror = "Error: The server is full";
                                                propertyBag.connectionerrornewstate = "MineWorld.States.ServerBrowserState";
                                                break;
                                            }
                                        default:
                                            {
                                                propertyBag.connectionerror = "Error: Unknow error";
                                                propertyBag.connectionerrornewstate = "MineWorld.States.ServerBrowserState";
                                                break;
                                            }
                                    }
                                }
                            }
                            catch 
                            {
                                propertyBag.connectionerror = "Error: Unknow error";
                                propertyBag.connectionerrornewstate = "MineWorld.States.SettingsState";
                            }
                            ChangeState("MineWorld.States.ErrorState");
                        }
                        break;

                    case NetMessageType.Data:
                        {
                            try
                            {
                                MineWorldMessage dataType = (MineWorldMessage)msgBuffer.ReadByte();
                                switch (dataType)
                                {
                                    case MineWorldMessage.BlockBulkTransfer:
                                        {
                                            anyPacketsReceived = true;

                                            try
                                            {
                                                byte x;
                                                byte y;

                                                x = msgBuffer.ReadByte();
                                                y = msgBuffer.ReadByte();
                                                propertyBag.mapLoadProgress[x, y] = true;
                                                for (byte dy = 0; dy < Defines.PACKETSIZE; dy++)
                                                {
                                                    for (byte z = 0; z < Defines.MAPSIZE; z++)
                                                    {
                                                        BlockType blockType = (BlockType)msgBuffer.ReadByte();
                                                        if (blockType != BlockType.None)
                                                        {
                                                            propertyBag.blockEngine.downloadList[x, y + dy, z] = blockType;
                                                        }
                                                    }
                                                }
                                                bool downloadComplete = true;
                                                for (x = 0; x < Defines.MAPSIZE; x++)
                                                    for (y = 0; y < Defines.MAPSIZE; y += Defines.PACKETSIZE)
                                                        if (propertyBag.mapLoadProgress[x, y] == false)
                                                        {
                                                            downloadComplete = false;
                                                            break;
                                                        }
                                                if (downloadComplete)
                                                {
                                                    ChangeState("MineWorld.States.MainGameState");
                                                    if (!Csettings.NoSound)
                                                        MediaPlayer.Stop();
                                                    propertyBag.blockEngine.DownloadComplete();
                                                }
                                            }
                                            catch
                                            {
                                                propertyBag.connectionerror = "Map Bulk Transfer Failed";
                                                propertyBag.connectionerrornewstate = "MineWorld.States.ServerBrowserState";
                                                ChangeState("MineWorld.States.ErrorState");
                                            }
                                        }
                                        break;

                                    case MineWorldMessage.DayUpdate:
                                        {
                                            propertyBag.time = msgBuffer.ReadFloat();
                                            break;
                                        }
                                    case MineWorldMessage.HealthUpdate:
                                        {
                                            // Health, Healthmax both int
                                            propertyBag.playerHealth = msgBuffer.ReadInt32();
                                            propertyBag.playerHealthMax = msgBuffer.ReadInt32();
                                        }
                                        break;
                                    case MineWorldMessage.PlayerPosition:
                                        {
                                            propertyBag.playerPosition = msgBuffer.ReadVector3();
                                            break;
                                        }
                                    case MineWorldMessage.PlayerRespawn:
                                        {
                                            propertyBag.playerPosition = msgBuffer.ReadVector3();
                                            propertyBag.allowRespawn = true;
                                            break;
                                        }
                                    case MineWorldMessage.BlockSet:
                                        {
                                            // x, y, z, type, all bytes
                                            byte x = msgBuffer.ReadByte();
                                            byte y = msgBuffer.ReadByte();
                                            byte z = msgBuffer.ReadByte();

                                            BlockType blockType = (BlockType)msgBuffer.ReadByte();
                                            if (blockType == BlockType.None)
                                            {
                                                if (propertyBag.blockEngine.BlockAtPoint(new Vector3(x, y, z)) != BlockType.None)
                                                {
                                                    propertyBag.blockEngine.RemoveBlock(x, y, z);
                                                }
                                            }
                                            else
                                            {
                                                if (propertyBag.blockEngine.BlockAtPoint(new Vector3(x, y, z)) != BlockType.None)
                                                {
                                                    propertyBag.blockEngine.RemoveBlock(x, y, z);
                                                }
                                                propertyBag.blockEngine.AddBlock(x, y, z, blockType);
                                            }
                                        }
                                        break;

                                    case MineWorldMessage.TriggerExplosion:
                                        {
                                            Vector3 blockPos = msgBuffer.ReadVector3();

                                            // Play the explosion sound.
                                            propertyBag.PlaySound(MineWorldSound.Explosion, blockPos);

                                            // Create some particles.
                                            propertyBag.particleEngine.CreateExplosionDebris(blockPos);

                                            // Figure out what the effect is.
                                            float distFromExplosive = (blockPos + 0.5f * Vector3.One - propertyBag.playerPosition).Length();
                                            if (distFromExplosive < 3)
                                                //if (propertyBag.godmode == false)
                                                //{
                                                //propertyBag.KillPlayer(Defines.deathByExpl);//"WAS KILLED IN AN EXPLOSION!");
                                                //}
                                                /*else*/
                                                if (distFromExplosive < 8)
                                                {
                                                    // If we're not in explosion mode, turn it on with the minimum ammount of shakiness.
                                                    if (propertyBag.screenEffect != ScreenEffect.Explosion)
                                                    {
                                                        propertyBag.screenEffect = ScreenEffect.Explosion;
                                                        propertyBag.screenEffectCounter = 2;
                                                    }
                                                    // If this bomb would result in a bigger shake, use its value.
                                                    propertyBag.screenEffectCounter = Math.Min(propertyBag.screenEffectCounter, (distFromExplosive - 2) / 5);
                                                }
                                        }
                                        break;

                                    case MineWorldMessage.PlayerJoined:
                                        {
                                            int playerId = msgBuffer.ReadInt32();
                                            string playerName = msgBuffer.ReadString();
                                            bool thisIsMe = msgBuffer.ReadBoolean();
                                            bool playerAlive = msgBuffer.ReadBoolean();
                                            propertyBag.playerList[playerId] = new ClientPlayer(null, (Game)this);
                                            propertyBag.playerList[playerId].Name = playerName;
                                            propertyBag.playerList[playerId].ID = playerId;
                                            propertyBag.playerList[playerId].Alive = playerAlive;
                                            propertyBag.playerList[playerId].Owncolor = Csettings.color;
                                            if (thisIsMe)
                                                propertyBag.playerMyId = playerId;
                                        }
                                        break;

                                    case MineWorldMessage.PlayerLeft:
                                        {
                                            int playerId = msgBuffer.ReadInt32();
                                            if (propertyBag.playerList.ContainsKey(playerId))
                                                propertyBag.playerList.Remove(playerId);
                                        }
                                        break;

                                    case MineWorldMessage.Killed:
                                        {
                                            //We received the death command :(
                                            string reason = msgBuffer.ReadString();
                                            propertyBag.KillPlayer(reason);
                                            break;
                                        }

                                    case MineWorldMessage.PlayerDead:
                                        {
                                            int playerId = msgBuffer.ReadInt32();
                                            if (propertyBag.playerList.ContainsKey(playerId))
                                            {
                                                ClientPlayer player = propertyBag.playerList[playerId];
                                                player.Alive = false;
                                                propertyBag.particleEngine.CreateBloodSplatter(player.Position, player.Owncolor);
                                                if (playerId != propertyBag.playerMyId)
                                                    propertyBag.PlaySound(MineWorldSound.Death, player.Position);
                                            }
                                        }
                                        break;

                                    case MineWorldMessage.PlayerAlive:
                                        {
                                            int playerId = msgBuffer.ReadInt32();
                                            if (propertyBag.playerList.ContainsKey(playerId))
                                            {
                                                ClientPlayer player = propertyBag.playerList[playerId];
                                                player.Alive = true;
                                            }
                                        }
                                        break;

                                    case MineWorldMessage.PlayerUpdate:
                                        {
                                            int playerId = msgBuffer.ReadInt32();
                                            if (propertyBag.playerList.ContainsKey(playerId))
                                            {
                                                ClientPlayer player = propertyBag.playerList[playerId];
                                                player.UpdatePosition(msgBuffer.ReadVector3(), gameTime.TotalGameTime.TotalSeconds);
                                                player.Heading = msgBuffer.ReadVector3();
                                            }
                                        }
                                        break;

                                    case MineWorldMessage.ChatMessage:
                                        {
                                            ChatMessageType chatType = (ChatMessageType)msgBuffer.ReadByte();
                                            string chatString = Defines.Sanitize(msgBuffer.ReadString());
                                            string author = Defines.Sanitize(msgBuffer.ReadString());
                                            propertyBag.addChatMessage(chatString, chatType, author);
                                        }
                                        break;
                                    case MineWorldMessage.PlaySound:
                                        {
                                            MineWorldSound sound = (MineWorldSound)msgBuffer.ReadByte();
                                            bool hasPosition = msgBuffer.ReadBoolean();
                                            if (hasPosition)
                                            {
                                                Vector3 soundPosition = msgBuffer.ReadVector3();
                                                propertyBag.PlaySound(sound, soundPosition);
                                            }
                                            else
                                                propertyBag.PlaySound(sound);
                                        }
                                        break;
                                    case MineWorldMessage.Hearthbeat:
                                        {
                                            propertyBag.lasthearthbeatreceived = DateTime.Now;
                                            break;
                                        }
                                }
                            }
                            catch { } //Error in a received message
                        }
                        break;
                }
            }
            // Make sure our network thread actually gets to run.
            Thread.Sleep(1);
        }

        protected override void Initialize()
        {
            Csettings.Directory = "ClientConfigs";
            Csettings.playerHandle = "Player";
            Csettings.volumeLevel = 1.0f;
            Csettings.RenderPretty = true;
            Csettings.DrawFrameRate = false;
            Csettings.InvertMouseYAxis = false;
            Csettings.NoSound = false;
            Csettings.mouseSensitivity = 0.005f;
            Csettings.color = Color.Red;
            Csettings.Width = 1024;
            Csettings.Height = 768;
            Csettings.Fullscreen = false;
            Csettings.Vsync = true;

            //Now moving to DatafileWriter only since it can read and write
            Datafile dataFile = new Datafile(Csettings.Directory + "/client.config.txt");
            if (dataFile.Data.ContainsKey("width"))
                Csettings.Width = int.Parse(dataFile.Data["width"], System.Globalization.CultureInfo.InvariantCulture);
            if (dataFile.Data.ContainsKey("height"))
                Csettings.Height = int.Parse(dataFile.Data["height"], System.Globalization.CultureInfo.InvariantCulture);
            if (dataFile.Data.ContainsKey("fullscreen"))
                Csettings.Fullscreen = bool.Parse(dataFile.Data["fullscreen"]);
            if (dataFile.Data.ContainsKey("vsync"))
                Csettings.Vsync = bool.Parse(dataFile.Data["vsync"]);
            if (dataFile.Data.ContainsKey("handle"))
                Csettings.playerHandle = dataFile.Data["handle"];
            if (dataFile.Data.ContainsKey("showfps"))
                Csettings.DrawFrameRate = bool.Parse(dataFile.Data["showfps"]);
            if (dataFile.Data.ContainsKey("yinvert"))
                Csettings.InvertMouseYAxis = bool.Parse(dataFile.Data["yinvert"]);
            if (dataFile.Data.ContainsKey("nosound"))
                Csettings.NoSound = bool.Parse(dataFile.Data["nosound"]);
            if (dataFile.Data.ContainsKey("pretty"))
                Csettings.RenderPretty = bool.Parse(dataFile.Data["pretty"]);
            if (dataFile.Data.ContainsKey("volume"))
                Csettings.volumeLevel = Math.Max(0, Math.Min(1, float.Parse(dataFile.Data["volume"], System.Globalization.CultureInfo.InvariantCulture)));
            if (dataFile.Data.ContainsKey("sensitivity"))
                Csettings.mouseSensitivity = Math.Max(0.001f, Math.Min(0.05f, float.Parse(dataFile.Data["sensitivity"], System.Globalization.CultureInfo.InvariantCulture) / 1000f));
            if (dataFile.Data.ContainsKey("color"))
            {
                if (dataFile.Data["color"] == "red")
                {
                    Csettings.color = Color.Red;
                }
                else
                {
                    Csettings.color = Color.Blue;
                }
            }


            //Now to read the key bindings
            if (!File.Exists(Csettings.Directory + "/keymap.txt"))
            {
                FileStream temp = File.Create(Csettings.Directory + "/keymap.txt");
                temp.Close();
                //Console.WriteLine("Keymap file does not exist, creating.");
            }
            dataFile = new Datafile(Csettings.Directory + "/keymap.txt");
            bool anyChanged = false;
            foreach (string key in dataFile.Data.Keys)
            {
                try
                {
                    KeyBoardButtons button = (KeyBoardButtons)Enum.Parse(typeof(KeyBoardButtons),dataFile.Data[key],true);
                    if (Enum.IsDefined(typeof(KeyBoardButtons), button))
                    {
                        if (keyBinds.BindKey(button, key, true))
                        {
                            anyChanged = true;
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Enum not defined for " + dataFile.Data[key] + ".");
                    }
                } catch { }
            }

            //If no keys are bound in this manner then create the default set
            if (!anyChanged)
            {
                keyBinds.CreateDefaultSet();
                keyBinds.SaveBinds(dataFile, Csettings.Directory + "/keymap.txt");
                //Console.WriteLine("Creating default keymap...");
            }

            graphicsDeviceManager.IsFullScreen = Csettings.Fullscreen;
            graphicsDeviceManager.PreferredBackBufferHeight = Csettings.Height;
            graphicsDeviceManager.PreferredBackBufferWidth = Csettings.Width;
            graphicsDeviceManager.SynchronizeWithVerticalRetrace = Csettings.Vsync;
            graphicsDeviceManager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            this.IsFixedTimeStep = false;
            graphicsDeviceManager.ApplyChanges();
            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            propertyBag.netClient.Shutdown("Client exiting.");
            
            base.OnExiting(sender, args);
        }

        public void ResetPropertyBag()
        {
            if (propertyBag != null)
                propertyBag.netClient.Shutdown("");

            propertyBag = new PropertyBag(this);
            propertyBag.playerHandle = Csettings.playerHandle;
            propertyBag.volumeLevel = Csettings.volumeLevel;
            propertyBag.mouseSensitivity = Csettings.mouseSensitivity;
            propertyBag.keyBinds = keyBinds;
            propertyBag.Owncolor = Csettings.color;
            msgBuffer = propertyBag.netClient.CreateBuffer();
        }

        protected override void LoadContent()
        {
            // Initialize the property bag.
            ResetPropertyBag();

            // Set the initial state to team selection
            ChangeState("MineWorld.States.TitleState");

            // Play the title music.
            if (!Csettings.NoSound)
            {
                songTitle = Content.Load<Song>("song_title");
                MediaPlayer.Play(songTitle);
                MediaPlayer.Volume = propertyBag.volumeLevel;
            }
        }
    }
}
