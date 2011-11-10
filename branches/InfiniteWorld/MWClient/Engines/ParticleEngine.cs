using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MineWorld.Engines
{
    public class Particle
    {
        public Color Color;
        public bool FlaggedForDeletion;
        public Vector3 Position;
        public float Size;
        public Vector3 Velocity;
        public int Stepstolive = 60;
    }

    public class ParticleEngine
    {
        private readonly MineWorldGame _gameInstance;
        private readonly Effect _particleEffect;
        private readonly List<Particle> _particleList;
        private readonly Random _randGen;
        private readonly VertexBuffer _vertexBuffer;
        private readonly VertexDeclaration _vertexDeclaration;
        private PropertyBag _p;

        public ParticleEngine(MineWorldGame gameInstance)
        {
            _gameInstance = gameInstance;
            _particleEffect = gameInstance.Content.Load<Effect>("effect_particle");
            _randGen = new Random();
            _particleList = new List<Particle>();

            _vertexDeclaration = new VertexDeclaration(gameInstance.GraphicsDevice,
                                                      VertexPositionTextureShade.VertexElements);
            VertexPositionTextureShade[] vertices = GenerateVertices();
            _vertexBuffer = new VertexBuffer(gameInstance.GraphicsDevice,
                                            vertices.Length*VertexPositionTextureShade.SizeInBytes,
                                            BufferUsage.WriteOnly);
            _vertexBuffer.SetData(vertices);
        }

        private VertexPositionTextureShade[] GenerateVertices()
        {
            VertexPositionTextureShade[] cubeVerts = new VertexPositionTextureShade[36];

            // BOTTOM
            cubeVerts[0] = new VertexPositionTextureShade(new Vector3(-1, -1, -1), new Vector2(0, 0), 0.3);
            cubeVerts[1] = new VertexPositionTextureShade(new Vector3(1, -1, -1), new Vector2(0, 0), 0.3);
            cubeVerts[2] = new VertexPositionTextureShade(new Vector3(-1, 1, -1), new Vector2(0, 0), 0.3);
            cubeVerts[3] = new VertexPositionTextureShade(new Vector3(1, -1, -1), new Vector2(0, 0), 0.3);
            cubeVerts[4] = new VertexPositionTextureShade(new Vector3(1, 1, -1), new Vector2(0, 0), 0.3);
            cubeVerts[5] = new VertexPositionTextureShade(new Vector3(-1, 1, -1), new Vector2(0, 0), 0.3);

            // TOP
            cubeVerts[30] = new VertexPositionTextureShade(new Vector3(-1, -1, 1), new Vector2(0, 0), 1.0);
            cubeVerts[31] = new VertexPositionTextureShade(new Vector3(1, -1, 1), new Vector2(0, 0), 1.0);
            cubeVerts[32] = new VertexPositionTextureShade(new Vector3(-1, 1, 1), new Vector2(0, 0), 1.0);
            cubeVerts[33] = new VertexPositionTextureShade(new Vector3(1, -1, 1), new Vector2(0, 0), 1.0);
            cubeVerts[34] = new VertexPositionTextureShade(new Vector3(1, 1, 1), new Vector2(0, 0), 1.0);
            cubeVerts[35] = new VertexPositionTextureShade(new Vector3(-1, 1, 1), new Vector2(0, 0), 1.0);

            // LEFT
            cubeVerts[6] = new VertexPositionTextureShade(new Vector3(-1, -1, -1), new Vector2(0, 0), 0.7);
            cubeVerts[7] = new VertexPositionTextureShade(new Vector3(-1, -1, 1), new Vector2(0, 0), 0.7);
            cubeVerts[8] = new VertexPositionTextureShade(new Vector3(-1, 1, 1), new Vector2(0, 0), 0.7);
            cubeVerts[9] = new VertexPositionTextureShade(new Vector3(-1, -1, -1), new Vector2(0, 0), 0.7);
            cubeVerts[10] = new VertexPositionTextureShade(new Vector3(-1, 1, 1), new Vector2(0, 0), 0.7);
            cubeVerts[11] = new VertexPositionTextureShade(new Vector3(-1, 1, -1), new Vector2(0, 0), 0.7);

            // RIGHT
            cubeVerts[12] = new VertexPositionTextureShade(new Vector3(1, -1, -1), new Vector2(0, 0), 0.7);
            cubeVerts[13] = new VertexPositionTextureShade(new Vector3(1, -1, 1), new Vector2(0, 0), 0.7);
            cubeVerts[14] = new VertexPositionTextureShade(new Vector3(1, 1, 1), new Vector2(0, 0), 0.7);
            cubeVerts[15] = new VertexPositionTextureShade(new Vector3(1, -1, -1), new Vector2(0, 0), 0.7);
            cubeVerts[16] = new VertexPositionTextureShade(new Vector3(1, 1, 1), new Vector2(0, 0), 0.7);
            cubeVerts[17] = new VertexPositionTextureShade(new Vector3(1, 1, -1), new Vector2(0, 0), 0.7);

            // FRONT
            cubeVerts[18] = new VertexPositionTextureShade(new Vector3(-1, 1, -1), new Vector2(0, 0), 0.5);
            cubeVerts[19] = new VertexPositionTextureShade(new Vector3(-1, 1, 1), new Vector2(0, 0), 0.5);
            cubeVerts[20] = new VertexPositionTextureShade(new Vector3(1, 1, 1), new Vector2(0, 0), 0.5);
            cubeVerts[21] = new VertexPositionTextureShade(new Vector3(-1, 1, -1), new Vector2(0, 0), 0.5);
            cubeVerts[22] = new VertexPositionTextureShade(new Vector3(1, 1, 1), new Vector2(0, 0), 0.5);
            cubeVerts[23] = new VertexPositionTextureShade(new Vector3(1, 1, -1), new Vector2(0, 0), 0.5);

            // BACK
            cubeVerts[24] = new VertexPositionTextureShade(new Vector3(-1, -1, -1), new Vector2(0, 0), 0.5);
            cubeVerts[25] = new VertexPositionTextureShade(new Vector3(-1, -1, 1), new Vector2(0, 0), 0.5);
            cubeVerts[26] = new VertexPositionTextureShade(new Vector3(1, -1, 1), new Vector2(0, 0), 0.5);
            cubeVerts[27] = new VertexPositionTextureShade(new Vector3(-1, -1, -1), new Vector2(0, 0), 0.5);
            cubeVerts[28] = new VertexPositionTextureShade(new Vector3(1, -1, 1), new Vector2(0, 0), 0.5);
            cubeVerts[29] = new VertexPositionTextureShade(new Vector3(1, -1, -1), new Vector2(0, 0), 0.5);

            return cubeVerts;
        }

        private static bool ParticleExpired(Particle particle)
        {
            return particle.FlaggedForDeletion;
        }

        public void Update(GameTime gameTime)
        {
            if (_p == null)
                return;

            foreach (Particle p in _particleList)
            {
                p.Position += (float) gameTime.ElapsedGameTime.TotalSeconds*p.Velocity;
                p.Velocity.Y -= 8*(float) gameTime.ElapsedGameTime.TotalSeconds;
                if (_p.BlockEngine.SolidAtPoint(p.Position))
                    p.FlaggedForDeletion = true;
                if (p.Stepstolive < 1)
                {
                    p.FlaggedForDeletion = true;
                }
            }
            for (int i = 0; i < _particleList.Count; i++)
            {
                _particleList[i].Stepstolive--;
            }
            _particleList.RemoveAll(ParticleExpired);
        }

        public void CreateExplosionDebris(Vector3 explosionPosition)
        {
            for (int i = 0; i < 50; i++)
            {
                Particle p = new Particle
                                 {
                                     Color = new Color(90, 60, 40),
                                     Size = (float) (_randGen.NextDouble()*0.4 + 0.05),
                                     Position = explosionPosition
                                 };
                p.Position.Y += (float) _randGen.NextDouble() - 0.5f;
                p.Velocity = new Vector3((float) _randGen.NextDouble()*8 - 4, (float) _randGen.NextDouble()*8,
                                         (float) _randGen.NextDouble()*8 - 4);
                _particleList.Add(p);
            }
        }

        public void CreateBloodSplatter(Vector3 playerPosition, Color color)
        {
            for (int i = 0; i < 30; i++)
            {
                Particle p = new Particle
                                 {
                                     Color = color,
                                     Size = (float) (_randGen.NextDouble()*0.2 + 0.05),
                                     Position = playerPosition
                                 };
                p.Position.Y -= (float) _randGen.NextDouble();
                p.Velocity = new Vector3((float) _randGen.NextDouble()*5 - 2.5f, (float) _randGen.NextDouble()*4f,
                                         (float) _randGen.NextDouble()*5 - 2.5f);
                _particleList.Add(p);
            }
        }

        public void Render(GraphicsDevice graphicsDevice)
        {
            // If we don't have _P, grab it from the current gameInstance.
            // We can't do this in the constructor because we are created in the property bag's constructor!
            if (_p == null)
                _p = _gameInstance.PropertyBag;

            foreach (Particle p in _particleList)
            {
                Matrix worldMatrix = Matrix.CreateScale(p.Size/2)*Matrix.CreateTranslation(p.Position);
                _particleEffect.Parameters["xWorld"].SetValue(worldMatrix);
                _particleEffect.Parameters["xView"].SetValue(_p.PlayerCamera.ViewMatrix);
                _particleEffect.Parameters["xProjection"].SetValue(_p.PlayerCamera.ProjectionMatrix);
                _particleEffect.Parameters["xColor"].SetValue(p.Color.ToVector4());
                _particleEffect.Begin();
                _particleEffect.Techniques[0].Passes[0].Begin();

                graphicsDevice.RenderState.CullMode = CullMode.None;
                graphicsDevice.VertexDeclaration = _vertexDeclaration;
                graphicsDevice.Vertices[0].SetSource(_vertexBuffer, 0, VertexPositionTextureShade.SizeInBytes);
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0,
                                              _vertexBuffer.SizeInBytes/VertexPositionTextureShade.SizeInBytes/3);

                _particleEffect.Techniques[0].Passes[0].End();
                _particleEffect.End();
            }
        }
    }
}