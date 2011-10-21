﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;

//Contains functions related to the state of the gameworld

namespace MineWorld
{
    public partial class MineWorldServer
    {
        public BlockType[, ,] blockList = null;    // In game coordinates, where Y points up.
        Random randGen = new Random();

        public void GenerateNewMap()
        {
            // Create our block world, translating the coordinates out of the cave generator (where Z points down)
            int templavablockcount = 0;
            int tempwaterblockcount = 0;
            CaveGenerator Cg = new CaveGenerator(Defines.MAPSIZE,Msettings);
            BlockType[, ,] worldData = Cg.GenerateCaveSystem();
            blockList = new BlockType[Defines.MAPSIZE, Defines.MAPSIZE, Defines.MAPSIZE];
            for (int i = 0; i < Defines.MAPSIZE; i++)
            {
                for (int j = 0; j < Defines.MAPSIZE; j++)
                {
                    for (int k = 0; k < Defines.MAPSIZE; k++)
                    {
                        //blockList[i, k, j] = worldData[i, j, k];
                        blockList[i, (int)(Defines.MAPSIZE - 1 - k), j] = worldData[i, j, k];
                        if (blockList[i, j, k] == BlockType.Lava)
                        {
                            templavablockcount++;
                        }
                        else if (blockList[i, j, k] == BlockType.Water)
                        {
                            tempwaterblockcount++;
                        }
                    }
                }
            }
            Totallavablockcount = templavablockcount;
            Totalwaterblockcount = tempwaterblockcount;
        }

        public void TerminateFinishedThreads()
        {
            List<MapSender> mapSendersToRemove = new List<MapSender>();
            foreach (MapSender ms in mapSendingProgress)
            {
                if (ms.finished)
                {
                    ms.stop();
                    mapSendersToRemove.Add(ms);
                }
            }
            foreach (MapSender ms in mapSendersToRemove)
            {
                mapSendingProgress.Remove(ms);
            }
        }

        public void KillPlayerSpecific(ServerPlayer player)
        {
            // Put variables to zero
            player.Health = 0;
            player.Alive = false;
            ConsoleWrite("PLAYER_DEAD: " + player.Name);

            SendPlayerHealthUpdate(player);
            SendPlayerDead(player);

            luaManager.RaiseEvent("playerondied",player.ID.ToString());
        }

        public void KillAllPlayers()
        {
            foreach (ServerPlayer dummy in playerList.Values)
            {
                KillPlayerSpecific(dummy);
            }
        }

        public double Get3DDistance(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;
            int dz = z2 - z1;
            double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            return distance;
        }


        public double Distf(Vector3 x, Vector3 y)
        {
            float dx = y.X - x.X;
            float dy = y.Y - x.Y;
            float dz = y.Z - x.Z;
            float dist = (float)(Math.Sqrt(dx * dx + dy * dy + dz * dz));

            return dist;
        }

        public bool InDirectSunLight(int i, int j , int k)
        {
            int s;
            j++;
            if ((int)j == Defines.MAPSIZE - 1)
            {
                return true;
            }
            for (s = j; s < Defines.MAPSIZE; s++)
            {
                BlockType blockatloc = blockList[i,s,k];
                if (!BlockInformation.IsLightTransparentBlock(blockatloc))
                {
                    return false;
                }
            }
            return true;
        }

        public Vector3 Auth_Position(Vector3 pos, ServerPlayer player)//check boundaries and legality of action
        {
            BlockType type = BlockAtPoint(pos);

            if (BlockInformation.IsPassibleBlock(type))
            {
                return pos;
            }
            else
            {
                if (player.Alive)
                {
                    ConsoleWrite("REFUSED NEW POSITION OF " + player.Name + " " + pos.X + "/" + pos.Y + "/" + pos.Z, ConsoleColor.Yellow);
                    ConsoleWrite("RETURNED OLD POSTION", ConsoleColor.Yellow);
                    return player.Position;
                }
                else//player is dead, return position silent
                {
                    return player.Position;
                }
            }
        }

        public Vector3 Auth_Heading(Vector3 head, ServerPlayer player)//check boundaries and legality of action
        {
            //TODO Code Auth_Heading
            return head;
        }

        public bool SaneBlockPosition(int x, int y, int z)
        {
            bool goodspot = false;

            if (x <= 0 || y <= 0 || z <= 0 || (int)x >= Defines.MAPSIZE - 1 || (int)y >= Defines.MAPSIZE - 1 || (int)z >= Defines.MAPSIZE - 1)
            {
                goodspot = false;
            }
            else
            {
                goodspot = true;
            }
            return goodspot;
        }

        public BlockType BlockAtPoint(Vector3 point)
        {
            int x = (int)point.X;
            int y = (int)point.Y;
            int z = (int)point.Z;
            if (!SaneBlockPosition(x,y,z))
                return BlockType.None;
            return blockList[x, y, z];
        }

