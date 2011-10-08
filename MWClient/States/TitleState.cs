using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using StateMasher;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace MineWorld.States
{
    public class TitleState : State
    {
        Texture2D texMenu;
        Rectangle drawRect;
        string nextState = null;
        DateTime titlescreenseconds;

        public override void OnEnter(string oldState)
        {
            _SM.IsMouseVisible = true;
            titlescreenseconds = DateTime.Now;

            texMenu = _SM.Content.Load<Texture2D>("menus/tex_menu_title");

            drawRect = new Rectangle(_SM.GraphicsDevice.Viewport.Width / 2 - 1024 / 2,
                                     _SM.GraphicsDevice.Viewport.Height / 2 - 768 / 2,
                                     1024,
                                     1024);
        }

        public override void OnLeave(string newState)
        {

        }

        public override string OnUpdate(GameTime gameTime, KeyboardState keyState, MouseState mouseState)
        {
            // Do network stuff.
            TimeSpan titlescreenbetween = DateTime.Now - titlescreenseconds;
            // After 3 seconds we go to the Serverbrowser state
            if (titlescreenbetween.TotalMilliseconds > 3000)
            {
                nextState = "MineWorld.States.ServerBrowserState";
            }

            return nextState;
        }

        public override void OnRenderAtEnter(GraphicsDevice graphicsDevice)
        {

        }

        public override void OnRenderAtUpdate(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            spriteBatch.Draw(texMenu, drawRect, Color.White);
            spriteBatch.End();
        }

        public override void OnKeyDown(Keys key)
        {
            if (key == Keys.Escape)
            {
                _SM.Exit();
            }
        }

        public override void OnKeyUp(Keys key)
        {

        }

        public override void OnMouseDown(MouseButtons button, int x, int y)
        {
            nextState = "MineWorld.States.ServerBrowserState";
            _P.PlaySound(MineWorldSound.ClickHigh);
        }

        public override void OnMouseUp(MouseButtons button, int x, int y)
        {

        }

        public override void OnMouseScroll(int scrollDelta)
        {

        }
    }
}
