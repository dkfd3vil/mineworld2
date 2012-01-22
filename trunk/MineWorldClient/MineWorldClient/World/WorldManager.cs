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
        float fTime = 0.201358f;

        //Our player for in the world
        Player player;

        //Our other players
        public Dictionary<long, ClientPlayer> playerlist = new Dictionary<long, ClientPlayer>();

        //Our block world
        public Vector3 Mapsize;
        public Chunk[, ,] Chunks;
        public BaseBlock[, ,] BlockMap;
        public BaseBlock[] Blocks;

        //Our sky
        Texture2D Sun;
        VertexPositionTexture[] SunArray;
        Texture2D Moon;
        VertexPositionTexture[] MoonArray;

        //Containing our blocks from terrain.png
        public Texture2D Terrain;

        //Effect ofcourse
        Effect effect;

        //A bool to see if everything is loaded
        bool worldmaploaded = false;

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
            Blocks = new BaseBlock[64];
            BlockMap = new BaseBlock[(int)Mapsize.X, (int)Mapsize.Y, (int)Mapsize.Z];
            CreateBlockTypes();

            //Load our sun and moon
            Moon = conmanager.Load<Texture2D>("Textures/moon");
            Sun = conmanager.Load<Texture2D>("Textures/sun");

            SunArray = new VertexPositionTexture[6];
            SunArray[0] = new VertexPositionTexture(new Vector3(-0.5f, 0, -0.5f), new Vector2(0, 0));
            SunArray[1] = new VertexPositionTexture(new Vector3(0.5f, 0, -0.5f), new Vector2(1, 0));
            SunArray[2] = new VertexPositionTexture(new Vector3(-0.5f, 0, 0.5f), new Vector2(0, 1));
            SunArray[3] = new VertexPositionTexture(new Vector3(0.5f, 0, -0.5f), new Vector2(1, 0));
            SunArray[4] = new VertexPositionTexture(new Vector3(0.5f, 0, 0.5f), new Vector2(1, 1));
            SunArray[5] = new VertexPositionTexture(new Vector3(-0.5f, 0, 0.5f), new Vector2(0, 1));

            MoonArray = new VertexPositionTexture[6];
            MoonArray[0] = new VertexPositionTexture(new Vector3(-0.5f, 0, -0.5f), new Vector2(0, 0));
            MoonArray[1] = new VertexPositionTexture(new Vector3(0.5f, 0, -0.5f), new Vector2(1, 0));
            MoonArray[2] = new VertexPositionTexture(new Vector3(-0.5f, 0, 0.5f), new Vector2(0, 1));
            MoonArray[3] = new VertexPositionTexture(new Vector3(0.5f, 0, -0.5f), new Vector2(1, 0));
            MoonArray[4] = new VertexPositionTexture(new Vector3(0.5f, 0, 0.5f), new Vector2(1, 1));
            MoonArray[5] = new VertexPositionTexture(new Vector3(-0.5f, 0, 0.5f), new Vector2(0, 1));

            //Load our effect
            effect = conmanager.Load<Effect>("Effects/DefaultEffect");

            //Load our terrain
            Terrain = conmanager.Load<Texture2D>("Textures/terrain");

            //Set unchanging effect parameters (Fog and a constant value used for lighting)
            effect.Parameters["FogEnabled"].SetValue(true);
            effect.Parameters["FogStart"].SetValue(128);
            effect.Parameters["FogEnd"].SetValue(160);
            effect.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(Matrix.Identity)));
        }

        public void Start()
        {
            BlockMap = new BaseBlock[(int)Mapsize.X, (int)Mapsize.Y, (int)Mapsize.Z];
            //This is called when we got all the info we need and then we construct a world on it
            //Lets create our map
            CreateSimpleMap();

            //Initialize the chunk array
            Chunks = new Chunk[(int)(Mapsize.X / 16), (int)(Mapsize.Y / 16), (int)(Mapsize.Z / 16)];

            for (int x = 0; x < (int)(Mapsize.X / 16); x++)
            {
                for (int y = 0; y < (int)(Mapsize.Y / 16); y++)
                {
                    for (int z = 0; z < (int)(Mapsize.Z / 16); z++)
                    {
                        Chunks[x, y, z] = new Chunk(new Vector3(x * 16, y * 16, z * 16), this, gamemanager.device); //Create each chunk with its position and pass it the game object
                    }
                }
            }

            //Initial update of the chunks
            UpdateChunks();
            worldmaploaded = true;
        }

        public void Update(GameTime gamefTime,InputHelper input)
        {
            //fTime increases at rate of 1 ingame day/night cycle per 20 minutes (actual value is 0 at dawn, 0.5pi at noon, pi at dusk, 1.5pi at midnight, and 0 or 2pi at dawn again)
            fTime += (float)(Math.PI / 36000);
            fTime %= (float)(MathHelper.TwoPi);

            UpdateChunks();

            if (input.IsNewPress((Keys)ClientKey.WireFrame))
            {
                gamemanager.Pbag.WireMode = !gamemanager.Pbag.WireMode;
            }
        }

        public void Draw()
        {
            //Set the void color and the fog color (based off of the fTime of day)
            gamemanager.device.Clear(Color.SkyBlue);
            effect.Parameters["FogColor"].SetValue(Color.SkyBlue.ToVector4());
            //effect.Parameters["FogColor"].SetValue(Color.Black.ToVector4());

            //Set some draw things
            gamemanager.device.DepthStencilState = DepthStencilState.None;
            gamemanager.device.BlendState = BlendState.AlphaBlend;
            gamemanager.device.SamplerStates[0] = SamplerState.PointWrap;
            gamemanager.device.DepthStencilState = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.GreaterEqual,
                ReferenceStencil = 254,
                DepthBufferEnable = true
            };

            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.CullCounterClockwiseFace;
            if (gamemanager.Pbag.WireMode)
            {
                rs.FillMode = FillMode.WireFrame;
            }
            else
            {
                rs.FillMode = FillMode.Solid;
            }
            gamemanager.device.RasterizerState = rs;

            //CHUNKS
            //Select a rendering technique from the effect file
            effect.CurrentTechnique = effect.Techniques["Technique1"];
            //Set the view and projection matrices, as well as the texture
            effect.Parameters["View"].SetValue(player.Cam.View);
            effect.Parameters["Projection"].SetValue(player.Cam.Projection);
            effect.Parameters["myTexture"].SetValue(Terrain);

            //Set lighting variables based off of fTime of day
            effect.Parameters["DiffuseColor"].SetValue(new Vector4(0.5f, 0.5f, 0.5f, 1) + new Vector4(0.5f, 0.5f, 0.5f, 0) * (float)(MathHelper.Clamp((float)Math.Sin(fTime) * 5, -1, 1)));
            effect.Parameters["Direction"].SetValue(new Vector3((float)(1 + ((fTime + MathHelper.PiOver2) % MathHelper.TwoPi) * (-1 / Math.PI)), (float)-(Math.Abs(1 + ((fTime + MathHelper.PiOver2) % MathHelper.TwoPi) * (-1 / Math.PI)) - 1) / 2, 0f));
            effect.Parameters["AmbientColor"].SetValue(new Vector4(0.25f, 0.25f, 0.25f, 1) + new Vector4(0.2f, 0.2f, 0.2f, 0) * (float)(MathHelper.Clamp((float)Math.Sin(fTime) * 5, -1, 1)));

            foreach (EffectPass pass in effect.CurrentTechnique.Passes) //For each pass in the current technique
            {
                foreach (Chunk curchunk in Chunks) //And for each chunk
                {
                    if (curchunk.bDraw && !curchunk.bEmpty) //If the chunk is loaded and it isn't empty
                    {
                        effect.Parameters["World"].SetValue(Matrix.CreateTranslation(curchunk.vPosition)); //Transform it to a world position
                        pass.Apply();
                        gamemanager.device.SetVertexBuffer(curchunk.buffer); //Load its data from its buffer
                        gamemanager.device.DrawPrimitives(PrimitiveType.TriangleList, 0, curchunk.buffer.VertexCount / 3); //Draw it
                    }
                }
            }

            //Draw the other players
            foreach (ClientPlayer dummy in playerlist.Values)
            {
                dummy.Draw(player.Cam.View, player.Cam.Projection);
            }

            effect.CurrentTechnique = effect.Techniques["Technique2"]; //Switch to technique 2 (gui and skybox)

            //SUN
            //Set the sun texture and world matrix (which transforms its position and angle based off of the fTime of day
            effect.Parameters["myTexture"].SetValue(Sun);
            effect.Parameters["World"].SetValue(Matrix.CreateScale(50) * Matrix.CreateFromYawPitchRoll(0, 0, fTime + MathHelper.PiOver2) * Matrix.CreateTranslation(player.Cam.Position + new Vector3((float)(Math.Cos(fTime) * 192), (float)(Math.Sin(fTime) * 192), 0)));
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gamemanager.device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, SunArray, 0, 2); //Draw it

            }
            //MOON
            //Set the texture, set the world matrix to be the same thing as the sun, but negated
            effect.Parameters["myTexture"].SetValue(Moon);
            effect.Parameters["World"].SetValue(Matrix.CreateScale(50) * Matrix.CreateFromYawPitchRoll(0, 0, (float)((fTime + MathHelper.PiOver2) + Math.PI)) * Matrix.CreateTranslation(player.Cam.Position - new Vector3((float)(Math.Cos(fTime) * 192), (float)(Math.Sin(fTime) * 192), 0)));
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gamemanager.device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, SunArray, 0, 2); //Draw it (the same vertex data can be used because it's still just a square

            }
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
            //This code is for test purpose and will be removed
            for (int x = 0; x < (int)Mapsize.X; x++)
            {
                for (int z = 0; z < (int)Mapsize.Z; z++)
                {
                    for (int y = 0; y < (int)Mapsize.Y; y++)
                    {
                        if (y > (Mapsize.Y / 2))
                        {
                            BlockMap[x, y, z] = Blocks[(int)BlockTypes.Air];
                        }
                        else
                        {
                            BlockMap[x, y, z] = Blocks[(int)BlockTypes.Dirt];
                        }
                    }
                }
            }
        }

        public void UpdateChunks()
        {
            //This method decides which chunks to load or not, based off of distance from player
            Chunk curchunk;
            for (int x = 0; x < (int)(Mapsize.X / 16); x++)
            {
                for (int y = 0; y < (int)(Mapsize.Y / 16); y++)
                {
                    for (int z = 0; z < (int)(Mapsize.Z / 16); z++)
                    {
                        curchunk = Chunks[x, y, z];
                        if (Vector3.Distance(new Vector3(x * 16, y * 16, z * 16), player.Position) < 256) //If the current chunk selected is within the distance
                        {
                            if (!curchunk.bDraw) //And if it isn't already loaded
                            {
                                curchunk.CreateVertices(); //Load it
                                curchunk.bDraw = true;
                            }
                        }
                        else
                        {
                            if (curchunk.bDraw) //Otherwise if it is out of the distance and it IS loaded
                            {
                                curchunk.lVertices = null; //Unload it
                                curchunk.buffer = null;
                                curchunk.bDraw = false;
                            }
                        }
                    }
                }
            }
        }

        public BlockTypes BlockAtPoint(Vector3 pos)
        {
            if (pos.X < 0 || pos.X >= Mapsize.X || pos.Y < 0 || pos.Y >= Mapsize.Y || pos.Z < 0 || pos.Z >= Mapsize.Z)
            {
                return BlockTypes.Air;
            }
            return BlockMap[(int)pos.X, (int)pos.Y, (int)pos.Z].Type;
        }

        // Returns true if we are solid at this point.
        public bool SolidAtPoint(Vector3 point)
        {
            return BlockAtPoint(point) != BlockTypes.Air;
        }

        public bool SolidAtPointForPlayer(Vector3 point)
        {
            return !BlockPassibleForPlayer(BlockAtPoint(point));
        }

        private bool BlockPassibleForPlayer(BlockTypes blockType)
        {
            if (blockType == BlockTypes.Air)
                return true;

            return false;
        }

        public void UseBlock(Vector3 pos)
        {
            if (pos.X < 0 || pos.X >= Mapsize.X || pos.Y < 0 || pos.Y >= Mapsize.Y || pos.Z < 0 || pos.Z >= Mapsize.Z)
            {
                return;
            }
            BlockMap[(int)pos.X, (int)pos.Y, (int)pos.Z].OnUse();
        }

        public bool Everythingloaded()
        {
            return worldmaploaded;
        }

        public void SetBlock(Vector3 pos, BlockTypes type)
        {
            if (pos.X < 0 || pos.X >= Mapsize.X || pos.Y < 0 || pos.Y >= Mapsize.Y || pos.Z < 0 || pos.Z >= Mapsize.Z)
            {
                return;
            }
            if (BlockMap[(int)pos.X, (int)pos.Y, (int)pos.Z].Type != type) //If the target block isn't already what you want it to be
            {
                BlockMap[(int)pos.X, (int)pos.Y, (int)pos.Z] = Blocks[(int)type]; //Then set the block to be that type
                Chunks[(int)Math.Floor((pos.X) / 16), (int)Math.Floor((pos.Y) / 16), (int)Math.Floor((pos.Z) / 16)].CreateVertices(); //Update current chunk
                try //Update surrounding chunks if needed
                {
                    if (Math.Round(pos.X + 1) % 16 == 0)
                    {
                        Chunks[(int)Math.Floor((pos.X) / 16) + 1, (int)Math.Floor((pos.Y) / 16), (int)Math.Floor((pos.Z) / 16)].CreateVertices();
                    }
                    else if (Math.Round(pos.X + 1) % 16 == 1)
                    {
                        Chunks[(int)Math.Floor((pos.X) / 16) - 1, (int)Math.Floor((pos.Y) / 16), (int)Math.Floor((pos.Z) / 16)].CreateVertices();
                    }
                    if (Math.Round(pos.Z + 1) % 16 == 0)
                    {
                        Chunks[(int)Math.Floor((pos.X) / 16), (int)Math.Floor((pos.Y) / 16), (int)Math.Floor((pos.Z) / 16) + 1].CreateVertices();
                    }
                    else if (Math.Round(pos.Z + 1) % 16 == 1)
                    {
                        Chunks[(int)Math.Floor((pos.X) / 16), (int)Math.Floor((pos.Y) / 16), (int)Math.Floor((pos.Z) / 16) - 1].CreateVertices();
                    }
                    if (Math.Round(pos.Y + 1) % 16 == 0)
                    {
                        Chunks[(int)Math.Floor((pos.X) / 16), (int)Math.Floor((pos.Y) / 16) + 1, (int)Math.Floor((pos.Z) / 16)].CreateVertices();
                    }
                    else if (Math.Round(pos.Y + 1) % 16 == 1)
                    {
                        Chunks[(int)Math.Floor((pos.X) / 16), (int)Math.Floor((pos.Y) / 16) - 1, (int)Math.Floor((pos.Z) / 16)].CreateVertices();
                    }
                }
                catch { }
            }
        }
    }
}
