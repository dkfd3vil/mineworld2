using System;
using Microsoft.Xna.Framework;

//Contains functions related to the state of the gameworld

namespace MineWorld
{
    public partial class MineWorldServer
    {
        private readonly Random _randGen = new Random();
        public BlockType[,,] BlockList; // In game coordinates, where Y points up.

        public void GenerateNewMap()
        {
            // Create our block world, translating the coordinates out of the cave generator (where Y points down)
            MapGenerator generator = new MapGenerator(Msettings.Mapseed, Msettings.MapsizeX, Msettings.MapsizeY,Msettings.MapsizeZ);
            generator.drawCube(0, 0, 0, Msettings.MapsizeX, Msettings.MapsizeY /2, Msettings.MapsizeZ,BlockType.Dirt);
            BlockList = generator.mapData;
            //blockList = Cg.GenerateCaveSystem();
            //BlockList = cg.GenerateSimpleCube();
        }

        public void KillPlayerSpecific(ServerPlayer player)
        {
            // Put variables to zero
            player.Health = 0;
            player.Alive = false;
            ConsoleWrite("PLAYER_DEAD: " + player.Name);

            SendPlayerHealthUpdate(player);
            SendPlayerDead(player);

            LuaManager.RaiseEvent("playerondied", player.ID.ToString());
        }

        public void KillAllPlayers()
        {
            foreach (ServerPlayer dummy in PlayerList.Values)
            {
                KillPlayerSpecific(dummy);
            }
        }

        public double Get3DDistance(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;
            int dz = z2 - z1;
            double distance = Math.Sqrt(dx*dx + dy*dy + dz*dz);

            return distance;
        }


        public double Distf(Vector3 x, Vector3 y)
        {
            float dx = y.X - x.X;
            float dy = y.Y - x.Y;
            float dz = y.Z - x.Z;
            float dist = (float) (Math.Sqrt(dx*dx + dy*dy + dz*dz));

            return dist;
        }

        public bool InDirectSunLight(int i, int j, int k)
        {
            int s;
            j++;
            if (j == Msettings.MapsizeY - 1)
            {
                return true;
            }
            for (s = j; s < Msettings.MapsizeY; s++)
            {
                BlockType blockatloc = BlockList[i, s, k];
                if (!BlockInformation.IsLightTransparentBlock(blockatloc))
                {
                    return false;
                }
            }
            ConsoleWrite("DEBUG FOUND BLOCK IN SUN");
            return true;
        }

        public Vector3 AuthPosition(Vector3 pos, ServerPlayer player) //check boundaries and legality of action
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
                    ConsoleWrite("REFUSED NEW POSITION OF " + player.Name + " " + pos.X + "/" + pos.Y + "/" + pos.Z,
                                 ConsoleColor.Yellow);
                    ConsoleWrite("RETURNED OLD POSTION", ConsoleColor.Yellow);
                    return player.Position;
                }
                else //player is dead, return position silent
                {
                    return player.Position;
                }
            }
        }

        public Vector3 AuthHeading(Vector3 head, ServerPlayer player) //check boundaries and legality of action
        {
            //TODO Code Auth_Heading
            return head;
        }

        public bool SaneBlockPosition(int x, int y, int z)
        {
            bool goodspot;

            if (x <= 0 || y <= 0 || z <= 0 || x >= Msettings.MapsizeX - 1 || y >= Msettings.MapsizeY - 1 ||
                z >= Msettings.MapsizeZ - 1)
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
            int x = (int) point.X;
            int y = (int) point.Y;
            int z = (int) point.Z;
            if (!SaneBlockPosition(x, y, z))
                return BlockType.None;
            return BlockList[x, y, z];
        }

        public bool RayCollision(Vector3 startPosition, Vector3 rayDirection, float distance, int searchGranularity,
                                 ref Vector3 hitPoint, ref Vector3 buildPoint)
        {
            Vector3 testPos = startPosition;
            Vector3 buildPos = startPosition;
            for (int i = 0; i < searchGranularity; i++)
            {
                testPos += rayDirection*distance/searchGranularity;
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

        public bool RayCollision(Vector3 startPosition, Vector3 rayDirection, float distance, int searchGranularity,
                                 ref Vector3 hitPoint, ref Vector3 buildPoint, BlockType ignore)
        {
            Vector3 testPos = startPosition;
            Vector3 buildPos = startPosition;
            for (int i = 0; i < searchGranularity; i++)
            {
                testPos += rayDirection*distance/searchGranularity;
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

        public Vector3 RayCollisionExact(Vector3 startPosition, Vector3 rayDirection, float distance,
                                         int searchGranularity, ref Vector3 hitPoint, ref Vector3 buildPoint)
        {
            Vector3 testPos = startPosition;
            Vector3 buildPos = startPosition;

            for (int i = 0; i < searchGranularity; i++)
            {
                testPos += rayDirection*distance/searchGranularity;
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

            int x = (int) hitPoint.X;
            int y = (int) hitPoint.Y;
            int z = (int) hitPoint.Z;

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

            int x = (int) buildPoint.X;
            int y = (int) buildPoint.Y;
            int z = (int) buildPoint.Z;

            // If there's someone there currently, bail.
            foreach (ServerPlayer p in PlayerList.Values)
            {
                if ((int) p.Position.X == x && (int) p.Position.Z == z &&
                    ((int) p.Position.Y == y || (int) p.Position.Y - 1 == y))
                    return;
            }

            // If it's out of bounds, bail.
            if (!SaneBlockPosition(x, y, z))
                return;

            // Build the block.
            SendSetBlock(x, y, z, blockType);

            // Play the sound.
            SendPlaySound(MineWorldSound.Build, player.Position);
        }

        public Vector3 IntifyVector(Vector3 vector)
        {
            Vector3 cleanvector = new Vector3 {X = (int) vector.X, Y = (int) vector.Y, Z = (int) vector.Z};
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
                Vector3 startPos = new Vector3(_randGen.Next(1, Msettings.MapsizeX), Msettings.MapsizeY,
                                               _randGen.Next(1, Msettings.MapsizeZ));

                // See if this is a safe place to drop.
                for (startPos.Y = Msettings.MapsizeY - 1; startPos.Y >= Msettings.MapsizeY/2; startPos.Y--)
                {
                    BlockType blockType = BlockAtPoint(startPos);
                    switch (blockType)
                    {
                        case BlockType.Lava:
                            break;
                        default:
                            if (blockType != BlockType.None)
                            {
                                // We have found a valid place to spawn, so spawn a few above it.
                                position = startPos + Vector3.UnitY*5;
                                positionFound = true;
                                break;
                            }
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
                position = new Vector3(_randGen.Next(1, Msettings.MapsizeX), Msettings.MapsizeY,
                                       _randGen.Next(1, Msettings.MapsizeZ));
            }

            // Drop the player on the middle of the block, not at the corner.
            position += new Vector3(0.5f, 0, 0.5f);

            return position;
        }
    }
}