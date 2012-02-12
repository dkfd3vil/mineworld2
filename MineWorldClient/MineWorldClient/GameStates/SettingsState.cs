using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using TomShane.Neoforce.Controls;

namespace MineWorld
{
    public class SettingsState : BaseState
    {
        GameStateManager gamemanager;
        Manager guiman;
        Window settingsmenu;
        Button back;
        TextBox playername;
        ScrollBar volume;

        public SettingsState(GameStateManager manager, GameStates associatedState)
            : base(manager, associatedState)
        {
            gamemanager = manager;
        }

        public override void LoadContent(ContentManager contentloader)
        {
            guiman = new Manager(gamemanager.game, gamemanager.graphics, "Default");
            guiman.Initialize();

            settingsmenu = new Window(guiman);
            settingsmenu.Init();
            settingsmenu.Resizable = false;
            settingsmenu.Movable = false;
            settingsmenu.CloseButtonVisible = false;
            settingsmenu.Text = "Settings Menu";
            settingsmenu.Width = 300;
            settingsmenu.Height = 400;
            settingsmenu.Center();
            settingsmenu.Visible = true;
            settingsmenu.BorderVisible = true;
            settingsmenu.Cursor = guiman.Skin.Cursors["Default"].Resource;

            back = new Button(guiman);
            back.Init();
            back.Text = "Go Back";
            back.Width = 200;
            back.Height = 50;
            back.Left = 50;
            back.Top = 300;
            back.Anchor = Anchors.Bottom;
            back.Parent = settingsmenu;

            playername = new TextBox(guiman);
            playername.Init();
            playername.Text = gamemanager.Pbag.Player.Name;
            playername.Width = 200;
            playername.Height = 50;
            playername.Left = 50;
            playername.Top = 0;
            playername.Anchor = Anchors.Bottom;
            playername.Parent = settingsmenu;

            volume = new ScrollBar(guiman, Orientation.Horizontal);
            volume.Init();
            //Todo check why volume.value is reseting it to 50 :S
            volume.Value = gamemanager.audiomanager.GetVolume();
            volume.Range = 100;
            volume.PageSize = 10;
            volume.StepSize = 1;
            volume.Width = 200;
            volume.Height = 50;
            volume.Left = 50;
            volume.Top = 50;
            volume.Anchor = Anchors.Bottom;
            volume.Parent = settingsmenu;

            guiman.Add(settingsmenu);

            gamemanager.game.IsMouseVisible = true;
        }

        public override void Unload()
        {
        }

        public override void Update(GameTime gameTime, InputHelper input)
        {
            guiman.Update(gameTime);
            if (back.Pushed)
            {
                back.Pushed = false;

                //Save all settings when back is pushed
                gamemanager.Pbag.Player.Name = playername.Text;
                gamemanager.audiomanager.SetVolume(volume.Value);

                //Also save it to the file
                gamemanager.SaveSettings();

                gamemanager.SwitchState(GameStates.MainMenuState);
            }
        }

        public override void Draw(GameTime gameTime, GraphicsDevice gDevice, SpriteBatch sBatch)
        {
            guiman.BeginDraw(gameTime);
            guiman.EndDraw();
        }
    }
}