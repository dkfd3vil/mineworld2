using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace MineWorld
{
    public enum BlockType : byte
    {
        None,
        Dirt,
        Ore,
        Gold,
        Diamond,
        Rock,
        Ladder,
        Explosive,
        Jump,
        Shock,
        BankRed,
        BankBlue,
        BeaconRed,
        BeaconBlue,
        HomeRed,
        HomeBlue,
        Road,
        SolidRed,
        SolidBlue,
        Metal,
        DirtSign,
        Adminblock,
        Grass,
        Lava,
        Lamp,
        TransRed,
        TransBlue,
        Water,
        Leaves,
        Wood,
        //Spring,
        MAXIMUM
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
        Road,
        RoadTop,
        RoadBottom,
        BeaconRed,
        BeaconBlue,
        Adminblock,
        Lamp,
        Grass,
        GrassSide,
        Water,
        //Spring,
        WoodSide,
        Wood,
        Leafs,
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
        public static uint GetCost(BlockType blockType)
        {
            switch (blockType)
            {
                case BlockType.BankRed:
                case BlockType.BankBlue:
                case BlockType.BeaconRed:
                case BlockType.BeaconBlue:
                    return 50;

                case BlockType.SolidRed:
                case BlockType.SolidBlue:
                    return 10;

                case BlockType.TransRed:
                case BlockType.TransBlue:
                    return 25;

                case BlockType.Road:
                    return 10;
                case BlockType.Jump:
                    return 25;
                case BlockType.Ladder:
                    return 25;
                case BlockType.Shock:
                    return 50;
                case BlockType.Explosive:
                case BlockType.Lava:
                    return 100;
                case BlockType.Lamp:
                    return 5;
            }

            return 1000;
        }

        public static BlockTexture GetTexture(BlockType blockType, BlockFaceDirection faceDir)
        {
            return GetTexture(blockType, faceDir, BlockType.None);
        }

        public static String GetTopTextureFile(BlockType blockType)
        {
            switch (blockType)
            {
                case BlockType.Water:
                    return "tex_block_trans_water";
                case BlockType.Metal:
                    return "tex_block_metal";
                case BlockType.Lava:
                    return "tex_block_lava";
                case BlockType.Rock:
                    return "tex_block_rock";
                case BlockType.Ore:
                    return "tex_block_ore";
                case BlockType.Gold:
                    return "tex_block_silver";
                case BlockType.Diamond:
                    return "tex_block_diamond";
                case BlockType.DirtSign:
                    return "tex_block_dirt";
                case BlockType.Adminblock:
                    return "tex_block_adminblock";
                case BlockType.Dirt:
                    return "tex_block_dirt";
                case BlockType.Grass:
                    return "tex_block_dirt";
                case BlockType.Leaves:
                    return "tex_block_leaves";
                case BlockType.Wood:
                    return "tex_block_tree_top";
                case BlockType.BankRed:
                    return "tex_block_bank_front_red";
                case BlockType.BankBlue:
                    return "tex_block_bank_front_blue";
                case BlockType.HomeBlue:
                    return "tex_block_home_blue";
                case BlockType.HomeRed:
                    return "tex_block_home_red";
                case BlockType.BeaconRed:
                    return "tex_block_beacon_top_red";
                case BlockType.BeaconBlue:
                    return "tex_block_beacon_top_blue";
                case BlockType.Road:
                    return "tex_block_road_top";
                case BlockType.Shock:
                    return "tex_block_teleporter_bottom";
                case BlockType.Jump:
                    return "tex_block_jump_top";
                case BlockType.SolidRed:
                    return "tex_block_red";
                case BlockType.SolidBlue:
                    return "tex_block_blue";
                case BlockType.TransRed:
                    return "tex_block_trans_red";
                case BlockType.TransBlue:
                    return "tex_block_trans_blue";
                case BlockType.Ladder:
                    return "tex_block_ladder";
                case BlockType.Explosive:
                    return "tex_block_explosive";
                case BlockType.Lamp:
                    return "tex_block_lamp";
            }

            return "";
        }
        

        public static BlockTexture GetTexture(BlockType blockType, BlockFaceDirection faceDir, BlockType blockAbove)
        {
            switch (blockType)
            {
                case BlockType.Water:
                    return BlockTexture.Water;
                //case BlockType.Spring:
                    //return BlockTexture.Spring;
                case BlockType.Metal:
                    return BlockTexture.Metal;
                case BlockType.Lava:
                    return BlockTexture.Lava;
                case BlockType.Rock:
                    return BlockTexture.Rock;
                case BlockType.Ore:
                    return BlockTexture.Ore;
                case BlockType.Gold:
                    return BlockTexture.Gold;
                case BlockType.Diamond:
                    return BlockTexture.Diamond;
                case BlockType.DirtSign:
                    return BlockTexture.DirtSign;
                case BlockType.Adminblock:
                    return BlockTexture.Adminblock;
                case BlockType.Dirt:
                    return BlockTexture.Dirt;

                case BlockType.Grass:
                    switch (faceDir)
                    {
                        case BlockFaceDirection.YIncreasing: return BlockTexture.Grass;
                        case BlockFaceDirection.ZDecreasing: return BlockTexture.GrassSide;
                        case BlockFaceDirection.ZIncreasing: return BlockTexture.GrassSide;
                        case BlockFaceDirection.XDecreasing: return BlockTexture.GrassSide;
                        case BlockFaceDirection.XIncreasing: return BlockTexture.GrassSide;
                        default: return BlockTexture.Dirt;
                    }

                case BlockType.Leaves:
                    return BlockTexture.Leafs;

                case BlockType.Wood:
                    switch (faceDir)
                    {
                        case BlockFaceDirection.YIncreasing: return BlockTexture.Wood;
                        case BlockFaceDirection.ZDecreasing: return BlockTexture.WoodSide;
                        case BlockFaceDirection.ZIncreasing: return BlockTexture.WoodSide;
                        case BlockFaceDirection.XDecreasing: return BlockTexture.WoodSide;
                        case BlockFaceDirection.XIncreasing: return BlockTexture.WoodSide;
                        default: return BlockTexture.Wood;
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

                case BlockType.BeaconRed:
                case BlockType.BeaconBlue:
                    switch (faceDir)
                    {
                        case BlockFaceDirection.YDecreasing:
                            return BlockTexture.LadderTop;
                        case BlockFaceDirection.YIncreasing:
                            return blockType == BlockType.BeaconRed ? BlockTexture.BeaconRed : BlockTexture.BeaconBlue;
                        case BlockFaceDirection.XDecreasing:
                        case BlockFaceDirection.XIncreasing:
                            return BlockTexture.TeleSideA;
                        case BlockFaceDirection.ZDecreasing:
                        case BlockFaceDirection.ZIncreasing:
                            return BlockTexture.TeleSideB;
                    }
                    break;

                case BlockType.Road:
                    if (faceDir == BlockFaceDirection.YIncreasing)
                        return BlockTexture.RoadTop;
                    else if (faceDir == BlockFaceDirection.YDecreasing||blockAbove!=BlockType.None) //Looks better but won't work with current graphics setup...
                        return BlockTexture.RoadBottom;
                    return BlockTexture.Road;

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

        public static bool IsLightEmittingBlock(BlockType type)
        {
            if (type == BlockType.Lava || type == BlockType.Shock || type==BlockType.Road || type==BlockType.Lamp) return true;
            return false;
        }

        public static bool IsLightTransparentBlock(BlockType type)
        {
            if (type == BlockType.None || type == BlockType.Water) return true;
            return false;
        }
    }
}
