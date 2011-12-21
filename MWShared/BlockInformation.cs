namespace MineWorld
{
    public enum BlockType : byte
    {
        Air = 0,
        Dirt = 1,
    }

    public enum BlockTexture : byte
    {
        None,
        Dirt,
        Rock,
        Lava,
        RedFlower,
        YellowFlower,
        Adminblock,
        Grass,
        GrassSide,
        WoodSide,
        Wood,
        Leafs,
        Water,
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
                case BlockType.YellowFlower:
                    {
                        return BlockTexture.YellowFlower;
                    }
                case BlockType.RedFlower:
                    {
                        return BlockTexture.RedFlower;
                    }
                case BlockType.Water:
                    {
                        return BlockTexture.Water;
                    }
                case BlockType.Lava:
                    {
                        return BlockTexture.Lava;
                    }
                case BlockType.Rock:
                    {
                        return BlockTexture.Rock;
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
                        case BlockFaceDirection.ZIncreasing:
                        case BlockFaceDirection.XDecreasing:
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
                        case BlockFaceDirection.ZIncreasing:
                        case BlockFaceDirection.XDecreasing:
                        case BlockFaceDirection.XIncreasing:
                            {
                                return BlockTexture.WoodSide;
                            }
                        default:
                            {
                                return BlockTexture.Wood;
                            }
                    }
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
                default:
                    return MineWorldSound.Dig;
            }
        }

        public static bool IsLightEmittingBlock(BlockType type)
        {
            switch (type)
            {
                case BlockType.Lava:
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
                case BlockType.Leafs:
                case BlockType.RedFlower:
                case BlockType.YellowFlower:
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
                case BlockType.Lava:
                case BlockType.YellowFlower:
                case BlockType.RedFlower:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsPlantBlock(BlockTexture text)
        {
            switch (text)
            {
                case BlockTexture.RedFlower:
                case BlockTexture.YellowFlower:
                    return true;
                default:
                    return false;
            }
        }
    }
}