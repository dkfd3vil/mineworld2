using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MineWorld.StateMasher;

namespace MineWorld
{
    public class MineWorldGame : StateMachine
    {
        public ClientSettings Csettings;
        public IPAddress Pargument;
        public KeyBindHandler KeyBinds = new KeyBindHandler();
        private NetBuffer _msgBuffer;
        private Song _songTitle;
        private double _timeSinceLastUpdate;

        public MineWorldGame(string[] args)
        {
            if (args.Length > 0)
            {
                IPAddress.TryParse(args[0], out Pargument);
            }
        }

        public void JoinGame(IPEndPoint serverEndPoint)
        {
            // Create our connect message.
            NetBuffer connectBuffer = PropertyBag.NetClient.CreateBuffer();
            connectBuffer.Write(Defines.MineworldBuild);
            connectBuffer.Write(PropertyBag.PlayerHandle);

            // Connect to the server.
            PropertyBag.NetClient.Connect(serverEndPoint, connectBuffer.ToArray());
        }

        public List<ServerInformation> EnumerateServers(float discoveryTime)
        {
            List<ServerInformation> serverList = new List<ServerInformation>();

            // Discover local servers.
            PropertyBag.NetClient.DiscoverLocalServers(5565);
            NetBuffer msgBuffer = PropertyBag.NetClient.CreateBuffer();
            float timeTaken = 0;
            while (timeTaken < discoveryTime)
            {
                NetMessageType msgType;
                while (PropertyBag.NetClient.ReadMessage(msgBuffer, out msgType))
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
            /*
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
            */
            return serverList;
        }

        public void UpdateNetwork(GameTime gameTime)
        {
            // Update the server with our status.
            _timeSinceLastUpdate += gameTime.ElapsedGameTime.TotalSeconds;
            if (_timeSinceLastUpdate > 0.05)
            {
                _timeSinceLastUpdate = 0;
                if (CurrentStateType == "MineWorld.States.MainGameState")
                    PropertyBag.SendPlayerUpdate();
            }

            // Recieve messages from the server.
            NetMessageType msgType;
            while (PropertyBag.NetClient.ReadMessage(_msgBuffer, out msgType))
            {
                switch (msgType)
                {
                    case NetMessageType.StatusChanged:
                        {
                            if (PropertyBag.NetClient.Status == NetConnectionStatus.Disconnected)
                            {
                                try
                                {
                                    string reason = _msgBuffer.ReadString();
                                    if (reason.Length != 0)
                                    {
                                        switch (reason)
                                        {
                                            case "kicked":
                                                {
                                                    ErrorManager.ErrorMsg = "You have been kicked from the server";
                                                    ErrorManager.NewState = "MineWorld.States.ServerBrowserState";
                                                    break;
                                                }
                                            case "banned":
                                                {
                                                    ErrorManager.ErrorMsg = "You have been banned from the server";
                                                    ErrorManager.NewState = "MineWorld.States.ServerBrowserState";
                                                    break;
                                                }
                                            case "disconnected":
                                                {
                                                    ErrorManager.ErrorMsg = "You have been disconnected from the server";
                                                    ErrorManager.NewState = "MineWorld.States.ServerBrowserState";
                                                    break;
                                                }
                                            default:
                                                {
                                                    ErrorManager.ErrorMsg = "Error: ( " + reason + " )";
                                                    ErrorManager.NewState = "MineWorld.States.ServerBrowserState";
                                                    break;
                                                }
                                        }
                                    }
                                }
                                catch
                                {
                                    ErrorManager.ErrorMsg = "Error: Unknow error";
                                    ErrorManager.NewState = "MineWorld.States.ServerBrowserState";
                                }
                                ChangeState("MineWorld.States.ErrorState");
                            }
                        }
                        break;
                    case NetMessageType.ConnectionApproval:
                        {
                            break;
                        }
                    case NetMessageType.ConnectionRejected:
                        {
                            try
                            {
                                string reason = _msgBuffer.ReadString();
                                if (reason.Length != 0)
                                {
                                    switch (reason)
                                    {
                                        case "bannedname":
                                            {
                                                ErrorManager.ErrorMsg =
                                                    "Error: The name you choosed is banned from the server";
                                                ErrorManager.NewState = "MineWorld.States.SettingsState";
                                                break;
                                            }
                                        case "noname":
                                            {
                                                ErrorManager.ErrorMsg = "Error: You didnt choose a name";
                                                ErrorManager.NewState = "MineWorld.States.SettingsState";
                                                break;
                                            }
                                        case "changename":
                                            {
                                                ErrorManager.ErrorMsg =
                                                    "Error: You need to change your name you cannot choose name (player)";
                                                ErrorManager.NewState = "MineWorld.States.SettingsState";
                                                break;
                                            }
                                        case "versionwrong":
                                            {
                                                ErrorManager.ErrorMsg =
                                                    "Error: Your client is out of date consider updating";
                                                ErrorManager.NewState = "MineWorld.States.ServerBrowserState";
                                                break;
                                            }
                                        case "banned":
                                            {
                                                ErrorManager.ErrorMsg = "Error: You are banned from this server";
                                                ErrorManager.NewState = "MineWorld.States.ServerBrowserState";
                                                break;
                                            }
                                        case "serverisfull":
                                            {
                                                ErrorManager.ErrorMsg = "Error: The server is full";
                                                ErrorManager.NewState = "MineWorld.States.ServerBrowserState";
                                                break;
                                            }
                                        default:
                                            {
                                                ErrorManager.ErrorMsg = "Error: Unknow error";
                                                ErrorManager.NewState = "MineWorld.States.ServerBrowserState";
                                                break;
                                            }
                                    }
                                }
                            }
                            catch
                            {
                                ErrorManager.ErrorMsg = "Error: Unknow error";
                                ErrorManager.NewState = "MineWorld.States.ServerBrowserState";
                            }
                            ChangeState("MineWorld.States.ErrorState");
                        }
                        break;

                    case NetMessageType.Data:
                        {
                            try
                            {
                                MineWorldMessage dataType = (MineWorldMessage) _msgBuffer.ReadByte();
                                switch (dataType)
                                {
                                    case MineWorldMessage.BlockBulkTransfer:
                                        {
                                            int x = _msgBuffer.ReadInt32();
                                            int y = _msgBuffer.ReadInt32();
                                            int z = _msgBuffer.ReadInt32();
                                            int seed = _msgBuffer.ReadInt32();

                                            Csettings.MapX = x;
                                            Csettings.MapY = y;
                                            Csettings.MapZ = z;
                                            Csettings.MapSeed = seed;

                                            //Needs to be replaced by BlockList
                                            MapGenerator generator = new MapGenerator(seed, x, y, z);
                                            generator.drawCube(0, 0, 0, x, y /2, z, BlockType.Dirt);
                                            PropertyBag.BlockEngine.SetBlockList(generator.mapData, x, y, z);
                                            if (!Csettings.NoSound)
                                                MediaPlayer.Stop();
                                            ChangeState("MineWorld.States.MainGameState");
                                        }
                                        break;

                                    case MineWorldMessage.PlayerId:
                                        {
                                            PropertyBag.PlayerMyId = _msgBuffer.ReadInt32();
                                            break;
                                        }

                                    case MineWorldMessage.DayUpdate:
                                        {
                                            PropertyBag.DayTime = _msgBuffer.ReadFloat();
                                            break;
                                        }
                                    case MineWorldMessage.HealthUpdate:
                                        {
                                            // Health, Healthmax both int
                                            PropertyBag.PlayerHealth = _msgBuffer.ReadInt32();
                                            PropertyBag.PlayerHealthMax = _msgBuffer.ReadInt32();
                                        }
                                        break;
                                    case MineWorldMessage.PlayerPosition:
                                        {
                                            PropertyBag.PlayerPosition = _msgBuffer.ReadVector3();
                                            break;
                                        }
                                    case MineWorldMessage.BlockSet:
                                        {
                                            // x, y, z, type, all bytes
                                            byte x = _msgBuffer.ReadByte();
                                            byte y = _msgBuffer.ReadByte();
                                            byte z = _msgBuffer.ReadByte();

                                            BlockType blockType = (BlockType) _msgBuffer.ReadByte();
                                            if (blockType == BlockType.None)
                                            {
                                                if (PropertyBag.BlockEngine.BlockAtPoint(new Vector3(x, y, z)) !=
                                                    BlockType.None)
                                                {
                                                    PropertyBag.BlockEngine.RemoveBlock(x, y, z);
                                                }
                                            }
                                            else
                                            {
                                                if (PropertyBag.BlockEngine.BlockAtPoint(new Vector3(x, y, z)) !=
                                                    BlockType.None)
                                                {
                                                    PropertyBag.BlockEngine.RemoveBlock(x, y, z);
                                                }
                                                PropertyBag.BlockEngine.AddBlock(x, y, z, blockType);
                                            }
                                        }
                                        break;

                                    case MineWorldMessage.TriggerExplosion:
                                        {
                                            Vector3 blockPos = _msgBuffer.ReadVector3();

                                            // Play the explosion sound.
                                            PropertyBag.PlaySound(MineWorldSound.Explosion, blockPos);

                                            // Create some particles.
                                            PropertyBag.ParticleEngine.CreateExplosionDebris(blockPos);

                                            // Figure out what the effect is.
                                            float distFromExplosive =
                                                (blockPos + 0.5f*Vector3.One - PropertyBag.PlayerPosition).Length();
                                            if (distFromExplosive < 3)
                                                //if (propertyBag.godmode == false)
                                                //{
                                                //propertyBag.KillPlayer(Defines.deathByExpl);//"WAS KILLED IN AN EXPLOSION!");
                                                //}
                                                /*else*/
                                                if (distFromExplosive < 8)
                                                {
                                                    // If we're not in explosion mode, turn it on with the minimum ammount of shakiness.
                                                    if (PropertyBag.ScreenEffect != ScreenEffect.Explosion)
                                                    {
                                                        PropertyBag.ScreenEffect = ScreenEffect.Explosion;
                                                        PropertyBag.ScreenEffectCounter = 2;
                                                    }
                                                    // If this bomb would result in a bigger shake, use its value.
                                                    PropertyBag.ScreenEffectCounter =
                                                        Math.Min(PropertyBag.ScreenEffectCounter,
                                                                 (distFromExplosive - 2)/5);
                                                }
                                        }
                                        break;

                                    case MineWorldMessage.PlayerJoined:
                                        {
                                            int playerId = _msgBuffer.ReadInt32();
                                            string playerName = _msgBuffer.ReadString();
                                            bool playerAlive = _msgBuffer.ReadBoolean();
                                            PropertyBag.PlayerList[playerId] = new ClientPlayer(this);
                                            PropertyBag.PlayerList[playerId].Name = playerName;
                                            PropertyBag.PlayerList[playerId].ID = playerId;
                                            PropertyBag.PlayerList[playerId].Alive = playerAlive;
                                        }
                                        break;

                                    case MineWorldMessage.PlayerLeft:
                                        {
                                            int playerId = _msgBuffer.ReadInt32();
                                            if (PropertyBag.PlayerList.ContainsKey(playerId))
                                            {
                                                PropertyBag.PlayerList.Remove(playerId);
                                            }
                                        }
                                        break;

                                    case MineWorldMessage.PlayerDead:
                                        {
                                            int playerId = _msgBuffer.ReadInt32();
                                            if (playerId == PropertyBag.PlayerMyId)
                                            {
                                                //TODO Implent death message
                                                PropertyBag.KillMySelf();
                                            }
                                            else if (PropertyBag.PlayerList.ContainsKey(playerId))
                                            {
                                                ClientPlayer player = PropertyBag.PlayerList[playerId];
                                                player.Alive = false;
                                                PropertyBag.ParticleEngine.CreateBloodSplatter(player.Position,
                                                                                               Color.Red);
                                                PropertyBag.PlaySound(MineWorldSound.Death, player.Position);
                                            }
                                        }
                                        break;

                                    case MineWorldMessage.PlayerAlive:
                                        {
                                            int playerId = _msgBuffer.ReadInt32();
                                            if (playerId == PropertyBag.PlayerMyId)
                                            {
                                                PropertyBag.MakeMySelfAlive();
                                            }
                                            if (PropertyBag.PlayerList.ContainsKey(playerId))
                                            {
                                                ClientPlayer player = PropertyBag.PlayerList[playerId];
                                                player.Alive = true;
                                            }
                                        }
                                        break;

                                    case MineWorldMessage.PlayerUpdate:
                                        {
                                            int playerId = _msgBuffer.ReadInt32();
                                            if (PropertyBag.PlayerList.ContainsKey(playerId))
                                            {
                                                ClientPlayer player = PropertyBag.PlayerList[playerId];
                                                player.UpdatePosition(_msgBuffer.ReadVector3(),
                                                                      gameTime.TotalGameTime.TotalSeconds);
                                                player.Heading = _msgBuffer.ReadVector3();
                                            }
                                        }
                                        break;

                                    case MineWorldMessage.ChatMessage:
                                        {
                                            ChatMessageType chatType = (ChatMessageType) _msgBuffer.ReadByte();
                                            string chatString = _msgBuffer.ReadString();
                                            string author = _msgBuffer.ReadString();
                                            PropertyBag.AddChatMessage(chatString, chatType, author);
                                        }
                                        break;
                                    case MineWorldMessage.PlaySound:
                                        {
                                            MineWorldSound sound = (MineWorldSound) _msgBuffer.ReadByte();
                                            bool hasPosition = _msgBuffer.ReadBoolean();
                                            if (hasPosition)
                                            {
                                                Vector3 soundPosition = _msgBuffer.ReadVector3();
                                                PropertyBag.PlaySound(sound, soundPosition);
                                            }
                                            else
                                                PropertyBag.PlaySound(sound);
                                        }
                                        break;
                                    case MineWorldMessage.Hearthbeat:
                                        {
                                            PropertyBag.Lasthearthbeatreceived = DateTime.Now;
                                            break;
                                        }
                                }
                            }
                            catch
                            {
                            } //Error in a received message
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
            Csettings.PlayerHandle = "Player";
            Csettings.VolumeLevel = 1.0f;
            Csettings.RenderPretty = true;
            Csettings.DrawFrameRate = false;
            Csettings.InvertMouseYAxis = false;
            Csettings.NoSound = false;
            Csettings.MouseSensitivity = 0.005f;
            Csettings.Width = 1024;
            Csettings.Height = 768;
            Csettings.Fullscreen = false;
            Csettings.Vsync = true;

            //Now moving to DatafileWriter only since it can read and write
            Datafile dataFile = new Datafile(Csettings.Directory + "/client.config.txt");
            if (dataFile.Data.ContainsKey("width"))
                Csettings.Width = int.Parse(dataFile.Data["width"], CultureInfo.InvariantCulture);
            if (dataFile.Data.ContainsKey("height"))
                Csettings.Height = int.Parse(dataFile.Data["height"], CultureInfo.InvariantCulture);
            if (dataFile.Data.ContainsKey("fullscreen"))
                Csettings.Fullscreen = bool.Parse(dataFile.Data["fullscreen"]);
            if (dataFile.Data.ContainsKey("vsync"))
                Csettings.Vsync = bool.Parse(dataFile.Data["vsync"]);
            if (dataFile.Data.ContainsKey("handle"))
                Csettings.PlayerHandle = dataFile.Data["handle"];
            if (dataFile.Data.ContainsKey("showfps"))
                Csettings.DrawFrameRate = bool.Parse(dataFile.Data["showfps"]);
            if (dataFile.Data.ContainsKey("yinvert"))
                Csettings.InvertMouseYAxis = bool.Parse(dataFile.Data["yinvert"]);
            if (dataFile.Data.ContainsKey("nosound"))
                Csettings.NoSound = bool.Parse(dataFile.Data["nosound"]);
            if (dataFile.Data.ContainsKey("pretty"))
                Csettings.RenderPretty = bool.Parse(dataFile.Data["pretty"]);
            if (dataFile.Data.ContainsKey("volume"))
                Csettings.VolumeLevel = Math.Max(0,
                                                 Math.Min(1,
                                                          float.Parse(dataFile.Data["volume"],
                                                                      CultureInfo.InvariantCulture)));
            if (dataFile.Data.ContainsKey("sensitivity"))
                Csettings.MouseSensitivity = Math.Max(0.001f,
                                                      Math.Min(0.05f,
                                                               float.Parse(dataFile.Data["sensitivity"],
                                                                           CultureInfo.InvariantCulture)/1000f));


            //Now to read the key bindings
            if (!File.Exists(Csettings.Directory + "/keymap.txt"))
            {
                FileStream temp = File.Create(Csettings.Directory + "/keymap.txt");
                temp.Close();
                //Console.WriteLine("Keymap file does not exist, creating.");
            }
            // Todo repair this to use keyboard and mousebinds ;)
            /*
            foreach (string key in dataFile.Data.Keys)
            {
                try
                {
                    CustomKeyBoardButtons button = (CustomKeyBoardButtons)Enum.Parse(typeof(CustomKeyBoardButtons),dataFile.Data[key],true);
                    if (Enum.IsDefined(typeof(CustomKeyBoardButtons), button))
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
                } 
                catch { }
            }
             */

            //HACK Create defaultset the whole time :P
            //If no keys are bound in this manner then create the default set
            if (true)
            {
                KeyBinds.CreateDefaultSet();
                KeyBinds.SaveBinds(dataFile, Csettings.Directory + "/keymap.txt");
                //Console.WriteLine("Creating default keymap...");
            }

            GraphicsDeviceManager.IsFullScreen = Csettings.Fullscreen;
            GraphicsDeviceManager.PreferredBackBufferHeight = Csettings.Height;
            GraphicsDeviceManager.PreferredBackBufferWidth = Csettings.Width;
            GraphicsDeviceManager.SynchronizeWithVerticalRetrace = Csettings.Vsync;
            GraphicsDeviceManager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            IsFixedTimeStep = false;
            GraphicsDeviceManager.ApplyChanges();
            base.Initialize();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            PropertyBag.NetClient.Shutdown("Client exiting.");

            base.OnExiting(sender, args);
        }

        public void ResetPropertyBag()
        {
            if (PropertyBag != null)
                PropertyBag.NetClient.Shutdown("");

            PropertyBag = new PropertyBag(this);
            PropertyBag.PlayerHandle = Csettings.PlayerHandle;
            PropertyBag.VolumeLevel = Csettings.VolumeLevel;
            PropertyBag.MouseSensitivity = Csettings.MouseSensitivity;
            _msgBuffer = PropertyBag.NetClient.CreateBuffer();
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
                _songTitle = Content.Load<Song>("song_title");
                MediaPlayer.Play(_songTitle);
                MediaPlayer.Volume = PropertyBag.VolumeLevel;
            }
        }
    }
}