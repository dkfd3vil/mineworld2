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
        public Dictionary<int, ClientPlayer> playerList = new Dictionary<int, ClientPlayer>();
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
        public bool playerDead = true;
        public bool allowRespawn = false;
        public int playerHealth = 0;
        public int playerHealthMax = 0;
        public float playerHoldBreath = 20;
        public DateTime lastBreath = DateTime.Now;
        public bool playerRadarMute = false;
        public string playerHandle = "Player";
        public float volumeLevel = 1.0f;
        public int playerMyId = 0;
        public Color Owncolor = new Color();

        //Movement flags
        public bool movingOnRoad = false;
        public bool sprinting = false;
        public bool swimming = false;
        public bool crouching = false;

        public float time = 1.0f;

        public float mouseSensitivity = 0.005f;

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
            NetPeerConfiguration netConfig = new NetPeerConfiguration("MINEWORLD");

            netClient = new NetClient(netConfig);
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
            //if (netClient.Status != NetConnectionStatus.Connected)
                //return;

            PlaySound(MineWorldSound.Death);
            playerVelocity = Vector3.Zero;
            playerDead = true;
            allowRespawn = false;
            screenEffect = ScreenEffect.Death;
            screenEffectCounter = 0;

            NetOutgoingMessage msgBuffer = netClient.CreateMessage();
            msgBuffer.Write((byte)MineWorldMessage.PlayerDead);
            msgBuffer.Write(deathMessage);
            netClient.SendMessage(msgBuffer, NetDeliveryMethod.ReliableUnordered);
        }

        public void RespawnPlayer()
        {
            if (netClient.ConnectionStatus != NetConnectionStatus.Connected)
                return;

            if(allowRespawn == false)
            {
                NetOutgoingMessage msgBuffer = netClient.CreateMessage();
                msgBuffer.Write((byte)MineWorldMessage.PlayerRespawn);
                netClient.SendMessage(msgBuffer, NetDeliveryMethod.ReliableUnordered);
                return;
            }

            playerDead = false;

            // Zero out velocity and reset camera and screen effects.
            playerVelocity = Vector3.Zero;
            screenEffect = ScreenEffect.None;
            screenEffectCounter = 0;
            UpdateCamera();

            // Tell the server we have respawned.
            NetOutgoingMessage msgBufferb = netClient.CreateMessage();
            msgBufferb.Write((byte)MineWorldMessage.PlayerAlive);
            netClient.SendMessage(msgBufferb, NetDeliveryMethod.ReliableUnordered);
        }

        public void PlaySound(MineWorldSound sound)
        {
            if (soundList.Count == 0)
                return;

            soundList[sound].Play();
        }

        public void PlaySound(MineWorldSound sound, Vector3 position)
        {
            if (soundList.Count == 0)
                return;

            float distance = (position - playerPosition).Length();
            float volume = Math.Max(0, 10 - distance) / 10.0f * volumeLevel;
            volume = volume > 1.0f ? 1.0f : volume < 0.0f ? 0.0f : volume;

            MediaPlayer.Volume = volume;

            soundList[sound].Play();
        }

        public void PlaySoundForEveryone(MineWorldSound sound, Vector3 position)
        {
            if (netClient.ConnectionStatus != NetConnectionStatus.Connected)
                return;

            // The PlaySound message can be used to instruct the server to have all clients play a directional sound.
            NetOutgoingMessage msgBuffer = netClient.CreateMessage();
            msgBuffer.Write((byte)MineWorldMessage.PlaySound);
            msgBuffer.Write((byte)sound);
            msgBuffer.Write(position);
            netClient.SendMessage(msgBuffer, NetDeliveryMethod.ReliableUnordered);
        }

        public void addChatMessage(string ChatString, ChatMessageType ChatType,string Author)
        {
            ChatMessage chatMsg = new ChatMessage(ChatString, ChatType, Author);
            chatBuffer.Insert(0, chatMsg);
            //chatFullBuffer.Insert(0, chatMsg);
            PlaySound(MineWorldSound.ClickLow);
        }

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

        public void UseTool(KeyBoardButtons key)
        {
            if (netClient.ConnectionStatus != NetConnectionStatus.Connected)
                return;

            NetOutgoingMessage msgBuffer = netClient.CreateMessage();
            msgBuffer.Write((byte)MineWorldMessage.UseTool);
            msgBuffer.Write((byte)key);
            msgBuffer.Write(playerPosition);
            msgBuffer.Write(playerCamera.GetLookVector());
            msgBuffer.Write((byte)BlockType.Leafs);
            netClient.SendMessage(msgBuffer, NetDeliveryMethod.ReliableUnordered);
        }

        public void SendPlayerUpdate()
        {
            if (netClient.ConnectionStatus != NetConnectionStatus.Connected)
                return;

            if (lastPosition != playerPosition)//do full network update
            {
                lastPosition = playerPosition;

                NetOutgoingMessage msgBuffer = netClient.CreateMessage();
                msgBuffer.Write((byte)MineWorldMessage.PlayerUpdate);//full
                msgBuffer.Write(playerPosition);
                msgBuffer.Write(playerCamera.GetLookVector());
                //msgBuffer.Write((byte)playerTools[playerToolSelected]);
                //msgBuffer.Write(playerToolCooldown > 0.001f);
                netClient.SendMessage(msgBuffer, NetDeliveryMethod.UnreliableSequenced);
            }
            else if (lastHeading != playerCamera.GetLookVector())
            {
                lastHeading = playerCamera.GetLookVector();
                NetOutgoingMessage msgBuffer = netClient.CreateMessage();
                msgBuffer.Write((byte)MineWorldMessage.PlayerUpdate1);//just heading
                msgBuffer.Write(lastHeading);
                //msgBuffer.Write((byte)playerTools[playerToolSelected]);
                //msgBuffer.Write(playerToolCooldown > 0.001f);
                netClient.SendMessage(msgBuffer, NetDeliveryMethod.UnreliableSequenced);
            }
        }

        public void SendPlayerHurt(int damage,bool flatdamage)
        {
            if (netClient.ConnectionStatus != NetConnectionStatus.Connected)
                return;

            NetOutgoingMessage msgBuffer = netClient.CreateMessage();
            msgBuffer.Write((byte)MineWorldMessage.PlayerHurt);
            msgBuffer.Write(damage);
            msgBuffer.Write(flatdamage);
            netClient.SendMessage(msgBuffer, NetDeliveryMethod.ReliableOrdered);
        }
    }
}
