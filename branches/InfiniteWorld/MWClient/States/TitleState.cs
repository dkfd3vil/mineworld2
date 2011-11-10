using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineWorld.StateMasher;

namespace MineWorld.States
{
    public class TitleState : State
    {
        private Rectangle _drawRect;
        private string _nextState;
        private Texture2D _texMenu;
        private DateTime _titlescreenseconds;

        public override void OnEnter(string oldState)
        {
            Sm.IsMouseVisible = true;
            _titlescreenseconds = DateTime.Now;

            _texMenu = Sm.Content.Load<Texture2D>("menus/tex_menu_title");

            _drawRect = new Rectangle(Sm.GraphicsDevice.Viewport.Width/2 - 1024/2,
                                      Sm.GraphicsDevice.Viewport.Height/2 - 768/2,
                                      1024,
                                      1024);
        }

        public override void OnLeave(string newState)
        {
        }

        public override string OnUpdate(GameTime gameTime, KeyboardState keyState, MouseState mouseState)
        {
            // Do network stuff.
            TimeSpan titlescreenbetween = DateTime.Now - _titlescreenseconds;
            // After 3 seconds we go to the Serverbrowser state
            if (titlescreenbetween.TotalMilliseconds > 3000)
            {
                _nextState = "MineWorld.States.ServerBrowserState";
            }

            return _nextState;
        }

        public override void OnRenderAtEnter(GraphicsDevice graphicsDevice)
        {
        }

        public override void OnRenderAtUpdate(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            spriteBatch.Draw(_texMenu, _drawRect, Color.White);
            spriteBatch.End();
        }

        public override void OnKeyDown(Keys key)
        {
            if (key == Keys.Escape)
            {
                Sm.Exit();
            }
        }

        public override void OnKeyUp(Keys key)
        {
        }

        public override void OnMouseDown(MouseButtons button, int x, int y)
        {
            _nextState = "MineWorld.States.ServerBrowserState";
            P.PlaySound(MineWorldSound.ClickHigh);
        }

        public override void OnMouseUp(MouseButtons button, int x, int y)
        {
        }

        public override void OnMouseScroll(int scrollDelta)
        {
        }
    }
}