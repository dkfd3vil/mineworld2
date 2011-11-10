using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MineWorld.Engines
{
    public class SkyEngine
    {
        private readonly MineWorldGame _gameInstance;
        private PropertyBag _p;

        public SkyEngine(MineWorldGame gameInstance)
        {
            _gameInstance = gameInstance;
        }

        public void Update(GameTime gameTime)
        {
            //Todo Refractor this
        }

        public void Render(GraphicsDevice graphicsDevice)
        {
            // If we don't have _P, grab it from the current gameInstance.
            // We can't do this in the constructor because we are created in the property bag's constructor!
            if (_p == null)
                _p = _gameInstance.PropertyBag;

            graphicsDevice.Clear(CalculateSkyColor(_p.DayTime));
        }

        private Color CalculateSkyColor(float time)
        {
            Color night = Color.Black; //TODO: Make these two a setting?
            Color day = Color.LightBlue; //yeah I feel dirty now
            Color newColor = Color.Black;
            newColor.R = CalculateGradientPoint(night.R, day.R, time);
            newColor.G = CalculateGradientPoint(night.G, day.G, time);
            newColor.B = CalculateGradientPoint(night.B, day.B, time);
            return newColor;
        }

        private static byte CalculateGradientPoint(byte value1, byte value2, float position)
        {
            int diff = Math.Abs(value1 - value2);
            float newPoint;
            if (value1 < value2)
            {
                newPoint = value1 + (diff*position);
            }
            else
            {
                newPoint = value2 + (diff*position);
            }
            return (byte) (int) newPoint;
        }
    }
}