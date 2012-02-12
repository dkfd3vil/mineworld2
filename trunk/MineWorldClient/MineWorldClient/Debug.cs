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
            if (game.Game.IsActive)
            {
                if (input.IsNewPress((Keys)ClientKey.Debug))
                {
                    Enabled = !Enabled;
                }
            }
        }

        public void Draw(GameTime gameTime, GraphicsDevice gDevice, SpriteBatch sBatch)
        {
            if (Enabled)
            {
                sBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
                sBatch.DrawString(myFont, "Position:" + game.Player.Position.ToString(), new Vector2(0, 0), Color.Black);
                sBatch.DrawString(myFont, "Angle:" + game.Player.Cam.Angle.ToString(), new Vector2(0, 15), Color.Black);
                sBatch.DrawString(myFont, "Selectedblock:" + game.Player.vAimBlock.ToString(), new Vector2(0, 30), Color.Black);
                sBatch.DrawString(myFont, "Selectedblocktype:" + game.Player.selectedblocktype.ToString(), new Vector2(0, 45), Color.Black);
                sBatch.DrawString(myFont, "Time:" + game.WorldManager.fTime.ToString(), new Vector2(0, 60), Color.Black);
                sBatch.DrawString(myFont, "Ping:" + game.Client.ServerConnection.AverageRoundtripTime.ToString(), new Vector2(0, 75), Color.Black);
                sBatch.DrawString(myFont, "Bytessend:" + game.Client.ServerConnection.Statistics.SentBytes.ToString(), new Vector2(0, 90), Color.Black);
                sBatch.DrawString(myFont, "Bytesreceived:" + game.Client.ServerConnection.Statistics.ReceivedBytes.ToString(), new Vector2(0, 105), Color.Black);
                sBatch.End();
            }
        }
    }
}
