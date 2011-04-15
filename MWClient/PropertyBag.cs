using System;
using System.Collections.Generic;
using System.Text;
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
using System.IO;

namespace MineWorld
{
    public class PropertyBag
    {
        // Hearthbeat.
        public DateTime lasthearthbeatreceived = DateTime.Now;

        // Game engines.
        public SkyEngine skyEngine = null;
        public BlockEngine blockEngine = null;
        public InterfaceEngine interfaceEngine = null;
        public PlayerEngine playerEngine = null;
        public ParticleEngine particleEngine = null;

        // Network stuff.
        public NetClient netClient = null;
        public Dictionary<uint, Player> playerList = new Dictionary<uint, Player>();
        public bool[,] mapLoadProgress = null;
        public string serverName = "";

        //Input stuff.
        public KeyBindHandler keyBinds = null;

        // Player variables.
        public Camera playerCamera = null;
        public Vector3 playerPosition = Vector3.Zero;
        public Vector3 playerVelocity = Vector3.Zero;
        public Vector3 lastPosition = Vector3.Zero;
        public Vector3 lastHeading = Vector3.Zero;
        //public PlayerTools[] playerTools = new PlayerTools[1] { PlayerTools.Pickaxe };
        //public int playerToolSelected = 0;
        //public BlockType[] playerBlocks = new BlockType[1] { BlockType.None };
        //public int playerBlockSelected = 0;
        public bool playerDead = true;
        public bool allowRespawn = false;
        public uint playerHealth = 0;
        public uint playerHealthMax = 0;
        public float playerHoldBreath = 20;
        public DateTime lastBreath = DateTime.Now;
        public bool playerRadarMute = false;
        //public float playerToolCooldown = 0;
        public string playerHandle = "Player";
        public float volumeLevel = 1.0f;
        public uint playerMyId = 0;
        //public float radarCooldown = 0;
        //public float radarDistance = 0;
        //public float radarValue = 0;
        //public float constructionGunAnimation = 0;
        public Color Owncolor = new Color();

        //Movement flags
        public bool movingOnRoad = false;
        public bool sprinting = false;
        public bool swimming = false;
        public bool crouching = false;

        public float time = 1.0f;

        // Error handeling variables
        public string connectionerror = "";
        public string connectionerrornewstate = "";

        public float mouseSensitivity = 0.005f;

        // Beacon variables.
        //public Dictionary<Vector3, Beacon> beaconList = new Dictionary<Vector3, Beacon>();

        // Screen effect stuff.
        private Random randGen = new Random();
        public ScreenEffect screenEffect = ScreenEffect.None;
        public double screenEffectCounter = 0;

        // Sound stuff.
        public Dictionary<MineWorldSound, SoundEffect> soundList = new Dictionary<MineWorldSound, SoundEffect>();

        // Chat stuff.
        public ChatMessageType chatMode = ChatMessageType.None;
        public int chatMaxBuffer = 5;
        public List<ChatMessage> chatBuffer = new List<ChatMessage>(); // chatBuffer[0] is most recent
        //public List<ChatMessage> chatFullBuffer = new List<ChatMessage>(); //same as above, holds last several messages
        public string chatEntryBuffer = "";

