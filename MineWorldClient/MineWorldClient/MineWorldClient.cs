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

        public MineWorldClient()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            //TODO Redo initialize of mineworldclient
            GameStateManager = new GameStateManager(Graphics, Content, this);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            GameStateManager.LoadContent();
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
