using System;
using System.Threading;

//Contains functions related to the state of the gameworld

namespace MineWorld
{
    public partial class MineWorldServer
    {
        private DateTime _lastCalcBlocks = DateTime.Now;
        private DateTime _lastCalcGrass = DateTime.Now;
        private DateTime _lastCalcHealthRegen = DateTime.Now;
        private DateTime _lastCalcLava = DateTime.Now;
        private DateTime _lastCalcWater = DateTime.Now;

        public void DoPhysics()
        {
            while (true)
            {
                TimeSpan timeSpanCalcHealthRegen = DateTime.Now - _lastCalcHealthRegen;

                TimeSpan timeSpanCalcBlocks = DateTime.Now - _lastCalcBlocks;
                TimeSpan timeSpanCalcLava = DateTime.Now - _lastCalcLava;
                TimeSpan timeSpanCalcGrass = DateTime.Now - _lastCalcGrass;
                TimeSpan timeSpanCalcWater = DateTime.Now - _lastCalcWater;

                // We calculate health regeneration every 1000 milleseconds
                if (timeSpanCalcHealthRegen.TotalMilliseconds > 1000)
                {
                    CalcHealthRegen();
                    _lastCalcHealthRegen = DateTime.Now;
                }

                // We calculate blocks that need action every 250 milleseconds
                if (timeSpanCalcBlocks.TotalMilliseconds > 250)
                {
                    CalcBlocks();
                    _lastCalcBlocks = DateTime.Now;
                }

                // Dont need to calc if its disabled
                if (StopFluids == false)
                {
                    // We calculate lava every 1000 milliseconds
                    if (timeSpanCalcLava.TotalMilliseconds > 1000)
                    {
                        CalcLava();
                        _lastCalcLava = DateTime.Now;
                    }
                    //We calculate water every 1000 milliseconds
                    if (timeSpanCalcWater.TotalMilliseconds > 1000)
                    {
                        CalcWater();
                        _lastCalcWater = DateTime.Now;
                    }
                }
                // We calculate grass every 2500 milliseconds
                // so it doesnt grow to fast
                if (timeSpanCalcGrass.TotalMilliseconds > 2500)
                {
                    CalcGrass();
                    CalcFlowers();
                    _lastCalcGrass = DateTime.Now;
                }
                Thread.Sleep(25);
            }
        }

        public void CalcHealthRegen()
        {
            foreach (ServerPlayer p in PlayerList.Values) //regeneration
            {
                if (p.Alive)
                {
                    // TODO Cast health to float otherwise data loss
                    p.Health += (p.HealthMax/100)*SAsettings.Playerregenrate;
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
                ServerPlayer player = PlayerList[p.NetConn];
                SendPlayerHealthUpdate(player);
            }
        }

        public void CalcLava()
        {
            for (int i = 0; i < Msettings.MapsizeX; i++)
                for (int j = 0; j < Msettings.MapsizeY; j++)
                    for (int k = 0; k < Msettings.MapsizeZ; k++)
                        if (BlockList[i, j, k] == BlockType.Lava)
                        {
                            // RULES FOR LAVA EXPANSION:
                            // if the block below is lava, do nothing (not even horisontal)
                            // if the block below is empty space, move itself down and disalow horisontal lava movement
                            // if the block below is something solid add lava to the sides
                            BlockType typeBelow = (j == 0) ? BlockType.Lava : BlockList[i, j - 1, k];
                            BlockType typeIincr = (i == Msettings.MapsizeX - 1)
                                                      ? BlockType.Lava
                                                      : BlockList[i + 1, j, k];
                            BlockType typeIdesc = (i == 0) ? BlockType.Lava : BlockList[i - 1, j, k];
                            BlockType typeKincr = (k == Msettings.MapsizeZ - 1)
                                                      ? BlockType.Lava
                                                      : BlockList[i, j, k + 1];
                            BlockType typeKdesc = (k == 0) ? BlockType.Lava : BlockList[i, j, k - 1];

                            bool doHorisontal = true;
                            if (typeBelow == BlockType.None)
                            {
                                if (j > 0)
                                {
                                    SendSetBlock(i, (j - 1), k, BlockType.Lava);
                                    SendRemoveBlock(i, j, k);
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
                                        SendSetBlock((i - 1), j, k, BlockType.Lava);
                                    }
                                }
                                if (typeIincr == BlockType.None)
                                {
                                    if (i < Msettings.MapsizeX)
                                    {
                                        SendSetBlock((i + 1), j, k, BlockType.Lava);
                                    }
                                }
                                if (typeKdesc == BlockType.None)
                                {
                                    if (k > 0)
                                    {
                                        SendSetBlock(i, j, (k - 1), BlockType.Lava);
                                    }
                                }
                                if (typeKincr == BlockType.None)
                                {
                                    if (k < Msettings.MapsizeZ)
                                    {
                                        SendSetBlock(i, j, (k + 1), BlockType.Lava);
                                    }
                                }
                            }
                        }
        }

        public void CalcGrass()
        {
            for (int i = 0; i < Msettings.MapsizeX; i++)
                for (int j = 0; j < Msettings.MapsizeY; j++)
                    for (int k = 0; k < Msettings.MapsizeZ; k++)
                        if (BlockList[i, j, k] == BlockType.Dirt)
                        {
                            if (j >= Msettings.MapsizeY/2)
                            {
                                if (InDirectSunLight(i, j, k))
                                {
                                    if (_randGen.Next(0, 6) == 3)
                                    {
                                        SendSetBlock(i, j, k, BlockType.Grass);
                                    }
                                }
                            }
                        }
        }

