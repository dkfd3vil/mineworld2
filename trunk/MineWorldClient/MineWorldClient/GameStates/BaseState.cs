﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MineWorld
{
    public abstract class BaseState
    {
        public GameStateManager Manager;
        public GameStates AssociatedState;
        public bool Contentloaded;

        public BaseState(GameStateManager manager, GameStates associatedState)
        {
            this.Manager = manager;
            this.AssociatedState = associatedState;
        }

        public abstract void Unload();
        public abstract void LoadContent(ContentManager contentloader);
        public abstract void Update(GameTime gameTime, InputHelper input);
        public abstract void Draw(GameTime gameTime,GraphicsDevice gDevice,SpriteBatch sBtach);
    }
}