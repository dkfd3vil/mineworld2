using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.IO;

namespace MineWorld
{
    [Serializable]
    public struct VertexPositionTextureShade
    {
        Vector3 pos;
        Vector2 tex;
        float shade;

        public static readonly VertexElement[] VertexElements = new VertexElement[]
        { 
            new VertexElement(0,0,VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
            new VertexElement(0,sizeof(float)*3,VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(0,sizeof(float)*5,VertexElementFormat.Single, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 1)               
        };

        public VertexPositionTextureShade(Vector3 position, Vector2 uv, double shade)
        {
            pos = position;
            tex = uv;
            this.shade = (float)shade;
        }

        public Vector3 Position { get { return pos; } set { pos = value; } }
        public Vector2 Tex { get { return tex; } set { tex = value; } }
        public float Shade { get { return shade; } set { shade = value; } }
        public static int SizeInBytes { get { return sizeof(float) * 6; } }
    }

    public class IMTexture
    {
        public Texture2D Texture = null;
        public Color LODColor = Color.Black;

        public IMTexture(Texture2D texture)
        {
            Texture = texture;
            LODColor = Color.Black;

            // If this is a null texture, use a black LOD color.
            if (Texture == null)
                return;

            // Calculate the load color dynamically.
            float r = 0, g = 0, b = 0;
            Color[] pixelData = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(pixelData);
            for (int i = 0; i < texture.Width; i++)
                for (int j = 0; j < texture.Height; j++)
                {
                    r += pixelData[i + j * texture.Width].R;
                    g += pixelData[i + j * texture.Width].G;
                    b += pixelData[i + j * texture.Width].B;
                }
            r /= texture.Width * texture.Height;
            g /= texture.Width * texture.Height;
            b /= texture.Width * texture.Height;
            LODColor = new Color(r / 256, g / 256, b / 256);
        }
    }

    public class BlockEngine
    {
        public BlockType[, ,] blockList = null;
        public BlockType[, ,] downloadList = null;
        Dictionary<int,bool>[,] faceMap = null;
        BlockTexture[,] blockTextureMap = null;
        public IMTexture[] blockTextures = null;
        Effect basicEffect;
        MineWorldGame gameInstance;
        DynamicVertexBuffer[,] vertexBuffers = null;
        bool[,] vertexListDirty = null;
        VertexDeclaration vertexDeclaration;
        BloomComponent bloomPosteffect;
        public Lighting lightingEngine = null;

        public void MakeRegionDirty(int texture, int region)
        {
            vertexListDirty[texture, region] = true;
        }

        public void MakeAllRegionsDirty()
        {
            for (int i = 0; i < (byte)BlockTexture.MAXIMUM; i++)
                for (int j = 0; j < NUMREGIONS; j++)
                    vertexListDirty[i, j] = true;
        }

        const int REGIONSIZE = 16;
        const int REGIONRATIO = Defines.MAPSIZE / REGIONSIZE;
        const int NUMREGIONS = REGIONRATIO * REGIONRATIO * REGIONRATIO;

        public void DownloadComplete()
        {
            lightingEngine.Initialize(downloadList);

            for (int i = 0; i < Defines.MAPSIZE; i++)
                for (int j = 0; j < Defines.MAPSIZE; j++)
                    for (int k = 0; k < Defines.MAPSIZE; k++)
                        if (downloadList[i, j, k] != BlockType.None)
                            AddBlock(i, j, k, downloadList[i, j, k]);
        }

        public BlockEngine(MineWorldGame gameInstance)
        {
            this.gameInstance = gameInstance;

            //Create our lightingengine
            lightingEngine = new Lighting();

            // Initialize the block list.
            downloadList = new BlockType[Defines.MAPSIZE, Defines.MAPSIZE, Defines.MAPSIZE];
            blockList = new BlockType[Defines.MAPSIZE, Defines.MAPSIZE, Defines.MAPSIZE];
            for (int i = 0; i < Defines.MAPSIZE; i++)
                for (int j = 0; j < Defines.MAPSIZE; j++)
                    for (int k = 0; k < Defines.MAPSIZE; k++)
                    {
                        downloadList[i, j, k] = BlockType.None;
                        blockList[i, j, k] = BlockType.None;
                    }

            // Initialize the face lists.
            faceMap = new Dictionary<int,bool>[(byte)BlockTexture.MAXIMUM, NUMREGIONS];
            for (BlockTexture blockTexture = BlockTexture.None; blockTexture < BlockTexture.MAXIMUM; blockTexture++)
                for (int r=0; r<NUMREGIONS; r++)
                    faceMap[(byte)blockTexture, r] = new Dictionary<int, bool>();

            // Initialize the texture map.
            blockTextureMap = new BlockTexture[(byte)BlockType.MAXIMUM, 6];
            for (BlockType blockType = BlockType.None; blockType < BlockType.MAXIMUM; blockType++)
                for (BlockFaceDirection faceDir = BlockFaceDirection.XIncreasing; faceDir < BlockFaceDirection.MAXIMUM; faceDir++)
                    blockTextureMap[(byte)blockType,(byte)faceDir] = BlockInformation.GetTexture(blockType, faceDir);

            // Check the textures
            if (!checkTextures())
            {
                //ToDo Show error by errorscreen
                Environment.Exit(1);
            }

            // Load the textures we'll use.
            blockTextures = new IMTexture[(byte)BlockTexture.MAXIMUM];
            blockTextures[(byte)BlockTexture.None] = new IMTexture(null);
            blockTextures[(byte)BlockTexture.Water] = new IMTexture(loadTextureFromMinecraft("terrain.png", 13, 12));
            blockTextures[(byte)BlockTexture.GrassSide] = new IMTexture(loadTextureFromMinecraft("terrain.png", 3, 0));
            blockTextures[(byte)BlockTexture.Grass] = new IMTexture(loadTextureFromMinecraft("terrain.png", 0, 0, true));
            blockTextures[(byte)BlockTexture.Adminblock] = new IMTexture(loadTextureFromMinecraft("terrain.png", 5, 2));
            blockTextures[(byte)BlockTexture.Dirt] = new IMTexture(loadTextureFromMinecraft("terrain.png", 2, 0));
            blockTextures[(byte)BlockTexture.Rock] = new IMTexture(loadTextureFromMinecraft("terrain.png", 0, 1));
            blockTextures[(byte)BlockTexture.Lava] = new IMTexture(loadTextureFromMinecraft("terrain.png", 15, 15));
            blockTextures[(byte)BlockTexture.Wood] = new IMTexture(loadTextureFromMinecraft("terrain.png", 5, 1));
            blockTextures[(byte)BlockTexture.WoodSide] = new IMTexture(loadTextureFromMinecraft("terrain.png", 4, 1));
            blockTextures[(byte)BlockTexture.Leafs] = new IMTexture(loadTextureFromMinecraft("terrain.png", 4, 3));
            blockTextures[(byte)BlockTexture.RedFlower] = new IMTexture(loadTextureFromMinecraft("terrain.png", 12, 0));
            blockTextures[(byte)BlockTexture.YellowFlower] = new IMTexture(loadTextureFromMinecraft("terrain.png", 13, 0));

            // Load our effects.
            basicEffect = gameInstance.Content.Load<Effect>("effect_basic");

            // Build vertex lists.
            vertexBuffers = new DynamicVertexBuffer[(byte)BlockTexture.MAXIMUM, NUMREGIONS];
            vertexListDirty = new bool[(byte)BlockTexture.MAXIMUM, NUMREGIONS];
            for (int i = 0; i < (byte)BlockTexture.MAXIMUM; i++)
                for (int j = 0; j < NUMREGIONS; j++)
                    vertexListDirty[i, j] = true;

            // Initialize any graphics stuff.
            vertexDeclaration = new VertexDeclaration(gameInstance.GraphicsDevice, VertexPositionTextureShade.VertexElements);

            // Initialize the bloom engine.
            if (gameInstance.Csettings.RenderPretty)
            {
                bloomPosteffect = new BloomComponent();
                bloomPosteffect.Load(gameInstance.GraphicsDevice, gameInstance.Content);
            }
            else
                bloomPosteffect = null;
        }

        public Texture2D loadTextureFromMinecraft(string file, int tileX, int tileY)
        {
            return loadTextureFromMinecraft(file, tileX, tileY, false);
        }

        public Texture2D loadTextureFromMinecraft(string file, int tileX, int tileY,bool unBiome)
        {
            Texture2D sheet = Texture2D.FromFile(gameInstance.GraphicsDevice, file);

            //calculate rectangle for tile in sheet
            int totalWidth = sheet.Width;
            int totalHeight = sheet.Height;
            int tileLeft = (totalWidth / 16) * tileX;
            int tileTop = (totalHeight / 16) * tileY;
            int tileRight = (totalWidth / 16) * (tileX+1);
            int tileBottom = (totalHeight / 16) * (tileY+1);

            //load sheet in array
            Color[] sheetPixels = new Color[totalWidth * totalHeight];
            sheet.GetData<Color>(sheetPixels);

            //Create array for tile
            Texture2D newtile = new Texture2D(gameInstance.GraphicsDevice, totalWidth / 16, totalHeight / 16);
            Color[] tilePixels = new Color[(totalWidth / 16) * (totalHeight / 16)];
            
            //Fill the tilepixels array
            int j = 0;
            for (int i = 0; i < sheetPixels.Length; i++)
            {
                int currentX = i % totalWidth;
                int currentY = i / totalWidth;
                if (currentX >= tileLeft && currentX < tileRight) //good column
                {
                    if (currentY >= tileTop && currentY < tileBottom) //good row
                    {
                        if (unBiome)
                        {
                            Color biome = new Color(58, 203, 0);

                            tilePixels[j] = Color.Lerp(biome, sheetPixels[i], 0.7f);
                        }
                        else
                        {
                            tilePixels[j] = sheetPixels[i];
                        }
                        j++;
                    }
                }
            }

            //put the array in a texture
            newtile.SetData<Color>(tilePixels);
            
            //and put it in the game :)
            return newtile;
        }

        // Check if someone removed some block textures (anti-hack)
        public bool checkTextures()
        {
            return File.Exists("terrain.png");
        }

        // Returns true if we are solid at this point.
        public bool SolidAtPoint(Vector3 point)
        {
            return BlockAtPoint(point) != BlockType.None; 
        }

        public bool SolidAtPointForPlayer(Vector3 point)
        {
            return !BlockInformation.IsPassibleBlock(BlockAtPoint(point));
        }

        public BlockType BlockAtPoint(Vector3 point)
        {
            int x = (int)point.X;
            int y = (int)point.Y;
            int z = (int)point.Z;
            if (x < 0 || y < 0 || z < 0 || x >= Defines.MAPSIZE || y >= Defines.MAPSIZE || z >= Defines.MAPSIZE)
                return BlockType.None;
            return blockList[x, y, z]; 
        }

        public bool RayCollision(Vector3 startPosition, Vector3 rayDirection, float distance, int searchGranularity, ref Vector3 hitPoint, ref Vector3 buildPoint)
        {
            Vector3 testPos = startPosition;
            Vector3 buildPos = startPosition;
            for (int i=0; i<searchGranularity; i++)
            {
                testPos += rayDirection * distance / searchGranularity;
                BlockType testBlock = BlockAtPoint(testPos);
                if (testBlock != BlockType.None && testBlock != BlockType.Water)
                {
                    hitPoint = testPos;
                    buildPoint = buildPos;
                    return true;
                }
                buildPos = testPos;
            }
            return false;
        }

        public void Update(GameTime gameTime)
        {
            lightingEngine.Update();
        }

        public void Render(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            RegenerateDirtyVertexLists();

            for (BlockTexture blockTexture = BlockTexture.None+1; blockTexture < BlockTexture.MAXIMUM; blockTexture++)
                for (int r = 0; r < NUMREGIONS; r++)
                {
                    // If this is empty, don't render it.
                    DynamicVertexBuffer regionBuffer = vertexBuffers[(byte)blockTexture, r];
                    if (regionBuffer == null)
                        continue;

                    // If this isn't in our view frustum, don't render it.
                    BoundingSphere regionBounds = new BoundingSphere(GetRegionCenter(r), REGIONSIZE);
                    BoundingFrustum boundingFrustum = new BoundingFrustum(gameInstance.propertyBag.playerCamera.ViewMatrix * gameInstance.propertyBag.playerCamera.ProjectionMatrix);
                    if (boundingFrustum.Contains(regionBounds) == ContainmentType.Disjoint)
                        continue;

                    // Make sure our vertex buffer is clean.
                    if (vertexListDirty[(byte)blockTexture, r])
                        continue;

                    // Actually render.
                    RenderVertexList(graphicsDevice, regionBuffer, blockTextures[(byte)blockTexture].Texture, blockTextures[(byte)blockTexture].LODColor, blockTexture, (float)gameTime.TotalRealTime.TotalSeconds);
                }

            // Apply posteffects.
            if (bloomPosteffect != null)
                bloomPosteffect.Draw(graphicsDevice);
        }

        private void RenderVertexList(GraphicsDevice graphicsDevice, DynamicVertexBuffer vertexBuffer, Texture2D blockTexture, Color lodColor, BlockTexture blocktex, float elapsedTime)
        {
            bool renderTranslucent = false;

            if (vertexBuffer == null)
                return;

            switch (blocktex)
            {
                case BlockTexture.Lava:
                    {
                        basicEffect.CurrentTechnique = basicEffect.Techniques["LavaBlock"];
                        basicEffect.Parameters["xTime"].SetValue(elapsedTime % 5);
                        break;
                    }
                case BlockTexture.Water:
                    {
                        //TODO Make own effect for water textures
                        renderTranslucent = true;
                        basicEffect.CurrentTechnique = basicEffect.Techniques["Block"];
                        break;
                    }
                default:
                    {
                        basicEffect.CurrentTechnique = basicEffect.Techniques["Block"];
                        break;
                    }
            }

            //graphicsDevice.RenderState.SourceBlend = Blend.Zero;
            basicEffect.Parameters["xWorld"].SetValue(Matrix.Identity);
            basicEffect.Parameters["xView"].SetValue(gameInstance.propertyBag.playerCamera.ViewMatrix);
            basicEffect.Parameters["xProjection"].SetValue(gameInstance.propertyBag.playerCamera.ProjectionMatrix);
            basicEffect.Parameters["xTexture"].SetValue(blockTexture);
            basicEffect.Parameters["xLODColor"].SetValue(lodColor.ToVector3());
            basicEffect.Begin();
            
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                graphicsDevice.RenderState.DepthBufferEnable = true;
                if (renderTranslucent)
                {
                    // TODO: Make translucent blocks look like we actually want them to look!
                    // We probably also want to pull this out to be rendered AFTER EVERYTHING ELSE IN THE GAME.
                    graphicsDevice.RenderState.DepthBufferWriteEnable = false;
                    graphicsDevice.RenderState.AlphaBlendEnable = true;
                    graphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    graphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                }

                graphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
                graphicsDevice.SamplerStates[0].MagFilter = TextureFilter.None;
                graphicsDevice.VertexDeclaration = vertexDeclaration;
                graphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionTextureShade.SizeInBytes);
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, vertexBuffer.SizeInBytes / VertexPositionTextureShade.SizeInBytes / 3);
                graphicsDevice.RenderState.CullMode = CullMode.None;

                if (renderTranslucent)
                {
                    graphicsDevice.RenderState.DepthBufferWriteEnable = true;
                    graphicsDevice.RenderState.AlphaBlendEnable = false;
                }

                pass.End();
            }
            
