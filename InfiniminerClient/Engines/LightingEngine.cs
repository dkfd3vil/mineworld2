using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Threading;

namespace MineWorld
{
    //TODO Redo bits of the lighting engine this stuff is way too fast written :P
    public class Lighting
    {
        private struct Light
        {
            public int X;
            public int Y;
            public int Z;
            public byte Intensity;

            public Light(int x, int y, int z, byte intensity)
            {
                X = x;
                Y = y;
                Z = z;
                Intensity = intensity;
            }
        }

        private BlockType[, ,] blockList = null;
        private byte[, ,] _lighting;
        private int[,] _lightHeight;

        private Queue<Light> toLight = new Queue<Light>();
        private Queue<Light> toDark = new Queue<Light>();
        private Dictionary<Vector3i,bool> toSun = new Dictionary<Vector3i,bool>();


        public Lighting()
        {
            _lighting = new byte[Defines.MAPSIZE, Defines.MAPSIZE, Defines.MAPSIZE];
        }

        public void Initialize(BlockType[, ,] temp)
        {
            blockList = temp;
            InitLighting();
        }

        private void InitLighting()
        {
            toLight.Clear();
            toDark.Clear();
            toSun.Clear();
            _lighting = new byte[Defines.MAPSIZE, Defines.MAPSIZE, Defines.MAPSIZE];
            _lightHeight = new int[Defines.MAPSIZE, Defines.MAPSIZE];
            LightRegion(0, Defines.MAPSIZE, 0, Defines.MAPSIZE);
            FillLighting();
        }

        private void LightRegion(int sx, int ex, int sz, int ez)
        {
            if (sx < 0) sx = 0;
            if (sz < 0) sz = 0;
            if (ex > Defines.MAPSIZE) ex = Defines.MAPSIZE;
            if (ez > Defines.MAPSIZE) ex = Defines.MAPSIZE;

            for (int x = sx; x < ex; x++)
            {
                for (int z = sz; z < ez; z++)
                {
                    bool inShadow = false;
                    for (int y = Defines.MAPSIZE - 1; y > 0; y--)
                    {
                        if (y == Defines.MAPSIZE - 1)
                        {
                            _lighting[x, y, z] = Defines.MAXLIGHT;
                            toLight.Enqueue(new Light(x, y, z, Defines.MAXLIGHT));
                        }
                        else
                        {
                            BlockType blockType = BlockAtPoint(x, y, z);
                            if (!BlockInformation.IsLightTransparentBlock(blockType) && !inShadow)
                            {
                                inShadow = true;
                                _lightHeight[x, z] = y + 1;
                            }
                            if (!inShadow)
                            {
                                _lighting[x, y, z] = Defines.MAXLIGHT;
                                toLight.Enqueue(new Light(x, y, z, Defines.MAXLIGHT));
                            }
                            else
                            {
                                _lighting[x, y, z] = Defines.MINLIGHT;
                                if (BlockInformation.IsLightEmittingBlock(blockType))
                                {
                                    toLight.Enqueue(new Light(x, y, z, Defines.MAXLIGHT));
                                }
                            }
                        }
                    }
                }
            }
        }

        public void BlockAdded(BlockType blockType, int x, int y, int z)
        {
            blockList[x, y, z] = blockType;
            if (BlockInformation.IsLightEmittingBlock(blockType))
            {
                toLight.Enqueue(new Light(x, y, z, Defines.MAXLIGHT));
            }
            if (!BlockInformation.IsLightTransparentBlock(blockType))
            {
                toDark.Enqueue(new Light(x, y, z, Defines.MAXLIGHT * 2));
            }
        }

        public void BlockRemoved(BlockType blockType, int x, int y, int z)
        {
            blockList[x, y, z] = blockType;
            if (!BlockInformation.IsLightTransparentBlock(blockType) && !BlockInformation.IsLightEmittingBlock(blockType))
            {
                toLight.Enqueue(new Light(x, y, z, _lighting[x, y, z]));
            }
            if (BlockInformation.IsLightEmittingBlock(blockType))
            {
                LightRemoved(x, y, z);
            }
        }

        private void LightAdded(int x, int y, int z, byte intensity)
        {
            _lighting[x, y, z] = intensity;
            toLight.Enqueue(new Light(x,y,z,intensity));
        }

