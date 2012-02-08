using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MineWorld
{
    public class Moon
    {
        Texture2D MoonTex;
        VertexPositionTexture[] MoonArray;

        Effect effect;

        float fTime;

        Vector3 playerpos;

        public Moon()
        {
        }

        public void Load(ContentManager conmanager)
        {
            MoonTex = conmanager.Load<Texture2D>("Textures/moon");
            effect = conmanager.Load<Effect>("Effects/DefaultEffect");

            MoonArray = new VertexPositionTexture[6];
            MoonArray[0] = new VertexPositionTexture(new Vector3(-0.5f, 0, -0.5f), new Vector2(0, 0));
            MoonArray[1] = new VertexPositionTexture(new Vector3(0.5f, 0, -0.5f), new Vector2(1, 0));
            MoonArray[2] = new VertexPositionTexture(new Vector3(-0.5f, 0, 0.5f), new Vector2(0, 1));
            MoonArray[3] = new VertexPositionTexture(new Vector3(0.5f, 0, -0.5f), new Vector2(1, 0));
            MoonArray[4] = new VertexPositionTexture(new Vector3(0.5f, 0, 0.5f), new Vector2(1, 1));
            MoonArray[5] = new VertexPositionTexture(new Vector3(-0.5f, 0, 0.5f), new Vector2(0, 1));
        }

        public void Update(float time,Vector3 pos)
        {
            fTime = time;
            playerpos = pos;
        }

        public void Draw(GraphicsDevice gDevice)
        {
            //MOON
            //Set the texture, set the world matrix to be the same thing as the sun, but negated
            effect.Parameters["myTexture"].SetValue(MoonTex);
            effect.Parameters["World"].SetValue(Matrix.CreateScale(50) * Matrix.CreateFromYawPitchRoll(0, 0, (float)((fTime + MathHelper.PiOver2) + Math.PI)) * Matrix.CreateTranslation(playerpos - new Vector3((float)(Math.Cos(fTime) * 192), (float)(Math.Sin(fTime) * 192), 0)));
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, MoonArray, 0, 2); //Draw it
            }
        }
    }
}
