using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Threading;
using System.ComponentModel;
using Lidgren.Network;
using System.Net;
using MineWorldData;

namespace MineWorld
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MineWorldClient : Game
    {
        //Global variable definitions
        GameStateManager GameStateManager;
        GraphicsDeviceManager Graphics;
        NetClient Client;
        NetIncomingMessage _msgBuffer;

        public MineWorldClient()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            NetPeerConfiguration netConfig = new NetPeerConfiguration("MineWorld");
            Client = new NetClient(netConfig);
        }

        protected override void Initialize()
        {
            //TODO Redo initialize of mineworldclient
            GameStateManager = new GameStateManager(Graphics,Content,this);
            Graphics.PreferredBackBufferWidth = 800;
            Graphics.PreferredBackBufferHeight = 600;
            Window.Title = "MineWorldClient Alpha";
            Graphics.ApplyChanges();

            //Set mouse to the middle
            //Mouse.SetPosition(1920 / 2, 1080 / 2);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            GameStateManager.LoadContent();
            //GameStateManager.SwitchState(GameStates.TitleState);
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            GameStateManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GameStateManager.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
