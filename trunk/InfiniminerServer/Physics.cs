﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;

//Contains functions related to the state of the gameworld

namespace MineWorld
{
    public partial class MineWorldServer
    {
        DateTime lastCalcTnt = DateTime.Now;
        DateTime lastCalcLava = DateTime.Now;
        DateTime lastCalcGrass = DateTime.Now;
        DateTime lastCalcWater = DateTime.Now;

        public void DoPhysics()
        {
            TimeSpan timeSpanCalcTnt = DateTime.Now - lastCalcTnt;
            TimeSpan timeSpanCalcLava = DateTime.Now - lastCalcLava;
            TimeSpan timeSpanCalcGrass = DateTime.Now - lastCalcGrass;
            TimeSpan timeSpanCalcWater = DateTime.Now - lastCalcWater;

            // We calculate tnt every 500 milleseconds
            if (timeSpanCalcTnt.TotalMilliseconds > 500)
            {
                CalcTnt();
                lastCalcTnt = DateTime.Now;
            }
            // We calculate lava every 1000 milliseconds
            if (timeSpanCalcLava.TotalMilliseconds > 1000)
            {
                CalcLava();
                lastCalcLava = DateTime.Now;
            }
            //We calculate water every 1000 milliseconds
            if (timeSpanCalcWater.TotalMilliseconds > 1000)
            {
                CalcWater();
                lastCalcWater = DateTime.Now;
            }
            // We calculate grass every 2500 milliseconds
            // so it doesnt grow to fast
            if (timeSpanCalcGrass.TotalMilliseconds > 2500)
            {
                CalcGrass();
                lastCalcGrass = DateTime.Now;
            }
        }

        public void CalcLava()
        {
            // If some admin disabled fluids we can skip this
            if (Ssettings.StopFluids == false)
            {
                bool[, ,] flowSleep = new bool[MAPSIZE, MAPSIZE, MAPSIZE]; //if true, do not calculate this turn

                for (ushort i = 0; i < MAPSIZE; i++)
                    for (ushort j = 0; j < MAPSIZE; j++)
                        for (ushort k = 0; k < MAPSIZE; k++)
                            flowSleep[i, j, k] = false;

                for (ushort i = 0; i < MAPSIZE; i++)
                    for (ushort j = 0; j < MAPSIZE; j++)
                        for (ushort k = 0; k < MAPSIZE; k++)
                            if (blockList[i, j, k] == BlockType.Lava && !flowSleep[i, j, k])
                            {
                                // RULES FOR LAVA EXPANSION:
                                // if the block below is lava, do nothing (not even horisontal)
                                // if the block below is empty space, move itself down and disalow horisontal lava movement
                                // if the block below is something solid add lava to the sides
                                BlockType typeBelow = (j == 0) ? BlockType.Lava : blockList[i, j - 1, k];
                                BlockType typeIincr = (i == 63) ? BlockType.Lava : blockList[i + 1, j, k];
                                BlockType typeIdesc = (i == 0) ? BlockType.Lava : blockList[i - 1, j, k];
                                BlockType typeKincr = (k == 63) ? BlockType.Lava : blockList[i, j, k + 1];
                                BlockType typeKdesc = (k == 0) ? BlockType.Lava : blockList[i, j, k - 1];

                                bool doHorisontal = true;
                                if (typeBelow == BlockType.None)
                                {
                                    if (j > 0)
                                    {
                                        SetBlock(i, (ushort)(j - 1), k, BlockType.Lava, PlayerTeam.None);
                                        RemoveBlock(i, j, k);
                                        flowSleep[i, j - 1, k] = true;
                                        //flowSleep[i, j, k] = true;
                                        doHorisontal = false;
                                    }
                                }
                                if (typeBelow == BlockType.Lava)
                                {
                                    doHorisontal = false;
                                }
                                if (doHorisontal)
                                {
                                    if (typeIdesc == BlockType.None)
                                    {
                                        if (i > 0)
                                        {
                                            SetBlock((ushort)(i - 1), j, k, BlockType.Lava, PlayerTeam.None);
                                            flowSleep[i - 1, j, k] = true;
                                        }
                                    }
                                    if (typeIincr == BlockType.None)
                                    {
                                        if (i < MAPSIZE)
                                        {
                                            SetBlock((ushort)(i + 1), j, k, BlockType.Lava, PlayerTeam.None);
                                            flowSleep[i + 1, j, k] = true;
                                        }
                                    }
                                    if (typeKdesc == BlockType.None)
                                    {
                                        if (k > 0)
                                        {
                                            SetBlock(i, j, (ushort)(k - 1), BlockType.Lava, PlayerTeam.None);
                                            flowSleep[i, j, k - 1] = true;
                                        }
                                    }
                                    if (typeKincr == BlockType.None)
                                    {
                                        if (k < MAPSIZE)
                                        {
                                            SetBlock(i, j, (ushort)(k + 1), BlockType.Lava, PlayerTeam.None);
                                            flowSleep[i, j, k + 1] = true;
                                        }
                                    }
                                }
                            }
            }
        }

        public void CalcGrass()
        {
            //Todo: Rework this CalcGrass()
            // More horror code :S
            for (ushort i = 0; i < MAPSIZE; i++)
                for (ushort j = 0; j < MAPSIZE; j++)
                    for (ushort k = 0; k < MAPSIZE; k++)
                        if (blockList[i, j, k] == BlockType.Dirt)
                        {
                            if (InDirectSunLight(i, j, k))
                            {
                                if (randGen.Next(0, 6) == 3)
                                {
                                    SetBlock(i, j, k, BlockType.Grass, PlayerTeam.None);
                                }
                            }
                        }
        }

        public void CalcTnt()
        {
            ushort x;
            ushort y;
            ushort z;
            // Explode TNT if lava touches it
            foreach (IClient p in playerList.Values)
            {
                foreach (Vector3 explosive in p.ExplosiveList)
                {
                    x = (ushort)explosive.X;
                    y = (ushort)explosive.Y;
                    z = (ushort)explosive.Z;
                    if (blockList[x + 1, y, z] == BlockType.Lava || blockList[x - 1, y, z] == BlockType.Lava || blockList[x, y, z + 1] == BlockType.Lava || blockList[x, y, z - 1] == BlockType.Lava || blockList[x, y + 1, z] == BlockType.Lava || blockList[x, y - 1, z] == BlockType.Lava)
                    {
                        DetonateAtPoint(x, y, z);
                    }
                }
            }
        }
        public void CalcWater()
        {
            //Todo implent water :S
        }
    }
}