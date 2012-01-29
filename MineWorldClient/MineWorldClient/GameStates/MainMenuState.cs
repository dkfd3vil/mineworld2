using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Ruminate.GUI.Content;
using Ruminate.GUI.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MineWorld
{
    class MainMenuState : BaseState
    {
        GameStateManager gamemanager;
        RuminateGUI gui;
        Button start;
        TextBox ipadress;
        Texture2D cursor;
        Vector2 cursorpos;


        public MainMenuState(GameStateManager manager, GameStates associatedState)
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

            ipadress = new TextBox(new Offset(50,50,100,100),20);
            gui.AddElement(ipadress);

            gamemanager.game.IsMouseVisible = false;
            cursor = contentloader.Load<Texture2D>("Textures/Ui/cursor");
        }

        public override void Update(GameTime gameTime, InputHelper input)
        {
            gui.Update();
            if (start.IsPressed)
            {
                gamemanager.Pbag.ClientSender.SendJoinGame(ipadress.Value);
                gamemanager.SwitchState(GameStates.LoadingState);
            }
            cursorpos = input.MousePosition;
        }

        public override void Draw(GameTime gameTime, GraphicsDevice gDevice, SpriteBatch sBatch)
        {
            gDevice.Clear(Color.Blue);
            gui.Draw();
            sBatch.Begin();
            sBatch.Draw(cursor, cursorpos, Color.White);
            sBatch.End();
        }
    }
}
