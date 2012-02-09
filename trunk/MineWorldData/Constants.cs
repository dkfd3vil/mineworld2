using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace MineWorldData
{
    public class Constants
    {
        public const int LINDGREN_VERSION = 275;
        public const int EASYCONFIG_VERSION = 61482;
        public const int MINEWORLDCLIENT_VERSION = 0;
        public const int MINEWORLDSERVER_VERSION = 0;
        public const int MINEWORLDDATA_VERSION = 0;
        public const int MINEWORLD_PORT = 5565;
    }

    public enum PacketType
    {
        PlayerLeft,
        PlayerJoined,
        WorldMapSize,
        PlayerInWorld,
        PlayerBlockSet,
        PlayerBlockUse,
        PlayerInitialUpdate,
        PlayerMovementUpdate,
    }

    public enum BlockTypes
    {
        Air = 0,
        Stone = 1,
        Grass = 2,
        Dirt = 3,
        Cobblestone = 4,
        Woodenplanks = 5,
        Saplings = 6,
        Bedrock = 7,
        Water = 8,
        Water2 = 9,
        Lava = 10,
        Lava2 = 11,
        Sand = 12,
        Gravel = 13,
        Goldore = 14,
        Ironore = 15,
        Coalore = 16,
        Wood = 17,
        Leaves = 18,
        Sponge = 19,
        Glass = 20,
        Lapisore = 21,
        Lapisblock = 22,
        Dispenser = 23,
        Sandstone = 24,
        Noteblock = 25,
        Bed = 26,
        Powererdrail = 27,
        Detectorrail = 28,
        Stickypiston = 29,
        Cobweb = 30,
        Tallgrass = 31,
        Deadbush = 32,
        Piston = 33,
        Pistonextension = 34,
        Wool = 35,
        unused1 = 36, //Block moved by piston
        Floweryellow = 37,
        Flowerred = 38,
        Brownmushroom = 39,
        Redmushroom = 40,
        Blockofgold = 41,
        Blockofiron = 42,
        Doubleslabs = 43,
        Slabs = 44,
        Bricks = 45,
        Tnt = 46,
        Bookshelf = 47,
        Mossstone = 48,
        Obsidian = 49,
        Torch = 50,
        Fire = 51,
        Monsterspawner = 52,
        Woodenstairs = 53,
        Chest = 54,
        Redstonewire = 55,
        Diamondore = 56,
        Blockofdiamond = 57,
        Craftingtable = 58,
        Wheatseeds = 59,
        unused2 = 60, //Dirt changes for seeds
        Furnace = 61,
        Burningfurnace = 62,
        Signpost = 63,
        MAX = 64,
    }
}
