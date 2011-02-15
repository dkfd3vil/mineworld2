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
        public int StepsToLife=60;
        public bool FlaggedForDeletion = false;
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
                p.Position.Y += (float)gameTime.ElapsedGameTime.TotalSeconds;
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
            p.Color = new Color(128,128,128);
            p.Size = 0.4f;
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

        public void Render(GraphicsDevice graphicsDevice)
        {
            // If we don't have _P, grab it from the current gameInstance.
            // We can't do this in the constructor because we are created in the property bag's constructor!
            if (_P == null)
                _P = gameInstance.propertyBag;

            foreach (Particle p in particleList)
            {
                Matrix worldMatrix = Matrix.CreateScale(p.Size / 2) * Matrix.CreateTranslation(p.Position);
                particleEffect.Parameters["xWorld"].SetValue(worldMatrix);
                particleEffect.Parameters["xView"].SetValue(_P.playerCamera.ViewMatrix);
                particleEffect.Parameters["xProjection"].SetValue(_P.playerCamera.ProjectionMatrix);
                particleEffect.Parameters["xColor"].SetValue(p.Color.ToVector4());
                particleEffect.Begin();
                particleEffect.Techniques[0].Passes[0].Begin();

                graphicsDevice.RenderState.CullMode = CullMode.None;
                graphicsDevice.VertexDeclaration = vertexDeclaration;
                graphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionTextureShade.SizeInBytes);
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, vertexBuffer.SizeInBytes / VertexPositionTextureShade.SizeInBytes / 3);

                particleEffect.Techniques[0].Passes[0].End();
                particleEffect.End();  
            }
        }
    }
}
