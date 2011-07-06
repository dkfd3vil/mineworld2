using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using StateMasher;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Lidgren.Network;

namespace MineWorld.States
{
    public class MainGameState : State
    {
        const float MOVESPEED = 3.5f;
        const float GRAVITY = -16.0f;
        const float JUMPVELOCITY = 6.0f;
        const float CLIMBVELOCITY = 2.5f;
        const float DIEVELOCITY = 20.0f;

        string nextState = null;
        bool mouseInitialized = false;

        public override void OnEnter(string oldState)
        {
            _SM.IsMouseVisible = false;
        }

        public override void OnLeave(string newState)
        {
            _P.chatEntryBuffer = "";
            _P.chatMode = ChatMessageType.None;
        }

        public override string OnUpdate(GameTime gameTime, KeyboardState keyState, MouseState mouseState)
        {
            // Update network stuff.
            (_SM as MineWorldGame).UpdateNetwork(gameTime);

            // Update the current screen effect.
            _P.screenEffectCounter += gameTime.ElapsedGameTime.TotalSeconds;

            // Update engines.
            _P.skyEngine.Update(gameTime);
            _P.playerEngine.Update(gameTime);
            _P.blockEngine.Update(gameTime);
            _P.particleEngine.Update(gameTime);
            _P.interfaceEngine.Update(gameTime);

            // Moving the mouse changes where we look.
            if (_SM.WindowHasFocus())
            {
                if (mouseInitialized)
                {
                    int dx = mouseState.X - _SM.GraphicsDevice.Viewport.Width / 2;
                    int dy = mouseState.Y - _SM.GraphicsDevice.Viewport.Height / 2;

                    if ((_SM as MineWorldGame).Csettings.InvertMouseYAxis)
                        dy = -dy;

                    _P.playerCamera.Yaw -= (float)dx * (_P.mouseSensitivity);
                    _P.playerCamera.Pitch = (float)Math.Min(Math.PI * 0.49, Math.Max(-Math.PI * 0.49, _P.playerCamera.Pitch - dy * (_P.mouseSensitivity)));
                }
                else
                {
                    mouseInitialized = true;
                }
                Mouse.SetPosition(_SM.GraphicsDevice.Viewport.Width / 2, _SM.GraphicsDevice.Viewport.Height / 2);
            }
            else
                mouseInitialized = false;

            // Update the player's position.
            if (!_P.playerDead)
                UpdatePlayerPosition(gameTime, keyState);

            // Update the camera regardless of if we're alive or not.
            _P.UpdateCamera(gameTime);

            TimeSpan timespanhearthbeat = DateTime.Now - _P.lasthearthbeatreceived;
            if (timespanhearthbeat.TotalMilliseconds > 5000)
            {
                // The server crashed lets exit
                nextState = "MineWorld.States.ServerBrowserState";
            }

            return nextState;
        }

