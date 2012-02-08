using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MineWorld
{
    public class Sun
    {
        Texture2D SunTex;
        VertexPositionTexture[] SunArray;

        Effect effect;

        float fTime;
        Vector3 playerpos;

        public Sun()
        {
        }

        public void Load(ContentManager conmanager)
        {
            SunTex = conmanager.Load<Texture2D>("Textures/sun");
            effect = conmanager.Load<Effect>("Effects/DefaultEffect");

            SunArray = new VertexPositionTexture[6];
            SunArray[0] = new VertexPositionTexture(new Vector3(-0.5f, 0, -0.5f), new Vector2(0, 0));
            SunArray[1] = new VertexPositionTexture(new Vector3(0.5f, 0, -0.5f), new Vector2(1, 0));
            SunArray[2] = new VertexPositionTexture(new Vector3(-0.5f, 0, 0.5f), new Vector2(0, 1));
            SunArray[3] = new VertexPositionTexture(new Vector3(0.5f, 0, -0.5f), new Vector2(1, 0));
            SunArray[4] = new VertexPositionTexture(new Vector3(0.5f, 0, 0.5f), new Vector2(1, 1));
            SunArray[5] = new VertexPositionTexture(new Vector3(-0.5f, 0, 0.5f), new Vector2(0, 1));
        }

        public void Update(float time,Vector3 pos)
        {
            fTime = time;
            playerpos = pos;
        }

        public void Draw(GraphicsDevice gDevice)
        {
            effect.CurrentTechnique = effect.Techniques["Technique2"]; //Switch to technique 2 (gui and skybox)

            //SUN
            //Set the sun texture and world matrix (which transforms its position and angle based off of the fTime of day
            effect.Parameters["myTexture"].SetValue(SunTex);
            effect.Parameters["World"].SetValue(Matrix.CreateScale(50) * Matrix.CreateFromYawPitchRoll(0, 0, fTime + MathHelper.PiOver2) * Matrix.CreateTranslation(playerpos + new Vector3((float)(Math.Cos(fTime) * 192), (float)(Math.Sin(fTime) * 192), 0)));
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, SunArray, 0, 2); //Draw it
            }
        }
    }
}
