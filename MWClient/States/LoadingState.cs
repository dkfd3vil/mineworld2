using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineWorld.StateMasher;

namespace MineWorld.States
{
    public class LoadingState : State
    {
        private static readonly string[] Hints = new[]
                                                     {
                                                         "Chuck Norris doesn't like beta",
                                                         "Explosives are irritating, so use em on the enemy",
                                                         "[Insert some loading message here] LOL :)",
                                                         "With extra bytes",
                                                         "NOW! 15 terrapixel",
                                                         "Bacon blocks are shown on your teammates' radar.\nUse them to mark important locations."
                                                         ,
                                                         "Build force fields to keep the enemy out of your tunnels.",
                                                         "Shock blocks will kill anyone who touches their underside.",
                                                         "Combine jump blocks and shock blocks to make deadly traps!",
                                                         "The Prospectron 3000 can detect gold and diamonds through walls.\nLet a prospector guide you when digging."
                                                         ,
                                                         "Miners can dig much faster than the other classes!\nUse them to quickly mine out an area."
                                                         ,
                                                         "Engineers can build force fields of the other team's color.\nUse this ability to create bridges only accessible to your team."
                                                         ,
                                                         "Movement speed is doubled on road blocks.\nUse them to cover horizontal distances quickly."
                                                         ,
                                                         "Return gold and diamonds to the surface to collect loot for your team!"
                                                         ,
                                                         "Press Q to quickly signal your teammates.",
                                                         "All constructions require metal ore.\nDig for some or take it from your team's banks."
                                                         ,
                                                         "Banks are indestructible - use them as walls even sappers can't pass!"
                                                         ,
                                                         "Don't have a scroll wheel?\nPress R to cycle through block types for the construction gun."
                                                         ,
                                                         "You can set your name and adjust your screen resolution\nby using the settings scree or the config files."
                                                     };

        private string[] _currentHint;

        private Rectangle _drawRect;
        private string _nextState;
        private Texture2D _texMenu;
        private SpriteFont _uiFont;

        public override void OnEnter(string oldState)
        {
            Sm.IsMouseVisible = false;

            _texMenu = Sm.Content.Load<Texture2D>("menus/tex_menu_loading");

            _drawRect = new Rectangle(Sm.GraphicsDevice.Viewport.Width/2 - 1024/2,
                                     Sm.GraphicsDevice.Viewport.Height/2 - 768/2,
                                     1024,
                                     1024);

            _uiFont = Sm.Content.Load<SpriteFont>("font_04b08");

            // Pick a random hint.
            Random randGen = new Random();
            _currentHint = Hints[randGen.Next(0, Hints.Length)].Split("\n".ToCharArray());
        }

        public override string OnUpdate(GameTime gameTime, KeyboardState keyState, MouseState mouseState)
        {
            // Do network stuff.
            (Sm as MineWorldGame).UpdateNetwork(gameTime);

            return _nextState;
        }

        public override void OnRenderAtUpdate(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            const string progressText = "Connecting/Loading";

            SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            spriteBatch.Draw(_texMenu, _drawRect, Color.White);
            spriteBatch.DrawString(_uiFont, progressText,
                                   new Vector2(
                                       ((int)
                                        (Sm.GraphicsDevice.Viewport.Width/2 - _uiFont.MeasureString(progressText).X/2)),
                                       _drawRect.Y + 430), Color.White);
            for (int i = 0; i < _currentHint.Length; i++)
                spriteBatch.DrawString(_uiFont, _currentHint[i],
                                       new Vector2(
                                           ((int)
                                            (Sm.GraphicsDevice.Viewport.Width/2 -
                                             _uiFont.MeasureString(_currentHint[i]).X/2)), _drawRect.Y + 600 + 25*i),
                                       Color.White);
            spriteBatch.End();
        }

        public override void OnKeyDown(Keys key)
        {
            if (key == Keys.Escape)
            {
                P.NetClient.Disconnect("Client disconnected.");
                _nextState = "MineWorld.States.ServerBrowserState";
            }
        }

        public override void OnKeyUp(Keys key)
        {
        }

        public override void OnMouseDown(MouseButtons button, int x, int y)
        {
        }

        public override void OnMouseUp(MouseButtons button, int x, int y)
        {
        }

        public override void OnMouseScroll(int scrollDelta)
        {
        }
    }
}