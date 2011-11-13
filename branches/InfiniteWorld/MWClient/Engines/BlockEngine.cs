using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MineWorld.BloomEffect;

namespace MineWorld.Engines
{
    [Serializable]
    public struct VertexPositionTextureShade
    {
        public static readonly VertexElement[] VertexElements = new[]
                                                                    {
                                                                        new VertexElement(0, 0,
                                                                                          VertexElementFormat.Vector3,
                                                                                          VertexElementMethod.Default,
                                                                                          VertexElementUsage.Position, 0)
                                                                        ,
                                                                        new VertexElement(0, sizeof (float)*3,
                                                                                          VertexElementFormat.Vector2,
                                                                                          VertexElementMethod.Default,
                                                                                          VertexElementUsage.
                                                                                              TextureCoordinate, 0),
                                                                        new VertexElement(0, sizeof (float)*5,
                                                                                          VertexElementFormat.Single,
                                                                                          VertexElementMethod.Default,
                                                                                          VertexElementUsage.
                                                                                              TextureCoordinate, 1)
                                                                    };

        private Vector3 _pos;
        private float _shade;
        private Vector2 _tex;

        public VertexPositionTextureShade(Vector3 position, Vector2 uv, double shade)
        {
            _pos = position;
            _tex = uv;
            _shade = (float) shade;
        }

        public Vector3 Position
        {
            get { return _pos; }
            set { _pos = value; }
        }

        public Vector2 Tex
        {
            get { return _tex; }
            set { _tex = value; }
        }

        public float Shade
        {
            get { return _shade; }
            set { _shade = value; }
        }

        public static int SizeInBytes
        {
            get { return sizeof (float)*6; }
        }
    }

    public class IMTexture
    {
        public Color LodColor = Color.Black;
        public Texture2D Texture;

        public IMTexture(Texture2D texture)
        {
            Texture = texture;
            LodColor = Color.Black;

            // If this is a null texture, use a black LOD color.
            if (Texture == null)
                return;

            // Calculate the load color dynamically.
            float r = 0, g = 0, b = 0;
            Color[] pixelData = new Color[texture.Width*texture.Height];
            texture.GetData(pixelData);
            for (int i = 0; i < texture.Width; i++)
                for (int j = 0; j < texture.Height; j++)
                {
                    r += pixelData[i + j*texture.Width].R;
                    g += pixelData[i + j*texture.Width].G;
                    b += pixelData[i + j*texture.Width].B;
                }
            r /= texture.Width*texture.Height;
            g /= texture.Width*texture.Height;
            b /= texture.Width*texture.Height;
            LodColor = new Color(r/256, g/256, b/256);
        }
    }

    public class BlockEngine
    {
        private readonly Effect _basicEffect;
        private readonly BlockTexture[,] _blockTextureMap;
        private readonly BloomComponent _bloomPosteffect;
        private readonly Dictionary<int, bool>[,] _faceMap;
        private readonly MineWorldGame _gameInstance;
        private readonly DynamicVertexBuffer[,] _vertexBuffers;
        private readonly VertexDeclaration _vertexDeclaration;
        private readonly bool[,] _vertexListDirty;
        private const int Numregions = 4096; //(Regionratio * regionratio * regionratio) Region total
        private const int Regionratio = 16; // (mapsize/regionsize) How many regions
        private const int Regionsize = 16; //Size of 1 region
        public BlockType[,,] BlockList;
        public IMTexture[] BlockTextures;

        // Initialize our mapsize
        private int _mapX;
        private int _mapY;
        private int _mapZ;

