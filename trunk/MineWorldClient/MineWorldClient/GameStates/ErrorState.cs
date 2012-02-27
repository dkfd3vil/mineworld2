using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace MineWorld
{
    public enum ErrorMsg
    {
        Kicked,
        Banned,
        ServerFull,
        VersionMismatch,
        ServerShutdown,
        ServerRestart,
        Unkown,
    }

    public class ErrorState : BaseState
    {
        GameStateManager gamemanager;
        SpriteFont myFont;
        public string error;
        Vector2 errorlocation;

        public ErrorState(GameStateManager manager, GameStates associatedState)
            : base(manager, associatedState)
        {
            gamemanager = manager;
        }

        public override void LoadContent(ContentManager contentloader)
        {
            myFont = contentloader.Load<SpriteFont>("Fonts/DefaultFont");
            gamemanager.game.IsMouseVisible = false;
        }

        public override void Unload()
        {
        }

        public void SetError(ErrorMsg msg)
        {
            switch (msg)
            {
                case ErrorMsg.Banned:
                    {
                        error = "You have been banned from the server";
                        break;
                    }
                case ErrorMsg.Kicked:
                    {
                        error = "You have been kicked from the server";
                        break;
                    }
                case ErrorMsg.ServerFull:
                    {
                        error = "The server is full";
                        break;
                    }
                case ErrorMsg.VersionMismatch:
                    {
                        error = "Client/Server version mismatch";
                        break;
                    }
                case ErrorMsg.ServerShutdown:
                    {
                        error = "Server has shutdown";
                        break;
                    }
                case ErrorMsg.ServerRestart:
                    {
                        error = "Server is restarting";
                        break;
                    }
                case ErrorMsg.Unkown:
                    {
                        error = "A unknown error has occured";
                        break;
                    }
            }
        }

        public override void Update(GameTime gameTime, InputHelper input)
        {
            if (gamemanager.game.IsActive)
            {
                if (input.AnyKeyPressed(true))
                {
                    gamemanager.SwitchState(GameStates.MainMenuState);
                }
            }
        }

        public override void Draw(GameTime gameTime, GraphicsDevice gDevice, SpriteBatch sBatch)
        {
            errorlocation = new Vector2(gamemanager.graphics.PreferredBackBufferWidth / 2, gamemanager.graphics.PreferredBackBufferHeight / 2);
            gDevice.Clear(Color.Black);
            sBatch.Begin();
            sBatch.DrawString(myFont,error,errorlocation,Color.White);
            sBatch.End();
        }
    }
}