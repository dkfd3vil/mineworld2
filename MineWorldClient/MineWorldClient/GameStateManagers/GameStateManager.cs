using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.Runtime.InteropServices;
using EasyConfig;
using MineWorldData;

namespace MineWorld
{
    public enum GameStates
    {
        TitleState,
        MainMenuState,
        LoadingState,
        MainGameState
    }

    public class GameStateManager
    {
        public MineWorldClient game;
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        public GraphicsDevice device;

        public AudioManager audiomanager;
        private InputHelper inputhelper;
        public ConfigFile config;
        public ContentManager conmanager;
        public PropertyBag Pbag;

        private TitleState titlestate;
        private MainMenuState mainmenustate;
        public LoadingState loadingstate;
        private MainGameState maingamestate;
        private BaseState[] screens;

        private BaseState curScreen;

        public GameStateManager(GraphicsDeviceManager man,ContentManager cman,MineWorldClient gam)
        {
            audiomanager = new AudioManager();
            config = new ConfigFile("data/settings.ini");
            inputhelper = new InputHelper();
            game = gam;
            conmanager = cman;
            graphics = man;
            device = graphics.GraphicsDevice;
            spriteBatch = new SpriteBatch(device);
            screens = new BaseState[]  
            { 
                titlestate = new TitleState(this,GameStates.TitleState),
                mainmenustate = new MainMenuState(this,GameStates.MainMenuState),
                loadingstate = new LoadingState(this,GameStates.LoadingState),
                maingamestate = new MainGameState(this, GameStates.MainGameState), 
            };
            //Set initial state in the manager itself
            curScreen = titlestate;
            Pbag = new PropertyBag(gam,this);
        }

        public void LoadContent()
        {
            LoadSettings();
            foreach (BaseState screen in screens)
                screen.LoadContent(conmanager);
        }

        public void Update(GameTime gameTime)
        {
            Pbag.ClientListener.Update();
            inputhelper.Update();
            curScreen.Update(gameTime,inputhelper);
        }

        public void SwitchState(GameStates newState)
        {
            foreach (BaseState screen in screens)
            {
                if (screen.AssociatedState == newState)
                {
                    curScreen = screen;
                    break;
                }
            }
        }

        public void ExitState()
        {
            game.Exit();
        }

        public void Draw(GameTime gameTime)
        {
            curScreen.Draw(gameTime);
        }

        public bool WindowHasFocus()
        {
            return GetForegroundWindow() == (int)game.Window.Handle;
        }

        public void LoadSettings()
        {
            game.Window.Title = "MineWorldClient v" + Constants.MINEWORLDCLIENT_VERSION;
            game.Window.AllowUserResizing = true;
            graphics.PreferredBackBufferHeight = config.SettingGroups["Video"].Settings["Height"].GetValueAsInt();
            graphics.PreferredBackBufferWidth = config.SettingGroups["Video"].Settings["Width"].GetValueAsInt();
            graphics.IsFullScreen = config.SettingGroups["Video"].Settings["Fullscreen"].GetValueAsBool();
            graphics.SynchronizeWithVerticalRetrace = config.SettingGroups["Video"].Settings["Vsync"].GetValueAsBool();
            graphics.PreferMultiSampling = config.SettingGroups["Video"].Settings["Multisampling"].GetValueAsBool();
            graphics.ApplyChanges();

            audiomanager.volume = config.SettingGroups["Sound"].Settings["Volume"].GetValueAsFloat() / 100;

            Pbag.Player.Name = config.SettingGroups["Player"].Settings["Name"].GetValueAsString();
        }

        [DllImport("user32.dll")]
        public static extern int GetForegroundWindow();
    }
}