        public BlockEngine(MineWorldGame gameInstance)
        {
            _gameInstance = gameInstance;

            // Initialize the face lists.
            _faceMap = new Dictionary<int, bool>[(byte) BlockTexture.MAXIMUM,Numregions];
            for (BlockTexture blockTexture = BlockTexture.None; blockTexture < BlockTexture.MAXIMUM; blockTexture++)
                for (int r = 0; r < Numregions; r++)
                    _faceMap[(byte) blockTexture, r] = new Dictionary<int, bool>();

            // Initialize the texture map.
            _blockTextureMap = new BlockTexture[(byte) BlockType.MAXIMUM,6];
            for (BlockType blockType = BlockType.None; blockType < BlockType.MAXIMUM; blockType++)
                for (BlockFaceDirection faceDir = BlockFaceDirection.XIncreasing;
                     faceDir < BlockFaceDirection.MAXIMUM;
                     faceDir++)
                    _blockTextureMap[(byte) blockType, (byte) faceDir] = BlockInformation.GetTexture(blockType, faceDir);

            // Check the textures
            if (!CheckTextures())
            {
                Debug.Assert(Convert.ToBoolean("Texture file not found"));
                //ToDo Show error by errorscreen
                Environment.Exit(1);
            }

            // Load the textures we'll use.
            BlockTextures = new IMTexture[(byte) BlockTexture.MAXIMUM];
            BlockTextures[(byte) BlockTexture.None] = new IMTexture(null);
            BlockTextures[(byte) BlockTexture.Water] = new IMTexture(LoadTextureFromMinecraft("terrain.png", 13, 12));
            BlockTextures[(byte) BlockTexture.GrassSide] = new IMTexture(LoadTextureFromMinecraft("terrain.png", 3, 0));
            BlockTextures[(byte) BlockTexture.Grass] = new IMTexture(LoadTextureFromMinecraft("terrain.png", 0, 0, true));
            BlockTextures[(byte) BlockTexture.Adminblock] = new IMTexture(LoadTextureFromMinecraft("terrain.png", 5, 2));
            BlockTextures[(byte) BlockTexture.Dirt] = new IMTexture(LoadTextureFromMinecraft("terrain.png", 2, 0));
            BlockTextures[(byte) BlockTexture.Rock] = new IMTexture(LoadTextureFromMinecraft("terrain.png", 0, 1));
            BlockTextures[(byte) BlockTexture.Lava] = new IMTexture(LoadTextureFromMinecraft("terrain.png", 15, 15));
            BlockTextures[(byte) BlockTexture.Wood] = new IMTexture(LoadTextureFromMinecraft("terrain.png", 5, 1));
            BlockTextures[(byte) BlockTexture.WoodSide] = new IMTexture(LoadTextureFromMinecraft("terrain.png", 4, 1));
            BlockTextures[(byte) BlockTexture.Leafs] = new IMTexture(LoadTextureFromMinecraft("terrain.png", 4, 3));
            BlockTextures[(byte) BlockTexture.RedFlower] = new IMTexture(LoadTextureFromMinecraft("terrain.png", 12, 0));
            BlockTextures[(byte) BlockTexture.YellowFlower] =
                new IMTexture(LoadTextureFromMinecraft("terrain.png", 13, 0));

            // Load our effects.
            _basicEffect = gameInstance.Content.Load<Effect>("effect_basic");

            // Build vertex lists.
            _vertexBuffers = new DynamicVertexBuffer[(byte) BlockTexture.MAXIMUM,Numregions];
            _vertexListDirty = new bool[(byte) BlockTexture.MAXIMUM,Numregions];
            for (int i = 0; i < (byte) BlockTexture.MAXIMUM; i++)
                for (int j = 0; j < Numregions; j++)
                    _vertexListDirty[i, j] = true;

            // Initialize any graphics stuff.
            _vertexDeclaration = new VertexDeclaration(gameInstance.GraphicsDevice,
                                                      VertexPositionTextureShade.VertexElements);

            // Initialize the bloom engine.
            if (gameInstance.Csettings.RenderPretty)
            {
                _bloomPosteffect = new BloomComponent();
                _bloomPosteffect.Load(gameInstance.GraphicsDevice, gameInstance.Content);
            }
            else
                _bloomPosteffect = null;
        }

        public void MakeRegionDirty(int texture, int region)
        {
            _vertexListDirty[texture, region] = true;
        }

