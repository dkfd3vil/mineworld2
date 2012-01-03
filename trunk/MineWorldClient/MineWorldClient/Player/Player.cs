﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using MineWorldData;

namespace MineWorld
{
    public class Player
    {
        //I actually did split up public and private, just to keep things simple outside of this class file
        private const float Movespeed = 3.5f;
        private const float Gravity = -16.0f;

        public int Myid;
        public string Name;
        public Camera Cam;
        public bool mousehasfoccus;
        public Vector3 Position;
        public Vector3 vTestVector;
        public float Speed = 0.5f;
        public float Sensitivity = 1;
        public Vector3 vAim;
        public Vector3 vAimBlock;
        public VertexPositionTexture[] rFaceMarker;
        public Vector3 vPlayerVel;
        public bool bUnderwater;
        public bool NoClip = true;
        public bool Debug = false;
        public int selectedblock = 0;
        public BlockTypes selectedblocktype = BlockTypes.Air;

        private PropertyBag game;

        //Drawing related vars
        SpriteFont myFont;
        Effect effect;
        Texture2D tGui;
        Texture2D tWaterOverlay;
        Texture2D tVignette;

        //Constructor
        public Player(PropertyBag gameIn)
        {
            Cam = new Camera(gameIn, new Vector3(0, 64, 0), new Vector3(0, 0, 0));
            game = gameIn;
            Position = new Vector3(32, 128, 32);
        }

        public void Load()
        {
            effect = game.GameManager.conmanager.Load<Effect>("Effects/DefaultEffect");
            myFont = game.GameManager.conmanager.Load<SpriteFont>("Fonts/DefaultFont");
            tGui = game.GameManager.conmanager.Load<Texture2D>("Textures/gui");
            tWaterOverlay = game.GameManager.conmanager.Load<Texture2D>("Textures/water");
            tVignette = game.GameManager.conmanager.Load<Texture2D>("Textures/vignette");
        }

