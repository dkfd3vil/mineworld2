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

namespace MineWorld
{
    public enum GameStates
    {
        TitleState,
        LoadingState,
        MainGameState
    }

    public class GameStateManager
    {
        public MineWorldClient game;
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        public GraphicsDevice device;
        private InputHelper inputhelper;
        public ContentManager conmanager;
        public PropertyBag Pbag;

        private TitleState titlestate;
        public LoadingState loadingstate;
        private MainGameState maingamestate;
        private BaseState[] screens;

        private BaseState curScreen;

        public GameStateManager(GraphicsDeviceManager man,ContentManager cman,MineWorldClient gam)
        {
            inputhelper = new InputHelper();
            game = gam;
            conmanager = cman;
            graphics = man;
            device = graphics.GraphicsDevice;
            spriteBatch = new SpriteBatch(device);
            screens = new BaseState[]  
            { 
                titlestate = new TitleState(this,GameStates.TitleState),
                loadingstate = new LoadingState(this,GameStates.LoadingState),
                maingamestate = new MainGameState(this, GameStates.MainGameState), 
            };
            //Set initial state in the manager itself
            curScreen = titlestate;
            Pbag = new PropertyBag(gam,this);
        }

        public void LoadContent()
        {
            foreach (BaseState screen in screens)
                screen.LoadContent(conmanager);
        }

        public void Update(GameTime gameTime)
        {
            Pbag.ReceiveMessages();
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

        [DllImport("user32.dll")]
        public static extern int GetForegroundWindow();
    }
}
