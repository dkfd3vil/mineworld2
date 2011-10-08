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
    public class DebugEngine
    {
        MineWorldGame gameInstance;
        PropertyBag _P;
        SpriteBatch spriteBatch;
        public SpriteFont uiFont;

        public DebugEngine(MineWorldGame gameInstance)
        {
            this.gameInstance = gameInstance;
            spriteBatch = new SpriteBatch(gameInstance.GraphicsDevice);

            // Load fonts.
            uiFont = gameInstance.Content.Load<SpriteFont>("font_04b08");
        }

        public void Update(GameTime gameTime)
        {
            if (_P == null)
                return;
        }

        public void Render(GraphicsDevice graphicsDevice)
        {
            // If we don't have _P, grab it from the current gameInstance.
            // We can't do this in the constructor because we are created in the property bag's constructor!
            if (_P == null)
                _P = gameInstance.propertyBag;

            Color chatColor = Color.Red;
            int y = 0;
            int x = 20;

            // Draw the UI.
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);

            spriteBatch.DrawString(uiFont, "Player dead: " + _P.playerDead.ToString(), new Vector2(x, y), chatColor);
            spriteBatch.DrawString(uiFont, "Player id : " + _P.playerMyId.ToString(), new Vector2(x, y + 16), chatColor);
            spriteBatch.DrawString(uiFont, "Player pos : " + _P.playerPosition.ToString(), new Vector2(x, y + 32), chatColor);
            spriteBatch.DrawString(uiFont, "Player name : " + _P.playerHandle.ToString(), new Vector2(x, y + 48), chatColor);
            spriteBatch.DrawString(uiFont, "Player holdingbreath : " + _P.playerHoldBreath.ToString(), new Vector2(x, y + 64), chatColor);
            spriteBatch.DrawString(uiFont, "Player playerlistcount : " + _P.playerList.Count.ToString(), new Vector2(x, y + 80), chatColor);
            spriteBatch.DrawString(uiFont, "Player screeneffect : " + _P.screenEffect.ToString(), new Vector2(x, y + 96), chatColor);
            spriteBatch.DrawString(uiFont, "Player screeneffectcounter : " + _P.screenEffectCounter.ToString(), new Vector2(x, y + 112), chatColor);
            spriteBatch.DrawString(uiFont, "Player movevector : " + _P.MoveVector.ToString(), new Vector2(x, y + 128), chatColor);

            spriteBatch.End();
        }
    }
}
