using System;
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

        //Misc other vars
        float fTime = 0.201358f;

        public MainGameState(GameStateManager manager, GameStates associatedState)
            : base(manager, associatedState)
        {
            gamemanager = manager;
        }

        public override void LoadContent(ContentManager contentloader)
        {
            //Load our world
            gamemanager.Pbag.WorldManager.Load();
            //Load our Player
            gamemanager.Pbag.Player.Load();
        }

        public override void Update(GameTime gameTime,InputHelper input)
        {
            //Lets see if we need to end this game
            if (input.IsNewPress(Keys.Back) || input.IsNewPress(Keys.Escape))
            {
                gamemanager.ExitState();
            }

            if (input.IsNewPress(Keys.F2))
            {
                gamemanager.graphics.IsFullScreen = !gamemanager.graphics.IsFullScreen;
            }

            //Time increases at rate of 1 ingame day/night cycle per 20 minutes (actual value is 0 at dawn, 0.5pi at noon, pi at dusk, 1.5pi at midnight, and 0 or 2pi at dawn again)
            fTime += (float)(Math.PI / 36000);
            fTime %= (float)(MathHelper.TwoPi);

            //Update chunks to load close ones, unload far ones
            gamemanager.Pbag.WorldManager.Update(fTime);
            gamemanager.Pbag.Player.Update(gameTime, input);
        }

        public override void Draw(GameTime gameTime)
        {
            gamemanager.Pbag.WorldManager.Draw();
            gamemanager.Pbag.Player.Draw();
        }
    }
}