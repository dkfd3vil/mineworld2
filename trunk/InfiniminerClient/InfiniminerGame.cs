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
        /*
        public void setServername(string newName)
        {
            propertyBag.serverName = newName;
        }
        */
        public void JoinGame(IPEndPoint serverEndPoint)
        {
            anyPacketsReceived = false;
            // Clear out the map load progress indicator.
            propertyBag.mapLoadProgress = new bool[64,64];
            for (int i = 0; i < 64; i++)
                for (int j=0; j<64; j++)
                    propertyBag.mapLoadProgress[i,j] = false;

            // Create our connect message.
            NetBuffer connectBuffer = propertyBag.netClient.CreateBuffer();
            connectBuffer.Write(propertyBag.playerHandle);
            connectBuffer.Write(Defines.MINEWORLD_VER);

            //Compression - will be ignored by regular servers.
            //connectBuffer.Write(true);

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
                string publicList = HttpRequest.Get("http://www.humorco.nl/mineworld/servers.php", null);
                foreach (string s in publicList.Split("\r\n".ToCharArray()))
                {
                    string[] args = s.Split(";".ToCharArray());
                    if (args.Length == 6)
                    {
                        IPAddress serverIp;
                        if (IPAddress.TryParse(args[1], out serverIp) && args[2] == "MineWorld")
                        {
                            ServerInformation serverInfo = new ServerInformation(serverIp, args[0], args[5], args[3], args[4]);
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
                            string message = "";
                            try
                            {
                                string reason = msgBuffer.ReadString();
                                if (reason.Length != 0)
                                {
                                    switch (reason)
                                    {
                                        case "bannedname":
                                            {
                                                message = "Error: The name you choosed is banned from the server";
                                                break;
                                            }
                                        case "noname":
                                            {
                                                message = "Error: You didnt choose a name";
                                                break;
                                            }
                                        case "changename":
                                            {
                                                message = "Error: You need to change your name you cannot choose name (player)";
                                                break;
                                            }
                                        case "versionwrong":
                                            {
                                                message = "Error: Your client is out of date consider updating";
                                                break;
                                            }
                                        case "banned":
                                            {
                                                message = "Error: You are banned from this server";
                                                break;
                                            }
                                        case "serverisfull":
                                            {
                                                message = "Error: The server is full";
                                                break;
                                            }
                                        default:
                                            {
                                                message = "Error: Unknow error";
                                                break;
                                            }
                                    }
                                }
                            }
                            catch 
                            {
                                message = "Error: Unknow error";
                            }
                            MessageBox.Show(message);
                            ChangeState("MineWorld.States.ServerBrowserState");
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
                                                //This is either the compression flag or the x coordiante
                                                //byte isCompressed = msgBuffer.ReadByte();
                                                byte x;
                                                byte y;
                                                
                                                //255 was used because it exceeds the map size - of course, bytes won't work anyway if map sizes are allowed to be this big, so this method is a non-issue
                                                /*
                                                if (isCompressed == 255)
                                                {
                                                    var compressed = msgBuffer.ReadBytes(msgBuffer.LengthBytes - msgBuffer.Position / 8);
                                                    var compressedstream = new System.IO.MemoryStream(compressed);
                                                    var decompresser = new System.IO.Compression.GZipStream(compressedstream, System.IO.Compression.CompressionMode.Decompress);

                                                    x = (byte)decompresser.ReadByte();
                                                    y = (byte)decompresser.ReadByte();
                                                    propertyBag.mapLoadProgress[x, y] = true;
                                                    for (byte dy = 0; dy < 16; dy++)
                                                        for (byte z = 0; z < 64; z++)
                                                        {
                                                            BlockType blockType = (BlockType)decompresser.ReadByte();
                                                            if (blockType != BlockType.None)
                                                                propertyBag.blockEngine.downloadList[x, y + dy, z] = blockType;
                                                        }
                                                }
                                                 */
                                                //else
                                                //{
                                                    x = msgBuffer.ReadByte();
                                                    y = msgBuffer.ReadByte();
                                                    propertyBag.mapLoadProgress[x, y] = true;
                                                    for (byte dy = 0; dy < 16; dy++)
                                                        for (byte z = 0; z < 64; z++)
                                                        {
                                                            BlockType blockType = (BlockType)msgBuffer.ReadByte();
                                                            if (blockType != BlockType.None)
                                                                propertyBag.blockEngine.downloadList[x, y + dy, z] = blockType;
                                                        }
                                                //}
                                                bool downloadComplete = true;
                                                for (x = 0; x < 64; x++)
                                                    for (y = 0; y < 64; y += 16)
                                                        if (propertyBag.mapLoadProgress[x, y] == false)
                                                        {
                                                            downloadComplete = false;
                                                            break;
                                                        }
                                                if (downloadComplete)
                                                {
                                                    ChangeState("MineWorld.States.TeamSelectionState");
                                                    if (!Csettings.NoSound)
                                                        MediaPlayer.Stop();
                                                    propertyBag.blockEngine.DownloadComplete();
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Console.OpenStandardError();
                                                Console.Error.WriteLine(e.Message);
                                                Console.Error.WriteLine(e.StackTrace);
                                                Console.Error.Close();
                                            }
                                        }
                                        break;

                                    case MineWorldMessage.SetBeacon:
                                        {
                                            Vector3 position = msgBuffer.ReadVector3();
                                            string text = msgBuffer.ReadString();
                                            PlayerTeam team = (PlayerTeam)msgBuffer.ReadByte();

                                            if (text == "")
                                            {
                                                if (propertyBag.beaconList.ContainsKey(position))
                                                    propertyBag.beaconList.Remove(position);
                                            }
                                            else
                                            {
                                                Beacon newBeacon = new Beacon();
                                                newBeacon.ID = text;
                                                newBeacon.Team = team;
                                                propertyBag.beaconList.Add(position, newBeacon);
                                            }
                                        }
                                        break;

                                    case MineWorldMessage.TriggerConstructionGunAnimation:
                                        {
                                            propertyBag.constructionGunAnimation = msgBuffer.ReadFloat();
                                            if (propertyBag.constructionGunAnimation <= -0.1)
                                                propertyBag.PlaySound(MineWorldSound.RadarSwitch);
                                        }
                                        break;

                                    case MineWorldMessage.ResourceUpdate:
                                        {
                                            // ore, cash, weight, max ore, max weight, team ore, red cash, blue cash, all uint
                                            propertyBag.playerOre = msgBuffer.ReadUInt32();
                                            propertyBag.playerCash = msgBuffer.ReadUInt32();
                                            propertyBag.playerWeight = msgBuffer.ReadUInt32();
                                            propertyBag.playerOreMax = msgBuffer.ReadUInt32();
                                            propertyBag.playerWeightMax = msgBuffer.ReadUInt32();
                                            propertyBag.teamOre = msgBuffer.ReadUInt32();
                                            propertyBag.teamRedCash = msgBuffer.ReadUInt32();
                                            propertyBag.teamBlueCash = msgBuffer.ReadUInt32();
                                        }
                                        break;

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
                                                    propertyBag.blockEngine.RemoveBlock(x, y, z);
                                            }
                                            else
                                            {
                                                if (propertyBag.blockEngine.BlockAtPoint(new Vector3(x, y, z)) != BlockType.None)
                                                    propertyBag.blockEngine.RemoveBlock(x, y, z);
                                                propertyBag.blockEngine.AddBlock(x, y, z, blockType);
                                                //CheckForStandingInLava();
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
                                                if (propertyBag.godmode == false)
                                                {
                                                    propertyBag.KillPlayer(Defines.deathByExpl);//"WAS KILLED IN AN EXPLOSION!");
                                                }
                                                else if (distFromExplosive < 8)
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

                                    case MineWorldMessage.PlayerSetTeam:
                                        {
                                            uint playerId = msgBuffer.ReadUInt32();
                                            if (propertyBag.playerList.ContainsKey(playerId))
                                            {
                                                Player player = propertyBag.playerList[playerId];
                                                player.Team = (PlayerTeam)msgBuffer.ReadByte();
                                            }
                                        }
                                        break;

                                    case MineWorldMessage.PlayerJoined:
                                        {
                                            uint playerId = msgBuffer.ReadUInt32();
                                            string playerName = msgBuffer.ReadString();
                                            bool thisIsMe = msgBuffer.ReadBoolean();
                                            bool playerAlive = msgBuffer.ReadBoolean();
                                            propertyBag.playerList[playerId] = new Player(null, (Game)this);
                                            propertyBag.playerList[playerId].Handle = playerName;
                                            propertyBag.playerList[playerId].ID = playerId;
                                            propertyBag.playerList[playerId].Alive = playerAlive;
                                            propertyBag.playerList[playerId].AltColours = Csettings.customColours;
                                            propertyBag.playerList[playerId].redTeam = Csettings.red;
                                            propertyBag.playerList[playerId].blueTeam = Csettings.blue;
                                            if (thisIsMe)
                                                propertyBag.playerMyId = playerId;
                                        }
                                        break;

                                    case MineWorldMessage.PlayerLeft:
                                        {
                                            uint playerId = msgBuffer.ReadUInt32();
                                            if (propertyBag.playerList.ContainsKey(playerId))
                                                propertyBag.playerList.Remove(playerId);
                                        }
                                        break;

                                    case MineWorldMessage.PlayerDead:
                                        {
                                            uint playerId = msgBuffer.ReadUInt32();
                                            if (propertyBag.playerList.ContainsKey(playerId))
                                            {
                                                Player player = propertyBag.playerList[playerId];
                                                player.Alive = false;
                                                //propertyBag.particleEngine.CreateBloodSplatter(player.Position, player.Team == PlayerTeam.Red ? Color.Red : Color.Blue);
                                                if (playerId != propertyBag.playerMyId)
                                                    propertyBag.PlaySound(MineWorldSound.Death, player.Position);
                                            }
                                        }
                                        break;

                                    case MineWorldMessage.PlayerAlive:
                                        {
                                            uint playerId = msgBuffer.ReadUInt32();
                                            if (propertyBag.playerList.ContainsKey(playerId))
                                            {
                                                Player player = propertyBag.playerList[playerId];
                                                player.Alive = true;
                                            }
                                        }
                                        break;

                                    case MineWorldMessage.PlayerUpdate:
                                        {
                                            uint playerId = msgBuffer.ReadUInt32();
                                            if (propertyBag.playerList.ContainsKey(playerId))
                                            {
                                                Player player = propertyBag.playerList[playerId];
                                                player.UpdatePosition(msgBuffer.ReadVector3(), gameTime.TotalGameTime.TotalSeconds);
                                                player.Heading = msgBuffer.ReadVector3();
                                                player.Tool = (PlayerTools)msgBuffer.ReadByte();
                                                player.UsingTool = msgBuffer.ReadBoolean();
                                                player.Score = (uint)(msgBuffer.ReadUInt16() * 100);
                                            }
                                        }
                                        break;

                                    case MineWorldMessage.GameOver:
                                        {
                                            propertyBag.teamWinners = (PlayerTeam)msgBuffer.ReadByte();
                                        }
                                        break;

                                    case MineWorldMessage.ChatMessage:
                                        {
                                            ChatMessageType chatType = (ChatMessageType)msgBuffer.ReadByte();
                                            string chatString = Defines.Sanitize(msgBuffer.ReadString());
                                            //Time to break it up into multiple lines
                                            propertyBag.addChatMessage(chatString, chatType, 10);
                                        }
                                        break;

                                    case MineWorldMessage.PlayerPing:
                                        {
                                            uint playerId = (uint)msgBuffer.ReadInt32();
                                            if (propertyBag.playerList.ContainsKey(playerId))
                                            {
                                                if (propertyBag.playerList[playerId].Team == propertyBag.playerTeam)
                                                {
                                                    propertyBag.playerList[playerId].Ping = 1;
                                                    propertyBag.PlaySound(MineWorldSound.Ping);
                                                }
                                            }
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
                                    case MineWorldMessage.PlayerCommandEnable:
                                        {
                                            PlayerCommands command = (PlayerCommands)msgBuffer.ReadByte();
                                            switch (command)
                                            {
                                                case PlayerCommands.None:
                                                    {
                                                        propertyBag.addChatMessage("Command not regonized", ChatMessageType.SayAll, 10);
                                                        break;
                                                    }
                                                case PlayerCommands.Godmode:
                                                    {
                                                        if (propertyBag.godmode == true)
                                                        {
                                                            propertyBag.godmode = false;
                                                            propertyBag.addChatMessage("Godmode disabled", ChatMessageType.SayAll, 10);
                                                        }
                                                        else
                                                        {
                                                            propertyBag.godmode = true;
                                                            propertyBag.addChatMessage("Godmode enabled", ChatMessageType.SayAll, 10);
                                                        }
                                                        break;
                                                    }
                                                case PlayerCommands.Stopfluids:
                                                    {
                                                        propertyBag.addChatMessage("Stopfluids enabled", ChatMessageType.SayAll, 10);
                                                        break;
                                                    }
                                                case PlayerCommands.Startfluids:
                                                    {
                                                        propertyBag.addChatMessage("Stopfluids disabled", ChatMessageType.SayAll, 10);
                                                        break;
                                                    }
                                                case PlayerCommands.Nocost:
                                                    {
                                                        if (propertyBag.nocost == true)
                                                        {
                                                            propertyBag.nocost = false;
                                                            propertyBag.addChatMessage("Nocost disabled", ChatMessageType.SayAll, 10);
                                                        }
                                                        else
                                                        {
                                                            propertyBag.nocost = true;
                                                            propertyBag.addChatMessage("Nocost enabled", ChatMessageType.SayAll, 10);
                                                        }
                                                        break;
                                                    }
                                                case PlayerCommands.Teleportto:
                                                    {
                                                        uint playerId = (uint)msgBuffer.ReadInt32();
                                                        if (propertyBag.playerList.ContainsKey(playerId))
                                                        {
                                                            Player player = propertyBag.playerList[playerId];
                                                            //Dont teleport to yourself :S
                                                            if (propertyBag.playerMyId == player.ID)
                                                            {
                                                                propertyBag.addChatMessage("Cant teleport to yourself", ChatMessageType.SayAll, 10);
                                                            }
                                                            //Dont teleport to dead players
                                                            else if (!player.Alive)
                                                            {
                                                                propertyBag.addChatMessage("Cant teleport to dead players", ChatMessageType.SayAll, 10);
                                                            }
                                                            else
                                                            {
                                                                propertyBag.screenEffect = ScreenEffect.Teleport;
                                                                propertyBag.PlaySoundForEveryone(MineWorldSound.Teleporter, propertyBag.playerPosition);
                                                                propertyBag.playerPosition = player.Position;
                                                            }
                                                        }
                                                        break;
                                                    }
                                                default:
                                                    break;
                                            }
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
            Csettings.customColours = false;
            Csettings.red = Defines.IM_RED;
            Csettings.redName = "Red";
            Csettings.blue = Defines.IM_BLUE;
            Csettings.blueName = "Blue";
            Csettings.Width = 1024;
            Csettings.Height = 768;
            Csettings.Fullscreen = false;

            //Now moving to DatafileWriter only since it can read and write
            Datafile dataFile = new Datafile(Csettings.Directory + "/client.config.txt");
            if (dataFile.Data.ContainsKey("width"))
                Csettings.Width = int.Parse(dataFile.Data["width"], System.Globalization.CultureInfo.InvariantCulture);
            if (dataFile.Data.ContainsKey("height"))
                Csettings.Height = int.Parse(dataFile.Data["height"], System.Globalization.CultureInfo.InvariantCulture);
            if (dataFile.Data.ContainsKey("fullscreen"))
                Csettings.Fullscreen = bool.Parse(dataFile.Data["fullscreen"]);
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
            if (dataFile.Data.ContainsKey("red_name"))
                Csettings.redName = dataFile.Data["red_name"].Trim();
            if (dataFile.Data.ContainsKey("blue_name"))
                Csettings.blueName = dataFile.Data["blue_name"].Trim();


            if (dataFile.Data.ContainsKey("red"))
            {
                Color temp = new Color();
                string[] data = dataFile.Data["red"].Split(',');
                try
                {
                    temp.R = byte.Parse(data[0].Trim());
                    temp.G = byte.Parse(data[1].Trim());
                    temp.B = byte.Parse(data[2].Trim());
                    temp.A = (byte)255;
                }
                catch {
                    Console.WriteLine("Invalid colour values for red");
                }
                if (temp.A != 0)
                {
                    Csettings.red = temp;
                    Csettings.customColours = true;
                }
            }

            if (dataFile.Data.ContainsKey("blue"))
            {
                Color temp = new Color();
                string[] data = dataFile.Data["blue"].Split(',');
                try
                {
                    temp.R = byte.Parse(data[0].Trim());
                    temp.G = byte.Parse(data[1].Trim());
                    temp.B = byte.Parse(data[2].Trim());
                    temp.A = (byte)255;
                }
                catch {
                    Console.WriteLine("Invalid colour values for blue");
                }
                if (temp.A != 0)
                {
                    Csettings.blue = temp;
                    Csettings.customColours = true;
                }
            }

            //Now to read the key bindings
            if (!File.Exists(Csettings.Directory + "/keymap.txt"))
            {
                FileStream temp = File.Create("keymap.txt");
                temp.Close();
                Console.WriteLine("Keymap file does not exist, creating.");
            }
            dataFile = new Datafile("keymap.txt");
            bool anyChanged = false;
            foreach (string key in dataFile.Data.Keys)
            {
                try
                {
                    Buttons button = (Buttons)Enum.Parse(typeof(Buttons),dataFile.Data[key],true);
                    if (Enum.IsDefined(typeof(Buttons), button))
                    {
                        if (keyBinds.BindKey(button, key, true))
                        {
                            anyChanged = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Enum not defined for " + dataFile.Data[key] + ".");
                    }
                } catch { }
            }

            //If no keys are bound in this manner then create the default set
            if (!anyChanged)
            {
                keyBinds.CreateDefaultSet();
                keyBinds.SaveBinds(dataFile, "keymap.txt");
                Console.WriteLine("Creating default keymap...");
            }
            graphicsDeviceManager.IsFullScreen = Csettings.Fullscreen;
            graphicsDeviceManager.PreferredBackBufferHeight = Csettings.Height;
            graphicsDeviceManager.PreferredBackBufferWidth = Csettings.Width;
            graphicsDeviceManager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
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

            propertyBag = new MineWorld.PropertyBag(this);
            propertyBag.playerHandle = Csettings.playerHandle;
            propertyBag.volumeLevel = Csettings.volumeLevel;
            propertyBag.mouseSensitivity = Csettings.mouseSensitivity;
            propertyBag.keyBinds = keyBinds;
            propertyBag.blue = Csettings.blue;
            propertyBag.red = Csettings.red;
            propertyBag.blueName = Csettings.blueName;
            propertyBag.redName = Csettings.redName;
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
