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

        public void RemoveBlock(ushort x, ushort y, ushort z)
        {
            if (!SaneBlockPosition(x, y, z))
                return;

            blockList[x, y, z] = BlockType.None;

            // x, y, z, type, all bytes
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.BlockSet);
            msgBuffer.Write((byte)x);
            msgBuffer.Write((byte)y);
            msgBuffer.Write((byte)z);
            msgBuffer.Write((byte)BlockType.None);
            foreach (IClient player in playerList.Values)
                //if (netConn.Status == NetConnectionStatus.Connected)
                player.AddQueMsg(msgBuffer, NetChannel.ReliableUnordered);
        }

        public void SetBlock(ushort x, ushort y, ushort z, BlockType blockType)
        {
            Debug.Assert(blockType != BlockType.None, "Setblock used for removal", "Block was sent " + blockType.ToString());

            if(!SaneBlockPosition(x,y,z))
                return;

            blockList[x, y, z] = blockType;

            // x, y, z, type, all bytes
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.BlockSet);
            msgBuffer.Write((byte)x);
            msgBuffer.Write((byte)y);
            msgBuffer.Write((byte)z);
            msgBuffer.Write((byte)blockType);
            foreach (IClient player in playerList.Values)
                //if (netConn.Status == NetConnectionStatus.Connected)
                    player.AddQueMsg(msgBuffer, NetChannel.ReliableUnordered);
        }

        public void GenerateNewMap()
        {
            // Create our block world, translating the coordinates out of the cave generator (where Z points down)
            int templavablockcount = 0;
            int tempwaterblockcount = 0;
            CaveGenerator Cg = new CaveGenerator(Defines.MAPSIZE,Msettings);
            BlockType[, ,] worldData = Cg.GenerateCaveSystem();
            blockList = new BlockType[Defines.MAPSIZE, Defines.MAPSIZE, Defines.MAPSIZE];
            for (ushort i = 0; i < Defines.MAPSIZE; i++)
            {
                for (ushort j = 0; j < Defines.MAPSIZE; j++)
                {
                    for (ushort k = 0; k < Defines.MAPSIZE; k++)
                    {
                        blockList[i, (ushort)(Defines.MAPSIZE - 1 - k), j] = worldData[i, j, k];
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

        public bool InDirectSunLight(ushort i, ushort j , ushort k)
        {
            ushort s;
            j++;
            if ((int)j == Defines.MAPSIZE - 2)
            {
                return true;
            }
            for (s = j; s < Defines.MAPSIZE; s++)
            {
                BlockType blockatloc = blockList[i,s,k];
                if (blockatloc != BlockType.None && blockatloc != BlockType.Leafs)
                {
                    return false;
                }
            }
            return true;
        }

        public Vector3 Auth_Position(Vector3 pos, Player pl)//check boundaries and legality of action
        {
            BlockType type = BlockAtPoint(pos);

            if (BlockInformation.IsPassibleBlock(type))
            {
                return pos;
            }
            else
            {
                if (pl.Alive)
                {
                    ConsoleWrite("Refused " + pl.Name + " " + pos.X + "/" + pos.Y + "/" + pos.Z, ConsoleColor.Yellow);
                    return pl.Position;
                }
                else//player is dead, return position silent
                {
                    return pl.Position;
                }
            }
        }

        public Vector3 Auth_Heading(Vector3 head)//check boundaries and legality of action
        {
            //TODO Code Auth_Heading
            return head;
        }

        public bool SaneBlockPosition(ushort x, ushort y, ushort z)
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
            ushort x = (ushort)point.X;
            ushort y = (ushort)point.Y;
            ushort z = (ushort)point.Z;
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

        public void RemoveBlock(IClient player, Vector3 playerPosition, Vector3 playerHeading)
        {
            Vector3 hitPoint = Vector3.Zero;
            Vector3 buildPoint = Vector3.Zero;
            if (!RayCollision(playerPosition, playerHeading, 2, 10, ref hitPoint, ref buildPoint, BlockType.Water))
                return;

            ushort x = (ushort)hitPoint.X;
            ushort y = (ushort)hitPoint.Y;
            ushort z = (ushort)hitPoint.Z;

            // If it's out of bounds, bail.
            if (!SaneBlockPosition(x, y, z))
                return;

            // Figure out what the result is.
            BlockType type = BlockAtPoint(hitPoint);

            if (BlockInformation.IsDiggable(type))
            {
                RemoveBlock(x, y, z);
                PlaySound(BlockInformation.GetBlockSound(type), player.Position);
            }
        }

        public void PlaceBlock(IClient player, Vector3 playerPosition, Vector3 playerHeading, BlockType blockType)
        {
            // If there's no surface within range, bail.
            Vector3 hitPoint = Vector3.Zero;
            Vector3 buildPoint = Vector3.Zero;
            if (!RayCollision(playerPosition, playerHeading, 6, 25, ref hitPoint, ref buildPoint, BlockType.Water))
                return;

            ushort x = (ushort)buildPoint.X;
            ushort y = (ushort)buildPoint.Y;
            ushort z = (ushort)buildPoint.Z;

            // If there's someone there currently, bail.
            foreach (IClient p in playerList.Values)
            {
                if ((int)p.Position.X == x && (int)p.Position.Z == z && ((int)p.Position.Y == y || (int)p.Position.Y - 1 == y))
                    return;
            }

            // If it's out of bounds, bail.
            if (!SaneBlockPosition(x, y, z))
                return;

            // Build the block.
            SetBlock(x, y, z, blockType);

            // Play the sound.
            PlaySound(MineWorldSound.ConstructionGun, player.Position);
        }

        public Vector3 intifyVector(Vector3 vector)
        {
            Vector3 cleanvector=new Vector3();
            cleanvector.X = (int)vector.X;
            cleanvector.Y = (int)vector.Y;
            cleanvector.Z = (int)vector.Z;
            return cleanvector;
        }
    }
}