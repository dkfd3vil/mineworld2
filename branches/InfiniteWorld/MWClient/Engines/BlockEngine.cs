using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;

namespace MineWorld.Engines
{
    public class BlockEngine
    {
        public MineWorldGame gameInstance;
        public Vector3 vDimensions;
        public Block[] rBlocks;
        public Block[, ,] rBlockMap;
        public Chunk[, ,] rChunks;
        Texture2D tTerrain;
        Effect effect;

        //Temp variable
        float fTime = 1.0f;

        public BlockEngine(MineWorldGame inst)
        {
            gameInstance = inst;
        }

        public void Initialize(int x, int y, int z, int seed)
        {
            effect = gameInstance.Content.Load<Effect>("Effect");

            tTerrain = gameInstance.Content.Load<Texture2D>("terrain");

            rBlockMap = new Block[x, y, z];
            rChunks = new Chunk[(int)(vDimensions.X / 16), (int)(vDimensions.Y / 16), (int)(vDimensions.Z / 16)];

            for (int xq = 0; x < (int)(vDimensions.X / 16); xq++)
            {
                for (int yq = 0; y < (int)(vDimensions.Y / 16); yq++)
                {
                    for (int zq = 0; z < (int)(vDimensions.Z / 16); zq++)
                    {
                        rChunks[x, y, z] = new Chunk(new Vector3(x * 16, y * 16, z * 16), this); //Create each chunk with its position and pass it the game object
                    }
                }
            }

            CreateBlockTypes();

            //Set unchanging effect parameters (Fog and a constant value used for lighting)
            effect.Parameters["FogEnabled"].SetValue(true);
            effect.Parameters["FogStart"].SetValue(128);
            effect.Parameters["FogEnd"].SetValue(160);
            effect.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(Matrix.Identity)));
        }

        public void Update()
        {
            UpdateChunks();
        }

        public void Draw()
        {
            //Set the void color and the fog color (based off of the time of day)
            gameInstance.GraphicsDevice.Clear(ClearOptions.Target, Color.SkyBlue.ToVector4() * (float)(MathHelper.Clamp((float)Math.Sin(fTime) * 5 + 1f, 0, 1)), 1f, 1);
            effect.Parameters["FogColor"].SetValue(Color.SkyBlue.ToVector4() * (float)(MathHelper.Clamp((float)Math.Sin(fTime) * 5 + 1f, 0, 1)));
            //effect.Parameters["FogColor"].SetValue(Color.Black.ToVector4());

            //Set some draw things
            gameInstance.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            gameInstance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            gameInstance.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            gameInstance.GraphicsDevice.DepthStencilState = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.GreaterEqual,
                ReferenceStencil = 254,
                DepthBufferEnable = true
            };

            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.CullCounterClockwiseFace;
            gameInstance.GraphicsDevice.RasterizerState = rs;

            //CHUNKS
            //Select a rendering technique from the effect file
            effect.CurrentTechnique = effect.Techniques["Technique1"];
            //Set the view and projection matrices, as well as the texture
            effect.Parameters["View"].SetValue(gameInstance.PropertyBag.PlayerCamera.ViewMatrix);
            effect.Parameters["Projection"].SetValue(gameInstance.PropertyBag.PlayerCamera.ProjectionMatrix);
            effect.Parameters["myTexture"].SetValue(tTerrain);

            //Set lighting variables based off of time of day
            effect.Parameters["DiffuseColor"].SetValue(new Vector4(0.5f, 0.5f, 0.5f, 1) + new Vector4(0.5f, 0.5f, 0.5f, 0) * (float)(MathHelper.Clamp((float)Math.Sin(fTime) * 5, -1, 1)));
            effect.Parameters["Direction"].SetValue(new Vector3((float)(1 + ((fTime + MathHelper.PiOver2) % MathHelper.TwoPi) * (-1 / Math.PI)), (float)-(Math.Abs(1 + ((fTime + MathHelper.PiOver2) % MathHelper.TwoPi) * (-1 / Math.PI)) - 1) / 2, 0f));
            effect.Parameters["AmbientColor"].SetValue(new Vector4(0.25f, 0.25f, 0.25f, 1) + new Vector4(0.2f, 0.2f, 0.2f, 0) * (float)(MathHelper.Clamp((float)Math.Sin(fTime) * 5, -1, 1)));

            foreach (EffectPass pass in effect.CurrentTechnique.Passes) //For each pass in the current technique
            {
                foreach (Chunk curchunk in rChunks) //And for each chunk
                {
                    if (curchunk.bDraw && !curchunk.bEmpty) //If the chunk is loaded and it isn't empty
                    {
                        effect.Parameters["World"].SetValue(Matrix.CreateTranslation(curchunk.vPosition)); //Transform it to a world position
                        pass.Apply();
                        gameInstance.GraphicsDevice.SetVertexBuffer(curchunk.buffer); //Load its data from its buffer
                        gameInstance.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, curchunk.buffer.VertexCount / 3); //Draw it
                    }
                }
            }
        }

        public void SetBlock(Vector3 pos, BlockType type)
        {
            if (rBlockMap[(int)pos.X, (int)pos.Y, (int)pos.Z] != rBlocks[(int)type]) //If the target block isn't already what you want it to be
            {
                rBlockMap[(int)pos.X, (int)pos.Y, (int)pos.Z] = rBlocks[(int)type]; //Then set the block to be that type
                rChunks[(int)Math.Floor((pos.X) / 16), (int)Math.Floor((pos.Y) / 16), (int)Math.Floor((pos.Z) / 16)].CreateVertices(); //Update current chunk
                try //Update surrounding chunks if needed
                {
                    if (Math.Round(pos.X + 1) % 16 == 0)
                    {
                        rChunks[(int)Math.Floor((pos.X) / 16) + 1, (int)Math.Floor((pos.Y) / 16), (int)Math.Floor((pos.Z) / 16)].CreateVertices();
                    }
                    else if (Math.Round(pos.X + 1) % 16 == 1)
                    {
                        rChunks[(int)Math.Floor((pos.X) / 16) - 1, (int)Math.Floor((pos.Y) / 16), (int)Math.Floor((pos.Z) / 16)].CreateVertices();
                    }
                    if (Math.Round(pos.Z + 1) % 16 == 0)
                    {
                        rChunks[(int)Math.Floor((pos.X) / 16), (int)Math.Floor((pos.Y) / 16), (int)Math.Floor((pos.Z) / 16) + 1].CreateVertices();
                    }
                    else if (Math.Round(pos.Z + 1) % 16 == 1)
                    {
                        rChunks[(int)Math.Floor((pos.X) / 16), (int)Math.Floor((pos.Y) / 16), (int)Math.Floor((pos.Z) / 16) - 1].CreateVertices();
                    }
                    if (Math.Round(pos.Y + 1) % 16 == 0)
                    {
                        rChunks[(int)Math.Floor((pos.X) / 16), (int)Math.Floor((pos.Y) / 16) + 1, (int)Math.Floor((pos.Z) / 16)].CreateVertices();
                    }
                    else if (Math.Round(pos.Y + 1) % 16 == 1)
                    {
                        rChunks[(int)Math.Floor((pos.X) / 16), (int)Math.Floor((pos.Y) / 16) - 1, (int)Math.Floor((pos.Z) / 16)].CreateVertices();
                    }
                }
                catch { }
            }
        }

        private void CreateBlockTypes()
        {
            //0 - Air
            rBlocks[(int)BlockType.Air] = new Block(new Vector2(), new Vector2(), new Vector2(), new Vector2(), new Vector2(), new Vector2(), false, true, false);
            //1 - Dirt
            rBlocks[(int)BlockType.Dirt] = new Block(new Vector2(2, 0), true, false, true);
        }

        private void UpdateChunks()
        {
            //This method decides which chunks to load or not, based off of distance from player
            Chunk curchunk;
            for (int x = 0; x < (int)(vDimensions.X / 16); x++)
            {
                for (int y = 0; y < (int)(vDimensions.Y / 16); y++)
                {
                    for (int z = 0; z < (int)(vDimensions.Z / 16); z++)
                    {
                        curchunk = rChunks[x, y, z];
                        if (Vector3.Distance(new Vector3(x * 16, y * 16, z * 16), gameInstance.PropertyBag.PlayerPosition) < 192) //If the current chunk selected is within the distance
                        {
                            if (!curchunk.bDraw) //And if it isn't already loaded
                            {
                                curchunk.CreateVertices(); //Load it
                                curchunk.bDraw = true;
                            }
                        }
                        else
                        {
                            if (curchunk.bDraw) //Otherwise if it is out of the distance and it IS loaded
                            {
                                curchunk.lVertices = null; //Unload it
                                curchunk.buffer = null;
                                curchunk.bDraw = false;
                            }
                        }
                    }
                }
            }
        }
    }
}