        public void CalcFlowers()
        {
            for (int i = 0; i < Msettings.MapsizeX; i++)
                for (int j = 0; j < Msettings.MapsizeY; j++)
                    for (int k = 0; k < Msettings.MapsizeZ; k++)
                        if (BlockList[i, j, k] == BlockType.Grass)
                        {
                            int rand = _randGen.Next(0, 1000);
                            if (j == Msettings.MapsizeY - 1)
                            {
                                break;
                            }
                            if (rand == 400)
                            {
                                if (BlockList[i, j + 1, k] == BlockType.None)
                                {
                                    SendSetBlock(i, ++j, k, BlockType.RedFlower);
                                }
                            }
                            else if (rand == 500)
                            {
                                if (BlockList[i, j + 1, k] == BlockType.None)
                                {
                                    SendSetBlock(i, ++j, k, BlockType.YellowFlower);
                                }
                            }
                        }
        }

        public void CalcBlocks()
        {
            // If water touches lava or otherwise around turn it into stone then
            for (int i = 0; i < Msettings.MapsizeX; i++)
                for (int j = 0; j < Msettings.MapsizeY; j++)
                    for (int k = 0; k < Msettings.MapsizeZ; k++)
                        if (BlockList[i, j, k] == BlockType.Water)
                        {
                            BlockType typeBelow = (j == 0) ? BlockType.Water : BlockList[i, j - 1, k];
                            BlockType typeIincr = (i == Msettings.MapsizeX - 1)
                                                      ? BlockType.Water
                                                      : BlockList[i + 1, j, k];
                            BlockType typeIdesc = (i == 0) ? BlockType.Water : BlockList[i - 1, j, k];
                            BlockType typeKincr = (k == Msettings.MapsizeZ - 1)
                                                      ? BlockType.Water
                                                      : BlockList[i, j, k + 1];
                            BlockType typeKdesc = (k == 0) ? BlockType.Water : BlockList[i, j, k - 1];

                            if (typeBelow == BlockType.Lava)
                            {
                                if (j > 0)
                                {
                                    SendSetBlock(i, (j - 1), k, BlockType.Rock);
                                }
                            }
                            if (typeIdesc == BlockType.Lava)
                            {
                                if (i > 0)
                                {
                                    SendSetBlock((i - 1), j, k, BlockType.Rock);
                                }
                            }
                            if (typeIincr == BlockType.Lava)
                            {
                                if (i < Msettings.MapsizeX)
                                {
                                    SendSetBlock((i + 1), j, k, BlockType.Rock);
                                }
                            }
                            if (typeKdesc == BlockType.Lava)
                            {
                                if (k > 0)
                                {
                                    SendSetBlock(i, j, (k - 1), BlockType.Rock);
                                }
                            }
                            if (typeKincr == BlockType.Lava)
                            {
                                if (k < Msettings.MapsizeZ)
                                {
                                    SendSetBlock(i, j, (k + 1), BlockType.Rock);
                                }
                            }
                        }
                        else if (BlockList[i, j, k] == BlockType.YellowFlower ||
                                 BlockList[i, j, k] == BlockType.RedFlower)
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
            for (int i = 0; i < Msettings.MapsizeX; i++)
                for (int j = 0; j < Msettings.MapsizeY; j++)
                    for (int k = 0; k < Msettings.MapsizeZ; k++)
                        if (BlockList[i, j, k] == BlockType.Water)
                        {
                            // RULES FOR WATER EXPANSION:
                            // if the block below is water, do nothing (not even horisontal)
                            // if the block below is empty space, move itself down and disalow horisontal lava movement
                            // if the block below is something solid add water to the sides
                            BlockType typeBelow = (j == 0) ? BlockType.Water : BlockList[i, j - 1, k];
                            BlockType typeIincr = (i == Msettings.MapsizeX - 1)
                                                      ? BlockType.Water
                                                      : BlockList[i + 1, j, k];
                            BlockType typeIdesc = (i == 0) ? BlockType.Water : BlockList[i - 1, j, k];
                            BlockType typeKincr = (k == Msettings.MapsizeZ - 1)
                                                      ? BlockType.Water
                                                      : BlockList[i, j, k + 1];
                            BlockType typeKdesc = (k == 0) ? BlockType.Water : BlockList[i, j, k - 1];

                            bool doHorisontal = true;
                            if (typeBelow == BlockType.None)
                            {
                                if (j > 0)
                                {
                                    SendSetBlock(i, (j - 1), k, BlockType.Water);
                                    SendRemoveBlock(i, j, k);
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
                                        SendSetBlock((i - 1), j, k, BlockType.Water);
                                    }
                                }
                                if (typeIincr == BlockType.None)
                                {
                                    if (i < Msettings.MapsizeX)
                                    {
                                        SendSetBlock((i + 1), j, k, BlockType.Water);
                                    }
                                }
                                if (typeKdesc == BlockType.None)
                                {
                                    if (k > 0)
                                    {
                                        SendSetBlock(i, j, (k - 1), BlockType.Water);
                                    }
                                }
                                if (typeKincr == BlockType.None)
                                {
                                    if (k < Msettings.MapsizeZ)
                                    {
                                        SendSetBlock(i, j, (k + 1), BlockType.Water);
                                    }
                                }
                            }
                        }
        }
    }
}