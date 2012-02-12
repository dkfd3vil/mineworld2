﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MineWorldData;

namespace MineWorld
{
    public class MainGameState : BaseState
    {
        public GameStateManager gamemanager;

        public MainGameState(GameStateManager manager, GameStates associatedState)
            : base(manager, associatedState)
        {
            gamemanager = manager;
        }

        public override void LoadContent(ContentManager contentloader)
        {
            //Load our world
            gamemanager.Pbag.WorldManager.Load(contentloader);
            //Load our Player
            gamemanager.Pbag.Player.Load(contentloader);
            //Load our debugger
            gamemanager.Pbag.Debugger.Load(contentloader);
        }

        public override void Unload()
        {
            gamemanager.Pbag.WorldManager.worldmaploaded = false;
        }

        public override void Update(GameTime gameTime,InputHelper input)
        {
            if (gamemanager.game.IsActive)
            {
                //Lets see if we need to end this game
                if (input.IsNewPress((Keys)ClientKey.Exit))
                {
                    gamemanager.Pbag.Client.Disconnect("exit");
                    gamemanager.SwitchState(GameStates.MainMenuState);
                }

                if (input.IsNewPress((Keys)ClientKey.FullScreen))
                {
                    gamemanager.graphics.ToggleFullScreen();
                }
            }

            //Update chunks to load close ones, unload far ones
            gamemanager.Pbag.WorldManager.Update(gameTime,input);
            gamemanager.Pbag.Player.Update(gameTime, input);
            gamemanager.Pbag.Debugger.Update(gameTime, input);
        }

        public override void Draw(GameTime gameTime, GraphicsDevice gDevice, SpriteBatch sBatch)
        {
            gamemanager.Pbag.WorldManager.Draw(gameTime, gDevice, sBatch);
            gamemanager.Pbag.Player.Draw(gameTime, gDevice, sBatch);
            gamemanager.Pbag.Debugger.Draw(gameTime, gDevice, sBatch);
        }
    }
}