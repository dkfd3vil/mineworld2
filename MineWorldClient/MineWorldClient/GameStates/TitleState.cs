using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace MineWorld
{
    class TitleState : BaseState
    {
        GameStateManager gamemanager;

        public TitleState(GameStateManager manager, GameStates associatedState)
            : base(manager, associatedState)
        {
            gamemanager = manager;
        }

        public override void LoadContent(ContentManager contentloader)
        {

        }

        public override void Update(GameTime gameTime, InputHelper input)
        {
            if(input.IsNewPress(Keys.Enter))
            {
                gamemanager.Pbag.JoinGame();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            gamemanager.device.Clear(Color.Black);
        }
    }
}
