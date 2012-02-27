using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineWorldData;
using Microsoft.Xna.Framework;

namespace MineWorld
{
    //This tool is just a test tool to show the possibilies
    public class Stacker : Tool
    {
        public WorldManager worldmanager;
        public Player player;

        public Stacker(Player player, WorldManager manager)
        {
            worldmanager = manager;
            this.player = player;
        }

        public override void Use()
        {
            if (player.GotSelection())
            {
                Vector3 temppos = player.GetFacingBlock();
                for (int height = (int)temppos.Y; height < Chunk.Height; height++)
                {
                    Vector3 pos = new Vector3(temppos.X,height,temppos.Z);
                    worldmanager.SetBlock((int)pos.X,(int)pos.Y,(int)pos.Z, player.selectedblocktype);
                }
            }
        }
    }
}