        public PropertyBag(MineWorldGame gameInstance)
        {
            // Initialize our network device.
            NetConfiguration netConfig = new NetConfiguration("MineWorldPlus");

            netClient = new NetClient(netConfig);
            netClient.SetMessageTypeEnabled(NetMessageType.ConnectionRejected, true);
            //netClient.SimulatedMinimumLatency = 0.1f;
            //netClient.SimulatedLatencyVariance = 0.05f;
            //netClient.SimulatedLoss = 0.1f;
            //netClient.SimulatedDuplicates = 0.05f;
            netClient.Start();

            // Initialize engines.
            skyEngine = new SkyEngine(gameInstance);
            blockEngine = new BlockEngine(gameInstance);
            interfaceEngine = new InterfaceEngine(gameInstance);
            playerEngine = new PlayerEngine(gameInstance);
            particleEngine = new ParticleEngine(gameInstance);

            // Create a camera.
            playerCamera = new Camera(gameInstance.GraphicsDevice);
            UpdateCamera();

            // Load sounds.
            if (!gameInstance.Csettings.NoSound)
            {
                soundList[MineWorldSound.DigDirt] = gameInstance.Content.Load<SoundEffect>("sounds/dig-dirt");
                soundList[MineWorldSound.DigMetal] = gameInstance.Content.Load<SoundEffect>("sounds/dig-metal");
                soundList[MineWorldSound.Ping] = gameInstance.Content.Load<SoundEffect>("sounds/ping");
                soundList[MineWorldSound.ConstructionGun] = gameInstance.Content.Load<SoundEffect>("sounds/build");
                soundList[MineWorldSound.Death] = gameInstance.Content.Load<SoundEffect>("sounds/death");
                soundList[MineWorldSound.CashDeposit] = gameInstance.Content.Load<SoundEffect>("sounds/cash");
                soundList[MineWorldSound.ClickHigh] = gameInstance.Content.Load<SoundEffect>("sounds/click-loud");
                soundList[MineWorldSound.ClickLow] = gameInstance.Content.Load<SoundEffect>("sounds/click-quiet");
                soundList[MineWorldSound.GroundHit] = gameInstance.Content.Load<SoundEffect>("sounds/hitground");
                soundList[MineWorldSound.Teleporter] = gameInstance.Content.Load<SoundEffect>("sounds/teleport");
                soundList[MineWorldSound.Jumpblock] = gameInstance.Content.Load<SoundEffect>("sounds/jumpblock");
                soundList[MineWorldSound.Explosion] = gameInstance.Content.Load<SoundEffect>("sounds/explosion");
                soundList[MineWorldSound.RadarHigh] = gameInstance.Content.Load<SoundEffect>("sounds/radar-high");
                soundList[MineWorldSound.RadarLow] = gameInstance.Content.Load<SoundEffect>("sounds/radar-low");
                soundList[MineWorldSound.RadarSwitch] = gameInstance.Content.Load<SoundEffect>("sounds/switch");
            }
        }

        public void KillPlayer(string deathMessage)
        {
            if (netClient.Status != NetConnectionStatus.Connected)
                return;

            PlaySound(MineWorldSound.Death);
            playerVelocity = Vector3.Zero;
            playerDead = true;
            allowRespawn = false;
            screenEffect = ScreenEffect.Death;
            screenEffectCounter = 0;

            NetBuffer msgBuffer = netClient.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.PlayerDead);
            msgBuffer.Write(deathMessage);
            netClient.SendMessage(msgBuffer, NetChannel.ReliableUnordered);
        }

        public void RespawnPlayer()
        {
            if (netClient.Status != NetConnectionStatus.Connected)
                return;

            if(allowRespawn == false)
            {
                NetBuffer msgBuffer = netClient.CreateBuffer();
                msgBuffer.Write((byte)MineWorldMessage.PlayerRespawn);
                netClient.SendMessage(msgBuffer, NetChannel.ReliableUnordered);
                return;
            }

            playerDead = false;

            // Zero out velocity and reset camera and screen effects.
            playerVelocity = Vector3.Zero;
            screenEffect = ScreenEffect.None;
            screenEffectCounter = 0;
            UpdateCamera();

            // Tell the server we have respawned.
            NetBuffer msgBufferb = netClient.CreateBuffer();
            msgBufferb.Write((byte)MineWorldMessage.PlayerAlive);
            netClient.SendMessage(msgBufferb, NetChannel.ReliableUnordered);
        }

        public void PlaySound(MineWorldSound sound)
        {
            if (soundList.Count == 0)
                return;

            soundList[sound].Play(volumeLevel);
        }

        public void PlaySound(MineWorldSound sound, Vector3 position)
        {
            if (soundList.Count == 0)
                return;

            float distance = (position - playerPosition).Length();
            float volume = Math.Max(0, 10 - distance) / 10.0f * volumeLevel;
            volume = volume > 1.0f ? 1.0f : volume < 0.0f ? 0.0f : volume;
            soundList[sound].Play(volume);
        }


        public void PlaySound(MineWorldSound sound, Vector3 position, int magnification)
        {
            if (soundList.Count == 0)
                return;
            float distance = (position - playerPosition).Length() - magnification;

            float volume = Math.Max(0, 64 - distance) / 10.0f * volumeLevel;

            volume = volume > 1.0f ? 1.0f : volume < 0.0f ? 0.0f : volume;

            soundList[sound].Play(volume);
        }

