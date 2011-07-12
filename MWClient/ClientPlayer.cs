using System;
using System.Collections.Generic;
using System.Text;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace MineWorld
{
    public class ClientPlayer : Player
    {
        public bool Kicked = false;
        public bool Godmode = false;
        public string Name = "";
        // DJ NOT NICE
        // TODO This needs to be done proper
        public int Health = 0;
        public int HealthMax = 100;
        public bool Alive = false;
        public int ID;
        public Vector3 Heading = Vector3.Zero;
        public Vector3 Position = Vector3.Zero;
        public NetConnection NetConn;
        public string IP = "";
        private List<InterpolationPacket> interpList = new List<InterpolationPacket>();
        public Color Owncolor = new Color();
        public SpriteModel SpriteModel;
        public Game gameInstance;

        public ClientPlayer(NetConnection netConn, Game gameInstance)
        {
            this.gameInstance = gameInstance;
            this.NetConn = netConn;
            this.ID = GetUniqueId();

            if (netConn != null)
                this.IP = netConn.RemoteEndpoint.Address.ToString();

            if (gameInstance != null)
            {
                this.SpriteModel = new SpriteModel(gameInstance, 4);
            }
        }

        static int uniqueId = 0;
        public static int GetUniqueId()
        {
            uniqueId += 1;
            return uniqueId;
        }

        private struct InterpolationPacket
        {
            public Vector3 position;
            public double gameTime;

            public InterpolationPacket(Vector3 position, double gameTime)
            {
                this.position = position;
                this.gameTime = gameTime;
            }
        }

        public void UpdatePosition(Vector3 position, double gameTime)
        {
            interpList.Add(new InterpolationPacket(position, gameTime));

            // If we have less than 10 packets, go ahead and set the position directly.
            if (interpList.Count < 10)
                Position = position;

            // If we have more than 10 packets, remove the oldest.
            if (interpList.Count > 10)
                interpList.RemoveAt(0);
        }

        public void StepInterpolation(double gameTime)
        {
            // We have 10 packets, so interpolate from the second to last to the last.
            if (interpList.Count == 10)
            {
                Vector3 a = interpList[8].position, b = interpList[9].position;
                double ta = interpList[8].gameTime, tb = interpList[9].gameTime;
                Vector3 d = b - a;
                double timeScale = (interpList[9].gameTime - interpList[0].gameTime) / 9;
                double timeAmount = Math.Min((gameTime - ta) / timeScale, 1);
                Position = a + d * (float)timeAmount;
            }
        }
    }
}
