using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace MineWorld
{
    public class Particle
    {
        public Vector3 Position;
        public float Size;
        public Color Color;
        public int StepsToLife=180;
        public int StepsToLifeStatic = 180;
        public bool FlaggedForDeletion = false;
        public float speed = 1;
        public bool fadeOut=true;
    }

    public class ParticleEngine
    {
        MineWorldGame gameInstance;
        PropertyBag _P;
        List<Particle> particleList;
        Effect particleEffect;
        Random randGen;
        VertexDeclaration vertexDeclaration;
        VertexBuffer vertexBuffer;

        public ParticleEngine(MineWorldGame gameInstance)
        {
            this.gameInstance = gameInstance;
            particleEffect = gameInstance.Content.Load<Effect>("effect_particle");
            randGen = new Random();
            particleList = new List<Particle>();

            vertexDeclaration = new VertexDeclaration(gameInstance.GraphicsDevice, VertexPositionTextureShade.VertexElements);
            VertexPositionTextureShade[] vertices = GenerateVertices();
            vertexBuffer = new VertexBuffer(gameInstance.GraphicsDevice, vertices.Length * VertexPositionTextureShade.SizeInBytes, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
        }

        private VertexPositionTextureShade[] GenerateVertices()
        {
            VertexPositionTextureShade[] cubeVerts = new VertexPositionTextureShade[6];

            // BOTTOM
            cubeVerts[0] = new VertexPositionTextureShade(new Vector3(-1, -1, -1), new Vector2(0, 0), 0.3);
            cubeVerts[1] = new VertexPositionTextureShade(new Vector3(1, -1, -1), new Vector2(0, 0), 0.3);
            cubeVerts[2] = new VertexPositionTextureShade(new Vector3(-1, 1, -1), new Vector2(0, 0), 0.3);
            cubeVerts[3] = new VertexPositionTextureShade(new Vector3(1, -1, -1), new Vector2(0, 0), 0.3);
            cubeVerts[4] = new VertexPositionTextureShade(new Vector3(1, 1, -1), new Vector2(0, 0), 0.3);
            cubeVerts[5] = new VertexPositionTextureShade(new Vector3(-1, 1, -1), new Vector2(0, 0), 0.3);
            return cubeVerts;
        }

        private static bool ParticleExpired(Particle particle)
        {
            return particle.FlaggedForDeletion;
        }

        public void Update(GameTime gameTime)
        {
            if (_P == null)
                return;

            foreach (Particle p in particleList)
            {
                p.Position.Y += (float)gameTime.ElapsedGameTime.TotalSeconds*p.speed;
                if (p.fadeOut)
                {
                    
                    p.Color.A = (byte)((p.StepsToLife / (p.StepsToLifeStatic + 0.001f))*255);
                }
                if (_P.blockEngine.SolidAtPoint(p.Position))
                    p.FlaggedForDeletion = true;
                if (p.StepsToLife < 1)
                {
                    p.FlaggedForDeletion = true;
                }
            }
            for (int i = 0; i < particleList.Count; i++)
            {
                particleList[i].StepsToLife--;
            }
            particleList.RemoveAll(ParticleExpired);
        }

        public void CreateExplosionDebris(Vector3 explosionPosition)
        {
            Particle p = new Particle();
            p.Color = new Color(255, 255, 255 - randGen.Next(0, 55));
            p.Size = 0.4f + (float)((float)randGen.Next(0, 100)/(float)200);
            p.speed = 4.0f+((float)randGen.Next(0, 100) / 200.0f);
            p.StepsToLife = 80;
            p.StepsToLifeStatic = 80;
            p.Position = explosionPosition;
            particleList.Add(p);
        }

        /*public void CreateBloodSplatter(Vector3 playerPosition, Color color)
        {
            for (int i = 0; i < 30; i++)
            {
                Particle p = new Particle();
                p.Color = color;
                p.Size = (float)(randGen.NextDouble()*0.2 + 0.05);
                p.Position = playerPosition;
                p.Position.Y -= (float)randGen.NextDouble();
                p.Velocity = new Vector3((float)randGen.NextDouble() * 5 - 2.5f, (float)randGen.NextDouble() * 4f, (float)randGen.NextDouble() * 5 - 2.5f);
                particleList.Add(p);
            }
        }*/

        public VertexPositionColor[] GenerateVertices(Vector3 cameraPosition, Vector3 drawPosition, Vector3 drawHeading, float drawScale,Color particleColor)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[6];

            Vector2 vTexStart = new Vector2(0, 0);

            Vector3 vToPlayer = cameraPosition - drawPosition;

            float dotProduct = Vector3.Dot(vToPlayer, drawHeading);
            Vector3 crossProduct = Vector3.Cross(vToPlayer, drawHeading);
            
            VertexPositionColor v1 = new VertexPositionColor(new Vector3(-0.375f * drawScale, 1 * drawScale, 0), particleColor);
            VertexPositionColor v2 = new VertexPositionColor(new Vector3(0.375f * drawScale, 1 * drawScale, 0), particleColor);
            VertexPositionColor v3 = new VertexPositionColor(new Vector3(-0.375f * drawScale, 0, 0), particleColor);
            VertexPositionColor v4 = new VertexPositionColor(new Vector3(0.375f * drawScale, 0, 0), particleColor);

            vertices[0] = v3;
            vertices[1] = v2;
            vertices[2] = v4;
            vertices[3] = v3;
            vertices[4] = v1;
            vertices[5] = v2;
            return vertices;
        }

        public void Render(GraphicsDevice graphicsDevice)
        {
            // If we don't have _P, grab it from the current gameInstance.
            // We can't do this in the constructor because we are created in the property bag's constructor!
            if (_P == null)
                _P = gameInstance.propertyBag;

            foreach (Particle p in particleList)
            {
                VertexPositionColor[] vertices = GenerateVertices(_P.playerCamera.Position, p.Position, Vector3.One, p.Size,p.Color);
                Matrix worldMatrix = Matrix.CreateScale(p.Size / 2) * Matrix.CreateTranslation(p.Position);


                Matrix world = Matrix.CreateBillboard(p.Position, _P.playerCamera.Position, Vector3.UnitY,null);
                particleEffect.Parameters["xWorld"].SetValue(world);
                particleEffect.Parameters["xView"].SetValue(_P.playerCamera.ViewMatrix);
                particleEffect.Parameters["xProjection"].SetValue(_P.playerCamera.ProjectionMatrix);
                particleEffect.Parameters["xColor"].SetValue(p.Color.ToVector4());
                particleEffect.Begin();
                particleEffect.Techniques[0].Passes[0].Begin();

                graphicsDevice.RenderState.CullMode = CullMode.None;
                graphicsDevice.RenderState.AlphaBlendEnable = true;
                graphicsDevice.VertexDeclaration = vertexDeclaration;
                graphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionTextureShade.SizeInBytes);
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
                graphicsDevice.RenderState.AlphaBlendEnable = false;
                particleEffect.Techniques[0].Passes[0].End();
                particleEffect.End();  
            }
        }
    }
}