            basicEffect.End();
        }

        private void RegenerateDirtyVertexLists()
        {
            for (BlockTexture blockTexture = BlockTexture.None+1; blockTexture < BlockTexture.MAXIMUM; blockTexture++)
                for (int r = 0; r < NUMREGIONS; r++)
                    if (vertexListDirty[(byte)blockTexture, r])
                    {
                        vertexListDirty[(byte)blockTexture, r] = false;
                        Dictionary<int, bool> faceList = faceMap[(byte)blockTexture, r];
                        vertexBuffers[(byte)blockTexture, r] = CreateVertexBufferFromFaceList(faceList, (byte)blockTexture, r);
                    }
        }

        public struct DynamicVertexBufferTag
        {
            public BlockEngine blockEngine;
            public int texture, region;
            public DynamicVertexBufferTag(BlockEngine blockEngine, int texture, int region)
            {
                this.blockEngine = blockEngine;
                this.texture = texture;
                this.region = region;
            }
        }

        // Create a dynamic vertex buffer. The arguments texture and region are used to flag a content reload if the device is lost.
        private DynamicVertexBuffer CreateVertexBufferFromFaceList(Dictionary<int, bool> faceList, int texture, int region)
        {
            if (faceList.Count == 0)
                return null;

            VertexPositionTextureShade[] vertexList = new VertexPositionTextureShade[faceList.Count * 6];
            ulong vertexPointer = 0;
            foreach (int faceInfo in faceList.Keys)
            {
                BuildFaceVertices(ref vertexList, vertexPointer, faceInfo, texture);
                vertexPointer += 6;            
            }
            DynamicVertexBuffer vertexBuffer = new DynamicVertexBuffer(gameInstance.GraphicsDevice, vertexList.Length * VertexPositionTextureShade.SizeInBytes, BufferUsage.WriteOnly);
            vertexBuffer.ContentLost += new EventHandler(vertexBuffer_ContentLost);
            vertexBuffer.Tag = new DynamicVertexBufferTag(this, texture, region);
            vertexBuffer.SetData(vertexList);
            return vertexBuffer;
        }

