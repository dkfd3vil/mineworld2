using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace MineWorld
{
    // I wrote myself a little debugger to find out some issues that iam having
    public class Debug
    {
        public bool Enabled;
        SpriteFont myFont;
        private PropertyBag game;

        public Debug(PropertyBag gameIn)
        {
            game = gameIn;
        }

        public void Load(ContentManager conmanager)
        {
            myFont = conmanager.Load<SpriteFont>("Fonts/DefaultFont");
        }

        public void Update(GameTime gameTime,InputHelper input)
        {
            if (input.IsNewPress((Keys)ClientKey.Debug))
            {
                Enabled = !Enabled;
            }
        }

        public void Draw()
        {
            if (Enabled)
            {
                game.GameManager.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
                game.GameManager.spriteBatch.DrawString(myFont, game.Player.Position.ToString(), new Vector2(0, 0), Color.Black);
                game.GameManager.spriteBatch.DrawString(myFont, game.Player.selectedblocktype.ToString(), new Vector2(0, 15), Color.Black);
                game.GameManager.spriteBatch.DrawString(myFont, game.Client.ServerConnection.AverageRoundtripTime.ToString(), new Vector2(0, 30), Color.Black);
                game.GameManager.spriteBatch.End();
            }
        }
    }
}
