using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MineWorldData;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace MineWorld
{
    public class WorldManager
    {
        //Our manager
        GameStateManager gamemanager;

        //Our fTime
        public float fTime = 0.201358f;

        //Our player for in the world
        public Player player;

        //Our other players
        public Dictionary<int, ClientPlayer> playerlist = new Dictionary<int, ClientPlayer>();

        //Our block world
        public int Mapsize;
        public Chunk[,] Chunks;
        public int ChunkX;
        public int ChunkZ;

        public BaseBlock[] Blocks;

        //Our sky
        Sun Sun = new Sun();
        Moon Moon = new Moon();

        //Our rasterstates
        RasterizerState Wired;
        RasterizerState Solid;

        //A bool to see if everything is loaded
        public bool worldmaploaded = false;

        //Custom texture path
        public string customtexturepath = "";

        //A bool for debug purpose
        public bool Debug = false;

        public WorldManager(GameStateManager manager,Player player)
        {
            this.gamemanager = manager;
            this.player = player;
        }

        public void Load(ContentManager conmanager)
        {
            //Load our blocks
            Blocks = new BaseBlock[(int)BlockTypes.MAX];
            CreateBlockTypes();

            //Load our sun and moon
            Sun.Load(conmanager);
            Moon.Load(conmanager);

            //Setup our rasterstates
            Wired = new RasterizerState();
            Wired.CullMode = CullMode.CullCounterClockwiseFace;
            Wired.FillMode = FillMode.WireFrame;
            Solid = new RasterizerState();
            Solid.CullMode = CullMode.CullCounterClockwiseFace;
            Solid.FillMode = FillMode.Solid;
        }

        public void Start()
        {
            //This is called when we got all the info we need and then we construct a world on it
            //Initialize the chunk array
            ChunkX = (int)Mapsize / Chunk.Size;
            ChunkZ = (int)Mapsize / Chunk.Size;

            Chunks = new Chunk[ChunkX, ChunkZ];

            for (int x = 0; x < ChunkX; x++)
            {
                for (int z = 0; z < ChunkZ; z++)
                {
                    Chunks[x, z] = new Chunk(x * Chunk.Size, z * Chunk.Size, this, gamemanager); //Create each chunk with its position and pass it the game object
                }
            }

            //Lets create our map
            CreateSimpleMap();

            worldmaploaded = true;
        }

        public void Update(GameTime gamefTime,InputHelper input)
        {
            //fTime increases at rate of 1 ingame day/night cycle per 20 minutes (actual value is 0 at dawn, 0.5pi at noon, pi at dusk, 1.5pi at midnight, and 0 or 2pi at dawn again)
            fTime += (float)(Math.PI / 36000);
            fTime %= (float)(MathHelper.TwoPi);

            foreach (Chunk c in Chunks)
            {
                c.Update(player.Position,fTime);
            }

            //Update our moon and sun for the correct offset
            Sun.Update(fTime, gamemanager.Pbag.Player.Position);
            Moon.Update(fTime, gamemanager.Pbag.Player.Position);

            if (gamemanager.game.IsActive)
            {
                if (input.IsNewPress((Keys)ClientKey.WireFrame))
                {
                    gamemanager.Pbag.WireMode = !gamemanager.Pbag.WireMode;
                }
            }
        }

        public void Draw(GameTime gameTime, GraphicsDevice gDevice, SpriteBatch sBatch)
        {
            gDevice.Clear(Color.SkyBlue);

            //Set some draw things
            gDevice.DepthStencilState = DepthStencilState.None;
            gDevice.BlendState = BlendState.AlphaBlend;
            gDevice.SamplerStates[0] = SamplerState.PointWrap;
            gDevice.DepthStencilState = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.GreaterEqual,
                ReferenceStencil = 254,
                DepthBufferEnable = true
            };

            if (gamemanager.Pbag.WireMode)
            {
                gDevice.RasterizerState = Wired;
            }
            else
            {
                gDevice.RasterizerState = Solid;
            }


            foreach (Chunk c in Chunks)
            {
                c.Draw(gDevice);
            }


            //After the blocks make sure fillmode = solid once again
            gDevice.RasterizerState = Solid;

            //Draw the other players
            foreach (ClientPlayer dummy in playerlist.Values)
            {
                dummy.Draw(player.Cam.View, player.Cam.Projection);
            }

            //Draw our beautifull sky
            Sun.Draw(gDevice);
            Moon.Draw(gDevice);
        }

        public void CreateBlockTypes()
        {
            //0 - Air
            Blocks[(int)BlockTypes.Air] = new BaseBlock(new Vector2(),BlockModel.Cube,false,true,false,BlockTypes.Air);
            //1 - Stone
            Blocks[(int)BlockTypes.Stone] = new BaseBlock(new Vector2(1, 0), BlockModel.Cube, true, false, true, BlockTypes.Stone);
            //2 - Grass
            Blocks[(int)BlockTypes.Grass] = new BaseBlock(new Vector2(0, 0), BlockModel.Cube, true, false, true, BlockTypes.Grass);
            //3 - Dirt
            Blocks[(int)BlockTypes.Dirt] = new BaseBlock(new Vector2(2, 0), BlockModel.Cube, true, false, true, BlockTypes.Dirt);
            //8 - Water
            Blocks[(int)BlockTypes.Water] = new BaseBlock(new Vector2(13, 12), BlockModel.Cube, false, true, false, BlockTypes.Water);
            //20 - Glass
            Blocks[(int)BlockTypes.Glass] = new BaseBlock(new Vector2(1, 3), BlockModel.Cube, true, true, true, BlockTypes.Glass);
            //25 - Noteblock
            Blocks[(int)BlockTypes.Noteblock] = new MusicBlock(new Vector2(10, 4), new Vector2(11, 4), new Vector2(11, 4), new Vector2(11, 4), new Vector2(11, 4), new Vector2(11, 4), BlockModel.Cube, true, false, true, BlockTypes.Noteblock, gamemanager.conmanager);
        }

        public void CreateSimpleMap()
        {
            //Empty the map first
            EmptyMap();
            //This code is for test purpose and will be removed
            for (int x = 0; x < Mapsize; x++)
            {
                for (int z = 0; z < Mapsize; z++)
                {
                    for (int y = 0; y < Chunk.Height; y++)
                    {
                        if (y < (Chunk.Height / 2))
                        {
                            SetMapBlock(x, y, z, BlockTypes.Dirt);
                        }
                    }
                }
            }
        }

        public void EmptyMap()
        {
            //This code is for test purpose and will be removed
            for (int x = 0; x < Mapsize; x++)
            {
                for (int z = 0; z < Mapsize; z++)
                {
                    for (int y = 0; y < Chunk.Height; y++)
                    {
                        SetMapBlock(x, y, z, BlockTypes.Air);
                    }
                }
            }
        }

        public Chunk GetChunkAtPosition(int x, int z)
        {
            Chunk c = Chunks[x / Chunk.Size, z / Chunk.Size];

            return c;
        }

        public void SetBlock(int x,int y, int z, BlockTypes type)
        {
            ////If its outside the map then ignore it
            if (x < 0 || x >= Mapsize || y < 0 || y >= Chunk.Height || z < 0 || z >= Mapsize)
            {
                return;
            }

            Chunk c = GetChunkAtPosition(x, z);

            int xb,yb,zb;
            xb = (x % Chunk.Size);
            yb = y;
            zb = (z % Chunk.Size);
            c.SetBlock(xb, yb, zb, Blocks[(int)type],true);
        }

        public void SetMapBlock(int x, int y, int z, BlockTypes type)
        {
            if (PointWithinMap(new Vector3(x, y, z)))
            {
                Chunk c = GetChunkAtPosition(x, z);

                int xb, yb, zb;
                xb = (x % Chunk.Size);
                yb = y;
                zb = (z % Chunk.Size);
                c.SetBlock(xb, yb, zb, Blocks[(int)type], false);
            }
        }

        public BaseBlock BlockAtPoint(Vector3 pos)
        {
            if (PointWithinMap(pos))
            {
                Chunk c = GetChunkAtPosition((int)pos.X, (int)pos.Z);

                int xb, yb, zb;
                xb = ((int)pos.X % Chunk.Size);
                yb = (int)pos.Y;
                zb = ((int)pos.Z % Chunk.Size);
                return c.GetBlockAtPoint(xb, yb, zb);
            }
            else
            {
                return Blocks[(int)(BlockTypes.Air)];
            }
        }

        public bool PointWithinMap(Vector3 pos)
        {
            ////If its outside the map then ignore it
            if (pos.X < 0 || pos.X >= Mapsize || pos.Y < 0 || pos.Y >= Chunk.Height || pos.Z < 0 || pos.Z >= Mapsize)
            {
                return false;
            }

            return true;
        }

        // Returns true if we are solid at this point.
        public bool SolidAtPoint(Vector3 point)
        {
            return BlockAtPoint(point).Type != BlockTypes.Air;
        }

        public bool SolidAtPointForPlayer(Vector3 point)
        {
            return !BlockPassibleForPlayer(BlockAtPoint(point));
        }

        private bool BlockPassibleForPlayer(BaseBlock block)
        {
            if (block.Type == BlockTypes.Air)
            {
                return true;
            }

            return false;
        }

        public bool Everythingloaded()
        {
            return worldmaploaded;
        }
    }
}
