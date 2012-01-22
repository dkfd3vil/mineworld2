using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineWorldData;

namespace MineWorld
{
    public class BlockAdder : Tool
    {
        public WorldManager worldmanager;
        public Player player;

        public BlockAdder(Player player,WorldManager manager)
        {
            worldmanager = manager;
            this.player = player;
        }

        public override void Use()
        {
            if (player.GotSelection())
            {
                worldmanager.SetBlock(player.GetFacingBlock(), BlockTypes.Dirt);
            }
        }
    }
}