        //Main update method, is called from Game1.cs's update method
        public void Update(GameTime gtime, InputHelper input)
        {
            if (NoClip) //If noclipped
            {
                if (input.IsCurPress(Keys.LeftShift))
                {
                    Speed = 1.0f;
                }
                else
                {
                    Speed = 0.5f;
                }
                if (input.IsCurPress(Keys.W))
                {
                    Position -= Cam.Forward * Speed;
                }
                if (input.IsCurPress(Keys.A))
                {
                    Position -= Cam.Right * Speed;
                }
                if (input.IsCurPress(Keys.S))
                {
                    Position += Cam.Forward * Speed;
                }
                if (input.IsCurPress(Keys.D))
                {
                    Position += Cam.Right * Speed;
                }
                if (input.IsCurPress(Keys.Space))
                {
                    Position += Vector3.Up * Speed;
                }
                if (input.IsCurPress(Keys.LeftControl))
                {
                    Position += Vector3.Down * Speed;
                }
            }
            else
            {
                if (input.IsCurPress(Keys.W))
                {
                    Position -= Cam.Forward * Speed;
                }
                if (input.IsCurPress(Keys.A))
                {
                    Position -= Cam.Right * Speed;
                }
                if (input.IsCurPress(Keys.S))
                {
                    Position += Cam.Forward * Speed;
                }
                if (input.IsCurPress(Keys.D))
                {
                    Position += Cam.Right * Speed;
                }

                //Execute standard movement
                Vector3 footPosition = Position + new Vector3(0f, -1.5f, 0f);
                Vector3 headPosition = Position + new Vector3(0f, 0.1f, 0f);
                Vector3 midPosition = Position + new Vector3(0f, -0.7f, 0f);

                if (game.WorldManager.BlockAtPoint(headPosition) == BlockTypes.Water)
                {
                    bUnderwater = true;
                }
                else
                {
                    bUnderwater = false;
                }

                vPlayerVel.Y += Gravity * (float)gtime.ElapsedGameTime.TotalSeconds;

                if (game.WorldManager.SolidAtPointForPlayer(footPosition) || game.WorldManager.SolidAtPointForPlayer(headPosition))
                {
                    BlockTypes standingOnBlock = game.WorldManager.BlockAtPoint(footPosition);
                    BlockTypes hittingHeadOnBlock = game.WorldManager.BlockAtPoint(headPosition);

                    // If the player has their head stuck in a block, push them down.
                    if (game.WorldManager.SolidAtPointForPlayer(headPosition))
                    {
                        int blockIn = (int)(headPosition.Y);
                        Position.Y = (blockIn - 0.15f);
                    }

                    // If the player is stuck in the ground, bring them out.
                    // This happens because we're standing on a block at -1.5, but stuck in it at -1.4, so -1.45 is the sweet spot.
                    if (game.WorldManager.SolidAtPointForPlayer(footPosition))
                    {
                        int blockOn = (int)(footPosition.Y);
                        Position.Y = (float)(blockOn + 1 + 1.45);
                    }

                    vPlayerVel.Y = 0;
                }
            }

            //Re-initialize aim and aimblock vectors
            vAim = new Vector3();
            vAimBlock = new Vector3();

            //Check along a path stemming from the camera's forward if there is a collision
            for (float i = 0; i <= 6; i += 0.01f)
            {
                if (i < 5.5f)
                {
                    vAim = Cam.Position - Cam.Forward * i;
                    try
                    {
                        if (game.WorldManager.BlockMap[(int)Math.Floor(vAim.X), (int)Math.Floor(vAim.Y), (int)Math.Floor(vAim.Z)].AimSolid)
                        {
                            break; //If there is, break the loop with the current aim vector
                        }
                    }
                    catch
                    {
                        vAim = new Vector3(-10, -10, -10);
                        break;
                    }
                }
                else
                {
                    vAim = new Vector3(-10, -10, -10);
                }
            } //Otherwise set it to be an empty vector

            if (vAim != new Vector3(-10, -10, -10))
            {
                vAimBlock = new Vector3((int)Math.Floor(vAim.X), (int)Math.Floor(vAim.Y), (int)Math.Floor(vAim.Z)); //Get the aim block based off of that aim vector
            }


            //cam.Position = vPosition + new Vector3(0, 1.167f, 0); //Set camera position to be player position plus 7/6ths on the z axis
            Cam.Position = Position;

            if (game.GameManager.WindowHasFocus())
            {
                if (mousehasfoccus)
                {
                    Cam.Rotate( //Rotate the camera based off of mouse position, set mouse position to be screen center
                        MathHelper.ToRadians((input.MousePosition.Y - game.GameManager.device.DisplayMode.Height / 2) * Sensitivity * 0.1f),
                        MathHelper.ToRadians((input.MousePosition.X - game.GameManager.device.DisplayMode.Width / 2) * Sensitivity * 0.1f),
                        0.0f
                        );
                }
                else
                {
                    mousehasfoccus = true;
                }
                Mouse.SetPosition(game.GameManager.device.DisplayMode.Width / 2, game.GameManager.device.DisplayMode.Height / 2);
            }
            else
            {
                mousehasfoccus = false;
            }

            CreateFaceMarker(); //Create the face marker's vertices - I need to redo this method

            if (input.IsNewPress(MouseButtons.LeftButton))
            {
                if (GotSelection())
                {
                    if (selectedblocktype == BlockTypes.Air)
                    {
                        game.WorldManager.SetBlock(vAimBlock, BlockTypes.Air);
                    }
                    else
                    {
                        game.WorldManager.SetBlock(GetFacingBlock(), selectedblocktype);
                    }
                    
                }
            }

            if (input.IsNewPress(MouseButtons.RightButton))
            {
                if (GotSelection())
                {
                    game.WorldManager.UseBlock(vAimBlock);
                }
            }

            if (input.IsNewPress(Keys.Left))
            {
                selectedblock -= 1;
                if (selectedblock < 0)
                {
                    selectedblock = 0;
                }
                selectedblocktype = (BlockTypes)selectedblock;
            }

            if (input.IsNewPress(Keys.Right))
            {
                selectedblock += 1;
                if (selectedblock > 63)
                {
                    selectedblock = 63;
                }
                selectedblocktype = (BlockTypes)selectedblock;
            }

            if (input.IsNewPress(Keys.F1))
            {
                Debug = !Debug;
            }

            //Update our camera
            Cam.Update();
        }


