using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MineWorldData;

namespace MineWorld
{
    public class Chunk
    {
        //All of these should be public
        private WorldManager world;
        private GameStateManager gameman;
        public List<VertexFormat> lVertices;
        public VertexBuffer buffer;
        public VertexDeclaration verdec;
        public BaseBlock[, ,] BlockMap;
        public Effect effect;
        public bool bEmpty;
        public bool bDraw;
        //Containing our blocks from terrain.png
        public Texture2D Terrain;
        float fTime;

        //These nummers are lowered by 1 cause a array starts at zero
        public static int Size = 16;
        public static int Height = 128;

        //We dont need a posy cause our height will always be the total height
        public int PosX;
        public int PosZ;

        //Constructor
        public Chunk(int x,int z, WorldManager gamein,GameStateManager man)
        {
            //Needed cause of the buffer copy
            gameman = man;
            world = gamein;

            PosX = x;
            PosZ = z;

            BlockMap = new BaseBlock[Size,Height,Size];

            //Load our effect
            effect = gameman.conmanager.Load<Effect>("Effects/DefaultEffect");

            //Load our terrain
            Terrain = gameman.conmanager.Load<Texture2D>("Textures/terrain");

            //Set unchanging effect parameters (Fog and a constant value used for lighting)
            effect.Parameters["FogEnabled"].SetValue(true);
            effect.Parameters["FogStart"].SetValue(256);
            effect.Parameters["FogEnd"].SetValue(512);
            effect.Parameters["FogColor"].SetValue(Color.SkyBlue.ToVector4());
            effect.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(Matrix.Identity)));
        }

        public void Update(Vector3 playerpos,float time)
        {
            fTime = time;
            if (Vector3.Distance(new Vector3(PosX, Height / 2, PosZ), playerpos) < 256)//If the current chunk selected is within the distance
            {
                if (!bDraw) //And if it isn't already loaded
                {
                    CreateVertices(); //Load it
                    bDraw = true;
                }
            }
            else
            {
                if (bDraw) //Otherwise if it is out of the distance and it IS loaded
                {
                    lVertices = null; //Unload it
                    buffer = null;
                    bDraw = false;
                }
            }
        }

        public void Draw(GraphicsDevice gDevice)
        {
            //CHUNKS
            //Select a rendering technique from the effect file
            effect.CurrentTechnique = effect.Techniques["Technique1"];
            //Set the view and projection matrices, as well as the texture
            effect.Parameters["View"].SetValue(world.player.Cam.View);
            effect.Parameters["Projection"].SetValue(world.player.Cam.Projection);
            effect.Parameters["myTexture"].SetValue(Terrain);

            //Set lighting variables based off of fTime of day
            effect.Parameters["DiffuseColor"].SetValue(new Vector4(0.5f, 0.5f, 0.5f, 1) + new Vector4(0.5f, 0.5f, 0.5f, 0) * (float)(MathHelper.Clamp((float)Math.Sin(fTime) * 5, -1, 1)));
            effect.Parameters["Direction"].SetValue(new Vector3((float)(1 + ((fTime + MathHelper.PiOver2) % MathHelper.TwoPi) * (-1 / Math.PI)), (float)-(Math.Abs(1 + ((fTime + MathHelper.PiOver2) % MathHelper.TwoPi) * (-1 / Math.PI)) - 1) / 2, 0f));
            effect.Parameters["AmbientColor"].SetValue(new Vector4(0.25f, 0.25f, 0.25f, 1) + new Vector4(0.2f, 0.2f, 0.2f, 0) * (float)(MathHelper.Clamp((float)Math.Sin(fTime) * 5, -1, 1)));

            foreach (EffectPass pass in effect.CurrentTechnique.Passes) //For each pass in the current technique
            {
                    if (bDraw && !bEmpty) //If the chunk is loaded and it isn't empty
                    {
                        effect.Parameters["World"].SetValue(Matrix.CreateTranslation(PosX,0,PosZ)); //Transform it to a world position
                        //effect.Parameters["World"].SetValue(Matrix.Identity * Matrix.CreateTranslation(PosX, 0, PosZ));
                        pass.Apply();
                        gDevice.SetVertexBuffer(buffer); //Load its data from its buffer
                        gDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, buffer.VertexCount / 3); //Draw it
                    }
            }
        }

        public void CreateVertices()
        {
            //Re-initialize the VertexFormat list
            lVertices = new List<VertexFormat>();

            for (int x = 0; x < Chunk.Size; x++)
            {
                for (int z = 0; z < Chunk.Size; z++)
                {
                    for (int y = 0; y < Height; y++) //For each block in the chunk
                    {
                        if (BlockMap[x, y, z].Transparent) //If it's transparent
                        {
                            if (
                                (
                                BlockMap[IntClamp(x + 1, 0, Size - 1), y, z].Type == BlockTypes.Air ||
                                BlockMap[IntClamp(x - 1, 0, Size - 1), y, z].Type == BlockTypes.Air ||
                                BlockMap[x, IntClamp(y + 1, 0, Height - 1), z].Type == BlockTypes.Air ||
                                BlockMap[x, IntClamp(y - 1, 0, Height - 1), z].Type == BlockTypes.Air ||
                                BlockMap[x, y, IntClamp(z + 1, 0, Size - 1)].Type == BlockTypes.Air ||
                                BlockMap[x, y, IntClamp(z - 1, 0, Size - 1)].Type == BlockTypes.Air //And if it's not completely surrounded
                                )
                                &&
                                BlockMap[x, y, z].Type != BlockTypes.Air //And if it's not air
                            )
                            {
                                CreateCubeVertices(x, y, z, BlockMap[x, y, z].Model); //Create vertices
                            }
                        }
                        else //If it's opaque
                        {
                            if (
                                BlockMap[IntClamp(x + 1, 0, Size - 1), y, z].Transparent ||
                                BlockMap[IntClamp(x - 1, 0, Size - 1), y, z].Transparent ||
                                BlockMap[x, IntClamp(y + 1, 0, Height - 1), z].Transparent ||
                                BlockMap[x, IntClamp(y - 1, 0, Height - 1), z].Transparent ||
                                BlockMap[x, y, IntClamp(z + 1, 0, Size - 1)].Transparent ||
                                BlockMap[x, y, IntClamp(z - 1, 0, Size - 1)].Transparent //And if it's not completely surrounded
                                )
                            {
                                CreateCubeVertices(x, y, z, BlockMap[x, y, z].Model); //Create vertices
                            }
                        }
                    }
                }
            }
            CopyToBuffers();
        }

        private void CreateCubeVertices(int x, int y, int z, BlockModel model)
        {
            bool Left;
            bool Right;
            bool Up;
            bool Down;
            bool Forward;
            bool Backward;
            BaseBlock currentblock = BlockMap[x, y, z]; //Get current block

            if (currentblock.Transparent) //Set boolean values if each face is visible or not - if it's transparent then it doesn't draw faces adjacent to blocks of the same type
            {
                Up = (BlockMap[x, IntClamp(y + 1, 0, Height - 1), z] != currentblock && (y + 1) == IntClamp(y + 1, 0, Height - 1) ? true : false);
                Forward = (BlockMap[x, y, IntClamp(z + 1, 0, Size - 1)] != currentblock && (z + 1) == IntClamp(z + 1, 0, Size - 1) ? true : false);
                Left = (BlockMap[IntClamp(x - 1, 0, Size - 1), y, z] != currentblock && (x - 1) == IntClamp(x - 1, 0, Size - 1) ? true : false);
                Right = (BlockMap[IntClamp(x + 1, 0, Size - 1), y, z] != currentblock && (x + 1) == IntClamp(x + 1, 0, Size - 1) ? true : false);
                Backward = (BlockMap[x, y, IntClamp(z - 1, 0, Size - 1)] != currentblock && (z - 1) == IntClamp(z - 1, 0, Size - 1) ? true : false);
                Down = (BlockMap[x, IntClamp(y - 1, 0, Height - 1), z] != currentblock && (y - 1) == IntClamp(y - 1, 0, Height - 1) ? true : false);
            }
            else //Set boolean values if each face is visible or not
            {
                Up = (BlockMap[x, IntClamp(y + 1, 0, Height - 1), z].Transparent && (y + 1) == IntClamp(y + 1, 0, Height - 1) ? true : false);
                Forward = (BlockMap[x, y, IntClamp(z + 1, 0, Size - 1)].Transparent && (z + 1) == IntClamp(z + 1, 0, Size - 1) ? true : false);
                Left = (BlockMap[IntClamp(x - 1, 0, Size - 1), y, z].Transparent && (x - 1) == IntClamp(x - 1, 0, Size - 1) ? true : false);
                Right = (BlockMap[IntClamp(x + 1, 0, Size - 1), y, z].Transparent && (x + 1) == IntClamp(x + 1, 0, Size - 1) ? true : false);
                Backward = (BlockMap[x, y, IntClamp(z - 1, 0, Size - 1)].Transparent && (z - 1) == IntClamp(z - 1, 0, Size - 1) ? true : false);
                Down = (BlockMap[x, IntClamp(y - 1, 0, Height - 1), z].Transparent && (y - 1) == IntClamp(y - 1, 0, Height - 1) ? true : false);
            }

            Up = true;
            Forward = true;
            Left = true;
            Right = true;
            Backward = true;
            Down = true;

            if (model == BlockModel.Cube)
            {
                if (Up) //Fill the vertex list with VertexFormats, which take 3 floats as a position, a vector3 as a normal, and a vector2 as UV mapping for textures
                {
                    lVertices.Add(new VertexFormat((float)x, (float)(y + 1), (float)z, new Vector3(0, 1, 0), currentblock.UVMapTop));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 1), (float)z, new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)x, (float)(y + 1), (float)(z + 1), new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 1), (float)z, new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 1), (float)(z + 1), new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((float)x, (float)(y + 1), (float)(z + 1), new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));
                }
                if (Forward)
                {
                    lVertices.Add(new VertexFormat((float)x, (float)(y + 1), (float)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 1), (float)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)x, (float)y, (float)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 1), (float)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((float)x, (float)y, (float)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(0f, 1f / 16f)));
                }
                if (Left)
                {
                    lVertices.Add(new VertexFormat((float)x, (float)(y + 1), (float)z, new Vector3(-1, 0, 0), currentblock.UVMapLeft));
                    lVertices.Add(new VertexFormat((float)x, (float)(y + 1), (float)(z + 1), new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)x, (float)y, (float)z, new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((float)x, (float)(y + 1), (float)(z + 1), new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)x, (float)y, (float)(z + 1), new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((float)x, (float)y, (float)z, new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(0f, 1f / 16f)));
                }
                if (Right)
                {
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 1), (float)(z + 1), new Vector3(1, 0, 0), currentblock.UVMapRight));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 1), (float)z, new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)(z + 1), new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 1), (float)z, new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)z, new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)(z + 1), new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(0f, 1f / 16f)));
                }
                if (Backward)
                {
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 1), (float)z, new Vector3(0, 0, -1), currentblock.UVMapBackward));
                    lVertices.Add(new VertexFormat((float)x, (float)(y + 1), (float)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((float)x, (float)(y + 1), (float)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)x, (float)y, (float)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(0f, 1f / 16f)));
                }
                if (Down)
                {
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)z, new Vector3(0, -1, 0), currentblock.UVMapBottom));
                    lVertices.Add(new VertexFormat((float)x, (float)y, (float)z, new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)(z + 1), new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((float)x, (float)y, (float)z, new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)x, (float)y, (float)(z + 1), new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)(z + 1), new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(0f, 1f / 16f)));
                }
            }
            else if (model == BlockModel.Cross)
            {
                lVertices.Add(new VertexFormat((float)x, (float)(y + 1), (float)z, Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop));
                lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 1), (float)(z + 1), Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                lVertices.Add(new VertexFormat((float)x, (float)y, (float)z, Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));

                lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 1), (float)(z + 1), Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)(z + 1), Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(1f / 16f, 1f / 16f)));
                lVertices.Add(new VertexFormat((float)x, (float)y, (float)z, Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));

                lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 1), (float)(z + 1), Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop));
                lVertices.Add(new VertexFormat((float)x, (float)(y + 1), (float)z, Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)(z + 1), Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));

                lVertices.Add(new VertexFormat((float)x, (float)(y + 1), (float)z, Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                lVertices.Add(new VertexFormat((float)x, (float)y, (float)z, Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(1f / 16f, 1f / 16f)));
                lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)(z + 1), Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));



                lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 1), (float)z, Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop));
                lVertices.Add(new VertexFormat((float)x, (float)(y + 1), (float)(z + 1), Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)z, Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));

                lVertices.Add(new VertexFormat((float)x, (float)(y + 1), (float)(z + 1), Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                lVertices.Add(new VertexFormat((float)x, (float)y, (float)(z + 1), Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(1f / 16f, 1f / 16f)));
                lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)z, Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));

                lVertices.Add(new VertexFormat((float)x, (float)(y + 1), (float)(z + 1), Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop));
                lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 1), (float)z, Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                lVertices.Add(new VertexFormat((float)x, (float)y, (float)(z + 1), Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));

                lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 1), (float)z, Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)z, Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(1f / 16f, 1f / 16f)));
                lVertices.Add(new VertexFormat((float)x, (float)y, (float)(z + 1), Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));
            }
            if (model == BlockModel.Slab)
            {
                if (Up) //Fill the vertex list with VertexFormats, which take 3 floats as a position, a vector3 as a normal, and a vector2 as UV mapping for textures
                {
                    lVertices.Add(new VertexFormat((float)x, (float)(y + 0.2f), (float)z, new Vector3(0, 1, 0), currentblock.UVMapTop));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 0.2f), (float)z, new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)x, (float)(y + 0.2f), (float)(z + 1), new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 0.2f), (float)z, new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 0.2f), (float)(z + 1), new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((float)x, (float)(y + 0.2f), (float)(z + 1), new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));
                }
                if (Forward)
                {
                    lVertices.Add(new VertexFormat((float)x, (float)(y + 0.2f), (float)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 0.2f), (float)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)x, (float)y, (float)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 0.2f), (float)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((float)x, (float)y, (float)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(0f, 1f / 16f)));
                }
                if (Left)
                {
                    lVertices.Add(new VertexFormat((float)x, (float)(y + 0.2f), (float)z, new Vector3(-1, 0, 0), currentblock.UVMapLeft));
                    lVertices.Add(new VertexFormat((float)x, (float)(y + 0.2f), (float)(z + 1), new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)x, (float)y, (float)z, new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((float)x, (float)(y + 0.2f), (float)(z + 1), new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)x, (float)y, (float)(z + 1), new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((float)x, (float)y, (float)z, new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(0f, 1f / 16f)));
                }
                if (Right)
                {
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 0.2f), (float)(z + 1), new Vector3(1, 0, 0), currentblock.UVMapRight));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 0.2f), (float)z, new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)(z + 1), new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 0.2f), (float)z, new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)z, new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)(z + 1), new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(0f, 1f / 16f)));
                }
                if (Backward)
                {
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)(y + 0.2f), (float)z, new Vector3(0, 0, -1), currentblock.UVMapBackward));
                    lVertices.Add(new VertexFormat((float)x, (float)(y + 0.2f), (float)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((float)x, (float)(y + 0.2f), (float)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)x, (float)y, (float)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(0f, 1f / 16f)));
                }
                if (Down)
                {
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)z, new Vector3(0, -1, 0), currentblock.UVMapBottom));
                    lVertices.Add(new VertexFormat((float)x, (float)y, (float)z, new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)(z + 1), new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((float)x, (float)y, (float)z, new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((float)x, (float)y, (float)(z + 1), new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((float)(x + 1), (float)y, (float)(z + 1), new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(0f, 1f / 16f)));
                }
            }
        }

        private void CopyToBuffers()
        {
            bEmpty = true; //Initialize bEmpty to true

            VertexFormat[] tempverticesarray; //Initialize a temporary array

            tempverticesarray = lVertices.ToArray(); //Turn the list into an array, fill the array with it

            if (tempverticesarray.Length > 0) //If the array isn't empty
            {
                buffer = new VertexBuffer(gameman.device, VertexFormat.VertexDeclaration, tempverticesarray.Length, BufferUsage.WriteOnly); //Initialize the buffer
                buffer.SetData(tempverticesarray); //Fill it
                bEmpty = false; //Set bEmpty to false, because it's not empty
            }
        }

        private int IntClamp(int Value, int Min, int Max)
        {
            //Same intclamp function, I just wanted it in this class too
            return (Value < Min ? Min : (Value > Max ? Max : Value));
        }

        public void SetBlock(int x,int y,int z,BaseBlock block,bool updatechunk)
        {
            if (BlockMap[x,y,z] != block) //If the target block isn't already what you want it to be
            {
                BlockMap[x, y, z] = block;//Then set the block to be that type
                if (updatechunk)
                {
                    CreateVertices();
                }
            }
        }

        public BaseBlock GetBlockAtPoint(int x,int y,int z)
        {
            return BlockMap[x, y, z];
        }
    }
}