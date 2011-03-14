using System;
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
        PlayerTeam[, ,] blockCreatorTeam = null;
        //const int MAPSIZE = 64;


        List<string> beaconIDList = new List<string>();
        Dictionary<Vector3, Beacon> beaconList = new Dictionary<Vector3, Beacon>();
        Random randGen = new Random();

        public string _GenerateBeaconID()
        {
            string id = "K";
            for (int i = 0; i < 3; i++)
                id += (char)randGen.Next(48, 58);
            return id;
        }

        public string GenerateBeaconID()
        {
            string newId = _GenerateBeaconID();
            while (beaconIDList.Contains(newId))
                newId = _GenerateBeaconID();
            beaconIDList.Add(newId);
            return newId;
        }

        public void RemoveBlock(ushort x, ushort y, ushort z)
        {
            if (!SaneBlockPosition(x, y, z))
                return;

            blockList[x, y, z] = BlockType.None;
            blockCreatorTeam[x, y, z] = PlayerTeam.None;

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

        public void SetBlock(ushort x, ushort y, ushort z, BlockType blockType, PlayerTeam team)
        {
            if(!SaneBlockPosition(x,y,z))
                return;

            if (blockType == BlockType.BeaconRed || blockType == BlockType.BeaconBlue)
            {
                Beacon newBeacon = new Beacon();
                newBeacon.ID = GenerateBeaconID();
                newBeacon.Team = blockType == BlockType.BeaconRed ? PlayerTeam.Red : PlayerTeam.Blue;
                beaconList[new Vector3(x, y, z)] = newBeacon;
                SendSetBeacon(new Vector3(x, y + 1, z), newBeacon.ID, newBeacon.Team);
            }

            if (blockType == BlockType.None && (blockList[x, y, z] == BlockType.BeaconRed || blockList[x, y, z] == BlockType.BeaconBlue))
            {
                if (beaconList.ContainsKey(new Vector3(x, y, z)))
                    beaconList.Remove(new Vector3(x, y, z));
                SendSetBeacon(new Vector3(x, y + 1, z), "", PlayerTeam.None);
            }

            blockList[x, y, z] = blockType;
            blockCreatorTeam[x, y, z] = team;

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

            //if (blockType == BlockType.Lava)
                //lavaBlockCount += 1;
        }

        public void GenerateNewMap()
        {
            // Create our block world, translating the coordinates out of the cave generator (where Z points down)
            int templavablockcount = 0;
            int tempwaterblockcount = 0;
            CaveGenerator Cg = new CaveGenerator(Defines.MAPSIZE,Msettings);
            BlockType[, ,] worldData = Cg.GenerateCaveSystem();
            blockList = new BlockType[Defines.MAPSIZE, Defines.MAPSIZE, Defines.MAPSIZE];
            blockCreatorTeam = new PlayerTeam[Defines.MAPSIZE, Defines.MAPSIZE, Defines.MAPSIZE];
            for (ushort i = 0; i < Defines.MAPSIZE; i++)
            {
                for (ushort j = 0; j < Defines.MAPSIZE; j++)
                {
                    for (ushort k = 0; k < Defines.MAPSIZE; k++)
                    {
                        blockList[i, (ushort)(Defines.MAPSIZE - 1 - k), j] = worldData[i, j, k];
                        blockCreatorTeam[i, j, k] = PlayerTeam.None;
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

        public void DepositForPlayers()
        {
            foreach (IClient p in playerList.Values)
            {
                if (p.Position.Y > Defines.MAPSIZE - Defines.GROUND_LEVEL)
                    DepositCash(p);
            }

            if (teamCashBlue >= Msettings.Winningcashamount && winningTeam == PlayerTeam.None)
                winningTeam = PlayerTeam.Blue;
            if (teamCashRed >= Msettings.Winningcashamount && winningTeam == PlayerTeam.None)
                winningTeam = PlayerTeam.Red;
        }

        public bool InDirectSunLight(ushort i, ushort j , ushort k)
        {
            ushort s;
            j++;
            if ((int)j == Defines.MAPSIZE - 1)
            {
                return true;
            }
            for (s = j; s < Defines.MAPSIZE; s++)
            {
                BlockType blockatloc = blockList[i,s,k];
                if (blockatloc != BlockType.None && blockatloc != BlockType.Leaves)
                {
                    return false;
                }
            }
            return true;
        }

        public Vector3 Auth_Position(Vector3 pos, Player pl)//check boundaries and legality of action
        {
            BlockType testpoint = BlockAtPoint(pos);

            if (testpoint == BlockType.None 
                //|| testpoint == BlockType.Fire 
                //|| testpoint == BlockType.Vacuum 
                || testpoint == BlockType.Water 
                || testpoint == BlockType.Lava 
                //|| testpoint == BlockType.StealthBlockB && pl.Team == PlayerTeam.Blue 
                || testpoint == BlockType.TransBlue && pl.Team == PlayerTeam.Blue 
                //|| testpoint == BlockType.TrapR && pl.Team == PlayerTeam.Blue 
                //|| testpoint == BlockType.TrapB && pl.Team == PlayerTeam.Red 
                //|| testpoint == BlockType.StealthBlockR && pl.Team == PlayerTeam.Red 
                || testpoint == BlockType.TransRed && pl.Team == PlayerTeam.Red)
            {//check if player is not in wall
                //falldamage
                /*
                if (testpoint == BlockType.Fire)
                {
                    //burn
                    if (pl.Health > 1)
                    {
                        pl.Health = pl.Health - 10;
                        if (pl.Health == 0)
                        {
                            pl.Weight = 0;
                            pl.Alive = false;

                            SendResourceUpdate(pl);
                            SendPlayerDead(pl);
                            ConsoleWrite(pl.Handle + " died in the fire.");
                        }
                    }
                }
                 */
            }
            else
            {
                if (pl.Alive)
                {
                    ConsoleWrite("refused " + pl.Handle + " " + pos.X + "/" + pos.Y + "/" + pos.Z);
                    return pl.Position;
                }
                else//player is dead, return position silent
                {
                    return pl.Position;
                }
            }
            return pos;
        }

        public Vector3 Auth_Heading(Vector3 head)//check boundaries and legality of action
        {
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

        public void UsePickaxe(IClient player, Vector3 playerPosition, Vector3 playerHeading)
        {
            Vector3 hitPoint = Vector3.Zero;
            Vector3 buildPoint = Vector3.Zero;
            if (!RayCollision(playerPosition, playerHeading, 2, 10, ref hitPoint, ref buildPoint, BlockType.Water))
                return;

            ushort x = (ushort)hitPoint.X;
            ushort y = (ushort)hitPoint.Y;
            ushort z = (ushort)hitPoint.Z;
            
            player.QueueAnimationBreak = true;

            // Figure out what the result is.
            bool removeBlock = false;
            uint giveOre = 0;
            uint giveCash = 0;
            uint giveWeight = 0;
            MineWorldSound sound = MineWorldSound.DigDirt;

            switch (BlockAtPoint(hitPoint))
            {
                case BlockType.Lava:
                    removeBlock = false;
                    break;

                case BlockType.Wood:
                case BlockType.Leaves:
                case BlockType.Rock:
                case BlockType.Dirt:
                case BlockType.DirtSign:
                    removeBlock = true;
                    sound = MineWorldSound.DigDirt;
                    break;

                case BlockType.Ore:
                    removeBlock = true;
                    giveOre = 20;
                    sound = MineWorldSound.DigMetal;
                    break;

                case BlockType.Gold:
                    removeBlock = true;
                    giveWeight = 1;
                    giveCash = 100;
                    sound = MineWorldSound.DigMetal;
                    break;

                case BlockType.Diamond:
                    removeBlock = true;
                    giveWeight = 1;
                    giveCash = 1000;
                    sound = MineWorldSound.DigMetal;
                    break;
                
                case BlockType.Adminblock:
                    removeBlock = false;
                    break;

                case BlockType.Grass:
                    removeBlock = true;
                    break;
            }

            if (giveOre > 0)
            {
                if (player.Ore < player.OreMax)
                {
                    player.Ore = Math.Min(player.Ore + giveOre, player.OreMax);
                    SendResourceUpdate(player);
                }
            }

            if (giveWeight > 0)
            {
                if (player.Weight < player.WeightMax)
                {
                    player.Weight = Math.Min(player.Weight + giveWeight, player.WeightMax);
                    player.Cash += giveCash;
                    SendResourceUpdate(player);
                }
                else
                    removeBlock = false;
            }

            if (removeBlock)
            {
                RemoveBlock(x, y, z);
                PlaySound(sound, player.Position);
            }
        }

        //private bool LocationNearBase(ushort x, ushort y, ushort z)
        //{
        //    for (int i=0; i<MAPSIZE; i++)
        //        for (int j=0; j<MAPSIZE; j++)
        //            for (int k = 0; k < MAPSIZE; k++)
        //                if (blockList[i, j, k] == BlockType.HomeBlue || blockList[i, j, k] == BlockType.HomeRed)
        //                {
        //                    double dist = Math.Sqrt(Math.Pow(x - i, 2) + Math.Pow(y - j, 2) + Math.Pow(z - k, 2));
        //                    if (dist < 3)
        //                        return true;
        //                }
        //    return false;
        //}

        public void UseConstructionGun(IClient player, Vector3 playerPosition, Vector3 playerHeading, BlockType blockType)
        {
            // If there's no surface within range, bail.
            Vector3 hitPoint = Vector3.Zero;
            Vector3 buildPoint = Vector3.Zero;
            if (!RayCollision(playerPosition, playerHeading, 6, 25, ref hitPoint, ref buildPoint, BlockType.Water))
                return;

            // If there's someone there currently, bail.
            bool actionFailed = false;
            ushort x = (ushort)buildPoint.X;
            ushort y = (ushort)buildPoint.Y;
            ushort z = (ushort)buildPoint.Z;
            foreach (IClient p in playerList.Values)
            {
                if ((int)p.Position.X == x && (int)p.Position.Z == z && ((int)p.Position.Y == y || (int)p.Position.Y - 1 == y))
                    actionFailed = true;
            }

            // If it's out of bounds, bail.
            if (!SaneBlockPosition(x,y,z))
                actionFailed = true;

            // If the player is too poor, bail
            // But if the got the nocost command enabled then build
            uint blockCost = BlockInformation.GetCost(blockType);
            if (player.nocost == true)
            {
                actionFailed = false;
            }
            else
            {
                if (blockCost > player.Ore)
                    actionFailed = true;
            }

            if (actionFailed)
            {
                // Decharge the player's gun.
                TriggerConstructionGunAnimation(player, -0.2f);
            }
            else
            {
                // Fire the player's gun.
                TriggerConstructionGunAnimation(player, 0.5f);

                // Build the block.
                SetBlock(x, y, z, blockType, player.Team);

                // Get the blockCost and substract it from the player
                if (player.nocost == false)
                {
                    player.Ore -= blockCost;
                    SendResourceUpdate(player);
                }

                // Play the sound.
                PlaySound(MineWorldSound.ConstructionGun, player.Position);

                // If it's an explosive block, add it to our list.
                if (blockType == BlockType.Explosive)
                    if (blockList[x + 1, y, z] == BlockType.Lava || blockList[x - 1, y, z] == BlockType.Lava || blockList[x,y,z+1] == BlockType.Lava || blockList[x,y,z-1] == BlockType.Lava || blockList[x,y+1,z] == BlockType.Lava || blockList[x,y-1,z] == BlockType.Lava)
                    {
                        // If you build tnt on lava it explodes on contact ;)
                        DetonateAtPoint(x, y, z);
                    }
                    else
                    {
                        player.ExplosiveList.Add(intifyVector(buildPoint));
                    }
            }
        }

        public Vector3 intifyVector(Vector3 vector){
            Vector3 cleanvector=new Vector3();
            cleanvector.X = (int)vector.X;
            cleanvector.Y = (int)vector.Y;
            cleanvector.Z = (int)vector.Z;
            return cleanvector;
        }

        public void UseDeconstructionGun(IClient player, Vector3 playerPosition, Vector3 playerHeading)
        {
            bool actionFailed = false;

            // If there's no surface within range, bail.
            Vector3 hitPoint = Vector3.Zero;
            Vector3 buildPoint = Vector3.Zero;
            if (!RayCollision(playerPosition, playerHeading, 6, 25, ref hitPoint, ref buildPoint, BlockType.Water))
                actionFailed = true;
            ushort x = (ushort)hitPoint.X;
            ushort y = (ushort)hitPoint.Y;
            ushort z = (ushort)hitPoint.Z;

            // If this is another team's block, bail.
            if (blockCreatorTeam[x, y, z] != player.Team)
                actionFailed = true;

            BlockType blockType = blockList[x, y, z];
            if (!(blockType == BlockType.SolidBlue ||
                blockType == BlockType.SolidRed ||
                blockType == BlockType.BankBlue ||
                blockType == BlockType.BankRed ||
                blockType == BlockType.Jump ||
                blockType == BlockType.Ladder ||
                blockType == BlockType.Road ||
                blockType == BlockType.Water ||
                blockType == BlockType.Shock ||
                blockType == BlockType.BeaconRed ||
                blockType == BlockType.BeaconBlue ||
                blockType == BlockType.TransBlue ||
                blockType == BlockType.TransRed))
                actionFailed = true;

            if (actionFailed)
            {
                // Decharge the player's gun.
                TriggerConstructionGunAnimation(player, -0.2f);
            }
            else
            {
                // Fire the player's gun.
                TriggerConstructionGunAnimation(player, 0.5f);

                // Remove the block.
                RemoveBlock(x, y, z);
                PlaySound(MineWorldSound.ConstructionGun, player.Position);
            }
        }

        public void TriggerConstructionGunAnimation(IClient player, float animationValue)
        {
            if (player.NetConn.Status != NetConnectionStatus.Connected)
                return;

            // ore, cash, weight, max ore, max weight, team ore, red cash, blue cash, all uint
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.TriggerConstructionGunAnimation);
            msgBuffer.Write(animationValue);
            player.AddQueMsg(msgBuffer, NetChannel.ReliableInOrder1);
        }

        public void UseSignPainter(IClient player, Vector3 playerPosition, Vector3 playerHeading)
        {
            // If there's no surface within range, bail.
            Vector3 hitPoint = Vector3.Zero;
            Vector3 buildPoint = Vector3.Zero;
            if (!RayCollision(playerPosition, playerHeading, 4, 25, ref hitPoint, ref buildPoint))
                return;
            ushort x = (ushort)hitPoint.X;
            ushort y = (ushort)hitPoint.Y;
            ushort z = (ushort)hitPoint.Z;

            if (blockList[x, y, z] == BlockType.Dirt)
            {
                SetBlock(x, y, z, BlockType.DirtSign, PlayerTeam.None);
                PlaySound(MineWorldSound.ConstructionGun, player.Position);
            }
            else if (blockList[x, y, z] == BlockType.DirtSign)
            {
                SetBlock(x, y, z, BlockType.Dirt, PlayerTeam.None);
                PlaySound(MineWorldSound.ConstructionGun, player.Position);
            }
        }

        public void ExplosionEffectAtPoint(int x, int y, int z)
        {
            // Send off the explosion to clients.
            NetBuffer msgBuffer = netServer.CreateBuffer();
            msgBuffer.Write((byte)MineWorldMessage.TriggerExplosion);
            msgBuffer.Write(new Vector3(x, y, z));
            foreach (IClient player in playerList.Values)
                //if (netConn.Status == NetConnectionStatus.Connected)
                player.AddQueMsg(msgBuffer,  NetChannel.ReliableUnordered);
            //Or not, there's no dedicated function for this effect >:(
        }

        public void DetonateAtPoint(int x, int y, int z)
        {
            // Remove the block that is detonating.
            RemoveBlock((ushort)x, (ushort)y, (ushort)z);

            // Remove this from any explosive lists it may be in.
            foreach (IClient p in playerList.Values)
            {
                p.ExplosiveList.Remove(new Vector3(x, y, z));
            }

            // Detonate the block.
                for (int dx = -4; dx <= 4; dx++)
                {
                    for (int dy = -4; dy <= 4; dy++)
                    {
                        for (int dz = -4; dz <= 4; dz++)
                        {
                        // Check if it's inside the sphere
                        if (Get3DDistance(dx + x, dy + y, dz + z, x, y, z) < 4)
                        {
                            if (x + dx <= 0 || y + dy <= 0 || z + dz <= 0 || x + dx >= Defines.MAPSIZE - 1 || y + dy >= Defines.MAPSIZE - 1 || z + dz >= Defines.MAPSIZE - 1)
                            {
                                break;
                            }
							// Chain reactions!
							if (blockList[x + dx, y + dy, z + dz] == BlockType.Explosive)
							{
								DetonateAtPoint(x + dx, y + dy, z + dz);
							}

                            // Detonation of normal blocks.
                            bool destroyBlock = true;
                            switch (blockList[x + dx, y + dy, z + dz])
                            {
                                case BlockType.Adminblock:
                                case BlockType.Metal:
                                    {
                                        destroyBlock = false;
                                        break;
                                    }
                                
                            }
                            if (destroyBlock)
                            {
                                RemoveBlock((ushort)(x + dx), (ushort)(y + dy), (ushort)(z + dz));
                            }
                        }
                    }
                }
            }
            ExplosionEffectAtPoint(x, y, z);
        }

        public void UseDetonator(IClient player)
        {
            while (player.ExplosiveList.Count > 0)
            {
                Vector3 blockPos = player.ExplosiveList[0];

                ushort x = (ushort)blockPos.X;
                ushort y = (ushort)blockPos.Y;
                ushort z = (ushort)blockPos.Z;

                player.ExplosiveList.RemoveAt(0);

                DetonateAtPoint(x, y, z);
            }
        }

        public void DepositOre(IClient player)
        {
            uint depositAmount = Math.Min(50, player.Ore);
            player.Ore -= depositAmount;
            if (player.Team == PlayerTeam.Red)
                teamOreRed = Math.Min(teamOreRed + depositAmount, 9999);
            else
                teamOreBlue = Math.Min(teamOreBlue + depositAmount, 9999);
        }

        public void WithdrawOre(IClient player)
        {
            if (player.Team == PlayerTeam.Red)
            {
                uint withdrawAmount = Math.Min(player.OreMax - player.Ore, Math.Min(50, teamOreRed));
                player.Ore += withdrawAmount;
                teamOreRed -= withdrawAmount;
            }
            else
            {
                uint withdrawAmount = Math.Min(player.OreMax - player.Ore, Math.Min(50, teamOreBlue));
                player.Ore += withdrawAmount;
                teamOreBlue -= withdrawAmount;
            }
        }

        public void DepositCash(IClient player)
        {
            if (player.Cash <= 0)
                return;

            player.Score += player.Cash;
                if (player.Team == PlayerTeam.Red)
                    teamCashRed += player.Cash;
                else
                    teamCashBlue += player.Cash;
                SendServerMessage("SERVER: " + player.Handle + " HAS EARNED $" + player.Cash + " FOR THE "+ " TEAM!");

            PlaySound(MineWorldSound.CashDeposit, player.Position);
            ConsoleWrite("DEPOSIT_CASH: " + player.Handle + ", " + player.Cash);

            player.Cash = 0;
            player.Weight = 0;

            foreach (IClient p in playerList.Values)
                SendResourceUpdate(p);
        }
    }
}