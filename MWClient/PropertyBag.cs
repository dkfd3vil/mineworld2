using System;
using System.Collections.Generic;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using MineWorld.Engines;

namespace MineWorld
{
    public class PropertyBag
    {
        // Hearthbeat.
        private readonly Random _randGen = new Random();
        public Vector3 MoveVector = Vector3.Zero;
        public BlockEngine BlockEngine;
        public List<ChatMessage> ChatBuffer = new List<ChatMessage>(); // chatBuffer[0] is most recent
        public string ChatEntryBuffer = "";
        public bool Chatting;
        public float DayTime = 1.0f;
        public DebugEngine DebugEngine;
        public bool DebugMode;
        public InterfaceEngine InterfaceEngine;
        public DateTime LastBreath = DateTime.Now;
        public Vector3 LastHeading = Vector3.Zero;
        public Vector3 LastPosition = Vector3.Zero;
        public DateTime Lasthearthbeatreceived = DateTime.Now;
        public float MouseSensitivity = 0.005f;
        public bool MovingOnRoad;

        // Network stuff.
        public NetClient NetClient;
        public ParticleEngine ParticleEngine;

        // Player variables.
        public Camera PlayerCamera;
        public bool PlayerDead = true;
        public PlayerEngine PlayerEngine;
        public string PlayerHandle = "Player";
        public int PlayerHealth;
        public int PlayerHealthMax;
        public float PlayerHoldBreath = 20;
        public Dictionary<int, ClientPlayer> PlayerList = new Dictionary<int, ClientPlayer>();
        public int PlayerMyId;
        public Vector3 PlayerPosition = Vector3.Zero;
        public Vector3 PlayerVelocity = Vector3.Zero;

        //Movement flags
        public ScreenEffect ScreenEffect = ScreenEffect.None;
        public double ScreenEffectCounter;
        public string ServerName = "";
        public SkyEngine SkyEngine;

        // Sound stuff.
        public Dictionary<MineWorldSound, SoundEffect> SoundList = new Dictionary<MineWorldSound, SoundEffect>();
        public bool Sprinting;
        public bool Swimming;
        public bool Underwater;
        public float VolumeLevel = 1.0f;

        // Chat stuff.

        public PropertyBag(MineWorldGame gameInstance)
        {
            // Initialize our network device.
            NetConfiguration netConfig = new NetConfiguration("MineWorld");

            NetClient = new NetClient(netConfig);
            NetClient.SetMessageTypeEnabled(NetMessageType.ConnectionRejected, true);
            NetClient.Start();

            // Initialize engines.
            SkyEngine = new SkyEngine(gameInstance);
            BlockEngine = new BlockEngine(gameInstance);
            InterfaceEngine = new InterfaceEngine(gameInstance);
            PlayerEngine = new PlayerEngine(gameInstance);
            ParticleEngine = new ParticleEngine(gameInstance);
            DebugEngine = new DebugEngine(gameInstance);

            // Create a camera.
            PlayerCamera = new Camera(gameInstance.GraphicsDevice);

            // Load sounds.
            if (!gameInstance.Csettings.NoSound)
            {
                SoundList[MineWorldSound.Dig] = gameInstance.Content.Load<SoundEffect>("sounds/dig-dirt");
                SoundList[MineWorldSound.Build] = gameInstance.Content.Load<SoundEffect>("sounds/build");
                SoundList[MineWorldSound.Death] = gameInstance.Content.Load<SoundEffect>("sounds/death");
                SoundList[MineWorldSound.ClickHigh] = gameInstance.Content.Load<SoundEffect>("sounds/click-loud");
                SoundList[MineWorldSound.ClickLow] = gameInstance.Content.Load<SoundEffect>("sounds/click-quiet");
                SoundList[MineWorldSound.GroundHit] = gameInstance.Content.Load<SoundEffect>("sounds/hitground");
                SoundList[MineWorldSound.Explosion] = gameInstance.Content.Load<SoundEffect>("sounds/explosion");
            }
        }

        public void KillMySelf()
        {
            PlaySound(MineWorldSound.Death);
            PlayerVelocity = Vector3.Zero;
            PlayerDead = true;
            ScreenEffect = ScreenEffect.Death;
            ScreenEffectCounter = 0;
        }

        public void MakeMySelfAlive()
        {
            PlayerDead = false;
            ScreenEffect = ScreenEffect.None;
            ScreenEffectCounter = 0;
        }

        public void SendRespawnRequest()
        {
            if (NetClient.Status != NetConnectionStatus.Connected)
                return;

            NetBuffer msgBuffer = NetClient.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.PlayerRequest);
            msgBuffer.Write((byte) PlayerRequests.Respawn);
            NetClient.SendMessage(msgBuffer, NetChannel.ReliableUnordered);
            return;
        }

        public void PlaySound(MineWorldSound sound)
        {
            if (SoundList.Count == 0)
                return;

            SoundList[sound].Play(VolumeLevel);
        }

