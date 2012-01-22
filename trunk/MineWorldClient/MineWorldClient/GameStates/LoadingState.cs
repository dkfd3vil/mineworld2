using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace MineWorld
{
    public class LoadingState : BaseState
    {
        GameStateManager gamemanager;
        Texture2D loadingimg;
        float loadingangle;
        Vector2 loadingorgin;
        Vector2 loadinglocation;
        Rectangle loadingrectangle;
        

        public LoadingState(GameStateManager manager, GameStates associatedState)
            : base(manager, associatedState)
        {
            gamemanager = manager;
        }

        public override void LoadContent(ContentManager contentloader)
        {
            loadingimg = contentloader.Load<Texture2D>("Textures/States/loading");
            loadingorgin = new Vector2(loadingimg.Width / 2, loadingimg.Height / 2);
            loadinglocation = new Vector2(gamemanager.graphics.PreferredBackBufferWidth / 2, gamemanager.graphics.PreferredBackBufferHeight / 2);
            loadingrectangle = new Rectangle(0, 0, loadingimg.Width, loadingimg.Height);

            gamemanager.game.IsMouseVisible = false;
        }

        public override void Update(GameTime gameTime, InputHelper input)
        {
            loadingangle += 0.01f;
            //If everything is loaded then lets play
            if (gamemanager.Pbag.WorldManager.Everythingloaded())
            {
                gamemanager.SwitchState(GameStates.MainGameState);
                gamemanager.Pbag.ClientSender.SendPlayerInWorld();
            }
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            loadingorgin = new Vector2(loadingimg.Width / 2, loadingimg.Height / 2);
            loadinglocation = new Vector2(gamemanager.graphics.PreferredBackBufferWidth / 2, gamemanager.graphics.PreferredBackBufferHeight / 2);
            loadingrectangle = new Rectangle(0, 0, loadingimg.Width, loadingimg.Height);
            gamemanager.device.Clear(Color.Black);
            gamemanager.spriteBatch.Begin();
            gamemanager.spriteBatch.Draw(loadingimg, loadinglocation, loadingrectangle, Color.White, loadingangle, loadingorgin, 1.0f, SpriteEffects.None, 1);
            gamemanager.spriteBatch.End();
        }
    }
}