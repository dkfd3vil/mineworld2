﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace MineWorld
{
    public enum BlockType : byte
    {
        None = 0,

        //Nature blocks
        //Start from 1 - 100 
        Dirt = 1,
        Ore = 3,
        Gold = 4,
        Diamond = 5,
        Rock = 6,
        Metal = 7,
        Grass = 8,
        Leafs = 9,
        Wood = 10,
        Water = 11,
        Lava = 12,
        Adminblock = 13,
        RedFlower = 14,

        //Nature + usermade blocks
        //Range from 101 - 150
        DirtSign = 101,

        //Usermade blocks
        //Range from 150 - 254
        Ladder = 150,
        Explosive = 151,
        Jump = 152,
        Shock = 153,
        BankRed = 154,
        BankBlue = 155,
        HomeRed = 156,
        HomeBlue = 157,
        Road = 158,
        SolidRed = 159,
        SolidBlue = 160,
        Lamp = 161,
        TransRed = 162,
        TransBlue = 163,

        MAXIMUM = 255
    }

    public enum BlockTexture : byte
    {
        None,
        Dirt,
        Ore,
        Gold,
        Diamond,
        Rock,
        Jump,
        JumpTop,
        Ladder,
        LadderTop,
        Explosive,
        Spikes,
        HomeRed,
        HomeBlue,
        BankTopRed,
        BankTopBlue,
        BankFrontRed,
        BankFrontBlue,
        BankLeftRed,
        BankLeftBlue,
        BankRightRed,
        BankRightBlue,
        BankBackRed,
        BankBackBlue,
        TeleTop,
        TeleBottom,
        TeleSideA,
        TeleSideB,
        SolidRed,
        SolidBlue,
        Metal,
        DirtSign,
        Lava,
        RedFlower,
        Road,
        RoadTop,
        RoadBottom,
        Adminblock,
        Lamp,
        Grass,
        GrassSide,
        WoodSide,
        Wood,
        Leafs,
        Water,
        TransRed,   // THESE MUST BE THE LAST TWO TEXTURES
        TransBlue,
        MAXIMUM
    }

    public enum BlockFaceDirection : byte
    {
        XIncreasing,
        XDecreasing,
        YIncreasing,
        YDecreasing,
        ZIncreasing,
        ZDecreasing,
        MAXIMUM
    }

    public class BlockInformation
    {   
        public static BlockTexture GetTexture(BlockType blockType, BlockFaceDirection faceDir)
        {
            switch (blockType)
            {
                case BlockType.Water:
                    {
                        return BlockTexture.Water;
                    }
                case BlockType.Metal:
                    {
                        return BlockTexture.Metal;
                    }
                case BlockType.Lava:
                    {
                        return BlockTexture.Lava;
                    }
                case BlockType.Rock:
                    {
                        return BlockTexture.Rock;
                    }
                case BlockType.Ore:
                    {
                        return BlockTexture.Ore;
                    }
                case BlockType.Gold:
                    {
                        return BlockTexture.Gold;
                    }
                case BlockType.Diamond:
                    {
                        return BlockTexture.Diamond;
                    }
                case BlockType.DirtSign:
                    {
                        return BlockTexture.DirtSign;
                    }
                case BlockType.Adminblock:
                    {
                        return BlockTexture.Adminblock;
                    }
                case BlockType.Dirt:
                    {
                        return BlockTexture.Dirt;
                    }
                case BlockType.Grass:
                    switch (faceDir)
                    {
                        case BlockFaceDirection.YIncreasing:
                            {
                                return BlockTexture.Grass;
                            }
                        case BlockFaceDirection.ZDecreasing: 
                            { 
                                return BlockTexture.GrassSide; 
                            }
                        case BlockFaceDirection.ZIncreasing:
                            {
                                return BlockTexture.GrassSide;
                            }
                        case BlockFaceDirection.XDecreasing:
                            {
                                return BlockTexture.GrassSide;
                            }
                        case BlockFaceDirection.XIncreasing:
                            {
                                return BlockTexture.GrassSide;
                            }
                        default:
                            {
                                return BlockTexture.Dirt;
                            }
                    }
                case BlockType.Leafs:
                    {
                        return BlockTexture.Leafs;
                    }

                case BlockType.Wood:
                    switch (faceDir)
                    {
                        case BlockFaceDirection.YIncreasing:
                            {
                                return BlockTexture.Wood;
                            }
                        case BlockFaceDirection.ZDecreasing:
                            {
                                return BlockTexture.WoodSide;
                            }
                        case BlockFaceDirection.ZIncreasing:
                            {
                                return BlockTexture.WoodSide;
                            }
                        case BlockFaceDirection.XDecreasing:
                            {
                                return BlockTexture.WoodSide;
                            }
                        case BlockFaceDirection.XIncreasing:
                            {
                                return BlockTexture.WoodSide;
                            }
                        default:
                            {
                                return BlockTexture.Wood;
                            }
                    }
                case BlockType.BankRed:
                    switch (faceDir)
                    {
                        case BlockFaceDirection.XIncreasing: return BlockTexture.BankFrontRed;
                        case BlockFaceDirection.XDecreasing: return BlockTexture.BankBackRed;
                        case BlockFaceDirection.ZIncreasing: return BlockTexture.BankLeftRed;
                        case BlockFaceDirection.ZDecreasing: return BlockTexture.BankRightRed;
                        default: return BlockTexture.BankTopRed;
                    }

                case BlockType.BankBlue:
                    switch (faceDir)
                    {
                        case BlockFaceDirection.XIncreasing: return BlockTexture.BankFrontBlue;
                        case BlockFaceDirection.XDecreasing: return BlockTexture.BankBackBlue;
                        case BlockFaceDirection.ZIncreasing: return BlockTexture.BankLeftBlue;
                        case BlockFaceDirection.ZDecreasing: return BlockTexture.BankRightBlue;
                        default: return BlockTexture.BankTopBlue;
                    }
                case BlockType.HomeBlue:
                    switch (faceDir)
                    {
                        case BlockFaceDirection.XIncreasing: return BlockTexture.HomeBlue;
                        case BlockFaceDirection.XDecreasing: return BlockTexture.HomeBlue;
                        case BlockFaceDirection.ZIncreasing: return BlockTexture.HomeBlue;
                        case BlockFaceDirection.ZDecreasing: return BlockTexture.HomeBlue;
                        default: return BlockTexture.Metal;
                    }
                case BlockType.HomeRed:
                    switch (faceDir)
                    {
                        case BlockFaceDirection.XIncreasing: return BlockTexture.HomeRed;
                        case BlockFaceDirection.XDecreasing: return BlockTexture.HomeRed;
                        case BlockFaceDirection.ZIncreasing: return BlockTexture.HomeRed;
                        case BlockFaceDirection.ZDecreasing: return BlockTexture.HomeRed;
                        default: return BlockTexture.Metal;
                    }
                case BlockType.Road:
                    switch (faceDir)
                    {
                        case BlockFaceDirection.YIncreasing:
                            {
                                return BlockTexture.RoadTop;
                            }
                        case BlockFaceDirection.YDecreasing:
                            {
                                return BlockTexture.RoadBottom;
                            }
                        default:
                            {
                                return BlockTexture.Road;
                            }
                    }
                case BlockType.Shock:
                    switch (faceDir)
                    {
                        case BlockFaceDirection.YDecreasing:
                            return BlockTexture.Spikes;
                        case BlockFaceDirection.YIncreasing:
                            return BlockTexture.TeleBottom;
                        case BlockFaceDirection.XDecreasing:
                        case BlockFaceDirection.XIncreasing:
                            return BlockTexture.TeleSideA;
                        case BlockFaceDirection.ZDecreasing:
                        case BlockFaceDirection.ZIncreasing:
                            return BlockTexture.TeleSideB;
                    }
                    break;

                case BlockType.Jump:
                    switch (faceDir)
                    {
                        case BlockFaceDirection.YDecreasing:
                            return BlockTexture.TeleBottom;
                        case BlockFaceDirection.YIncreasing:
                            return BlockTexture.JumpTop;
                        case BlockFaceDirection.XDecreasing:
                        case BlockFaceDirection.XIncreasing:
                            return BlockTexture.Jump;
                        case BlockFaceDirection.ZDecreasing:
                        case BlockFaceDirection.ZIncreasing:
                            return BlockTexture.Jump;
                    }
                    break;

                case BlockType.SolidRed:
                    return BlockTexture.SolidRed;
                case BlockType.SolidBlue:
                    return BlockTexture.SolidBlue;
                case BlockType.TransRed:
                    return BlockTexture.TransRed;
                case BlockType.TransBlue:
                    return BlockTexture.TransBlue;

                case BlockType.Ladder:
                    if (faceDir == BlockFaceDirection.YDecreasing || faceDir == BlockFaceDirection.YIncreasing)
                        return BlockTexture.LadderTop;
                    else
                        return BlockTexture.Ladder;

                case BlockType.Explosive:
                    return BlockTexture.Explosive;
                case BlockType.Lamp:
                    return BlockTexture.Lamp;
            }

            return BlockTexture.None;
        }

        public static bool IsDiggable(BlockType type)
        {
            switch (type)
            {
                case BlockType.Adminblock:
                case BlockType.None:
                case BlockType.Water:
                    return false;
                default:
                    return true;
            }
        }

        public static MineWorldSound GetBlockSound(BlockType type)
        {
            switch (type)
            {
                case BlockType.Ore:
                case BlockType.Diamond:
                case BlockType.Gold:
                    return MineWorldSound.DigMetal;
                default:
                    return MineWorldSound.DigDirt;
            }
        }

        public static bool IsLightEmittingBlock(BlockType type)
        {
            switch (type)
            {
                case BlockType.Lava:
                case BlockType.Shock:
                case BlockType.Road:
                case BlockType.Lamp:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsLightTransparentBlock(BlockType type)
        {
            switch (type)
            {
                case BlockType.None:
                case BlockType.Water:
                case BlockType.Leafs:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsTransparentBlock(BlockType type)
        {
            switch (type)
            {
                case BlockType.Water:
                case BlockType.Grass:
                case BlockType.Leafs:
                case BlockType.TransBlue:
                case BlockType.TransRed:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsPassibleBlock(BlockType type)
        {
            switch (type)
            {
                case BlockType.None:
                case BlockType.Water:
                case BlockType.TransBlue:
                case BlockType.TransRed:
                case BlockType.Grass:
                case BlockType.Lava:
                case BlockType.RedFlower:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsPlantBlock(BlockTexture text)
        {
            if (text == BlockTexture.RedFlower) return true;
            return false;
        }
    }
}