        public void PlaySound(MineWorldSound sound, Vector3 position)
        {
            if (SoundList.Count == 0)
                return;

            float distance = (position - PlayerPosition).Length();
            float volume = Math.Max(0, 10 - distance)/10.0f*VolumeLevel;
            volume = volume > 1.0f ? 1.0f : volume < 0.0f ? 0.0f : volume;
            SoundList[sound].Play(volume);
        }

        public void PlaySoundForEveryone(MineWorldSound sound, Vector3 position)
        {
            if (NetClient.Status != NetConnectionStatus.Connected)
                return;

            // The PlaySound message can be used to instruct the server to have all clients play a directional sound.
            NetBuffer msgBuffer = NetClient.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.PlaySound);
            msgBuffer.Write((byte) sound);
            msgBuffer.Write(position);
            NetClient.SendMessage(msgBuffer, NetChannel.ReliableUnordered);
        }

        public void AddChatMessage(string chatString, ChatMessageType chatType, string author)
        {
            ChatMessage chatMsg = new ChatMessage(chatString, chatType, author);

            ChatBuffer.Insert(0, chatMsg);
            PlaySound(MineWorldSound.ClickLow);
        }

        // Version used during updates.
        public void UpdateCamera(GameTime gameTime)
        {
            if (gameTime == null)
            {
                return;
            }

            ScreenEffectCounter += gameTime.ElapsedGameTime.TotalSeconds;

            switch (ScreenEffect)
            {
                case ScreenEffect.Explosion:
                    {
                        // For 0 to 2, shake the camera.
                        if (ScreenEffectCounter < 2)
                        {
                            Vector3 newPosition = PlayerPosition;
                            newPosition.X += (float) (2 - ScreenEffectCounter)*(float) (_randGen.NextDouble() - 0.5)*0.5f;
                            newPosition.Y += (float) (2 - ScreenEffectCounter)*(float) (_randGen.NextDouble() - 0.5)*0.5f;
                            newPosition.Z += (float) (2 - ScreenEffectCounter)*(float) (_randGen.NextDouble() - 0.5)*0.5f;
                            if (!BlockEngine.SolidAtPointForPlayer(newPosition) &&
                                (newPosition - PlayerPosition).Length() < 0.7f)
                                PlayerCamera.Position = newPosition;
                        }
                            // For 2 to 3, move the camera back.
                        else if (ScreenEffectCounter < 3)
                        {
                            Vector3 lerpVector = PlayerPosition - PlayerCamera.Position;
                            PlayerCamera.Position += 0.5f*lerpVector;
                        }
                        else
                        {
                            ScreenEffect = ScreenEffect.None;
                            ScreenEffectCounter = 0;
                            PlayerCamera.Position = PlayerPosition;
                        }
                        break;
                    }
                default:
                    {
                        PlayerCamera.Position = PlayerPosition;
                        break;
                    }
            }

            PlayerCamera.Update();
        }

        public void UseTool(CustomMouseButtons button)
        {
            if (NetClient.Status != NetConnectionStatus.Connected)
                return;

            NetBuffer msgBuffer = NetClient.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.UseTool);
            msgBuffer.Write((byte) button);
            msgBuffer.Write(PlayerPosition);
            msgBuffer.Write(PlayerCamera.GetLookVector());
            //HACK Hardcoded the block that the player places
            msgBuffer.Write((byte) BlockType.Leafs);
            NetClient.SendMessage(msgBuffer, NetChannel.ReliableUnordered);
        }

        public void SendPlayerUpdate()
        {
            if (NetClient.Status != NetConnectionStatus.Connected)
                return;

            if (LastPosition != PlayerPosition) //do full network update
            {
                LastPosition = PlayerPosition;

                NetBuffer msgBuffer = NetClient.CreateBuffer();
                msgBuffer.Write((byte) MineWorldMessage.PlayerUpdate); //full
                msgBuffer.Write(PlayerPosition);
                msgBuffer.Write(PlayerCamera.GetLookVector());
                NetClient.SendMessage(msgBuffer, NetChannel.UnreliableInOrder1);
            }
            else if (LastHeading != PlayerCamera.GetLookVector())
            {
                LastHeading = PlayerCamera.GetLookVector();
                NetBuffer msgBuffer = NetClient.CreateBuffer();
                msgBuffer.Write((byte) MineWorldMessage.PlayerUpdate1); //just heading
                msgBuffer.Write(LastHeading);
                NetClient.SendMessage(msgBuffer, NetChannel.UnreliableInOrder1);
            }
        }

        public void SendPlayerHurt(int damage, bool flatdamage)
        {
            if (NetClient.Status != NetConnectionStatus.Connected)
                return;

            NetBuffer msgBuffer = NetClient.CreateBuffer();
            msgBuffer.Write((byte) MineWorldMessage.PlayerHurt);
            msgBuffer.Write(damage);
            msgBuffer.Write(flatdamage);
            NetClient.SendMessage(msgBuffer, NetChannel.ReliableInOrder1);
        }
    }
}