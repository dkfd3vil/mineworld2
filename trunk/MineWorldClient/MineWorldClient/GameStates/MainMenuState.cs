using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using TomShane.Neoforce.Controls;
using Microsoft.Xna.Framework.Graphics;

namespace MineWorld
{
    class MainMenuState : BaseState
    {
        GameStateManager gamemanager;
        Manager guiman;
        Window mainmenu;
        Button play;
        Button settings;
        Button exit;

        public MainMenuState(GameStateManager manager, GameStates associatedState)
            : base(manager, associatedState)
        {
            gamemanager = manager;
        }

        public override void LoadContent(ContentManager contentloader)
        {
            guiman = new Manager(gamemanager.game, gamemanager.graphics, "Default");
            guiman.Initialize();

            mainmenu = new Window(guiman);
            mainmenu.Init();
            mainmenu.Resizable = false;
            mainmenu.Movable = false;
            mainmenu.CloseButtonVisible = false;
            mainmenu.Text = "Main Menu";
            mainmenu.Width = 300;
            mainmenu.Height = 400;
            mainmenu.Center();
            mainmenu.Visible = true;
            mainmenu.BorderVisible = true;
            //mainmenu.Cursor = guiman.Skin.Cursors["Default"].Resource;

            play = new Button(guiman);
            play.Init();
            play.Text = "Play";
            play.Width = 200;
            play.Height = 50;
            play.Left = 50;
            play.Top = 0;
            play.Anchor = Anchors.Bottom;
            play.Parent = mainmenu;

            settings = new Button(guiman);
            settings.Init();
            settings.Text = "Settings";
            settings.Width = 200;
            settings.Height = 50;
            settings.Left = 50;
            settings.Top = 50;
            settings.Anchor = Anchors.Bottom;
            settings.Parent = mainmenu;

            exit = new Button(guiman);
            exit.Init();
            exit.Text = "Exit";
            exit.Width = 200;
            exit.Height = 50;
            exit.Left = 50;
            exit.Top = 100;
            exit.Anchor = Anchors.Bottom;
            exit.Parent = mainmenu;

            guiman.Cursor = guiman.Skin.Cursors["Default"].Resource;
            guiman.Add(mainmenu);

            gamemanager.game.IsMouseVisible = true;
        }

        public override void Unload()
        {
        }

        public override void Update(GameTime gameTime, InputHelper input)
        {
            guiman.Update(gameTime);
            if (gamemanager.game.IsActive)
            {
                if (play.Pushed)
                {
                    play.Pushed = false;
                    gamemanager.Pbag.ClientSender.SendJoinGame("127.0.0.1");
                    gamemanager.SwitchState(GameStates.LoadingState);
                }
                if (settings.Pushed)
                {
                    settings.Pushed = false;
                    gamemanager.SwitchState(GameStates.SettingsState);
                }
                if (exit.Pushed)
                {
                    exit.Pushed = false;
                    gamemanager.ExitGame();
                }
            }
        }

        public override void Draw(GameTime gameTime, GraphicsDevice gDevice, SpriteBatch sBatch)
        {
            guiman.BeginDraw(gameTime);
            guiman.EndDraw();
        }
    }
}
