using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineWorldData;
using System.IO;
using Microsoft.Xna.Framework;

namespace MineWorldServer
{
    public class GameWorld
    {
        MineWorldServer mineserver;

        public GameWorld(MineWorldServer mines)
        {
            mineserver = mines;
        }

        public void GenerateSpawnPosition(ServerPlayer player)
        {
            player.Position = mineserver.MapManager.GenerateSpawnPosition();
        }
    }
}
