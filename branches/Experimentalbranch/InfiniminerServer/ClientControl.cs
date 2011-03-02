//Todo Implent Server side handeling of client
/*
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;

//Contains functions related to the state of the players in the gameworld

namespace MineWorld
{
    public partial class MineWorldServer
    {
        public void KillPlayer(IClient p,string deathMessage)
        {
            if (p.NetConn.Status != NetConnectionStatus.Connected)
                return;

            //PlaySound(MineWorldSound.Death);
            //playerPosition = new Vector3(randGen.Next(2, 62), 66, randGen.Next(2, 62));
            //playerVelocity = Vector3.Zero;
            //playerDead = true;
            //screenEffect = ScreenEffect.Death;
            //screenEffectCounter = 0;
            //netClient.SendMessage(msgBuffer, NetChannel.ReliableUnordered);
            p.Alive = false;
            p.Position = new Vector3(0,0,0);
            p.Velocity = Vector3.Zero;

            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.KillPlayer);
            msgBuffer.Write(deathMessage);
            p.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder4);
        }

        public void RespawnPlayer(IClient p)
        {
            if (p.NetConn.Status != NetConnectionStatus.Connected)
                return;

            // Respawn a few blocks above a safe position above altitude 0.
            bool positionFound = false;

            // Try 100 times; use a potentially invalid position if we fail.
            for (int i = 0; i < 100; i++)
            {
                // Pick a random starting point.
                Vector3 startPos = new Vector3(randGen.Next(2, 62), 63, randGen.Next(2, 62));

                // See if this is a safe place to drop.
                for (startPos.Y = 63; startPos.Y >= 54; startPos.Y--)
                {
                    BlockType blockType = BlockAtPoint(startPos);
                    if (blockType == BlockType.Lava)
                        break;
                    else if (blockType != BlockType.None)
                    {
                        // We have found a valid place to spawn, so spawn a few above it.
                        p.Position = startPos + Vector3.UnitY * 5;
                        positionFound = true;
                        break;
                    }
                }

                // If we found a position, no need to try anymore!
                if (positionFound)
                    break;
            }

            // If we failed to find a spawn point, drop randomly.
            if (!positionFound)
                p.Position = new Vector3(randGen.Next(2, 62), 66, randGen.Next(2, 62));

            // Drop the player on the middle of the block, not at the corner.
            p.Position += new Vector3(0.5f, 0, 0.5f);

            // Zero out velocity and reset camera and screen effects.
            p.Velocity = Vector3.Zero;
            p.Alive = true;
            //screenEffect = ScreenEffect.None;
            //screenEffectCounter = 0;
            //UpdateCamera();

            // Tell the server we have respawned.
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.RespawnPlayer);
            msgBuffer.Write(p.Position);
            p.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder4);
            //foreach ()
        }
    }
}
*/