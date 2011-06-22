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
        DateTime lastCalcHealthRegen = DateTime.Now;

        DateTime lastCalcBlocks = DateTime.Now;
        DateTime lastCalcLava = DateTime.Now;
        DateTime lastCalcGrass = DateTime.Now;
        DateTime lastCalcWater = DateTime.Now;

        public void DoPhysics()
        {
            while (true)
            {
                TimeSpan timeSpanCalcHealthRegen = DateTime.Now - lastCalcHealthRegen;

                TimeSpan timeSpanCalcBlocks = DateTime.Now - lastCalcBlocks;
                TimeSpan timeSpanCalcLava = DateTime.Now - lastCalcLava;
                TimeSpan timeSpanCalcGrass = DateTime.Now - lastCalcGrass;
                TimeSpan timeSpanCalcWater = DateTime.Now - lastCalcWater;

                // We calculate health regeneration every 1000 milleseconds
                if (timeSpanCalcHealthRegen.TotalMilliseconds > 1000)
                {
                    CalcHealthRegen();
                    lastCalcHealthRegen = DateTime.Now;
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
                    CalcFlowers();
                    lastCalcGrass = DateTime.Now;
                }
                Thread.Sleep(25);
            }
        }

        public void CalcHealthRegen()
        {
            foreach (ServerPlayer p in playerList.Values)//regeneration
            {
                if (p.Alive && p.Canhealthregen)
                {
                    // TODO Cast health to float otherwise data loss
                    p.Health = (p.HealthMax / 100) * SAsettings.Playerregenrate;
                    if (p.Health >= p.HealthMax)
                    {
                        p.Health = p.HealthMax;
                    }
                }
                else 
                {
                    // Player is dead
                    // Just a extra check to make sure if the player is dead
                    // That they dont have health values other then 0
                    p.Health = 0;
                }
                ServerPlayer player = playerList[p.NetConn];
                SendHealthUpdate(player);
            }
        }

        public void CalcLava()
        {
            for (int i = 0; i < Defines.MAPSIZE; i++)
                for (int j = 0; j < Defines.MAPSIZE; j++)
                    for (int k = 0; k < Defines.MAPSIZE; k++)
                        if (blockList[i, j, k] == BlockType.Lava)
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
                                    SetBlock(i, (int)(j - 1), k, BlockType.Lava);
                                    RemoveBlock(i, j, k);
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
                                        SetBlock((int)(i - 1), j, k, BlockType.Lava);
                                    }
                                }
                                if (typeIincr == BlockType.None)
                                {
                                    if (i < Defines.MAPSIZE)
                                    {
                                        SetBlock((int)(i + 1), j, k, BlockType.Lava);
                                    }
                                }
                                if (typeKdesc == BlockType.None)
                                {
                                    if (k > 0)
                                    {
                                        SetBlock(i, j, (int)(k - 1), BlockType.Lava);
                                    }
                                }
                                if (typeKincr == BlockType.None)
                                {
                                    if (k < Defines.MAPSIZE)
                                    {
                                        SetBlock(i, j, (int)(k + 1), BlockType.Lava);
                                    }
                                }
                            }
                        }
        }

        public void CalcGrass()
        {
            for (int i = 0; i < Defines.MAPSIZE; i++)
                for (int j = 0; j < Defines.MAPSIZE; j++)
                    for (int k = 0; k < Defines.MAPSIZE; k++)
                        if (blockList[i, j, k] == BlockType.Dirt)
                        {
                            if (j >= Defines.GROUND_LEVEL)
                            {
                                if (InDirectSunLight(i, j, k))
                                {
                                    if (randGen.Next(0, 6) == 3)
                                    {
                                        SetBlock(i, j, k, BlockType.Grass);
                                    }
                                }
                            }
                        }
        }

        public void CalcFlowers()
        {
            for (int i = 0; i < Defines.MAPSIZE; i++)
                for (int k = 0; k < Defines.MAPSIZE; k++)
                    for (int j = 0; j < Defines.MAPSIZE; j++)
                        if (blockList[i, j, k] == BlockType.Grass)
                        {
                            int rand = randGen.Next(0, 1000);
                            if (j == Defines.MAPSIZE - 1)
                            {
                                break;
                            }
                            if (rand == 400)
                            {
                                if (blockList[i, j + 1, k] == BlockType.None)
                                {
                                    SetBlock(i, ++j, k, BlockType.RedFlower);
                                }
                            }
                            else if (rand == 500)
                            {
                                if (blockList[i, j + 1, k] == BlockType.None)
                                {
                                    SetBlock(i, ++j, k, BlockType.YellowFlower);
                                }
                            }
                        }
        }

        public void CalcBlocks()
        {
            // If water touches lava or otherwise around turn it into stone then
            for (int i = 0; i < Defines.MAPSIZE; i++)
                for (int j = 0; j < Defines.MAPSIZE; j++)
                    for (int k = 0; k < Defines.MAPSIZE; k++)
                        if (blockList[i, j, k] == BlockType.Water)
                        {
                            BlockType typeBelow = (j == 0) ? BlockType.Water : blockList[i, j - 1, k];
                            BlockType typeIincr = (i == Defines.MAPSIZE - 1) ? BlockType.Water : blockList[i + 1, j, k];
                            BlockType typeIdesc = (i == 0) ? BlockType.Water : blockList[i - 1, j, k];
                            BlockType typeKincr = (k == Defines.MAPSIZE - 1) ? BlockType.Water : blockList[i, j, k + 1];
                            BlockType typeKdesc = (k == 0) ? BlockType.Water : blockList[i, j, k - 1];

                            if (typeBelow == BlockType.Lava)
                            {
                                if (j > 0)
                                {
                                    SetBlock(i, (j - 1), k, BlockType.Rock);
                                }
                            }
                            if (typeIdesc == BlockType.Lava)
                            {
                                if (i > 0)
                                {
                                    SetBlock((i - 1), j, k, BlockType.Rock);
                                }
                            }
                            if (typeIincr == BlockType.Lava)
                            {
                                if (i < Defines.MAPSIZE)
                                {
                                    SetBlock((i + 1), j, k, BlockType.Rock);
                                }
                            }
                            if (typeKdesc == BlockType.Lava)
                            {
                                if (k > 0)
                                {
                                    SetBlock(i, j, (k - 1), BlockType.Rock);
                                }
                            }
                            if (typeKincr == BlockType.Lava)
                            {
                                if (k < Defines.MAPSIZE)
                                {
                                    SetBlock(i, j, (k + 1), BlockType.Rock);
                                }
                            }
                        }
                        else if (blockList[i, j, k] == BlockType.YellowFlower || blockList[i, j, k] == BlockType.RedFlower)
                        {
                            //TODO WIP Removal of flowers when there is no grass beneath them
                            //int y = j;
                            //BlockType typeBelow = blockList[i, y - 1, k];
                            //if (typeBelow != BlockType.Grass)
                            //{
                                //Omg a floating flower
                                //SetBlock(i, j, k, BlockType.None);
                            //}
                        }
        }

        public void CalcWater()
        {
            for (int i = 0; i < Defines.MAPSIZE; i++)
                for (int j = 0; j < Defines.MAPSIZE; j++)
                    for (int k = 0; k < Defines.MAPSIZE; k++)
                        if (blockList[i, j, k] == BlockType.Water)
                        {
                            // RULES FOR WATER EXPANSION:
                            // if the block below is water, do nothing (not even horisontal)
                            // if the block below is empty space, move itself down and disalow horisontal lava movement
                            // if the block below is something solid add water to the sides
                            BlockType typeBelow = (j == 0) ? BlockType.Water : blockList[i, j - 1, k];
                            BlockType typeIincr = (i == Defines.MAPSIZE - 1) ? BlockType.Water : blockList[i + 1, j, k];
                            BlockType typeIdesc = (i == 0) ? BlockType.Water : blockList[i - 1, j, k];
                            BlockType typeKincr = (k == Defines.MAPSIZE - 1) ? BlockType.Water : blockList[i, j, k + 1];
                            BlockType typeKdesc = (k == 0) ? BlockType.Water : blockList[i, j, k - 1];

                            bool doHorisontal = true;
                            if (typeBelow == BlockType.None)
                            {
                                if (j > 0)
                                {
                                    SetBlock(i,(j - 1), k, BlockType.Water);
                                    RemoveBlock(i, j, k);
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
                                        SetBlock((i - 1), j, k, BlockType.Water);
                                    }
                                }
                                if (typeIincr == BlockType.None)
                                {
                                    if (i < Defines.MAPSIZE)
                                    {
                                        SetBlock((i + 1), j, k, BlockType.Water);
                                    }
                                }
                                if (typeKdesc == BlockType.None)
                                {
                                    if (k > 0)
                                    {
                                        SetBlock(i, j,(k - 1), BlockType.Water);
                                    }
                                }
                                if (typeKincr == BlockType.None)
                                {
                                    if (k < Defines.MAPSIZE)
                                    {
                                        SetBlock(i, j,(k + 1), BlockType.Water);
                                    }
                                }
                            }
                        }
        }
    }
}