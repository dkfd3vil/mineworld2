using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Ruminate.GUI.Content;
using Ruminate.GUI.Framework;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics;

namespace MineWorld
{
    class TitleState : BaseState
    {
        GameStateManager gamemanager;
        Song introsong;
        bool introstarted = false;
        Texture2D background;
        Rectangle size;

        public TitleState(GameStateManager manager, GameStates associatedState)
            : base(manager, associatedState)
        {
            gamemanager = manager;
        }

        public override void LoadContent(ContentManager contentloader)
        {
            gamemanager.game.IsMouseVisible = false;
            introsong = contentloader.Load<Song>("Music/intro");
            background = contentloader.Load<Texture2D>("Textures/States/titlestate");
            size.Width = gamemanager.graphics.PreferredBackBufferWidth;
            size.Height = gamemanager.graphics.PreferredBackBufferHeight;
        }

        public override void Update(GameTime gameTime, InputHelper input)
        {
            if (!introstarted)
            {
                gamemanager.audiomanager.PlaySong(introsong, false);
                introstarted = true;
            }
            if(input.AnyKeyPressed(true))
            {
                gamemanager.audiomanager.StopPlaying();
                gamemanager.SwitchState(GameStates.MainMenuState);
            }
        }

        public override void Draw(GameTime gameTime, GraphicsDevice gDevice, SpriteBatch sBatch)
        {
            gDevice.Clear(Color.Black);
            sBatch.Begin();
            sBatch.Draw(background, size, Color.White);
            sBatch.End();
        }
    }
}
