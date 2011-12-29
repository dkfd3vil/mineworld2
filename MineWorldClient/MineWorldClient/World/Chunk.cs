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
        private WorldManager game;
        private GraphicsDevice device;
        public Vector3 vPosition;
        public List<VertexFormat> lVertices;
        public VertexBuffer buffer;
        public VertexDeclaration verdec;
        public bool bEmpty;
        public bool bDraw;

        //Constructor
        public Chunk(Vector3 pos, WorldManager gamein,GraphicsDevice device)
        {
            //Needed cause of the buffer copy
            this.device = device;
            vPosition = pos;
            game = gamein;
        }

        public void CreateVertices()
        {
            //Re-initialize the VertexFormat list
            lVertices = new List<VertexFormat>();

            for (int x = (int)vPosition.X; x < vPosition.X + 16; x++)
            {
                for (int z = (int)vPosition.Z; z < vPosition.Z + 16; z++)
                {
                    for (int y = (int)vPosition.Y; y < vPosition.Y + 16; y++) //For each block in the chunk
                    {
                        if (game.BlockMap[x, y, z].Transparent) //If it's transparent
                        {
                            if (
                                (
                                game.BlockMap[IntClamp(x + 1, 0, (int)game.Mapsize.X - 1), y, z] == game.Blocks[(int)BlockTypes.Air] ||
                                game.BlockMap[IntClamp(x - 1, 0, (int)game.Mapsize.X - 1), y, z] == game.Blocks[(int)BlockTypes.Air] ||
                                game.BlockMap[x, IntClamp(y + 1, 0, (int)game.Mapsize.Y - 1), z] == game.Blocks[(int)BlockTypes.Air] ||
                                game.BlockMap[x, IntClamp(y - 1, 0, (int)game.Mapsize.Y - 1), z] == game.Blocks[(int)BlockTypes.Air] ||
                                game.BlockMap[x, y, IntClamp(z + 1, 0, (int)game.Mapsize.Z - 1)] == game.Blocks[(int)BlockTypes.Air] ||
                                game.BlockMap[x, y, IntClamp(z - 1, 0, (int)game.Mapsize.Z - 1)] == game.Blocks[(int)BlockTypes.Air] //And if it's not completely surrounded
                                )
                                &&
                                game.BlockMap[x, y, z] != game.Blocks[(int)BlockTypes.Air] //And if it's not air
                            )
                            {
                                CreateCubeVertices(x, y, z, game.BlockMap[x, y, z].Model); //Create vertices
                            }
                        }
                        else //If it's opaque
                        {
                            if (
                                game.BlockMap[IntClamp(x + 1, 0, (int)game.Mapsize.X - 1), y, z].Transparent ||
                                game.BlockMap[IntClamp(x - 1, 0, (int)game.Mapsize.X - 1), y, z].Transparent ||
                                game.BlockMap[x, IntClamp(y + 1, 0, (int)game.Mapsize.Y - 1), z].Transparent ||
                                game.BlockMap[x, IntClamp(y - 1, 0, (int)game.Mapsize.Y - 1), z].Transparent ||
                                game.BlockMap[x, y, IntClamp(z + 1, 0, (int)game.Mapsize.Z - 1)].Transparent ||
                                game.BlockMap[x, y, IntClamp(z - 1, 0, (int)game.Mapsize.Z - 1)].Transparent //And if it's not completely surrounded
                                )
                            {
                                CreateCubeVertices(x, y, z, game.BlockMap[x, y, z].Model); //Create vertices
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
            BaseBlock currentblock = game.BlockMap[x, y, z]; //Get current block

            if (currentblock.Transparent) //Set boolean values if each face is visible or not - if it's transparent then it doesn't draw faces adjacent to blocks of the same type
            {
                Up = (game.BlockMap[x, IntClamp(y + 1, 0, (int)game.Mapsize.Y - 1), z] != currentblock && (y + 1) == IntClamp(y + 1, 0, (int)game.Mapsize.Y - 1) ? true : false);
                Forward = (game.BlockMap[x, y, IntClamp(z + 1, 0, (int)game.Mapsize.Z - 1)] != currentblock && (z + 1) == IntClamp(z + 1, 0, (int)game.Mapsize.Z - 1) ? true : false);
                Left = (game.BlockMap[IntClamp(x - 1, 0, (int)game.Mapsize.X - 1), y, z] != currentblock && (x - 1) == IntClamp(x - 1, 0, (int)game.Mapsize.X - 1) ? true : false);
                Right = (game.BlockMap[IntClamp(x + 1, 0, (int)game.Mapsize.X - 1), y, z] != currentblock && (x + 1) == IntClamp(x + 1, 0, (int)game.Mapsize.X - 1) ? true : false);
                Backward = (game.BlockMap[x, y, IntClamp(z - 1, 0, (int)game.Mapsize.Z - 1)] != currentblock && (z - 1) == IntClamp(z - 1, 0, (int)game.Mapsize.Z - 1) ? true : false);
                Down = (game.BlockMap[x, IntClamp(y - 1, 0, (int)game.Mapsize.Y - 1), z] != currentblock && (y - 1) == IntClamp(y - 1, 0, (int)game.Mapsize.Y - 1) ? true : false);
            }
            else //Set boolean values if each face is visible or not
            {
                Up = (game.BlockMap[x, IntClamp(y + 1, 0, (int)game.Mapsize.Y - 1), z].Transparent && (y + 1) == IntClamp(y + 1, 0, (int)game.Mapsize.Y - 1) ? true : false);
                Forward = (game.BlockMap[x, y, IntClamp(z + 1, 0, (int)game.Mapsize.Z - 1)].Transparent && (z + 1) == IntClamp(z + 1, 0, (int)game.Mapsize.Z - 1) ? true : false);
                Left = (game.BlockMap[IntClamp(x - 1, 0, (int)game.Mapsize.X - 1), y, z].Transparent && (x - 1) == IntClamp(x - 1, 0, (int)game.Mapsize.X - 1) ? true : false);
                Right = (game.BlockMap[IntClamp(x + 1, 0, (int)game.Mapsize.X - 1), y, z].Transparent && (x + 1) == IntClamp(x + 1, 0, (int)game.Mapsize.X - 1) ? true : false);
                Backward = (game.BlockMap[x, y, IntClamp(z - 1, 0, (int)game.Mapsize.Z - 1)].Transparent && (z - 1) == IntClamp(z - 1, 0, (int)game.Mapsize.Z - 1) ? true : false);
                Down = (game.BlockMap[x, IntClamp(y - 1, 0, (int)game.Mapsize.Y - 1), z].Transparent && (y - 1) == IntClamp(y - 1, 0, (int)game.Mapsize.Y - 1) ? true : false);
            }

            //Make the positions local
            x -= (int)vPosition.X;
            y -= (int)vPosition.Y;
            z -= (int)vPosition.Z;

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
                buffer = new VertexBuffer(device, VertexFormat.VertexDeclaration, tempverticesarray.Length, BufferUsage.WriteOnly); //Initialize the buffer
                buffer.SetData(tempverticesarray); //Fill it
                bEmpty = false; //Set bEmpty to false, because it's not empty
            }
        }

        private int IntClamp(int Value, int Min, int Max)
        {
            //Same intclamp function, I just wanted it in this class too
            return (Value < Min ? Min : (Value > Max ? Max : Value));
        }
    }
}