        void vertexBuffer_ContentLost(object sender, EventArgs e)
        {
            DynamicVertexBuffer dvb = sender as DynamicVertexBuffer;
            if (dvb != null)
            {
                DynamicVertexBufferTag tag = (DynamicVertexBufferTag)dvb.Tag;
                tag.blockEngine.MakeRegionDirty(tag.texture, tag.region);
            }
        }

        private void BuildFaceVertices(ref VertexPositionTextureShade[] vertexList, ulong vertexPointer, int faceInfo, int texture)
        {
            // Decode the face information.
            int x = 0, y = 0, z = 0;
            BlockFaceDirection faceDir = BlockFaceDirection.MAXIMUM;
            DecodeBlockFace(faceInfo, ref x, ref y, ref z, ref faceDir);

            // Check height.
            float height;

            switch ((BlockTexture)texture)
            {
                case BlockTexture.RedFlower:
                case BlockTexture.YellowFlower:
                    height = 0.5f;
                    break;
                default:
                    height = 1.0f;
                    break;
            }  

            // Insert the vertices.
            if (!BlockInformation.IsPlantBlock((BlockTexture)texture))
            {
                switch (faceDir)
                {
                    case BlockFaceDirection.XIncreasing:
                        {
                            vertexList[vertexPointer + 0] = new VertexPositionTextureShade(new Vector3(x + 1, y + height, z + 1), new Vector2(0, 0), Math.Min(lightingEngine.GetLight(x + 1, y, z), 0.6));
                            vertexList[vertexPointer + 1] = new VertexPositionTextureShade(new Vector3(x + 1, y + height, z), new Vector2(1, 0), Math.Min(lightingEngine.GetLight(x + 1, y, z), 0.6));
                            vertexList[vertexPointer + 2] = new VertexPositionTextureShade(new Vector3(x + 1, y, z + 1), new Vector2(0, 1), Math.Min(lightingEngine.GetLight(x + 1, y, z), 0.6));
                            vertexList[vertexPointer + 3] = new VertexPositionTextureShade(new Vector3(x + 1, y, z + 1), new Vector2(0, 1), Math.Min(lightingEngine.GetLight(x + 1, y, z), 0.6));
                            vertexList[vertexPointer + 4] = new VertexPositionTextureShade(new Vector3(x + 1, y + height, z), new Vector2(1, 0), Math.Min(lightingEngine.GetLight(x + 1, y, z), 0.6));
                            vertexList[vertexPointer + 5] = new VertexPositionTextureShade(new Vector3(x + 1, y, z), new Vector2(1, 1), Math.Min(lightingEngine.GetLight(x + 1, y, z), 0.6));
                        }
                        break;


                    case BlockFaceDirection.XDecreasing:
                        {
                            vertexList[vertexPointer + 0] = new VertexPositionTextureShade(new Vector3(x, y + height, z), new Vector2(0, 0), Math.Min(lightingEngine.GetLight(x - 1, y, z), 0.6));
                            vertexList[vertexPointer + 1] = new VertexPositionTextureShade(new Vector3(x, y + height, z + 1), new Vector2(1, 0), Math.Min(lightingEngine.GetLight(x - 1, y, z), 0.6));
                            vertexList[vertexPointer + 2] = new VertexPositionTextureShade(new Vector3(x, y, z + 1), new Vector2(1, 1), Math.Min(lightingEngine.GetLight(x - 1, y, z), 0.6));
                            vertexList[vertexPointer + 3] = new VertexPositionTextureShade(new Vector3(x, y + height, z), new Vector2(0, 0), Math.Min(lightingEngine.GetLight(x - 1, y, z), 0.6));
                            vertexList[vertexPointer + 4] = new VertexPositionTextureShade(new Vector3(x, y, z + 1), new Vector2(1, 1), Math.Min(lightingEngine.GetLight(x - 1, y, z), 0.6));
                            vertexList[vertexPointer + 5] = new VertexPositionTextureShade(new Vector3(x, y, z), new Vector2(0, 1), Math.Min(lightingEngine.GetLight(x - 1, y, z), 0.6));
                        }
                        break;

                    case BlockFaceDirection.YIncreasing:
                        {
                            vertexList[vertexPointer + 0] = new VertexPositionTextureShade(new Vector3(x, y + height, z), new Vector2(0, 1), Math.Min(lightingEngine.GetLight(x, y + 1, z), 0.8));
                            vertexList[vertexPointer + 1] = new VertexPositionTextureShade(new Vector3(x + 1, y + height, z), new Vector2(0, 0), Math.Min(lightingEngine.GetLight(x, y + 1, z), 0.8));
                            vertexList[vertexPointer + 2] = new VertexPositionTextureShade(new Vector3(x + 1, y + height, z + 1), new Vector2(1, 0), Math.Min(lightingEngine.GetLight(x, y + 1, z), 0.8));
                            vertexList[vertexPointer + 3] = new VertexPositionTextureShade(new Vector3(x, y + height, z), new Vector2(0, 1), Math.Min(lightingEngine.GetLight(x, y + 1, z), 0.8));
                            vertexList[vertexPointer + 4] = new VertexPositionTextureShade(new Vector3(x + 1, y + height, z + 1), new Vector2(1, 0), Math.Min(lightingEngine.GetLight(x, y + 1, z), 0.8));
                            vertexList[vertexPointer + 5] = new VertexPositionTextureShade(new Vector3(x, y + height, z + 1), new Vector2(1, 1), Math.Min(lightingEngine.GetLight(x, y + 1, z), 0.8));
                        }
                        break;

                    case BlockFaceDirection.YDecreasing:
                        {
                            vertexList[vertexPointer + 0] = new VertexPositionTextureShade(new Vector3(x + 1, y, z + 1), new Vector2(0, 0), Math.Min(lightingEngine.GetLight(x, y - 1, z), 0.2));
                            vertexList[vertexPointer + 1] = new VertexPositionTextureShade(new Vector3(x + 1, y, z), new Vector2(1, 0), Math.Min(lightingEngine.GetLight(x, y - 1, z), 0.2));
                            vertexList[vertexPointer + 2] = new VertexPositionTextureShade(new Vector3(x, y, z + 1), new Vector2(0, 1), Math.Min(lightingEngine.GetLight(x, y - 1, z), 0.2));
                            vertexList[vertexPointer + 3] = new VertexPositionTextureShade(new Vector3(x, y, z + 1), new Vector2(0, 1), Math.Min(lightingEngine.GetLight(x, y - 1, z), 0.2));
                            vertexList[vertexPointer + 4] = new VertexPositionTextureShade(new Vector3(x + 1, y, z), new Vector2(1, 0), Math.Min(lightingEngine.GetLight(x, y - 1, z), 0.2));
                            vertexList[vertexPointer + 5] = new VertexPositionTextureShade(new Vector3(x, y, z), new Vector2(1, 1), Math.Min(lightingEngine.GetLight(x, y - 1, z), 0.2));
                        }
                        break;

                    case BlockFaceDirection.ZIncreasing:
                        {
                            vertexList[vertexPointer + 0] = new VertexPositionTextureShade(new Vector3(x, y + height, z + 1), new Vector2(0, 0), Math.Min(lightingEngine.GetLight(x, y, z + 1), 0.4));
                            vertexList[vertexPointer + 1] = new VertexPositionTextureShade(new Vector3(x + 1, y + height, z + 1), new Vector2(1, 0), Math.Min(lightingEngine.GetLight(x, y, z + 1), 0.4));
                            vertexList[vertexPointer + 2] = new VertexPositionTextureShade(new Vector3(x + 1, y, z + 1), new Vector2(1, 1), Math.Min(lightingEngine.GetLight(x, y, z + 1), 0.4));
                            vertexList[vertexPointer + 3] = new VertexPositionTextureShade(new Vector3(x, y + height, z + 1), new Vector2(0, 0), Math.Min(lightingEngine.GetLight(x, y, z + 1), 0.4));
                            vertexList[vertexPointer + 4] = new VertexPositionTextureShade(new Vector3(x + 1, y, z + 1), new Vector2(1, 1), Math.Min(lightingEngine.GetLight(x, y, z + 1), 0.4));
                            vertexList[vertexPointer + 5] = new VertexPositionTextureShade(new Vector3(x, y, z + 1), new Vector2(0, 1), Math.Min(lightingEngine.GetLight(x, y, z + 1), 0.4));
                        }
                        break;

                    case BlockFaceDirection.ZDecreasing:
                        {
                            vertexList[vertexPointer + 0] = new VertexPositionTextureShade(new Vector3(x + 1, y + height, z), new Vector2(0, 0), Math.Min(lightingEngine.GetLight(x, y, z - 1), 0.4));
                            vertexList[vertexPointer + 1] = new VertexPositionTextureShade(new Vector3(x, y + height, z), new Vector2(1, 0), Math.Min(lightingEngine.GetLight(x, y, z - 1), 0.4));
                            vertexList[vertexPointer + 2] = new VertexPositionTextureShade(new Vector3(x + 1, y, z), new Vector2(0, 1), Math.Min(lightingEngine.GetLight(x, y, z - 1), 0.4));
                            vertexList[vertexPointer + 3] = new VertexPositionTextureShade(new Vector3(x + 1, y, z), new Vector2(0, 1), Math.Min(lightingEngine.GetLight(x, y, z - 1), 0.4));
                            vertexList[vertexPointer + 4] = new VertexPositionTextureShade(new Vector3(x, y + height, z), new Vector2(1, 0), Math.Min(lightingEngine.GetLight(x, y, z - 1), 0.4));
                            vertexList[vertexPointer + 5] = new VertexPositionTextureShade(new Vector3(x, y, z), new Vector2(1, 1), Math.Min(lightingEngine.GetLight(x, y, z - 1), 0.4));
                        }
                        break;
                }
            }
            else
            {
                switch (faceDir)
                {
                    case BlockFaceDirection.XIncreasing:
                        {
                            vertexList[vertexPointer + 0] = new VertexPositionTextureShade(new Vector3(x + 0.5f, y + height, z + 1), new Vector2(0, 0), Math.Min(lightingEngine.GetLight(x + 1, y, z), 0.6));
                            vertexList[vertexPointer + 1] = new VertexPositionTextureShade(new Vector3(x + 0.5f, y + height, z), new Vector2(1, 0), Math.Min(lightingEngine.GetLight(x + 1, y, z), 0.6));
                            vertexList[vertexPointer + 2] = new VertexPositionTextureShade(new Vector3(x + 0.5f, y, z + 1), new Vector2(0, 1), Math.Min(lightingEngine.GetLight(x + 1, y, z), 0.6));
                            vertexList[vertexPointer + 3] = new VertexPositionTextureShade(new Vector3(x + 0.5f, y, z + 1), new Vector2(0, 1), Math.Min(lightingEngine.GetLight(x + 1, y, z), 0.6));
                            vertexList[vertexPointer + 4] = new VertexPositionTextureShade(new Vector3(x + 0.5f, y + height, z), new Vector2(1, 0), Math.Min(lightingEngine.GetLight(x + 1, y, z), 0.6));
                            vertexList[vertexPointer + 5] = new VertexPositionTextureShade(new Vector3(x + 0.5f, y, z), new Vector2(1, 1), Math.Min(lightingEngine.GetLight(x + 1, y, z), 0.6));
                        }
                        break;


                    case BlockFaceDirection.XDecreasing:
                        {
                            vertexList[vertexPointer + 0] = new VertexPositionTextureShade(new Vector3(x + 0.5f, y + height, z), new Vector2(0, 0), Math.Min(lightingEngine.GetLight(x - 1, y, z), 0.6));
                            vertexList[vertexPointer + 1] = new VertexPositionTextureShade(new Vector3(x + 0.5f, y + height, z + 1), new Vector2(1, 0), Math.Min(lightingEngine.GetLight(x - 1, y, z), 0.6));
                            vertexList[vertexPointer + 2] = new VertexPositionTextureShade(new Vector3(x + 0.5f, y, z + 1), new Vector2(1, 1), Math.Min(lightingEngine.GetLight(x - 1, y, z), 0.6));
                            vertexList[vertexPointer + 3] = new VertexPositionTextureShade(new Vector3(x + 0.5f, y + height, z), new Vector2(0, 0), Math.Min(lightingEngine.GetLight(x - 1, y, z), 0.6));
                            vertexList[vertexPointer + 4] = new VertexPositionTextureShade(new Vector3(x + 0.5f, y, z + 1), new Vector2(1, 1), Math.Min(lightingEngine.GetLight(x - 1, y, z), 0.6));
                            vertexList[vertexPointer + 5] = new VertexPositionTextureShade(new Vector3(x + 0.5f, y, z), new Vector2(0, 1), Math.Min(lightingEngine.GetLight(x - 1, y, z), 0.6));
                        }
                        break;

                    case BlockFaceDirection.ZIncreasing:
                        {
                            vertexList[vertexPointer + 0] = new VertexPositionTextureShade(new Vector3(x, y + height, z + 0.5f), new Vector2(0, 0), Math.Min(lightingEngine.GetLight(x, y, z + 1), 0.4));
                            vertexList[vertexPointer + 1] = new VertexPositionTextureShade(new Vector3(x + 1, y + height, z + 0.5f), new Vector2(1, 0), Math.Min(lightingEngine.GetLight(x, y, z + 1), 0.4));
                            vertexList[vertexPointer + 2] = new VertexPositionTextureShade(new Vector3(x + 1, y, z + 0.5f), new Vector2(1, 1), Math.Min(lightingEngine.GetLight(x, y, z + 1), 0.4));
                            vertexList[vertexPointer + 3] = new VertexPositionTextureShade(new Vector3(x, y + height, z + 0.5f), new Vector2(0, 0), Math.Min(lightingEngine.GetLight(x, y, z + 1), 0.4));
                            vertexList[vertexPointer + 4] = new VertexPositionTextureShade(new Vector3(x + 1, y, z + 0.5f), new Vector2(1, 1), Math.Min(lightingEngine.GetLight(x, y, z + 1), 0.4));
                            vertexList[vertexPointer + 5] = new VertexPositionTextureShade(new Vector3(x, y, z + 0.5f), new Vector2(0, 1), Math.Min(lightingEngine.GetLight(x, y, z + 1), 0.4));
                        }
                        break;

                    case BlockFaceDirection.ZDecreasing:
                        {
                            vertexList[vertexPointer + 0] = new VertexPositionTextureShade(new Vector3(x + 1, y + height, z + 0.5f), new Vector2(0, 0), Math.Min(lightingEngine.GetLight(x, y, z - 1), 0.4));
                            vertexList[vertexPointer + 1] = new VertexPositionTextureShade(new Vector3(x, y + height, z + 0.5f), new Vector2(1, 0), Math.Min(lightingEngine.GetLight(x, y, z - 1), 0.4));
                            vertexList[vertexPointer + 2] = new VertexPositionTextureShade(new Vector3(x + 1, y, z + 0.5f), new Vector2(0, 1), Math.Min(lightingEngine.GetLight(x, y, z - 1), 0.4));
                            vertexList[vertexPointer + 3] = new VertexPositionTextureShade(new Vector3(x + 1, y, z + 0.5f), new Vector2(0, 1), Math.Min(lightingEngine.GetLight(x, y, z - 1), 0.4));
                            vertexList[vertexPointer + 4] = new VertexPositionTextureShade(new Vector3(x, y + height, z + 0.5f), new Vector2(1, 0), Math.Min(lightingEngine.GetLight(x, y, z - 1), 0.4));
                            vertexList[vertexPointer + 5] = new VertexPositionTextureShade(new Vector3(x, y, z + 0.5f), new Vector2(1, 1), Math.Min(lightingEngine.GetLight(x, y, z - 1), 0.4));
                        }
                        break;
                }
            }
        }

