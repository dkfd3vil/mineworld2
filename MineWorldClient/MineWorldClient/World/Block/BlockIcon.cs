using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineWorldData;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MineWorld.World.Block
{
    public class BlockIcon
    {
        public Texture2D Terrain;
        public BlockTypes Type;
        public Texture2D Icon;
        public Vector2 uvMap;

        public BlockIcon(Texture2D ter, Vector2 pos, BlockTypes type)
        {
            Terrain = ter;
            uvMap = pos;
            Type = type;
            //icon = new Texture2D
        }

        public Texture2D GetIcon()
        {
            return Icon;
        }
    }
}