        public void SetBlockList(BlockType[,,] data, int x, int y, int z)
        {
            _mapX = x;
            _mapY = y;
            _mapZ = z;
            BlockList = new BlockType[x,y,z];
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    for (int k = 0; k < x; k++)
                    {
                        AddBlock(i, j, k, data[i, j, k]);
                    }
                }
            }
        }

        public Texture2D LoadTextureFromMinecraft(string file, int tileX, int tileY)
        {
            return LoadTextureFromMinecraft(file, tileX, tileY, false);
        }

        public Texture2D LoadTextureFromMinecraft(string file, int tileX, int tileY, bool unBiome)
        {
            Texture2D sheet = Texture2D.FromFile(_gameInstance.GraphicsDevice, file);

            //calculate rectangle for tile in sheet
            int totalWidth = sheet.Width;
            int totalHeight = sheet.Height;
            int tileLeft = (totalWidth/16)*tileX;
            int tileTop = (totalHeight/16)*tileY;
            int tileRight = (totalWidth/16)*(tileX + 1);
            int tileBottom = (totalHeight/16)*(tileY + 1);

            //load sheet in array
            Color[] sheetPixels = new Color[totalWidth*totalHeight];
            sheet.GetData(sheetPixels);

            //Create array for tile
            Texture2D newtile = new Texture2D(_gameInstance.GraphicsDevice, totalWidth/16, totalHeight/16);
            Color[] tilePixels = new Color[(totalWidth/16)*(totalHeight/16)];

            //Fill the tilepixels array
            int j = 0;
            for (int i = 0; i < sheetPixels.Length; i++)
            {
                int currentX = i%totalWidth;
                int currentY = i/totalWidth;
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
            newtile.SetData(tilePixels);

            //and put it in the game :)
            return newtile;
        }

        // Check if someone removed some block textures (anti-hack)
        public bool CheckTextures()
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
            int x = (int) point.X;
            int y = (int) point.Y;
            int z = (int) point.Z;
            if (x < 0 || y < 0 || z < 0 || x >= _mapX || y >= _mapY || z >= _mapZ)
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
        }

        public void Render(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            RegenerateDirtyVertexLists();

            for (BlockTexture blockTexture = BlockTexture.None + 1; blockTexture < BlockTexture.MAXIMUM; blockTexture++)
                for (int r = 0; r < Numregions; r++)
                {
                    // If this is empty, don't render it.
                    DynamicVertexBuffer regionBuffer = _vertexBuffers[(byte) blockTexture, r];
                    if (regionBuffer == null)
                        continue;

                    // If this isn't in our view frustum, don't render it.
                    BoundingSphere regionBounds = new BoundingSphere(GetRegionCenter(r), Regionsize);
                    BoundingFrustum boundingFrustum =
                        new BoundingFrustum(_gameInstance.PropertyBag.PlayerCamera.ViewMatrix*
                                            _gameInstance.PropertyBag.PlayerCamera.ProjectionMatrix);
                    if (boundingFrustum.Contains(regionBounds) == ContainmentType.Disjoint)
                        continue;

                    // Make sure our vertex buffer is clean.
                    if (_vertexListDirty[(byte) blockTexture, r])
                        continue;

                    // Actually render.
                    RenderVertexList(graphicsDevice, regionBuffer, BlockTextures[(byte) blockTexture].Texture,
                                     BlockTextures[(byte) blockTexture].LodColor, blockTexture,
                                     (float) gameTime.TotalRealTime.TotalSeconds);
                }

            // Apply posteffects.
            if (_bloomPosteffect != null)
                _bloomPosteffect.Draw(graphicsDevice);
        }

        private void RenderVertexList(GraphicsDevice graphicsDevice, DynamicVertexBuffer vertexBuffer,
                                      Texture2D blockTexture, Color lodColor, BlockTexture blocktex, float elapsedTime)
        {
            bool renderTranslucent = false;

            if (vertexBuffer == null)
                return;

            switch (blocktex)
            {
                case BlockTexture.Lava:
                    {
                        _basicEffect.CurrentTechnique = _basicEffect.Techniques["LavaBlock"];
                        _basicEffect.Parameters["xTime"].SetValue(elapsedTime%5);
                        break;
                    }
                case BlockTexture.Water:
                    {
                        //TODO Make own effect for water textures
                        renderTranslucent = true;
                        _basicEffect.CurrentTechnique = _basicEffect.Techniques["Block"];
                        break;
                    }
                default:
                    {
                        _basicEffect.CurrentTechnique = _basicEffect.Techniques["Block"];
                        break;
                    }
            }

            //graphicsDevice.RenderState.SourceBlend = Blend.Zero;
            _basicEffect.Parameters["xWorld"].SetValue(Matrix.Identity);
            _basicEffect.Parameters["xView"].SetValue(_gameInstance.PropertyBag.PlayerCamera.ViewMatrix);
            _basicEffect.Parameters["xProjection"].SetValue(_gameInstance.PropertyBag.PlayerCamera.ProjectionMatrix);
            _basicEffect.Parameters["xTexture"].SetValue(blockTexture);
            _basicEffect.Parameters["xLODColor"].SetValue(lodColor.ToVector3());
            _basicEffect.Begin();

            foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
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
                graphicsDevice.VertexDeclaration = _vertexDeclaration;
                graphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionTextureShade.SizeInBytes);
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0,
                                              vertexBuffer.SizeInBytes/VertexPositionTextureShade.SizeInBytes/3);
                graphicsDevice.RenderState.CullMode = CullMode.None;

                if (renderTranslucent)
                {
                    graphicsDevice.RenderState.DepthBufferWriteEnable = true;
                    graphicsDevice.RenderState.AlphaBlendEnable = false;
                }

                pass.End();
            }

            _basicEffect.End();
        }

        private void RegenerateDirtyVertexLists()
        {
            for (BlockTexture blockTexture = BlockTexture.None + 1; blockTexture < BlockTexture.MAXIMUM; blockTexture++)
                for (int r = 0; r < Numregions; r++)
                    if (_vertexListDirty[(byte) blockTexture, r])
                    {
                        _vertexListDirty[(byte) blockTexture, r] = false;
                        Dictionary<int, bool> faceList = _faceMap[(byte) blockTexture, r];
                        _vertexBuffers[(byte) blockTexture, r] = CreateVertexBufferFromFaceList(faceList,
                                                                                               (byte) blockTexture, r);
                    }
        }

        // Create a dynamic vertex buffer. The arguments texture and region are used to flag a content reload if the device is lost.
        private DynamicVertexBuffer CreateVertexBufferFromFaceList(Dictionary<int, bool> faceList, int texture,
                                                                   int region)
        {
            if (faceList.Count == 0)
                return null;

            VertexPositionTextureShade[] vertexList = new VertexPositionTextureShade[faceList.Count*6];
            ulong vertexPointer = 0;
            foreach (int faceInfo in faceList.Keys)
            {
                BuildFaceVertices(ref vertexList, vertexPointer, faceInfo, texture);
                vertexPointer += 6;
            }
            DynamicVertexBuffer vertexBuffer = new DynamicVertexBuffer(_gameInstance.GraphicsDevice,
                                                                       vertexList.Length*
                                                                       VertexPositionTextureShade.SizeInBytes,
                                                                       BufferUsage.WriteOnly);
            vertexBuffer.ContentLost += vertexBuffer_ContentLost;
            vertexBuffer.Tag = new DynamicVertexBufferTag(this, texture, region);
            vertexBuffer.SetData(vertexList);
            return vertexBuffer;
        }

        private void vertexBuffer_ContentLost(object sender, EventArgs e)
        {
            DynamicVertexBuffer dvb = sender as DynamicVertexBuffer;
            if (dvb != null)
            {
                DynamicVertexBufferTag tag = (DynamicVertexBufferTag) dvb.Tag;
                tag.BlockEngine.MakeRegionDirty(tag.Texture, tag.Region);
            }
        }

        private void BuildFaceVertices(ref VertexPositionTextureShade[] vertexList, ulong vertexPointer, int faceInfo,
                                       int texture)
        {
            // Decode the face information.
            int x = 0, y = 0, z = 0;
            BlockFaceDirection faceDir = BlockFaceDirection.MAXIMUM;
            DecodeBlockFace(faceInfo, ref x, ref y, ref z, ref faceDir);

            // Check height.
            float height;

            switch ((BlockTexture) texture)
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
            if (!BlockInformation.IsPlantBlock((BlockTexture) texture))
            {
                switch (faceDir)
                {
                    case BlockFaceDirection.XIncreasing:
                        {
                            vertexList[vertexPointer + 0] =
                                new VertexPositionTextureShade(new Vector3(x + 1, y + height, z + 1), new Vector2(0, 0),
                                                               0.6);
                            vertexList[vertexPointer + 1] =
                                new VertexPositionTextureShade(new Vector3(x + 1, y + height, z), new Vector2(1, 0), 0.6);
                            vertexList[vertexPointer + 2] = new VertexPositionTextureShade(
                                new Vector3(x + 1, y, z + 1), new Vector2(0, 1), 0.6);
                            vertexList[vertexPointer + 3] = new VertexPositionTextureShade(
                                new Vector3(x + 1, y, z + 1), new Vector2(0, 1), 0.6);
                            vertexList[vertexPointer + 4] =
                                new VertexPositionTextureShade(new Vector3(x + 1, y + height, z), new Vector2(1, 0), 0.6);
                            vertexList[vertexPointer + 5] = new VertexPositionTextureShade(new Vector3(x + 1, y, z),
                                                                                           new Vector2(1, 1), 0.6);
                        }
                        break;


                    case BlockFaceDirection.XDecreasing:
                        {
                            vertexList[vertexPointer + 0] = new VertexPositionTextureShade(
                                new Vector3(x, y + height, z), new Vector2(0, 0), 0.6);
                            vertexList[vertexPointer + 1] =
                                new VertexPositionTextureShade(new Vector3(x, y + height, z + 1), new Vector2(1, 0), 0.6);
                            vertexList[vertexPointer + 2] = new VertexPositionTextureShade(new Vector3(x, y, z + 1),
                                                                                           new Vector2(1, 1), 0.6);
                            vertexList[vertexPointer + 3] = new VertexPositionTextureShade(
                                new Vector3(x, y + height, z), new Vector2(0, 0), 0.6);
                            vertexList[vertexPointer + 4] = new VertexPositionTextureShade(new Vector3(x, y, z + 1),
                                                                                           new Vector2(1, 1), 0.6);
                            vertexList[vertexPointer + 5] = new VertexPositionTextureShade(new Vector3(x, y, z),
                                                                                           new Vector2(0, 1), 0.6);
                        }
                        break;

                    case BlockFaceDirection.YIncreasing:
                        {
                            vertexList[vertexPointer + 0] = new VertexPositionTextureShade(
                                new Vector3(x, y + height, z), new Vector2(0, 1), 0.8);
                            vertexList[vertexPointer + 1] =
                                new VertexPositionTextureShade(new Vector3(x + 1, y + height, z), new Vector2(0, 0), 0.8);
                            vertexList[vertexPointer + 2] =
                                new VertexPositionTextureShade(new Vector3(x + 1, y + height, z + 1), new Vector2(1, 0),
                                                               0.8);
                            vertexList[vertexPointer + 3] = new VertexPositionTextureShade(
                                new Vector3(x, y + height, z), new Vector2(0, 1), 0.8);
                            vertexList[vertexPointer + 4] =
                                new VertexPositionTextureShade(new Vector3(x + 1, y + height, z + 1), new Vector2(1, 0),
                                                               0.8);
                            vertexList[vertexPointer + 5] =
                                new VertexPositionTextureShade(new Vector3(x, y + height, z + 1), new Vector2(1, 1), 0.8);
                        }
                        break;

                    case BlockFaceDirection.YDecreasing:
                        {
                            vertexList[vertexPointer + 0] = new VertexPositionTextureShade(
                                new Vector3(x + 1, y, z + 1), new Vector2(0, 0), 0.2);
                            vertexList[vertexPointer + 1] = new VertexPositionTextureShade(new Vector3(x + 1, y, z),
                                                                                           new Vector2(1, 0), 0.2);
                            vertexList[vertexPointer + 2] = new VertexPositionTextureShade(new Vector3(x, y, z + 1),
                                                                                           new Vector2(0, 1), 0.2);
                            vertexList[vertexPointer + 3] = new VertexPositionTextureShade(new Vector3(x, y, z + 1),
                                                                                           new Vector2(0, 1), 0.2);
                            vertexList[vertexPointer + 4] = new VertexPositionTextureShade(new Vector3(x + 1, y, z),
                                                                                           new Vector2(1, 0), 0.2);
                            vertexList[vertexPointer + 5] = new VertexPositionTextureShade(new Vector3(x, y, z),
                                                                                           new Vector2(1, 1), 0.2);
                        }
                        break;

                    case BlockFaceDirection.ZIncreasing:
                        {
                            vertexList[vertexPointer + 0] =
                                new VertexPositionTextureShade(new Vector3(x, y + height, z + 1), new Vector2(0, 0), 0.4);
                            vertexList[vertexPointer + 1] =
                                new VertexPositionTextureShade(new Vector3(x + 1, y + height, z + 1), new Vector2(1, 0),
                                                               0.4);
                            vertexList[vertexPointer + 2] = new VertexPositionTextureShade(
                                new Vector3(x + 1, y, z + 1), new Vector2(1, 1), 0.4);
                            vertexList[vertexPointer + 3] =
                                new VertexPositionTextureShade(new Vector3(x, y + height, z + 1), new Vector2(0, 0), 0.4);
                            vertexList[vertexPointer + 4] = new VertexPositionTextureShade(
                                new Vector3(x + 1, y, z + 1), new Vector2(1, 1), 0.4);
                            vertexList[vertexPointer + 5] = new VertexPositionTextureShade(new Vector3(x, y, z + 1),
                                                                                           new Vector2(0, 1), 0.4);
                        }
                        break;

                    case BlockFaceDirection.ZDecreasing:
                        {
                            vertexList[vertexPointer + 0] =
                                new VertexPositionTextureShade(new Vector3(x + 1, y + height, z), new Vector2(0, 0), 0.4);
                            vertexList[vertexPointer + 1] = new VertexPositionTextureShade(
                                new Vector3(x, y + height, z), new Vector2(1, 0), 0.4);
                            vertexList[vertexPointer + 2] = new VertexPositionTextureShade(new Vector3(x + 1, y, z),
                                                                                           new Vector2(0, 1), 0.4);
                            vertexList[vertexPointer + 3] = new VertexPositionTextureShade(new Vector3(x + 1, y, z),
                                                                                           new Vector2(0, 1), 0.4);
                            vertexList[vertexPointer + 4] = new VertexPositionTextureShade(
                                new Vector3(x, y + height, z), new Vector2(1, 0), 0.4);
                            vertexList[vertexPointer + 5] = new VertexPositionTextureShade(new Vector3(x, y, z),
                                                                                           new Vector2(1, 1), 0.4);
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
                            vertexList[vertexPointer + 0] =
                                new VertexPositionTextureShade(new Vector3(x + 0.5f, y + height, z + 1),
                                                               new Vector2(0, 0), 0.6);
                            vertexList[vertexPointer + 1] =
                                new VertexPositionTextureShade(new Vector3(x + 0.5f, y + height, z), new Vector2(1, 0),
                                                               0.6);
                            vertexList[vertexPointer + 2] =
                                new VertexPositionTextureShade(new Vector3(x + 0.5f, y, z + 1), new Vector2(0, 1), 0.6);
                            vertexList[vertexPointer + 3] =
                                new VertexPositionTextureShade(new Vector3(x + 0.5f, y, z + 1), new Vector2(0, 1), 0.6);
                            vertexList[vertexPointer + 4] =
                                new VertexPositionTextureShade(new Vector3(x + 0.5f, y + height, z), new Vector2(1, 0),
                                                               0.6);
                            vertexList[vertexPointer + 5] = new VertexPositionTextureShade(new Vector3(x + 0.5f, y, z),
                                                                                           new Vector2(1, 1), 0.6);
                        }
                        break;


                    case BlockFaceDirection.XDecreasing:
                        {
                            vertexList[vertexPointer + 0] =
                                new VertexPositionTextureShade(new Vector3(x + 0.5f, y + height, z), new Vector2(0, 0),
                                                               0.6);
                            vertexList[vertexPointer + 1] =
                                new VertexPositionTextureShade(new Vector3(x + 0.5f, y + height, z + 1),
                                                               new Vector2(1, 0), 0.6);
                            vertexList[vertexPointer + 2] =
                                new VertexPositionTextureShade(new Vector3(x + 0.5f, y, z + 1), new Vector2(1, 1), 0.6);
                            vertexList[vertexPointer + 3] =
                                new VertexPositionTextureShade(new Vector3(x + 0.5f, y + height, z), new Vector2(0, 0),
                                                               0.6);
                            vertexList[vertexPointer + 4] =
                                new VertexPositionTextureShade(new Vector3(x + 0.5f, y, z + 1), new Vector2(1, 1), 0.6);
                            vertexList[vertexPointer + 5] = new VertexPositionTextureShade(new Vector3(x + 0.5f, y, z),
                                                                                           new Vector2(0, 1), 0.6);
                        }
                        break;

                    case BlockFaceDirection.ZIncreasing:
                        {
                            vertexList[vertexPointer + 0] =
                                new VertexPositionTextureShade(new Vector3(x, y + height, z + 0.5f), new Vector2(0, 0),
                                                               0.4);
                            vertexList[vertexPointer + 1] =
                                new VertexPositionTextureShade(new Vector3(x + 1, y + height, z + 0.5f),
                                                               new Vector2(1, 0), 0.4);
                            vertexList[vertexPointer + 2] =
                                new VertexPositionTextureShade(new Vector3(x + 1, y, z + 0.5f), new Vector2(1, 1), 0.4);
                            vertexList[vertexPointer + 3] =
                                new VertexPositionTextureShade(new Vector3(x, y + height, z + 0.5f), new Vector2(0, 0),
                                                               0.4);
                            vertexList[vertexPointer + 4] =
                                new VertexPositionTextureShade(new Vector3(x + 1, y, z + 0.5f), new Vector2(1, 1), 0.4);
                            vertexList[vertexPointer + 5] = new VertexPositionTextureShade(new Vector3(x, y, z + 0.5f),
                                                                                           new Vector2(0, 1), 0.4);
                        }
                        break;

                    case BlockFaceDirection.ZDecreasing:
                        {
                            vertexList[vertexPointer + 0] =
                                new VertexPositionTextureShade(new Vector3(x + 1, y + height, z + 0.5f),
                                                               new Vector2(0, 0), 0.4);
                            vertexList[vertexPointer + 1] =
                                new VertexPositionTextureShade(new Vector3(x, y + height, z + 0.5f), new Vector2(1, 0),
                                                               0.4);
                            vertexList[vertexPointer + 2] =
                                new VertexPositionTextureShade(new Vector3(x + 1, y, z + 0.5f), new Vector2(0, 1), 0.4);
                            vertexList[vertexPointer + 3] =
                                new VertexPositionTextureShade(new Vector3(x + 1, y, z + 0.5f), new Vector2(0, 1), 0.4);
                            vertexList[vertexPointer + 4] =
                                new VertexPositionTextureShade(new Vector3(x, y + height, z + 0.5f), new Vector2(1, 0),
                                                               0.4);
                            vertexList[vertexPointer + 5] = new VertexPositionTextureShade(new Vector3(x, y, z + 0.5f),
                                                                                           new Vector2(1, 1), 0.4);
                        }
                        break;
                }
            }
        }

        private void _AddBlock(int x, int y, int z, BlockFaceDirection dir, BlockType type, int x2, int y2, int z2,
                               BlockFaceDirection dir2)
        {
            BlockType type2 = BlockList[x2, y2, z2];
            if (!BlockInformation.IsTransparentBlock(type) && !BlockInformation.IsTransparentBlock(type2) &&
                type == type2)
                HideQuad(x2, y2, z2, dir2, type2);
            else if ((type2 == BlockType.Water) && type2 == type)
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
            if (x <= 0 || y <= 0 || z <= 0 || x >= _mapX - 1 || y >= _mapY - 1 || z >= _mapZ - 1)
                return;

            BlockList[x, y, z] = blockType;

            _AddBlock(x, y, z, BlockFaceDirection.XIncreasing, blockType, x + 1, y, z, BlockFaceDirection.XDecreasing);
            _AddBlock(x, y, z, BlockFaceDirection.XDecreasing, blockType, x - 1, y, z, BlockFaceDirection.XIncreasing);
            _AddBlock(x, y, z, BlockFaceDirection.YIncreasing, blockType, x, y + 1, z, BlockFaceDirection.YDecreasing);
            _AddBlock(x, y, z, BlockFaceDirection.YDecreasing, blockType, x, y - 1, z, BlockFaceDirection.YIncreasing);
            _AddBlock(x, y, z, BlockFaceDirection.ZIncreasing, blockType, x, y, z + 1, BlockFaceDirection.ZDecreasing);
            _AddBlock(x, y, z, BlockFaceDirection.ZDecreasing, blockType, x, y, z - 1, BlockFaceDirection.ZIncreasing);
        }

        private void _RemoveBlock(int x, int y, int z, BlockFaceDirection dir, int x2, int y2, int z2,
                                  BlockFaceDirection dir2)
        {
            BlockType type = BlockList[x, y, z];
            BlockType type2 = BlockList[x2, y2, z2];
            if (!BlockInformation.IsTransparentBlock(type) && !BlockInformation.IsTransparentBlock(type2) &&
                type == type2)
                ShowQuad(x2, y2, z2, dir2, type2);
            else
                HideQuad(x, y, z, dir, type);
        }

        public void RemoveBlock(int x, int y, int z)
        {
            if (x <= 0 || y <= 0 || z <= 0 || x >= _mapX - 1 || y >= _mapY - 1 || z >= _mapZ - 1)
                return;

            _RemoveBlock(x, y, z, BlockFaceDirection.XIncreasing, x + 1, y, z, BlockFaceDirection.XDecreasing);
            _RemoveBlock(x, y, z, BlockFaceDirection.XDecreasing, x - 1, y, z, BlockFaceDirection.XIncreasing);
            _RemoveBlock(x, y, z, BlockFaceDirection.YIncreasing, x, y + 1, z, BlockFaceDirection.YDecreasing);
            _RemoveBlock(x, y, z, BlockFaceDirection.YDecreasing, x, y - 1, z, BlockFaceDirection.YIncreasing);
            _RemoveBlock(x, y, z, BlockFaceDirection.ZIncreasing, x, y, z + 1, BlockFaceDirection.ZDecreasing);
            _RemoveBlock(x, y, z, BlockFaceDirection.ZDecreasing, x, y, z - 1, BlockFaceDirection.ZIncreasing);

            BlockList[x, y, z] = BlockType.None;
        }

        private int EncodeBlockFace(int x, int y, int z, BlockFaceDirection faceDir)
        {
            //TODO: OPTIMIZE BY HARD CODING VALUES IN
            return (x + y*_mapX + z*_mapY*_mapZ + (byte) faceDir*_mapX*_mapY*_mapZ);
        }

        private void DecodeBlockFace(int faceCode, ref int x, ref int y, ref int z, ref BlockFaceDirection faceDir)
        {
            x = (faceCode%_mapX);
            faceCode = (faceCode - x)/_mapX;
            y = (faceCode%_mapY);
            faceCode = (faceCode - y)/_mapY;
            z = (faceCode%_mapZ);
            faceCode = (faceCode - z)/_mapZ;
            faceDir = (BlockFaceDirection) faceCode;
        }

        // Returns the region that a block at (x,y,z) should belong in.
        private int GetRegion(int x, int y, int z)
        {
            return (x/Regionsize + (y/Regionsize)*Regionratio + (z/Regionsize)*Regionratio*Regionratio);
        }

        private Vector3 GetRegionCenter(int regionNumber)
        {
            int x = regionNumber%Regionratio;
            regionNumber = (regionNumber - x)/Regionratio;
            int y = regionNumber%Regionratio;
            regionNumber = (regionNumber - y)/Regionratio;
            int z = regionNumber;
            return new Vector3(x*Regionsize + Regionsize/2, y*Regionsize + Regionsize/2, z*Regionsize + Regionsize/2);
        }

        private void ShowQuad(int x, int y, int z, BlockFaceDirection faceDir, BlockType blockType)
        {
            BlockTexture blockTexture = _blockTextureMap[(byte) blockType, (byte) faceDir];
            int blockFace = EncodeBlockFace(x, y, z, faceDir);
            int region = GetRegion(x, y, z);
            if (!_faceMap[(byte) blockTexture, region].ContainsKey(blockFace))
                _faceMap[(byte) blockTexture, region].Add(blockFace, true);
            _vertexListDirty[(byte) blockTexture, region] = true;
        }

        private void HideQuad(int x, int y, int z, BlockFaceDirection faceDir, BlockType blockType)
        {
            BlockTexture blockTexture = _blockTextureMap[(byte) blockType, (byte) faceDir];
            int blockFace = EncodeBlockFace(x, y, z, faceDir);
            int region = GetRegion(x, y, z);
            if (_faceMap[(byte) blockTexture, region].ContainsKey(blockFace))
                _faceMap[(byte) blockTexture, region].Remove(blockFace);
            _vertexListDirty[(byte) blockTexture, region] = true;
        }

        public struct DynamicVertexBufferTag
        {
            public BlockEngine BlockEngine;
            public int Region;
            public int Texture;

            public DynamicVertexBufferTag(BlockEngine blockEngine, int texture, int region)
            {
                BlockEngine = blockEngine;
                Texture = texture;
                Region = region;
            }
        }
    }
}