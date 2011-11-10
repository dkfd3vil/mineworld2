using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MineWorld.Engines
{
    public class DebugEngine
    {
        private readonly MineWorldGame _gameInstance;
        private readonly SpriteBatch _spriteBatch;
        private PropertyBag _p;
        private readonly SpriteFont _uiFont;

        public DebugEngine(MineWorldGame gameInstance)
        {
            _gameInstance = gameInstance;
            _spriteBatch = new SpriteBatch(gameInstance.GraphicsDevice);

            // Load fonts.
            _uiFont = gameInstance.Content.Load<SpriteFont>("font_04b08");
        }

        public void Render(GraphicsDevice graphicsDevice)
        {
            // If we don't have _P, grab it from the current gameInstance.
            // We can't do this in the constructor because we are created in the property bag's constructor!
            if (_p == null)
                _p = _gameInstance.PropertyBag;

            if (_p.DebugMode)
            {
                Color chatColor = Color.Red;
                const int y = 0;
                const int x = 20;

                // Draw the UI.
                _spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);

                _spriteBatch.DrawString(_uiFont, "Player dead: " + _p.PlayerDead.ToString(), new Vector2(x, y), chatColor);
                _spriteBatch.DrawString(_uiFont, "Player id : " + _p.PlayerMyId.ToString(), new Vector2(x, y + 16),
                                       chatColor);
                _spriteBatch.DrawString(_uiFont, "Player pos : " + _p.PlayerPosition.ToString(), new Vector2(x, y + 32),
                                       chatColor);
                _spriteBatch.DrawString(_uiFont, "Player name : " + _p.PlayerHandle, new Vector2(x, y + 48), chatColor);
                _spriteBatch.DrawString(_uiFont, "Player holdingbreath : " + _p.PlayerHoldBreath.ToString(),
                                       new Vector2(x, y + 64), chatColor);
                _spriteBatch.DrawString(_uiFont, "Player playerlistcount : " + _p.PlayerList.Count.ToString(),
                                       new Vector2(x, y + 80), chatColor);
                _spriteBatch.DrawString(_uiFont, "Player screeneffect : " + _p.ScreenEffect.ToString(),
                                       new Vector2(x, y + 96), chatColor);
                _spriteBatch.DrawString(_uiFont, "Player screeneffectcounter : " + _p.ScreenEffectCounter.ToString(),
                                       new Vector2(x, y + 112), chatColor);
                _spriteBatch.DrawString(_uiFont, "Player movevector : " + _p.MoveVector.ToString(),
                                       new Vector2(x, y + 128), chatColor);

                _spriteBatch.End();
            }
        }
    }
}