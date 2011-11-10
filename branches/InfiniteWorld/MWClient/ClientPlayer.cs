using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MineWorld
{
    public class ClientPlayer
    {
        private readonly List<InterpolationPacket> _interpList = new List<InterpolationPacket>();
        public bool Alive;
        public Vector3 Heading = Vector3.Zero;
        public int ID;
        public string Name = "";
        public Vector3 Position = Vector3.Zero;
        public SpriteModel SpriteModel;
        

        public ClientPlayer(MineWorldGame gameinstance)
        {
            SpriteModel = new SpriteModel(gameinstance);
        }

        public void UpdatePosition(Vector3 position, double gameTime)
        {
            _interpList.Add(new InterpolationPacket(position, gameTime));

            // If we have less than 10 packets, go ahead and set the position directly.
            if (_interpList.Count < 10)
                Position = position;

            // If we have more than 10 packets, remove the oldest.
            if (_interpList.Count > 10)
                _interpList.RemoveAt(0);
        }

        public void StepInterpolation(double gameTime)
        {
            // We have 10 packets, so interpolate from the second to last to the last.
            if (_interpList.Count == 10)
            {
                Vector3 a = _interpList[8].Position, b = _interpList[9].Position;
                double ta = _interpList[8].GameTime;
                Vector3 d = b - a;
                double timeScale = (_interpList[9].GameTime - _interpList[0].GameTime)/9;
                double timeAmount = Math.Min((gameTime - ta)/timeScale, 1);
                Position = a + d*(float) timeAmount;
            }
        }

        private struct InterpolationPacket
        {
            public readonly double GameTime;
            public readonly Vector3 Position;

            public InterpolationPacket(Vector3 position, double gameTime)
            {
                Position = position;
                GameTime = gameTime;
            }
        }
    }
}