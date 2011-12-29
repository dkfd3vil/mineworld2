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
        SpriteFont myfont;
        

        public LoadingState(GameStateManager manager, GameStates associatedState)
            : base(manager, associatedState)
        {
            gamemanager = manager;
        }

        public override void LoadContent(ContentManager contentloader)
        {
            myfont = contentloader.Load<SpriteFont>("SpriteFont1");
        }

        public override void Update(GameTime gameTime, InputHelper input)
        {
            //throw new NotImplementedException();
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            gamemanager.device.Clear(Color.Blue);
            gamemanager.spriteBatch.Begin();
            gamemanager.spriteBatch.DrawString(myfont,"LOADING !!!",new Vector2(gamemanager.graphics.PreferredBackBufferWidth / 2,gamemanager.graphics.PreferredBackBufferHeight / 2),Color.Black);
            gamemanager.spriteBatch.End();
        }
    }
}