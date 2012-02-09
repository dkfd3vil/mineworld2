using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineWorldData;
using Microsoft.Xna.Framework;

namespace MineWorld
{
    public class BlockRemover : Tool
    {
        public WorldManager worldmanager;
        public Player player;

        public BlockRemover(Player player, WorldManager manager)
        {
            worldmanager = manager;
            this.player = player;
        }

        public override void Use()
        {
            if (player.GotSelection())
            {
                Vector3 block = player.vAimBlock;
                worldmanager.SetBlock((int)block.X,(int)block.Y,(int)block.Z, BlockTypes.Air);
            }
        }
    }
}