        private void _AddBlock(int x, int y, int z, BlockFaceDirection dir, BlockType type, int x2, int y2, int z2, BlockFaceDirection dir2)
        {
            BlockType type2 = blockList[x2, y2, z2];
            if (!BlockInformation.IsTransparentBlock(type) && !BlockInformation.IsTransparentBlock(type2) && type == type2)
                HideQuad(x2, y2, z2, dir2, type2);
            else
                if ((type2 == BlockType.Water) && type2 == type)
                {
                    HideQuad(x, y, z, dir, type);
                    HideQuad(x2, y2, z2, dir2, type);
                }
                else
                {
                    ShowQuad(x, y, z, dir, type);
                }
        }

        public void AddBlock(int x, int y, int z, BlockType blockType)
        {
            if (x <= 0 || y <= 0 || z <= 0 || x >= Defines.MAPSIZE - 1 || y >= Defines.MAPSIZE - 1 || z >= Defines.MAPSIZE - 1)
                return;

            blockList[x, y, z] = blockType;

            _AddBlock(x, y, z, BlockFaceDirection.XIncreasing, blockType, x + 1, y, z, BlockFaceDirection.XDecreasing);
            _AddBlock(x, y, z, BlockFaceDirection.XDecreasing, blockType, x - 1, y, z, BlockFaceDirection.XIncreasing);
            _AddBlock(x, y, z, BlockFaceDirection.YIncreasing, blockType, x, y + 1, z, BlockFaceDirection.YDecreasing);
            _AddBlock(x, y, z, BlockFaceDirection.YDecreasing, blockType, x, y - 1, z, BlockFaceDirection.YIncreasing);
            _AddBlock(x, y, z, BlockFaceDirection.ZIncreasing, blockType, x, y, z + 1, BlockFaceDirection.ZDecreasing);
            _AddBlock(x, y, z, BlockFaceDirection.ZDecreasing, blockType, x, y, z - 1, BlockFaceDirection.ZIncreasing);
            lightingEngine.BlockAdded(blockType, x, y, z);
        }