        public bool RayCollision(Vector3 startPosition, Vector3 rayDirection, float distance, int searchGranularity, ref Vector3 hitPoint, ref Vector3 buildPoint)
        {
            Vector3 testPos = startPosition;
            Vector3 buildPos = startPosition;
            for (int i = 0; i < searchGranularity; i++)
            {
                testPos += rayDirection * distance / searchGranularity;
                BlockType testBlock = BlockAtPoint(testPos);
                if (testBlock != BlockType.None)
                {
                    hitPoint = testPos;
                    buildPoint = buildPos;
                    return true;
                }
                buildPos = testPos;
            }
            return false;
        }

        public bool RayCollision(Vector3 startPosition, Vector3 rayDirection, float distance, int searchGranularity, ref Vector3 hitPoint, ref Vector3 buildPoint, BlockType ignore)
        {
            Vector3 testPos = startPosition;
            Vector3 buildPos = startPosition;
            for (int i = 0; i < searchGranularity; i++)
            {
                testPos += rayDirection * distance / searchGranularity;
                BlockType testBlock = BlockAtPoint(testPos);
                if (testBlock != BlockType.None && testBlock != ignore)
                {
                    hitPoint = testPos;
                    buildPoint = buildPos;
                    return true;
                }
                buildPos = testPos;
            }
            return false;
        }

        public Vector3 RayCollisionExact(Vector3 startPosition, Vector3 rayDirection, float distance, int searchGranularity, ref Vector3 hitPoint, ref Vector3 buildPoint)
        {
            Vector3 testPos = startPosition;
            Vector3 buildPos = startPosition;

            for (int i = 0; i < searchGranularity; i++)
            {
                testPos += rayDirection * distance / searchGranularity;
                BlockType testBlock = BlockAtPoint(testPos);
                if (testBlock != BlockType.None)
                {
                    hitPoint = testPos;
                    buildPoint = buildPos;
                    return hitPoint;
                }
                buildPos = testPos;
            }

            return startPosition;
        }

        public void RemoveBlock(ServerPlayer player, Vector3 playerPosition, Vector3 playerHeading)
        {
            Vector3 hitPoint = Vector3.Zero;
            Vector3 buildPoint = Vector3.Zero;
            if (!RayCollision(playerPosition, playerHeading, 2, 10, ref hitPoint, ref buildPoint, BlockType.Water))
                return;

            int x = (int)hitPoint.X;
            int y = (int)hitPoint.Y;
            int z = (int)hitPoint.Z;

            // If it's out of bounds, bail.
            if (!SaneBlockPosition(x, y, z))
                return;

            // Figure out what the result is.
            BlockType type = BlockAtPoint(hitPoint);

            if (BlockInformation.IsDiggable(type))
            {
                SendRemoveBlock(x, y, z);
                SendPlaySound(BlockInformation.GetBlockSound(type), player.Position);
            }
        }

        public void PlaceBlock(ServerPlayer player, Vector3 playerPosition, Vector3 playerHeading, BlockType blockType)
        {
            // If there's no surface within range, bail.
            Vector3 hitPoint = Vector3.Zero;
            Vector3 buildPoint = Vector3.Zero;
            if (!RayCollision(playerPosition, playerHeading, 6, 25, ref hitPoint, ref buildPoint, BlockType.Water))
                return;

            int x = (int)buildPoint.X;
            int y = (int)buildPoint.Y;
            int z = (int)buildPoint.Z;

            // If there's someone there currently, bail.
            foreach (ServerPlayer p in playerList.Values)
            {
                if ((int)p.Position.X == x && (int)p.Position.Z == z && ((int)p.Position.Y == y || (int)p.Position.Y - 1 == y))
                    return;
            }

            // If it's out of bounds, bail.
            if (!SaneBlockPosition(x, y, z))
                return;

            // Build the block.
            SendSetBlock(x, y, z, blockType);

            // Play the sound.
            SendPlaySound(MineWorldSound.ConstructionGun, player.Position);
        }

        public Vector3 intifyVector(Vector3 vector)
        {
            Vector3 cleanvector=new Vector3();
            cleanvector.X = (int)vector.X;
            cleanvector.Y = (int)vector.Y;
            cleanvector.Z = (int)vector.Z;
            return cleanvector;
        }

        public Vector3 GenerateSpawnLocation()
        {
            Vector3 position = new Vector3();
            // Respawn a few blocks above a safe position above altitude 0.
            bool positionFound = false;

            // Try 20 times; use a potentially invalid position if we fail.
            for (int i = 0; i < 20; i++)
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
                        position = startPos + Vector3.UnitY * 5;
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
            {
                position = new Vector3(randGen.Next(2, 62), 66, randGen.Next(2, 62));
            }

            // Drop the player on the middle of the block, not at the corner.
            position += new Vector3(0.5f, 0, 0.5f);

            return position;
        }
    }
}