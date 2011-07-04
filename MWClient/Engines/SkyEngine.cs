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

namespace MineWorld
{
    public class SkyEngine
    {
        MineWorldGame gameInstance;
        PropertyBag _P;

        public SkyEngine(MineWorldGame gameInstance)
        {
            this.gameInstance = gameInstance;
        }

        public void Update(GameTime gameTime)
        {
            //Todo Refractor this
        }

        public void Render(GraphicsDevice graphicsDevice)
        {
            // If we don't have _P, grab it from the current gameInstance.
            // We can't do this in the constructor because we are created in the property bag's constructor!
            if (_P == null)
                _P = gameInstance.propertyBag;

            graphicsDevice.Clear(calculateSkyColor(_P.time));
        }

        Color calculateSkyColor(float time)
        {
            Color night = Color.Black; //TODO: Make these two a setting?
            Color day = Color.LightBlue; //yeah I feel dirty now
            Color newColor = Color.Black;
            newColor.R = calculateGradientPoint(night.R, day.R, time);
            newColor.G = calculateGradientPoint(night.G, day.G, time);
            newColor.B = calculateGradientPoint(night.B, day.B, time);
            return newColor;
        }

        byte calculateGradientPoint(byte value1, byte value2, float position)
        {
            int diff = Math.Abs((int)value1 - (int)value2);
            float newPoint = 0;
            if (value1 < value2)
            {
                newPoint = value1 + ((float)diff * position);
            }
            else
            {
                newPoint = value2 + ((float)diff * position);
            }
            return (byte)(int)newPoint;
        }
    }
}