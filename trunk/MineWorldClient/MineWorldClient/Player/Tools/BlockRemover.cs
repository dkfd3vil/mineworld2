using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineWorldData;

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
                worldmanager.SetBlock(player.vAimBlock, BlockTypes.Air);
            }
        }
    }
}