        private void UpdatePlayerPosition(GameTime gameTime, KeyboardState keyState)
        {
            // Apply "gravity".
            Vector3 footPosition = _P.playerPosition + new Vector3(0f, -1.5f, 0f);
            Vector3 headPosition = _P.playerPosition + new Vector3(0f, 0.1f, 0f);
            Vector3 midPosition = _P.playerPosition + new Vector3(0f, -0.7f, 0f);

            if (_P.blockEngine.BlockAtPoint(footPosition) == BlockType.Water || _P.blockEngine.BlockAtPoint(headPosition) == BlockType.Water || _P.blockEngine.BlockAtPoint(midPosition) == BlockType.Water)
            {
                _P.swimming = true;

                if (_P.blockEngine.BlockAtPoint(headPosition) == BlockType.Water)
                {
                    if (_P.playerHoldBreath == 20)
                    {
                        _P.playerVelocity.Y *= 0.2f;
                    }
                    if (_P.playerHoldBreath > 9)
                    {
                        _P.screenEffect = ScreenEffect.Water;
                        _P.screenEffectCounter = 0.5;
                    }
                    else
                    {
                        _P.screenEffect = ScreenEffect.Drown;
                        _P.screenEffectCounter = 0.5;
                    }
                }
                else
                {
                    _P.playerHoldBreath = 20;
                }
            }
            else
            {
                _P.swimming = false;
                _P.lastBreath = DateTime.Now;
                _P.playerHoldBreath = 20;
            }

            if (_P.swimming)
            {
                TimeSpan timeSpan = DateTime.Now - _P.lastBreath;
                _P.playerVelocity.Y += (GRAVITY / 8) * (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (timeSpan.TotalMilliseconds > 500)
                {
                    _P.lastBreath = DateTime.Now;
                    _P.playerHoldBreath -= 1;
                    if (_P.playerHoldBreath <= 0)
                    {
                        _P.SendPlayerHurt(20, false);
                    }
                    //_P.PlaySoundForEveryone(MineWorldSound.Death, _P.playerPosition);
                }
            }
            else
            {
                _P.playerVelocity.Y += GRAVITY * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            // Let the server know and then move on
            _P.SendPlayerUpdate();

            if (_P.blockEngine.SolidAtPointForPlayer(footPosition) || _P.blockEngine.SolidAtPointForPlayer(headPosition))
            {
                BlockType standingOnBlock = _P.blockEngine.BlockAtPoint(footPosition);
                BlockType hittingHeadOnBlock = _P.blockEngine.BlockAtPoint(headPosition);

                // If we"re hitting the ground with a high velocity, die!
                // Except if we have godmode ;)
                if (standingOnBlock != BlockType.None && _P.playerVelocity.Y < 0)
                {
                    float fallDamage = Math.Abs(_P.playerVelocity.Y) / DIEVELOCITY;
                    if (fallDamage >= 0.5)
                    {
                        // Fall damage of 0.5 maps to a screenEffectCounter value of 2, meaning that the effect doesn't appear.
                        // Fall damage of 1.0 maps to a screenEffectCounter value of 0, making the effect very strong.
                        _P.screenEffect = ScreenEffect.Fall;
                        _P.screenEffectCounter = 2 - (fallDamage - 0.5) * 4;
                        _P.PlaySoundForEveryone(MineWorldSound.GroundHit, _P.playerPosition);
                    }
                }
                else
                {
                    _P.PlaySoundForEveryone(MineWorldSound.GroundHit, _P.playerPosition);
                }

                if (_P.blockEngine.SolidAtPointForPlayer(midPosition))
                {
                    //_P.KillPlayer(Defines.deathByCrush);
                    _P.SendPlayerHurt(100, false);
                }

                // If the player has their head stuck in a block, push them down.
                if (_P.blockEngine.SolidAtPointForPlayer(headPosition))
                {
                    int blockIn = (int)(headPosition.Y);
                    _P.playerPosition.Y = (float)(blockIn - 0.15f);
                }

                // If the player is stuck in the ground, bring them out.
                // This happens because we're standing on a block at -1.5, but stuck in it at -1.4, so -1.45 is the sweet spot.
                if (_P.blockEngine.SolidAtPointForPlayer(footPosition))
                {
                    int blockOn = (int)(footPosition.Y);
                    _P.playerPosition.Y = (float)(blockOn + 1 + 1.45);
                }

                _P.playerVelocity.Y = 0;

                // Logic for standing on a block.
                switch (standingOnBlock)
                {
                    case BlockType.Lava:
                        _P.SendPlayerHurt(100, false);
                        break;
                }

                // Logic for bumping your head on a block.
                switch (hittingHeadOnBlock)
                {
                    case BlockType.Lava:
                        _P.SendPlayerHurt(100, false);
                        break;
                }
            }
            if (!_P.blockEngine.SolidAtPointForPlayer(midPosition + _P.playerVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds))
            {
                _P.playerPosition += _P.playerVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            // Death by falling off the map.
            if (_P.playerPosition.Y < - Defines.MAPSIZE/2)
            {
                _P.SendPlayerHurt(100,false);
                //_P.KillPlayer(Defines.deathByMiss);
                return;
            }

            // Pressing forward moves us in the direction we"re looking.
            Vector3 moveVector = Vector3.Zero;

            if (_P.chatMode == ChatMessageType.None)
            {
                if ((_SM as MineWorldGame).keyBinds.IsPressed(KeyBoardButtons.Forward))//keyState.IsKeyDown(Keys.W))
                    moveVector += _P.playerCamera.GetLookVector();
                if ((_SM as MineWorldGame).keyBinds.IsPressed(KeyBoardButtons.Backward))//keyState.IsKeyDown(Keys.S))
                    moveVector -= _P.playerCamera.GetLookVector();
                if ((_SM as MineWorldGame).keyBinds.IsPressed(KeyBoardButtons.Right))//keyState.IsKeyDown(Keys.D))
                    moveVector += _P.playerCamera.GetRightVector();
                if ((_SM as MineWorldGame).keyBinds.IsPressed(KeyBoardButtons.Left))//keyState.IsKeyDown(Keys.A))
                    moveVector -= _P.playerCamera.GetRightVector();
                //Sprinting
                if ((_SM as MineWorldGame).keyBinds.IsPressed(KeyBoardButtons.Sprint))//keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift))
                    _P.sprinting = true;
                if ((_SM as MineWorldGame).keyBinds.IsPressed(KeyBoardButtons.Crouch))
                    _P.crouching = false;
            }

            if (moveVector.X != 0 || moveVector.Z != 0)
            {
                // "Flatten" the movement vector so that we don"t move up/down.
                moveVector.Y = 0;
                moveVector.Normalize();
                moveVector *= MOVESPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_P.movingOnRoad)
                    moveVector *= 2;
                // Sprinting doubles speed, even if already on road
                if (_P.sprinting)
                    moveVector *= 1.5f;
                if (_P.swimming)
                    moveVector *= 0.5f;
                if (_P.crouching)
                    moveVector.Y = -1;

                // Attempt to move, doing collision stuff.
                if (TryToMoveTo(moveVector, gameTime))
                {
                }
                else
                {
                    if (!TryToMoveTo(new Vector3(0, 0, moveVector.Z), gameTime)) 
                    {
                    }
                    if (!TryToMoveTo(new Vector3(moveVector.X, 0, 0), gameTime)) 
                    { 
                    }
                }
            }
            //Reset movement flags
            _P.movingOnRoad = false;
            _P.sprinting = false;
            _P.swimming = false;
            _P.crouching = false;
        }

        private bool TryToMoveTo(Vector3 moveVector, GameTime gameTime)
        {
            // Build a "test vector" that is a little longer than the move vector.
            float moveLength = moveVector.Length();
            Vector3 testVector = moveVector;
            testVector.Normalize();
            testVector = testVector * (moveLength);// + 0.1f);

            // Apply this test vector.
            Vector3 movePosition = _P.playerPosition + testVector;
            Vector3 midBodyPoint = movePosition + new Vector3(0, -0.7f, 0);
            Vector3 lowerBodyPoint = movePosition + new Vector3(0, -1.4f, 0);

            if (!_P.blockEngine.SolidAtPointForPlayer(movePosition) && !_P.blockEngine.SolidAtPointForPlayer(lowerBodyPoint) && !_P.blockEngine.SolidAtPointForPlayer(midBodyPoint))
            {
                testVector = moveVector;
                testVector.Normalize();
                testVector = testVector * (moveLength + 0.11f); // Makes sure the camera doesnt move too close to the block ;)
                movePosition = _P.playerPosition + testVector;
                midBodyPoint = movePosition + new Vector3(0, -0.7f, 0);
                lowerBodyPoint = movePosition + new Vector3(0, -1.4f, 0);

                if (!_P.blockEngine.SolidAtPointForPlayer(movePosition) && !_P.blockEngine.SolidAtPointForPlayer(lowerBodyPoint) && !_P.blockEngine.SolidAtPointForPlayer(midBodyPoint))
                {
                    _P.playerPosition = _P.playerPosition + moveVector;
                    return true;
                }
            }

            // It's solid there, so while we can't move we have officially collided with it.
            BlockType lowerBlock = _P.blockEngine.BlockAtPoint(lowerBodyPoint);
            BlockType midBlock = _P.blockEngine.BlockAtPoint(midBodyPoint);
            BlockType upperBlock = _P.blockEngine.BlockAtPoint(movePosition);

            // It's solid there, so see if it's a lava block. If so, touching it will kill us!
            if (upperBlock == BlockType.Lava || lowerBlock == BlockType.Lava || midBlock == BlockType.Lava)
            {
                _P.SendPlayerHurt(100, false);
                return true;
            }

            // If it's a ladder, move up.
            //if (upperBlock == BlockType.Ladder || lowerBlock == BlockType.Ladder || midBlock == BlockType.Ladder)
            //{
                //_P.playerVelocity.Y = CLIMBVELOCITY;
                //Vector3 footPosition = _P.playerPosition + new Vector3(0f, -1.5f, 0f);
                //if (_P.blockEngine.SolidAtPointForPlayer(footPosition))
                    //_P.playerPosition.Y += 0.1f;
                //return true;
            //}

            return false;
        }

        public override void OnRenderAtEnter(GraphicsDevice graphicsDevice)
        {

        }

        public override void OnRenderAtUpdate(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            _P.skyEngine.Render(graphicsDevice);
            _P.playerEngine.Render(graphicsDevice);
            _P.playerEngine.RenderPlayerNames(graphicsDevice);
            _P.blockEngine.Render(graphicsDevice, gameTime);
            _P.particleEngine.Render(graphicsDevice);
            _P.interfaceEngine.Render(graphicsDevice);

            _SM.Window.Title = Defines.MINEWORLDCLIENT_VERSION;
        }

        //DateTime startChat = DateTime.Now;
        public override void OnCharEntered(EventInput.CharacterEventArgs e)
        {
            if ((int)e.Character < 32 || (int)e.Character > 126) //From space to tilde
                return; //Do nothing
            if (_P.chatMode != ChatMessageType.None)
            {
                //Chat delay to avoid entering the "start chat" key, an unfortunate side effect of the new key bind system
                //TimeSpan diff = DateTime.Now - startChat;
                //if (diff.Milliseconds >= 2)
                    //if (!(Keyboard.GetState().IsKeyDown(Keys.LeftControl) || Keyboard.GetState().IsKeyDown(Keys.RightControl)))
                    //{
                        _P.chatEntryBuffer += e.Character;
                    //}
            }
        }

        private void HandleInput(KeyBoardButtons input)
        {
            switch (input)
            {
                case KeyBoardButtons.AltFire:
                case KeyBoardButtons.Fire:
                    {
                        // If we're dead, come back to life.
                        if (_P.playerDead && _P.screenEffectCounter > 2)
                        {
                            _P.RespawnPlayer();
                        }
                        else if (!_P.playerDead)
                        {
                            _P.UseTool(input);
                        }
                        break;
                    }
                case KeyBoardButtons.Jump:
                    {
                        Vector3 footPosition = _P.playerPosition + new Vector3(0f, -1.5f, 0f);
                        Vector3 midPosition = _P.playerPosition + new Vector3(0f, -0.7f, 0f);
                        if (_P.blockEngine.SolidAtPointForPlayer(footPosition) && _P.playerVelocity.Y == 0)
                        {
                            _P.playerVelocity.Y = JUMPVELOCITY;
                            float amountBelowSurface = ((int)footPosition.Y) + 1 - footPosition.Y;
                            _P.playerPosition.Y += amountBelowSurface + 0.01f;
                        }

                        if (_P.blockEngine.BlockAtPoint(midPosition) == BlockType.Water)
                        {
                            _P.playerVelocity.Y = JUMPVELOCITY * 0.4f;
                        }
                    }
                    break;
                case KeyBoardButtons.Say:
                    _P.chatMode = ChatMessageType.Say;
                    //startChat = DateTime.Now;
                    break;
            }
        }

        public override void OnKeyDown(Keys key)
        {
            // Exit!
            if (key == Keys.Y && Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                _P.netClient.Disconnect("Client disconnected.");
                nextState = "MineWorld.States.ServerBrowserState";
            }

            // Pixelcide!
            if (key == Keys.K && Keyboard.GetState().IsKeyDown(Keys.Escape) && !_P.playerDead)
            {
                _P.SendPlayerHurt(100, false);
                return;
            }

            if (_P.chatMode != ChatMessageType.None)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.LeftControl) || Keyboard.GetState().IsKeyDown(Keys.RightControl))
                {
                    if (key == Keys.V)
                    {
                        _P.chatEntryBuffer += System.Windows.Forms.Clipboard.GetText();
                        return;
                    }
                    else if (key == Keys.C)
                    {
                        System.Windows.Forms.Clipboard.SetText(_P.chatEntryBuffer);
                        return;
                    }
                    else if (key == Keys.X)
                    {
                        System.Windows.Forms.Clipboard.SetText(_P.chatEntryBuffer);
                        _P.chatEntryBuffer = "";
                        return;
                    }
                }
                // Put the characters in the chat buffer.
                if (key == Keys.Enter)
                {
                    // If we have an actual message to send, fire it off at the server.
                    if (_P.chatEntryBuffer.Length > 0)
                    {
                        if(_P.netClient.ConnectionStatus == NetConnectionStatus.Connected)
                        {
                            //Todo remove this check and let the server check for client commands
                            //This is nessarcy cause of how currently the client commands system works :S
                            if(_P.chatEntryBuffer.StartsWith("/"))
                            {
                                NetBuffer msgBuffer = _P.netClient.CreateBuffer();
                                msgBuffer.Write((byte)MineWorldMessage.PlayerCommand);
                                msgBuffer.Write(_P.chatEntryBuffer);
                                _P.netClient.SendMessage(msgBuffer, NetChannel.ReliableInOrder6);
                            }
                            else
                            {
                                NetBuffer msgBuffer = _P.netClient.CreateBuffer();
                                msgBuffer.Write((byte)MineWorldMessage.ChatMessage);
                                msgBuffer.Write((byte)_P.chatMode);
                                msgBuffer.Write(_P.chatEntryBuffer);
                                msgBuffer.Write(_P.playerHandle);
                                _P.netClient.SendMessage(msgBuffer, NetChannel.ReliableInOrder3);
                            }
                        }
                    }

                    _P.chatEntryBuffer = "";
                    _P.chatMode = ChatMessageType.None;
                }
                else if (key == Keys.Back)
                {
                    if (_P.chatEntryBuffer.Length > 0)
                        _P.chatEntryBuffer = _P.chatEntryBuffer.Substring(0, _P.chatEntryBuffer.Length - 1);
                }
                else if (key == Keys.Escape)
                {
                    _P.chatEntryBuffer = "";
                    _P.chatMode = ChatMessageType.None;
                }
                return;
            }else if (!_P.playerDead)
                HandleInput((_SM as MineWorldGame).keyBinds.GetBound(key));
            
        }

        public override void OnKeyUp(Keys key)
        {
        }

        public override void OnMouseDown(MouseButtons button, int x, int y)
        {
                HandleInput((_SM as MineWorldGame).keyBinds.GetBound(button));
        }

        public override void OnMouseUp(MouseButtons button, int x, int y)
        {
        }

        public override void OnMouseScroll(int scrollDelta)
        {
            if (_P.playerDead)
                return;
            else
            {
                if (scrollDelta >= 120)
                {
                    HandleInput((_SM as MineWorldGame).keyBinds.GetBound(MouseButtons.WheelUp));
                }
                else if (scrollDelta <= -120)
                {
                    HandleInput((_SM as MineWorldGame).keyBinds.GetBound(MouseButtons.WheelDown));
                }
            }
        }
    }
}
