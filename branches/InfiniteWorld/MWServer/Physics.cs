using System;
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
        DateTime lastCalcRegen = DateTime.Now;

        DateTime lastCalcBlocks = DateTime.Now;
        DateTime lastCalcLava = DateTime.Now;
        DateTime lastCalcGrass = DateTime.Now;
        DateTime lastCalcWater = DateTime.Now;

        public void DoPhysics()
        {
            while (true)
            {
                TimeSpan timeSpanCalcRegen = DateTime.Now - lastCalcRegen;

                TimeSpan timeSpanCalcBlocks = DateTime.Now - lastCalcBlocks;
                TimeSpan timeSpanCalcLava = DateTime.Now - lastCalcLava;
                TimeSpan timeSpanCalcGrass = DateTime.Now - lastCalcGrass;
                TimeSpan timeSpanCalcWater = DateTime.Now - lastCalcWater;

                // We calculate health regeneration every 250 milleseconds
                if (timeSpanCalcRegen.TotalMilliseconds > 250)
                {
                    CalcRegen();
                    lastCalcRegen = DateTime.Now;
                }

                // We calculate blocks that need action every 250 milleseconds
                if (timeSpanCalcBlocks.TotalMilliseconds > 250)
                {
                    CalcBlocks();
                    lastCalcBlocks = DateTime.Now;
                }

                // Dont need to calc if its disabled
                if (StopFluids == false)
                {
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
                }
                // We calculate grass every 2500 milliseconds
                // so it doesnt grow to fast
                if (timeSpanCalcGrass.TotalMilliseconds > 2500)
                {
                    CalcGrass();
                    lastCalcGrass = DateTime.Now;
                }
                System.Threading.Thread.Sleep(25);
            }
        }

        public void CalcRegen()
        {
            foreach (Player p in playerList.Values)//regeneration
            {
                if (p.Alive)
                {
                    if (p.Health >= p.HealthMax)
                    {
                        p.Health = p.HealthMax;
                    }
                    else
                    {
                        p.Health = p.Health + 1;
                        IClient player = playerList[p.NetConn];
                        SendResourceUpdate(player);
                    }
                }
            }

        }

        public void CalcLava()
        {
            // If some admin disabled fluids we can skip this
            bool[, ,] flowSleep = new bool[Defines.MAPSIZE, Defines.MAPSIZE, Defines.MAPSIZE]; //if true, do not calculate this turn

            for (ushort i = 0; i < Defines.MAPSIZE; i++)
                for (ushort j = 0; j < Defines.MAPSIZE; j++)
                    for (ushort k = 0; k < Defines.MAPSIZE; k++)
                        flowSleep[i, j, k] = false;

            for (ushort i = 0; i < Defines.MAPSIZE; i++)
                for (ushort j = 0; j < Defines.MAPSIZE; j++)
                    for (ushort k = 0; k < Defines.MAPSIZE; k++)
                        if (blockList[i, j, k] == BlockType.Lava && !flowSleep[i, j, k])
                        {
                            // RULES FOR LAVA EXPANSION:
                            // if the block below is lava, do nothing (not even horisontal)
                            // if the block below is empty space, move itself down and disalow horisontal lava movement
                            // if the block below is something solid add lava to the sides
                            BlockType typeBelow = (j == 0) ? BlockType.Lava : blockList[i, j - 1, k];
                            BlockType typeIincr = ((int)i == Defines.MAPSIZE - 1) ? BlockType.Lava : blockList[i + 1, j, k];
                            BlockType typeIdesc = (i == 0) ? BlockType.Lava : blockList[i - 1, j, k];
                            BlockType typeKincr = ((int)k == Defines.MAPSIZE - 1) ? BlockType.Lava : blockList[i, j, k + 1];
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
                            if (typeBelow != BlockType.None)
                            {
                                doHorisontal = true;
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
                                    if (i < Defines.MAPSIZE)
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
                                    if (k < Defines.MAPSIZE)
                                    {
                                        SetBlock(i, j, (ushort)(k + 1), BlockType.Lava, PlayerTeam.None);
                                        flowSleep[i, j, k + 1] = true;
                                    }
                                }
                            }
                        }
        }

        public void CalcGrass()
        {
            for (ushort i = 0; i < Defines.MAPSIZE; i++)
                for (ushort j = 0; j < Defines.MAPSIZE; j++)
                    for (ushort k = 0; k < Defines.MAPSIZE; k++)
                        if (blockList[i, j, k] == BlockType.Dirt)
                        {
                            if (j >= Defines.GROUND_LEVEL)
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
        }

        public void CalcBlocks()
        {
            ushort x;
            ushort y;
            ushort z;
            // Explode TNT if lava touches it
            foreach (IClient p in playerList.Values)
            {
                try
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
                catch
                {
                    //no more explosives in chain reaction
                }
            }
            // If water touches lava or otherwise around turn it into stone then
            for (ushort i = 0; i < Defines.MAPSIZE; i++)
                for (ushort j = 0; j < Defines.MAPSIZE; j++)
                    for (ushort k = 0; k < Defines.MAPSIZE; k++)
                        if (blockList[i, j, k] == BlockType.Water)
                        {
                            BlockType typeBelow = (j == 0) ? BlockType.Water : blockList[i, j - 1, k];
                            BlockType typeIincr = ((int)i == Defines.MAPSIZE - 1) ? BlockType.Water : blockList[i + 1, j, k];
                            BlockType typeIdesc = (i == 0) ? BlockType.Water : blockList[i - 1, j, k];
                            BlockType typeKincr = ((int)k == Defines.MAPSIZE - 1) ? BlockType.Water : blockList[i, j, k + 1];
                            BlockType typeKdesc = (k == 0) ? BlockType.Water : blockList[i, j, k - 1];

                            if (typeBelow == BlockType.Lava)
                            {
                                if (j > 0)
                                {
                                    SetBlock(i, (ushort)(j - 1), k, BlockType.Rock, PlayerTeam.None);
                                }
                            }
                            if (typeIdesc == BlockType.Lava)
                            {
                                if (i > 0)
                                {
                                    SetBlock((ushort)(i - 1), j, k, BlockType.Rock, PlayerTeam.None);
                                }
                            }
                            if (typeIincr == BlockType.Lava)
                            {
                                if (i < Defines.MAPSIZE)
                                {
                                    SetBlock((ushort)(i + 1), j, k, BlockType.Rock, PlayerTeam.None);
                                }
                            }
                            if (typeKdesc == BlockType.Lava)
                            {
                                if (k > 0)
                                {
                                    SetBlock(i, j, (ushort)(k - 1), BlockType.Rock, PlayerTeam.None);
                                }
                            }
                            if (typeKincr == BlockType.Lava)
                            {
                                if (k < Defines.MAPSIZE)
                                {
                                    SetBlock(i, j, (ushort)(k + 1), BlockType.Rock, PlayerTeam.None);
                                }
                            }
                        }
        }

        public void CalcWater()
        {
            // If some admin disabled fluids we can skip this
            bool[, ,] flowSleep = new bool[Defines.MAPSIZE, Defines.MAPSIZE, Defines.MAPSIZE]; //if true, do not calculate this turn

            for (ushort i = 0; i < Defines.MAPSIZE; i++)
                for (ushort j = 0; j < Defines.MAPSIZE; j++)
                    for (ushort k = 0; k < Defines.MAPSIZE; k++)
                        flowSleep[i, j, k] = false;

            for (ushort i = 0; i < Defines.MAPSIZE; i++)
                for (ushort j = 0; j < Defines.MAPSIZE; j++)
                    for (ushort k = 0; k < Defines.MAPSIZE; k++)
                        if (blockList[i, j, k] == BlockType.Water && !flowSleep[i, j, k])
                        {
                            // RULES FOR WATER EXPANSION:
                            // if the block below is water, do nothing (not even horisontal)
                            // if the block below is empty space, move itself down and disalow horisontal lava movement
                            // if the block below is something solid add water to the sides
                            BlockType typeBelow = (j == 0) ? BlockType.Water : blockList[i, j - 1, k];
                            BlockType typeIincr = ((int)i == Defines.MAPSIZE - 1) ? BlockType.Water : blockList[i + 1, j, k];
                            BlockType typeIdesc = (i == 0) ? BlockType.Water : blockList[i - 1, j, k];
                            BlockType typeKincr = ((int)k == Defines.MAPSIZE - 1) ? BlockType.Water : blockList[i, j, k + 1];
                            BlockType typeKdesc = (k == 0) ? BlockType.Water : blockList[i, j, k - 1];

                            bool doHorisontal = true;
                            if (typeBelow == BlockType.None)
                            {
                                if (j > 0)
                                {
                                    SetBlock(i, (ushort)(j - 1), k, BlockType.Water, PlayerTeam.None);
                                    RemoveBlock(i, j, k);
                                    flowSleep[i, j - 1, k] = true;
                                    //flowSleep[i, j, k] = true;
                                    doHorisontal = false;
                                }
                            }
                            if (typeBelow != BlockType.None)
                            {
                                doHorisontal = true;
                            }
                            if (typeBelow == BlockType.Water)
                            {
                                doHorisontal = false;
                            }
                            if (doHorisontal)
                            {
                                if (typeIdesc == BlockType.None)
                                {
                                    if (i > 0)
                                    {
                                        SetBlock((ushort)(i - 1), j, k, BlockType.Water, PlayerTeam.None);
                                        flowSleep[i - 1, j, k] = true;
                                    }
                                }
                                if (typeIincr == BlockType.None)
                                {
                                    if (i < Defines.MAPSIZE)
                                    {
                                        SetBlock((ushort)(i + 1), j, k, BlockType.Water, PlayerTeam.None);
                                        flowSleep[i + 1, j, k] = true;
                                    }
                                }
                                if (typeKdesc == BlockType.None)
                                {
                                    if (k > 0)
                                    {
                                        SetBlock(i, j, (ushort)(k - 1), BlockType.Water, PlayerTeam.None);
                                        flowSleep[i, j, k - 1] = true;
                                    }
                                }
                                if (typeKincr == BlockType.None)
                                {
                                    if (k < Defines.MAPSIZE)
                                    {
                                        SetBlock(i, j, (ushort)(k + 1), BlockType.Water, PlayerTeam.None);
                                        flowSleep[i, j, k + 1] = true;
                                    }
                                }
                            }
                        }
        }
    }
}