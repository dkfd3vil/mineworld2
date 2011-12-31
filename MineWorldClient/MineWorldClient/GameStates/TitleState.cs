using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Ruminate.GUI.Content;
using Ruminate.GUI.Framework;

namespace MineWorld
{
    class TitleState : BaseState
    {
        GameStateManager gamemanager;
        RuminateGUI gui;
        Button start;
        ScrollBars bar;

        public TitleState(GameStateManager manager, GameStates associatedState)
            : base(manager, associatedState)
        {
            gamemanager = manager;
            gui = new RuminateGUI(gamemanager.game);
            gui.SetTheme(new EmbeddedTheme(gui));
        }

        public override void LoadContent(ContentManager contentloader)
        {
            start = new Button(new Offset(10, 120, 80, 100), "StartGame", null);
            gui.AddElement(start);

            gamemanager.game.IsMouseVisible = true;
        }

        public override void Update(GameTime gameTime, InputHelper input)
        {
            gui.Update();
            if(input.IsNewPress(Keys.Enter))
            {
                gamemanager.Pbag.JoinGame();
            }
            if (start.IsPressed)
            {
                gamemanager.Pbag.JoinGame();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            gamemanager.device.Clear(Color.White);
            gui.Draw();
        }
    }
}