        private void _RemoveBlock(int x, int y, int z, BlockFaceDirection dir, int x2, int y2, int z2, BlockFaceDirection dir2)
        {
            BlockType type = blockList[x, y, z];
            BlockType type2 = blockList[x2, y2, z2];
            if (!BlockInformation.IsTransparentBlock(type) && !BlockInformation.IsTransparentBlock(type2) && type == type2)
                ShowQuad(x2, y2, z2, dir2, type2);
            else
                HideQuad(x, y, z, dir, type);
        }

        public void RemoveBlock(int x, int y, int z)
        {
            if (x <= 0 || y <= 0 || z <= 0 || x >= Defines.MAPSIZE - 1 || y >= Defines.MAPSIZE - 1 || z >= Defines.MAPSIZE - 1)
                return;

            _RemoveBlock(x, y, z, BlockFaceDirection.XIncreasing, x + 1, y, z, BlockFaceDirection.XDecreasing);
            _RemoveBlock(x, y, z, BlockFaceDirection.XDecreasing, x - 1, y, z, BlockFaceDirection.XIncreasing);
            _RemoveBlock(x, y, z, BlockFaceDirection.YIncreasing, x, y + 1, z, BlockFaceDirection.YDecreasing);
            _RemoveBlock(x, y, z, BlockFaceDirection.YDecreasing, x, y - 1, z, BlockFaceDirection.YIncreasing);
            _RemoveBlock(x, y, z, BlockFaceDirection.ZIncreasing, x, y, z + 1, BlockFaceDirection.ZDecreasing);
            _RemoveBlock(x, y, z, BlockFaceDirection.ZDecreasing, x, y, z - 1, BlockFaceDirection.ZIncreasing);

            blockList[x, y, z] = BlockType.None;
            lightingEngine.BlockRemoved(BlockType.None, x, y, z);
        }