        public void Draw()
        {
            if (rFaceMarker != null)
            {
                //FACE MARKER
                //Set world matrix to be default
                effect.Parameters["World"].SetValue(Matrix.Identity);
                //Set the texture to be the gui texture
                effect.Parameters["myTexture"].SetValue(tGui);

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    game.GameManager.device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, rFaceMarker, 0, 2); //Draw the face marker
                }
            }

            //////////////////////
            //   START THE 2D   //
            //////////////////////

            game.GameManager.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone); //Start 2d rendering
            if (bUnderwater) //If underwater, apply several overlays
            {
                game.GameManager.spriteBatch.Draw(tWaterOverlay, new Rectangle(0, 0, game.GameManager.device.DisplayMode.Width, game.GameManager.device.DisplayMode.Height), Color.DarkBlue);
                game.GameManager.spriteBatch.Draw(tWaterOverlay, new Rectangle(0, 0, game.GameManager.device.DisplayMode.Width, game.GameManager.device.DisplayMode.Height), Color.White);
                game.GameManager.spriteBatch.Draw(tVignette, new Rectangle(0, 0, game.GameManager.device.DisplayMode.Width, game.GameManager.device.DisplayMode.Height), Color.White);
            }

            //Cursor!
            game.GameManager.spriteBatch.Draw(tGui, new Rectangle(game.GameManager.graphics.PreferredBackBufferWidth / 2 - 16, game.GameManager.graphics.PreferredBackBufferHeight / 2 - 16, 32, 32), new Rectangle(0, 0, 32, 32), Color.White);

            if (Debug)
            {
                game.GameManager.spriteBatch.DrawString(myFont, Position.ToString(), new Vector2(0, 0), Color.Black);
                game.GameManager.spriteBatch.DrawString(myFont, selectedblocktype.ToString(), new Vector2(0, 15), Color.Black);
            }

            game.GameManager.spriteBatch.End();
        }

        public bool GotSelection()
        {
            if (vAim == new Vector3(-10, -10, -10))
            {
                return false;
            }

            return true;
        }

        private bool TryToMoveTo(Vector3 moveVector)
        {
            // Build a "test vector" that is a little longer than the move vector.
            float moveLength = moveVector.Length();
            Vector3 testVector = moveVector;
            testVector.Normalize();
            testVector = testVector * (moveLength); // + 0.1f);

            // Apply this test vector.
            Vector3 movePosition = Position + testVector;
            Vector3 midBodyPoint = movePosition + new Vector3(0, -0.7f, 0);
            Vector3 lowerBodyPoint = movePosition + new Vector3(0, -1.4f, 0);

            if (!game.WorldManager.SolidAtPointForPlayer(movePosition) &&
                !game.WorldManager.SolidAtPointForPlayer(lowerBodyPoint) &&
                !game.WorldManager.SolidAtPointForPlayer(midBodyPoint))
            {
                testVector = moveVector;
                testVector.Normalize();
                testVector = testVector * (moveLength + 0.11f);
                // Makes sure the camera doesnt move too close to the block ;)
                movePosition = Position + testVector;
                midBodyPoint = movePosition + new Vector3(0, -0.7f, 0);
                lowerBodyPoint = movePosition + new Vector3(0, -1.4f, 0);

                if (!game.WorldManager.SolidAtPointForPlayer(movePosition) &&
                    !game.WorldManager.SolidAtPointForPlayer(lowerBodyPoint) &&
                    !game.WorldManager.SolidAtPointForPlayer(midBodyPoint))
                {
                    //vPosition = vPosition + moveVector;
                    return true;
                }
            }
            return false;
        }

        public void CreateFaceMarker()
        {
            //Create the face marker's vertices - I need to redo this method

            Vector3 vDifference = GetFacingBlock() - vAimBlock; //Get a difference vector to see where the facing block is local to the aim block
            rFaceMarker = new VertexPositionTexture[6]; //Initialize the array of vectors - we only need 6 to make a square

            //Check the differences and draw the face marker accordingly
            if (vDifference.X == -1)
            {
                rFaceMarker[0] = new VertexPositionTexture(new Vector3(vAimBlock.X - 0.01f, vAimBlock.Y + 1, vAimBlock.Z), new Vector2(0.5f, 0));
                rFaceMarker[1] = new VertexPositionTexture(new Vector3(vAimBlock.X - 0.01f, vAimBlock.Y + 1, vAimBlock.Z + 1), new Vector2(1, 0));
                rFaceMarker[2] = new VertexPositionTexture(new Vector3(vAimBlock.X - 0.01f, vAimBlock.Y, vAimBlock.Z), new Vector2(0.5f, 1));
                rFaceMarker[3] = new VertexPositionTexture(new Vector3(vAimBlock.X - 0.01f, vAimBlock.Y + 1, vAimBlock.Z + 1), new Vector2(1, 0));
                rFaceMarker[4] = new VertexPositionTexture(new Vector3(vAimBlock.X - 0.01f, vAimBlock.Y, vAimBlock.Z + 1), new Vector2(1, 1));
                rFaceMarker[5] = new VertexPositionTexture(new Vector3(vAimBlock.X - 0.01f, vAimBlock.Y, vAimBlock.Z), new Vector2(0.5f, 1));
            }
            else if (vDifference.X == 1)
            {
                rFaceMarker[0] = new VertexPositionTexture(new Vector3(vAimBlock.X + 1.01f, vAimBlock.Y + 1, vAimBlock.Z + 1), new Vector2(0.5f, 0));
                rFaceMarker[1] = new VertexPositionTexture(new Vector3(vAimBlock.X + 1.01f, vAimBlock.Y + 1, vAimBlock.Z), new Vector2(1, 0));
                rFaceMarker[2] = new VertexPositionTexture(new Vector3(vAimBlock.X + 1.01f, vAimBlock.Y, vAimBlock.Z + 1), new Vector2(0.5f, 1));
                rFaceMarker[3] = new VertexPositionTexture(new Vector3(vAimBlock.X + 1.01f, vAimBlock.Y + 1, vAimBlock.Z), new Vector2(1, 0));
                rFaceMarker[4] = new VertexPositionTexture(new Vector3(vAimBlock.X + 1.01f, vAimBlock.Y, vAimBlock.Z), new Vector2(1, 1));
                rFaceMarker[5] = new VertexPositionTexture(new Vector3(vAimBlock.X + 1.01f, vAimBlock.Y, vAimBlock.Z + 1), new Vector2(0.5f, 1));
            }
            else if (vDifference.Y == -1)
            {
                rFaceMarker[0] = new VertexPositionTexture(new Vector3(vAimBlock.X + 1, vAimBlock.Y - 0.01f, vAimBlock.Z + 1), new Vector2(0.5f, 0));
                rFaceMarker[1] = new VertexPositionTexture(new Vector3(vAimBlock.X + 1, vAimBlock.Y - 0.01f, vAimBlock.Z), new Vector2(1, 0));
                rFaceMarker[2] = new VertexPositionTexture(new Vector3(vAimBlock.X, vAimBlock.Y - 0.01f, vAimBlock.Z + 1), new Vector2(0.5f, 1));
                rFaceMarker[3] = new VertexPositionTexture(new Vector3(vAimBlock.X + 1, vAimBlock.Y - 0.01f, vAimBlock.Z), new Vector2(1, 0));
                rFaceMarker[4] = new VertexPositionTexture(new Vector3(vAimBlock.X, vAimBlock.Y - 0.01f, vAimBlock.Z), new Vector2(1, 1));
                rFaceMarker[5] = new VertexPositionTexture(new Vector3(vAimBlock.X, vAimBlock.Y - 0.01f, vAimBlock.Z + 1), new Vector2(0.5f, 1));
            }
            else if (vDifference.Y == 1)
            {
                rFaceMarker[0] = new VertexPositionTexture(new Vector3(vAimBlock.X + 1, vAimBlock.Y + 1.01f, vAimBlock.Z), new Vector2(0.5f, 0));
                rFaceMarker[1] = new VertexPositionTexture(new Vector3(vAimBlock.X + 1, vAimBlock.Y + 1.01f, vAimBlock.Z + 1), new Vector2(1, 0));
                rFaceMarker[2] = new VertexPositionTexture(new Vector3(vAimBlock.X, vAimBlock.Y + 1.01f, vAimBlock.Z), new Vector2(0.5f, 1));
                rFaceMarker[3] = new VertexPositionTexture(new Vector3(vAimBlock.X + 1, vAimBlock.Y + 1.01f, vAimBlock.Z + 1), new Vector2(1, 0));
                rFaceMarker[4] = new VertexPositionTexture(new Vector3(vAimBlock.X, vAimBlock.Y + 1.01f, vAimBlock.Z + 1), new Vector2(1, 1));
                rFaceMarker[5] = new VertexPositionTexture(new Vector3(vAimBlock.X, vAimBlock.Y + 1.01f, vAimBlock.Z), new Vector2(0.5f, 1));
            }
            else if (vDifference.Z == 1)
            {
                rFaceMarker[0] = new VertexPositionTexture(new Vector3(vAimBlock.X, vAimBlock.Y + 1, vAimBlock.Z + 1.01f), new Vector2(0.5f, 0));
                rFaceMarker[1] = new VertexPositionTexture(new Vector3(vAimBlock.X + 1, vAimBlock.Y + 1, vAimBlock.Z + 1.01f), new Vector2(1, 0));
                rFaceMarker[2] = new VertexPositionTexture(new Vector3(vAimBlock.X, vAimBlock.Y, vAimBlock.Z + 1.01f), new Vector2(0.5f, 1));
                rFaceMarker[3] = new VertexPositionTexture(new Vector3(vAimBlock.X + 1, vAimBlock.Y + 1, vAimBlock.Z + 1.01f), new Vector2(1, 0));
                rFaceMarker[4] = new VertexPositionTexture(new Vector3(vAimBlock.X + 1, vAimBlock.Y, vAimBlock.Z + 1.01f), new Vector2(1, 1));
                rFaceMarker[5] = new VertexPositionTexture(new Vector3(vAimBlock.X, vAimBlock.Y, vAimBlock.Z + 1.01f), new Vector2(0.5f, 1));
            }
            else if (vDifference.Z == -1)
            {
                rFaceMarker[0] = new VertexPositionTexture(new Vector3(vAimBlock.X + 1, vAimBlock.Y + 1, vAimBlock.Z - 0.01f), new Vector2(0.5f, 0));
                rFaceMarker[1] = new VertexPositionTexture(new Vector3(vAimBlock.X, vAimBlock.Y + 1, vAimBlock.Z - 0.01f), new Vector2(1, 0));
                rFaceMarker[2] = new VertexPositionTexture(new Vector3(vAimBlock.X + 1, vAimBlock.Y, vAimBlock.Z - 0.01f), new Vector2(0.5f, 1));
                rFaceMarker[3] = new VertexPositionTexture(new Vector3(vAimBlock.X, vAimBlock.Y + 1, vAimBlock.Z - 0.01f), new Vector2(1, 0));
                rFaceMarker[4] = new VertexPositionTexture(new Vector3(vAimBlock.X, vAimBlock.Y, vAimBlock.Z - 0.01f), new Vector2(1, 1));
                rFaceMarker[5] = new VertexPositionTexture(new Vector3(vAimBlock.X + 1, vAimBlock.Y, vAimBlock.Z - 0.01f), new Vector2(0.5f, 1));
            }
        }

        public Vector3 GetFacingBlock()
        {
            //Initialize vectors and a float which will be used to sort out which axis is most different
            Vector3 vDifference = new Vector3();
            Vector3 vFacingBlock = new Vector3();
            float tempcomp = 0;
            vDifference = vAim - vAimBlock - new Vector3(0.5f, 0.5f, 0.5f); //Get aim vec local to aim block

            //This method works by getting on which axis the local aim position is greatest - i.e. which face the cursor is on
            if (Math.Abs(vDifference.X) > Math.Abs(tempcomp))
            {
                tempcomp = vDifference.X;
                vFacingBlock = new Vector3(vAimBlock.X + Math.Sign(vDifference.X), vAimBlock.Y, vAimBlock.Z);
            }
            if (Math.Abs(vDifference.Y) > Math.Abs(tempcomp))
            {
                tempcomp = vDifference.Y;
                vFacingBlock = new Vector3(vAimBlock.X, vAimBlock.Y + Math.Sign(vDifference.Y), vAimBlock.Z);
            }
            if (Math.Abs(vDifference.Z) > Math.Abs(tempcomp))
            {
                tempcomp = vDifference.Z;
                vFacingBlock = new Vector3(vAimBlock.X, vAimBlock.Y, vAimBlock.Z + Math.Sign(vDifference.Z));
            }

            return vFacingBlock;
        }
    }
}