        private void LightRemoved(int x, int y, int z)
        {
            _lighting[x, y, z] = Defines.MINLIGHT;
            toDark.Enqueue(new Light(x, y, z, Defines.MAXLIGHT * 2));
        }

        public void Update()
        {
            DeFillLighting();
            FillLighting();
        }

        private void FillLighting()
        {
            while (toLight.Count > 0)
            {
                Light light = toLight.Dequeue();
                if (light.Intensity >= Defines.MINLIGHT)
                {
                    CheckLight((int)light.X + 1, (int)light.Y, (int)light.Z, light.Intensity);
                    CheckLight((int)light.X - 1, (int)light.Y, (int)light.Z, light.Intensity);
                    CheckLight((int)light.X, (int)light.Y + 1, (int)light.Z, light.Intensity);
                    CheckLight((int)light.X, (int)light.Y - 1, (int)light.Z, light.Intensity);
                    CheckLight((int)light.X, (int)light.Y, (int)light.Z + 1, light.Intensity);
                    CheckLight((int)light.X, (int)light.Y, (int)light.Z - 1, light.Intensity);
                }
            }
        }

        private void CheckLight(int x, int y, int z, byte intensity)
        {
            intensity = (byte)(intensity - 1);
            if (SaneBlockPosition(x,y,z))
            {
                if (_lighting[x, y, z] < intensity)
                {
                    _lighting[x, y, z] = intensity;
                    if (BlockInformation.IsLightTransparentBlock(BlockAtPoint(x,y,z)))
                    {
                        toLight.Enqueue(new Light(x, y, z, intensity));
                    }
                }
            }
        }

        private void DeFillLighting()
        {
            while (toDark.Count > 0)
            {
                Light dark = toDark.Dequeue();
                if (dark.Intensity > Defines.MINLIGHT)
                {
                    CheckDark((int)dark.X + 1, (int)dark.Y, (int)dark.Z, dark.Intensity);
                    CheckDark((int)dark.X - 1, (int)dark.Y, (int)dark.Z, dark.Intensity);
                    CheckDark((int)dark.X, (int)dark.Y + 1, (int)dark.Z, dark.Intensity);
                    CheckDark((int)dark.X, (int)dark.Y - 1, (int)dark.Z, dark.Intensity);
                    CheckDark((int)dark.X, (int)dark.Y, (int)dark.Z + 1, dark.Intensity);
                    CheckDark((int)dark.X, (int)dark.Y, (int)dark.Z - 1, dark.Intensity);
                }
            }
        }

        private void CheckDark(int x, int y, int z, byte intensity)
        {
            intensity = (byte)(intensity - 1);
            if (SaneBlockPosition(x,y,z))
            {
                if (intensity > Defines.MINLIGHT && _lighting[x, y, z] != Defines.MINLIGHT)
                {
                    _lighting[x, y, z] = Defines.MINLIGHT;
                    // If we're in sunlight on a light emitter we need to requeue
                    if (y >= _lightHeight[x, z] || BlockInformation.IsLightEmittingBlock(BlockAtPoint(x,y,z)))
                    {
                        // We darked a light so schedule it to be relit
                        toLight.Enqueue(new Light(x, y, z, Defines.MAXLIGHT+1));
                    }
                    toDark.Enqueue(new Light(x, y, z, intensity));
                }
            }
        }

        private bool SaneBlockPosition(int x, int y, int z)
        {
            bool goodspot = false;

            if (x <= 0 || y <= 0 || z <= 0 || x >= Defines.MAPSIZE - 1 || y >= Defines.MAPSIZE - 1 || z >= Defines.MAPSIZE - 1)
            {
                goodspot = false;
            }
            else
            {
                goodspot = true;
            }
            return goodspot;
        }

        public float GetLight(int x, int y, int z)
        {
            if (y == Defines.MAPSIZE - 1)
            {
                //HACK We set this value manual put it needs to be fixed
                return 7.5f;
            }
            if (SaneBlockPosition(x, y, z))
            {
                float result = ((float)_lighting[x, y, z]) / (float)Defines.MAXLIGHT;
                return result * 1.5f;
            }
            else
            {
                return 0.01f;
            }
        }

        private BlockType BlockAtPoint(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= Defines.MAPSIZE || y >= Defines.MAPSIZE || z >= Defines.MAPSIZE)
            {
                return BlockType.None;
            }
            return blockList[x, y, z];
        }
    }
}