        private int EncodeBlockFace(int x, int y, int z, BlockFaceDirection faceDir)
        {
            //TODO: OPTIMIZE BY HARD CODING VALUES IN
            return (x + y * Defines.MAPSIZE + z * Defines.MAPSIZE * Defines.MAPSIZE + (byte)faceDir * Defines.MAPSIZE * Defines.MAPSIZE * Defines.MAPSIZE);
        }

        private void DecodeBlockFace(int faceCode, ref int x, ref int y, ref int z, ref BlockFaceDirection faceDir)
        {
            x = (faceCode % Defines.MAPSIZE);
            faceCode = (faceCode - x) / Defines.MAPSIZE;
            y = (faceCode % Defines.MAPSIZE);
            faceCode = (faceCode - y) / Defines.MAPSIZE;
            z = (faceCode % Defines.MAPSIZE);
            faceCode = (faceCode - z) / Defines.MAPSIZE;
            faceDir = (BlockFaceDirection)faceCode;
        }

        // Returns the region that a block at (x,y,z) should belong in.
        private int GetRegion(int x, int y, int z)
        {
            return (x / REGIONSIZE + (y / REGIONSIZE) * REGIONRATIO + (z / REGIONSIZE) * REGIONRATIO * REGIONRATIO);
        }

        private Vector3 GetRegionCenter(int regionNumber)
        {
            int x, y, z;
            x = regionNumber % REGIONRATIO;
            regionNumber = (regionNumber - x) / REGIONRATIO;
            y = regionNumber % REGIONRATIO;
            regionNumber = (regionNumber - y) / REGIONRATIO;
            z = regionNumber;
            return new Vector3(x * REGIONSIZE + REGIONSIZE / 2, y * REGIONSIZE + REGIONSIZE / 2, z * REGIONSIZE + REGIONSIZE / 2);            
        }

        private void ShowQuad(int x, int y, int z, BlockFaceDirection faceDir, BlockType blockType)
        {
            BlockTexture blockTexture = blockTextureMap[(byte)blockType, (byte)faceDir];
            int blockFace = EncodeBlockFace(x, y, z, faceDir);
            int region = GetRegion(x, y, z);
            if (!faceMap[(byte)blockTexture, region].ContainsKey(blockFace))
                faceMap[(byte)blockTexture, region].Add(blockFace, true);
            vertexListDirty[(byte)blockTexture, region] = true;
        }

        private void HideQuad(int x, int y, int z, BlockFaceDirection faceDir, BlockType blockType)
        {
            BlockTexture blockTexture = blockTextureMap[(byte)blockType, (byte)faceDir];
            int blockFace = EncodeBlockFace(x, y, z, faceDir);
            int region = GetRegion(x, y, z);
            if (faceMap[(byte)blockTexture, region].ContainsKey(blockFace))
                faceMap[(byte)blockTexture, region].Remove(blockFace);
            vertexListDirty[(byte)blockTexture, region] = true;
        }
    }
}