        public void PlaySoundForEveryone(MineWorldSound sound, Vector3 position)
        {
            if (netClient.Status != NetConnectionStatus.Connected)
                return;

            // The PlaySound message can be used to instruct the server to have all clients play a directional sound.
            NetBuffer msgBuffer = netClient.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.PlaySound);
            msgBuffer.Write((byte)sound);
            msgBuffer.Write(position);
            netClient.SendMessage(msgBuffer, NetChannel.ReliableUnordered);
        }

        public void addChatMessage(string ChatString, ChatMessageType ChatType,string Author)
        {
            /*
            string[] text = chatString.Split(' ');
            string textFull = "";
            string textLine = "";
            int newlines = 0;

            float curWidth = 0;
            for (int i = 0; i < text.Length; i++)
            {//each(string part in text){
                string part = text[i];
                if (i != text.Length - 1)
                    part += ' '; //Correct for lost spaces
                float incr = interfaceEngine.uiFont.MeasureString(part).X;
                curWidth += incr;
                if (curWidth > 1024 - 64) //Assume default resolution, unfortunately
                {
                    if (textLine.IndexOf(' ') < 0)
                    {
                        curWidth = 0;
                        textFull = textFull + "\n" + textLine;
                        textLine = "";
                    }
                    else
                    {
                        curWidth = incr;
                        textFull = textFull + "\n" + textLine;
                        textLine = part;
                    }
                    newlines++;
                }
                else
                {
                    textLine = textLine + part;
                }
            }
            if (textLine != "")
            {
                textFull += "\n" + textLine;
                newlines++;
            }

            if (textFull == "")
                textFull = chatString;
             */

            ChatMessage chatMsg = new ChatMessage(ChatString, ChatType, Author);
            
            chatBuffer.Insert(0, chatMsg);
            //chatFullBuffer.Insert(0, chatMsg);
            PlaySound(MineWorldSound.ClickLow);
        }

        //public void Teleport()
        //{
        //    float x = (float)randGen.NextDouble() * 74 - 5;
        //    float z = (float)randGen.NextDouble() * 74 - 5;
        //    //playerPosition = playerHomeBlock + new Vector3(0.5f, 3, 0.5f);
        //    playerPosition = new Vector3(x, 74, z);
        //    screenEffect = ScreenEffect.Teleport;
        //    screenEffectCounter = 0;
        //    UpdateCamera();
        //}

        // Version used during updates.
        public void UpdateCamera(GameTime gameTime)
        {
            // If we have a gameTime object, apply screen jitter.
            if (screenEffect == ScreenEffect.Explosion)
            {
                if (gameTime != null)
                {
                    screenEffectCounter += gameTime.ElapsedGameTime.TotalSeconds;
                    // For 0 to 2, shake the camera.
                    if (screenEffectCounter < 2)
                    {
                        Vector3 newPosition = playerPosition;
                        newPosition.X += (float)(2 - screenEffectCounter) * (float)(randGen.NextDouble() - 0.5) * 0.5f;
                        newPosition.Y += (float)(2 - screenEffectCounter) * (float)(randGen.NextDouble() - 0.5) * 0.5f;
                        newPosition.Z += (float)(2 - screenEffectCounter) * (float)(randGen.NextDouble() - 0.5) * 0.5f;
                        if (!blockEngine.SolidAtPointForPlayer(newPosition) && (newPosition - playerPosition).Length() < 0.7f)
                            playerCamera.Position = newPosition;
                    }
                    // For 2 to 3, move the camera back.
                    else if (screenEffectCounter < 3)
                    {
                        Vector3 lerpVector = playerPosition - playerCamera.Position;
                        playerCamera.Position += 0.5f * lerpVector;
                    }
                    else
                    {
                        screenEffect = ScreenEffect.None;
                        screenEffectCounter = 0;
                        playerCamera.Position = playerPosition;
                    }
                }
            }
            else
            {
                playerCamera.Position = playerPosition;
            }
            playerCamera.Update();
        }

        public void UpdateCamera()
        {
            UpdateCamera(null);
        }

        /*
        public void equipWeps()
        {
            bool allWeps = true;//TODO We are giving every player all tools

            playerToolSelected = 0;
            playerBlockSelected = 0;
            //HACK !!!!!!!
            if (allWeps)
            {
                playerTools = new PlayerTools[5] { PlayerTools.Pickaxe,
                PlayerTools.ConstructionGun,
                PlayerTools.DeconstructionGun,
                PlayerTools.ProspectingRadar,
                PlayerTools.Detonator };

                playerBlocks = new BlockType[16] {
                                             BlockType.SolidRed,
                                             BlockType.SolidBlue,
                                             BlockType.TransRed,
                                             BlockType.TransBlue,
                                             BlockType.Road,
                                             BlockType.Ladder,
                                             BlockType.Jump,
                                             BlockType.Shock,
                                             BlockType.BeaconRed,
                                             BlockType.BeaconBlue,
                                             BlockType.BankRed,
                                             BlockType.BankBlue,
                                             BlockType.Explosive,
                                             BlockType.Road,
                                             BlockType.Lava,
                                             BlockType.Adminblock,};
            }
        }
         */

        public void UseTool(KeyBoardButtons key)
        {
            if (netClient.Status != NetConnectionStatus.Connected)
                return;

            NetBuffer msgBuffer = netClient.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.UseTool);
            msgBuffer.Write((byte)key);
            msgBuffer.Write(playerPosition);
            msgBuffer.Write(playerCamera.GetLookVector());
            msgBuffer.Write((byte)BlockType.Leaves);
            netClient.SendMessage(msgBuffer, NetChannel.ReliableUnordered);
        }
        /*
        public void FireRadar()
        {
            if (netClient.Status != NetConnectionStatus.Connected)
                return;

            playerToolCooldown = GetToolCooldown(PlayerTools.ProspectingRadar);

            NetBuffer msgBuffer = netClient.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.UseTool);
            msgBuffer.Write(playerPosition);
            msgBuffer.Write(playerCamera.GetLookVector());
            msgBuffer.Write((byte)PlayerTools.ProspectingRadar);
            msgBuffer.Write((byte)BlockType.None);
            netClient.SendMessage(msgBuffer, NetChannel.ReliableUnordered);
        }
        */
        /*
        public void FirePickaxe()
        {
            if (netClient.Status != NetConnectionStatus.Connected)
                return;

            playerToolCooldown = GetToolCooldown(PlayerTools.Pickaxe);

            NetBuffer msgBuffer = netClient.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.UseTool);
            msgBuffer.Write(playerPosition);
            msgBuffer.Write(playerCamera.GetLookVector());
            msgBuffer.Write((byte)PlayerTools.Pickaxe);
            msgBuffer.Write((byte)BlockType.None);
            netClient.SendMessage(msgBuffer, NetChannel.ReliableUnordered);
        }
         */
        /*
        public void FireConstructionGun(BlockType blockType)
        {
            if (netClient.Status != NetConnectionStatus.Connected)
                return;

            playerToolCooldown = GetToolCooldown(PlayerTools.ConstructionGun);
            constructionGunAnimation = -5;

            // Send the message.
            NetBuffer msgBuffer = netClient.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.UseTool);
            msgBuffer.Write(playerPosition);
            msgBuffer.Write(playerCamera.GetLookVector());
            msgBuffer.Write((byte)PlayerTools.ConstructionGun);
            msgBuffer.Write((byte)blockType);
            netClient.SendMessage(msgBuffer, NetChannel.ReliableUnordered);
        }
         */

        /*
        public void FireDeconstructionGun()
        {
            if (netClient.Status != NetConnectionStatus.Connected)
                return;

            playerToolCooldown = GetToolCooldown(PlayerTools.DeconstructionGun);
            constructionGunAnimation = -5;

            // Send the message.
            NetBuffer msgBuffer = netClient.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.UseTool);
            msgBuffer.Write(playerPosition);
            msgBuffer.Write(playerCamera.GetLookVector());
            msgBuffer.Write((byte)PlayerTools.DeconstructionGun);
            msgBuffer.Write((byte)BlockType.None);
            netClient.SendMessage(msgBuffer, NetChannel.ReliableUnordered);
        }
         */

        /*
        public void FireDetonator()
        {
            if (netClient.Status != NetConnectionStatus.Connected)
                return;

            playerToolCooldown = GetToolCooldown(PlayerTools.Detonator);

            // Send the message.
            NetBuffer msgBuffer = netClient.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.UseTool);
            msgBuffer.Write(playerPosition);
            msgBuffer.Write(playerCamera.GetLookVector());
            msgBuffer.Write((byte)PlayerTools.Detonator);
            msgBuffer.Write((byte)BlockType.None);
            netClient.SendMessage(msgBuffer, NetChannel.ReliableUnordered);
        }
         */

        /*
        public void ToggleRadar()
        {
            playerRadarMute = !playerRadarMute;
            PlaySound(MineWorldSound.RadarSwitch);
        }
        */
        /*
        public void ReadRadar(ref float distanceReading, ref float valueReading)
        {
            valueReading = 0;
            distanceReading = 30;

            // Scan out along the camera axis for 30 meters.
            for (int i = -3; i <= 3; i++)
                for (int j = -3; j <= 3; j++)
                {
                    Matrix rotation = Matrix.CreateRotationX((float)(i * Math.PI / 128)) * Matrix.CreateRotationY((float)(j * Math.PI / 128));
                    Vector3 scanPoint = playerPosition;
                    Vector3 lookVector = Vector3.Transform(playerCamera.GetLookVector(), rotation);
                    for (int k = 0; k < 60; k++)
                    {
                        BlockType blockType = blockEngine.BlockAtPoint(scanPoint);
                        if (blockType == BlockType.Gold)
                        {
                            distanceReading = Math.Min(distanceReading, 0.5f * k);
                            valueReading = Math.Max(valueReading, 200);
                        }
                        else if (blockType == BlockType.Diamond)
                        {
                            distanceReading = Math.Min(distanceReading, 0.5f * k);
                            valueReading = Math.Max(valueReading, 1000);
                        }
                        scanPoint += 0.5f * lookVector;
                    }
                }
        }
         */

        // Returns true if the player is able to use a bank right now.
        /*
        public bool AtBankTerminal()
        {
            // Figure out what we're looking at.
            Vector3 hitPoint = Vector3.Zero;
            Vector3 buildPoint = Vector3.Zero;
            if (!blockEngine.RayCollision(playerPosition, playerCamera.GetLookVector(), 2.5f, 25, ref hitPoint, ref buildPoint))
                return false;

            // If it's a valid bank object, we're good!
            BlockType blockType = blockEngine.BlockAtPoint(hitPoint);
            if (blockType == BlockType.BankRed && playerTeam == PlayerTeam.Red)
                return true;
            if (blockType == BlockType.BankBlue && playerTeam == PlayerTeam.Blue)
                return true;
            return false;
        }
         */

        // Returns true if the player is looking at it's own homebase
        /*
        public bool AtHomeBase()
        {
            // Figure out what we're looking at.
            Vector3 hitPoint = Vector3.Zero;
            Vector3 buildPoint = Vector3.Zero;
            if (!blockEngine.RayCollision(playerPosition, playerCamera.GetLookVector(), 2.5f, 25, ref hitPoint, ref buildPoint))
                return false;

            // If it's a valid bank object, we're good!
            BlockType blockType = blockEngine.BlockAtPoint(hitPoint);
            if (blockType == BlockType.HomeRed && playerTeam == PlayerTeam.Red)
                return true;
            if (blockType == BlockType.HomeBlue && playerTeam == PlayerTeam.Blue)
                return true;
            return false;
        }
         */
        /*
        public float GetToolCooldown(PlayerTools tool)
        {
            switch (tool)
            {
                case PlayerTools.Pickaxe: return 0.55f;
                case PlayerTools.Detonator: return 0.01f;
                case PlayerTools.ConstructionGun: return 0.5f;
                case PlayerTools.DeconstructionGun: return 0.5f;
                case PlayerTools.ProspectingRadar: return 0.5f;
                default: return 0;
            }
        }
        */
        public void SendPlayerUpdate()
        {
            if (netClient.Status != NetConnectionStatus.Connected)
                return;

            if (lastPosition != playerPosition)//do full network update
            {
                lastPosition = playerPosition;

                NetBuffer msgBuffer = netClient.CreateBuffer();
                msgBuffer.Write((byte)MineWorldMessage.PlayerUpdate);//full
                msgBuffer.Write(playerPosition);
                msgBuffer.Write(playerCamera.GetLookVector());
                //msgBuffer.Write((byte)playerTools[playerToolSelected]);
                //msgBuffer.Write(playerToolCooldown > 0.001f);
                netClient.SendMessage(msgBuffer, NetChannel.UnreliableInOrder1);
            }
            else if (lastHeading != playerCamera.GetLookVector())
            {
                lastHeading = playerCamera.GetLookVector();
                NetBuffer msgBuffer = netClient.CreateBuffer();
                msgBuffer.Write((byte)MineWorldMessage.PlayerUpdate1);//just heading
                msgBuffer.Write(lastHeading);
                //msgBuffer.Write((byte)playerTools[playerToolSelected]);
                //msgBuffer.Write(playerToolCooldown > 0.001f);
                netClient.SendMessage(msgBuffer, NetChannel.UnreliableInOrder1);
            }
            /*
            else
            {
                NetBuffer msgBuffer = netClient.CreateBuffer();
                msgBuffer.Write((byte)MineWorldMessage.PlayerUpdate2);//just tools
                //msgBuffer.Write((byte)playerTools[playerToolSelected]);
                //msgBuffer.Write(playerToolCooldown > 0.001f);
                netClient.SendMessage(msgBuffer, NetChannel.UnreliableInOrder1);
            }
             */
        }

        public void SendPlayerHurt(uint damage,bool flatdamage)
        {
            if (netClient.Status != NetConnectionStatus.Connected)
                return;

            NetBuffer msgBuffer = netClient.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.PlayerHurt);
            msgBuffer.Write(damage);
            msgBuffer.Write(flatdamage);
            netClient.SendMessage(msgBuffer, NetChannel.ReliableInOrder1);
        }
    }
}
