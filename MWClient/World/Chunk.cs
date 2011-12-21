using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MineWorld.Engines;

namespace MineWorld
{
    public class Chunk
    {
        //All of these should be public
        private BlockEngine game;
        public Vector3 vPosition;
        public List<VertexFormat> lVertices;
        public VertexBuffer buffer;
        public VertexDeclaration verdec;
        public bool bEmpty;
        public bool bDraw;

        //Constructor
        public Chunk(Vector3 pos, BlockEngine gamein)
        {
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
                        if (game.rBlockMap[x, y, z].Transparent) //If it's transparent
                        {
                            if (
                                (
                                game.rBlockMap[IntClamp(x + 1, 0, (int)game.vDimensions.X - 1), y, z] == game.rBlocks[0] ||
                                game.rBlockMap[IntClamp(x - 1, 0, (int)game.vDimensions.X - 1), y, z] == game.rBlocks[0] ||
                                game.rBlockMap[x, IntClamp(y + 1, 0, (int)game.vDimensions.Y - 1), z] == game.rBlocks[0] ||
                                game.rBlockMap[x, IntClamp(y - 1, 0, (int)game.vDimensions.Y - 1), z] == game.rBlocks[0] ||
                                game.rBlockMap[x, y, IntClamp(z + 1, 0, (int)game.vDimensions.Z - 1)] == game.rBlocks[0] ||
                                game.rBlockMap[x, y, IntClamp(z - 1, 0, (int)game.vDimensions.Z - 1)] == game.rBlocks[0] //And if it's not completely surrounded
                                )
                                &&
                                game.rBlockMap[x, y, z] != game.rBlocks[0] //And if it's not air
                            )
                            {
                                CreateCubeVertices(x, y, z, game.rBlockMap[x, y, z].Model); //Create vertices
                            }
                        }
                        else //If it's opaque
                        {
                            if (
                                game.rBlockMap[IntClamp(x + 1, 0, (int)game.vDimensions.X - 1), y, z].Transparent ||
                                game.rBlockMap[IntClamp(x - 1, 0, (int)game.vDimensions.X - 1), y, z].Transparent ||
                                game.rBlockMap[x, IntClamp(y + 1, 0, (int)game.vDimensions.Y - 1), z].Transparent ||
                                game.rBlockMap[x, IntClamp(y - 1, 0, (int)game.vDimensions.Y - 1), z].Transparent ||
                                game.rBlockMap[x, y, IntClamp(z + 1, 0, (int)game.vDimensions.Z - 1)].Transparent ||
                                game.rBlockMap[x, y, IntClamp(z - 1, 0, (int)game.vDimensions.Z - 1)].Transparent //And if it's not completely surrounded
                                )
                            {
                                CreateCubeVertices(x, y, z, game.rBlockMap[x, y, z].Model); //Create vertices
                            }
                        }
                    }
                }
            }
            CopyToBuffers();
        }

        private void CreateCubeVertices(int x, int y, int z, int model)
        {
            bool Left;
            bool Right;
            bool Up;
            bool Down;
            bool Forward;
            bool Backward;
            Block currentblock = game.rBlockMap[x, y, z]; //Get current block

            if (currentblock.Transparent) //Set boolean values if each face is visible or not - if it's transparent then it doesn't draw faces adjacent to blocks of the same type
            {
                Up = (game.rBlockMap[x, IntClamp(y + 1, 0, (int)game.vDimensions.Y - 1), z] != currentblock && (y + 1) == IntClamp(y + 1, 0, (int)game.vDimensions.Y - 1) ? true : false);
                Forward = (game.rBlockMap[x, y, IntClamp(z + 1, 0, (int)game.vDimensions.Z - 1)] != currentblock && (z + 1) == IntClamp(z + 1, 0, (int)game.vDimensions.Z - 1) ? true : false);
                Left = (game.rBlockMap[IntClamp(x - 1, 0, (int)game.vDimensions.X - 1), y, z] != currentblock && (x - 1) == IntClamp(x - 1, 0, (int)game.vDimensions.X - 1) ? true : false);
                Right = (game.rBlockMap[IntClamp(x + 1, 0, (int)game.vDimensions.X - 1), y, z] != currentblock && (x + 1) == IntClamp(x + 1, 0, (int)game.vDimensions.X - 1) ? true : false);
                Backward = (game.rBlockMap[x, y, IntClamp(z - 1, 0, (int)game.vDimensions.Z - 1)] != currentblock && (z - 1) == IntClamp(z - 1, 0, (int)game.vDimensions.Z - 1) ? true : false);
                Down = (game.rBlockMap[x, IntClamp(y - 1, 0, (int)game.vDimensions.Y - 1), z] != currentblock && (y - 1) == IntClamp(y - 1, 0, (int)game.vDimensions.Y - 1) ? true : false);
            }
            else //Set boolean values if each face is visible or not
            {
                Up = (game.rBlockMap[x, IntClamp(y + 1, 0, (int)game.vDimensions.Y - 1), z].Transparent && (y + 1) == IntClamp(y + 1, 0, (int)game.vDimensions.Y - 1) ? true : false);
                Forward = (game.rBlockMap[x, y, IntClamp(z + 1, 0, (int)game.vDimensions.Z - 1)].Transparent && (z + 1) == IntClamp(z + 1, 0, (int)game.vDimensions.Z - 1) ? true : false);
                Left = (game.rBlockMap[IntClamp(x - 1, 0, (int)game.vDimensions.X - 1), y, z].Transparent && (x - 1) == IntClamp(x - 1, 0, (int)game.vDimensions.X - 1) ? true : false);
                Right = (game.rBlockMap[IntClamp(x + 1, 0, (int)game.vDimensions.X - 1), y, z].Transparent && (x + 1) == IntClamp(x + 1, 0, (int)game.vDimensions.X - 1) ? true : false);
                Backward = (game.rBlockMap[x, y, IntClamp(z - 1, 0, (int)game.vDimensions.Z - 1)].Transparent && (z - 1) == IntClamp(z - 1, 0, (int)game.vDimensions.Z - 1) ? true : false);
                Down = (game.rBlockMap[x, IntClamp(y - 1, 0, (int)game.vDimensions.Y - 1), z].Transparent && (y - 1) == IntClamp(y - 1, 0, (int)game.vDimensions.Y - 1) ? true : false);
            }

            //Make the positions local
            x -= (int)vPosition.X;
            y -= (int)vPosition.Y;
            z -= (int)vPosition.Z;

            if (model == 0)
            {
                if (Up) //Fill the vertex list with VertexFormats, which take 3 bytes as a position, a vector3 as a normal, and a vector2 as UV mapping for textures
                {
                    lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)z, new Vector3(0, 1, 0), currentblock.UVMapTop));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)z, new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)(z + 1), new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)z, new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)(z + 1), new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)(z + 1), new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));
                }
                if (Forward)
                {
                    lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(0f, 1f / 16f)));
                }
                if (Left)
                {
                    lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)z, new Vector3(-1, 0, 0), currentblock.UVMapLeft));
                    lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)(z + 1), new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)z, new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)(z + 1), new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)(z + 1), new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)z, new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(0f, 1f / 16f)));
                }
                if (Right)
                {
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)(z + 1), new Vector3(1, 0, 0), currentblock.UVMapRight));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)z, new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)(z + 1), new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)z, new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)z, new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)(z + 1), new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(0f, 1f / 16f)));
                }
                if (Backward)
                {
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)z, new Vector3(0, 0, -1), currentblock.UVMapBackward));
                    lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(0f, 1f / 16f)));
                }
                if (Down)
                {
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)z, new Vector3(0, -1, 0), currentblock.UVMapBottom));
                    lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)z, new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)(z + 1), new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)z, new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)(z + 1), new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)(z + 1), new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(0f, 1f / 16f)));
                }
            }
            else if (model == 1)
            {
                lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)z, Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop));
                lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)(z + 1), Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)z, Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));

                lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)(z + 1), Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)(z + 1), Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(1f / 16f, 1f / 16f)));
                lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)z, Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));

                lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)(z + 1), Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop));
                lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)z, Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)(z + 1), Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));

                lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)z, Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)z, Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(1f / 16f, 1f / 16f)));
                lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)(z + 1), Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));



                lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)z, Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop));
                lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)(z + 1), Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)z, Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));

                lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)(z + 1), Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)(z + 1), Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(1f / 16f, 1f / 16f)));
                lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)z, Vector3.Normalize(new Vector3(1, 0, 1)), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));

                lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)(z + 1), Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop));
                lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)z, Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)(z + 1), Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));

                lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)z, Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)z, Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(1f / 16f, 1f / 16f)));
                lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)(z + 1), Vector3.Normalize(new Vector3(-1, 0, -1)), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));
            }
            if (model == 2)
            {
                if (Up) //Fill the vertex list with VertexFormats, which take 3 bytes as a position, a vector3 as a normal, and a vector2 as UV mapping for textures
                {
                    lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)z, new Vector3(0, 1, 0), currentblock.UVMapTop));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)z, new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)(z + 1), new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)z, new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)(z + 1), new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)(z + 1), new Vector3(0, 1, 0), currentblock.UVMapTop + new Vector2(0f, 1f / 16f)));
                }
                if (Forward)
                {
                    lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(0f, 0.5f / 16f)));

                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(1f / 16f, 0.5f / 16f)));
                    lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)(z + 1), new Vector3(0, 0, 1), currentblock.UVMapForward + new Vector2(0f, 0.5f / 16f)));
                }
                if (Left)
                {
                    lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)z, new Vector3(-1, 0, 0), currentblock.UVMapLeft));
                    lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)(z + 1), new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)z, new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(0f, 0.5f / 16f)));

                    lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)(z + 1), new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)(z + 1), new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(1f / 16f, 0.5f / 16f)));
                    lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)z, new Vector3(-1, 0, 0), currentblock.UVMapLeft + new Vector2(0f, 0.5f / 16f)));
                }
                if (Right)
                {
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)(z + 1), new Vector3(1, 0, 0), currentblock.UVMapRight));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)z, new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)(z + 1), new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(0f, 0.5f / 16f)));

                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)z, new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)z, new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(1f / 16f, 0.5f / 16f)));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)(z + 1), new Vector3(1, 0, 0), currentblock.UVMapRight + new Vector2(0f, 0.5f / 16f)));
                }
                if (Backward)
                {
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)(y + 1), (byte)z, new Vector3(0, 0, -1), currentblock.UVMapBackward));
                    lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(0f, 0.5f / 16f)));

                    lVertices.Add(new VertexFormat((byte)x, (byte)(y + 1), (byte)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(1f / 16f, 0.5f / 16f)));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)z, new Vector3(0, 0, -1), currentblock.UVMapBackward + new Vector2(0f, 0.5f / 16f)));
                }
                if (Down)
                {
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)z, new Vector3(0, -1, 0), currentblock.UVMapBottom));
                    lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)z, new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)(z + 1), new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(0f, 1f / 16f)));

                    lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)z, new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(1f / 16f, 0f)));
                    lVertices.Add(new VertexFormat((byte)x, (byte)y, (byte)(z + 1), new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(1f / 16f, 1f / 16f)));
                    lVertices.Add(new VertexFormat((byte)(x + 1), (byte)y, (byte)(z + 1), new Vector3(0, -1, 0), currentblock.UVMapBottom + new Vector2(0f, 1f / 16f)));
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
                buffer = new VertexBuffer(game.gameInstance.GraphicsDevice, VertexFormat.VertexDeclaration, tempverticesarray.Length, BufferUsage.WriteOnly); //Initialize the buffer
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