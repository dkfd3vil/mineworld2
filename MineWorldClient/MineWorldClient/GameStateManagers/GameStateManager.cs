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
        MainGameState,
        ErrorState,
        SettingsState,
        ServerBrowsingState,
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
        private LoadingState loadingstate;
        private MainGameState maingamestate;
        private ErrorState errorstate;
        private SettingsState settingsstate;
        private ServerBrowsingState serverbrowsingstate;
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
                errorstate = new ErrorState(this,GameStates.ErrorState),
                settingsstate = new SettingsState(this,GameStates.SettingsState),
                serverbrowsingstate = new ServerBrowsingState(this,GameStates.ServerBrowsingState),
            };
            //curScreen = titlestate;
            Pbag = new PropertyBag(gam,this);

            //Set initial state in the manager itself
            SwitchState(GameStates.TitleState);
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
                    //This is true for the first time
                    if (curScreen != null)
                    {
                        //Call unload for our currentscreen
                        curScreen.Unload();
                        curScreen.Contentloaded = false;
                    }

                    //Switch our currentscreen to our new screen
                    curScreen = screen;

                    //If our new screen content isnt loaded yet call it
                    if (curScreen.Contentloaded == false)
                    {
                        curScreen.LoadContent(conmanager);
                        curScreen.Contentloaded = true;
                    }
                    break;
                }
            }
        }

        public void SetErrorState(ErrorMsg msg)
        {
            errorstate.SetError(msg);
            SwitchState(GameStates.ErrorState);
        }

        public void AddServer(ServerInformation server)
        {
            serverbrowsingstate.AddServer(server);
        }

        public void RemoveServer(ServerInformation server)
        {
            serverbrowsingstate.RemoveServer(server);
        }

        public void ExitGame()
        {
            game.Exit();
        }

        public void Draw(GameTime gameTime)
        {
            curScreen.Draw(gameTime,device,spriteBatch);
        }

        public void LoadSettings()
        {
            game.Window.Title = "MineWorldClient v" + Constants.MINEWORLDCLIENT_VERSION.ToString();
            game.Window.AllowUserResizing = true;
            game.Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

            graphics.PreferredBackBufferHeight = config.SettingGroups["Video"].Settings["Height"].GetValueAsInt();
            graphics.PreferredBackBufferWidth = config.SettingGroups["Video"].Settings["Width"].GetValueAsInt();
            graphics.IsFullScreen = config.SettingGroups["Video"].Settings["Fullscreen"].GetValueAsBool();
            graphics.SynchronizeWithVerticalRetrace = config.SettingGroups["Video"].Settings["Vsync"].GetValueAsBool();
            graphics.PreferMultiSampling = config.SettingGroups["Video"].Settings["Multisampling"].GetValueAsBool();
            graphics.ApplyChanges();

            audiomanager.SetVolume(config.SettingGroups["Sound"].Settings["Volume"].GetValueAsInt());

            Pbag.Player.Name = config.SettingGroups["Player"].Settings["Name"].GetValueAsString();

            Pbag.WorldManager.customtexturepath = config.SettingGroups["Game"].Settings["Customtexturepath"].GetValueAsString();
        }

        public void SaveSettings()
        {
            config.SettingGroups["Video"].Settings["Height"].SetValue(graphics.PreferredBackBufferHeight);
            config.SettingGroups["Video"].Settings["Width"].SetValue(graphics.PreferredBackBufferWidth);
            config.SettingGroups["Video"].Settings["Fullscreen"].SetValue(graphics.IsFullScreen);
            config.SettingGroups["Video"].Settings["Vsync"].SetValue(graphics.SynchronizeWithVerticalRetrace);
            config.SettingGroups["Video"].Settings["Multisampling"].SetValue(graphics.PreferMultiSampling);

            config.SettingGroups["Sound"].Settings["Volume"].SetValue(audiomanager.GetVolume());

            config.SettingGroups["Player"].Settings["Name"].SetValue(Pbag.Player.Name);

            config.Save("data/settings.ini");
        }

        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            graphics.PreferredBackBufferWidth = game.Window.ClientBounds.Width;
            graphics.PreferredBackBufferHeight = game.Window.ClientBounds.Height;
        }
    }
}
