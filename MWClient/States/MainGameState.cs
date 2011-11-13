using System;
using System.Windows.Forms;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineWorld.StateMasher;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace MineWorld.States
{
    public class MainGameState : State
    {
        private const float Movespeed = 3.5f;
        private const float Gravity = -16.0f;
        private const float Jumpvelocity = 6.0f;
        private const float Dievelocity = 20.0f;
        private readonly InputHelper _input = new InputHelper();

        private bool _mouseInitialized;
        private string _nextState;
        private DateTime _startChat = DateTime.Now;

        public override void OnEnter(string oldState)
        {
            Sm.IsMouseVisible = false;
        }

        public override void OnLeave(string newState)
        {
            P.ChatEntryBuffer = "";
            P.Chatting = false;
        }

        public override string OnUpdate(GameTime gameTime, KeyboardState keyState, MouseState mouseState)
        {
            // Update network stuff.
            (Sm as MineWorldGame).UpdateNetwork(gameTime);

            // Update input stuff.
            _input.Update();

            // Handle input.
            HandleInput();

            // Update the current screen effect.
            P.ScreenEffectCounter += gameTime.ElapsedGameTime.TotalSeconds;

            // Update engines.
            P.SkyEngine.Update(gameTime);
            P.PlayerEngine.Update(gameTime);
            P.BlockEngine.Update(gameTime);
            P.ParticleEngine.Update(gameTime);
            P.InterfaceEngine.Update(gameTime);

            // Moving the mouse changes where we look.
            if (Sm.WindowHasFocus())
            {
                if (_mouseInitialized)
                {
                    int dx = mouseState.X - Sm.GraphicsDevice.Viewport.Width/2;
                    int dy = mouseState.Y - Sm.GraphicsDevice.Viewport.Height/2;

                    if ((Sm as MineWorldGame).Csettings.InvertMouseYAxis)
                        dy = -dy;

                    P.PlayerCamera.Yaw -= dx*(P.MouseSensitivity);
                    P.PlayerCamera.Pitch =
                        (float)
                        Math.Min(Math.PI*0.49, Math.Max(-Math.PI*0.49, P.PlayerCamera.Pitch - dy*(P.MouseSensitivity)));
                }
                else
                {
                    _mouseInitialized = true;
                }
                Mouse.SetPosition(Sm.GraphicsDevice.Viewport.Width/2, Sm.GraphicsDevice.Viewport.Height/2);
            }
            else
                _mouseInitialized = false;

            // Update the player's position.
            if (!P.PlayerDead)
                UpdatePlayerPosition(gameTime);

            // Update the camera regardless of if we're alive or not.
            P.UpdateCamera(gameTime);

            // Check for hearthbeat packets
            TimeSpan timespanhearthbeat = DateTime.Now - P.Lasthearthbeatreceived;
            if (timespanhearthbeat.TotalMilliseconds > 5000)
            {
                // The server crashed or connection lost lets exit
                ErrorManager.ErrorMsg = "Connection lost to server";
                ErrorManager.NewState = "MineWorld.States.ServerBrowserState";
                _nextState = "MineWorld.States.ErrorState";
            }

            return _nextState;
        }

        private void HandleInput()
        {
            if (_input.IsCurPress(Keys.W))
            {
                P.MoveVector += P.PlayerCamera.GetLookVector();
            }
            if (_input.IsCurPress(Keys.A))
            {
                P.MoveVector -= P.PlayerCamera.GetRightVector();
            }
            if (_input.IsCurPress(Keys.S))
            {
                P.MoveVector -= P.PlayerCamera.GetLookVector();
            }
            if (_input.IsCurPress(Keys.D))
            {
                P.MoveVector += P.PlayerCamera.GetRightVector();
            }
            if (_input.IsNewPress(Keys.F5))
            {
                P.DebugMode = !P.DebugMode;
            }
        }

        private void UpdatePlayerPosition(GameTime gameTime)
        {
            // Apply "gravity".
            Vector3 footPosition = P.PlayerPosition + new Vector3(0f, -1.5f, 0f);
            Vector3 headPosition = P.PlayerPosition + new Vector3(0f, 0.1f, 0f);
            Vector3 midPosition = P.PlayerPosition + new Vector3(0f, -0.7f, 0f);

            if (P.BlockEngine.BlockAtPoint(footPosition) == BlockType.Water ||
                P.BlockEngine.BlockAtPoint(midPosition) == BlockType.Water)
            {
                P.Swimming = true;

                if (P.BlockEngine.BlockAtPoint(headPosition) == BlockType.Water)
                {
                    P.Underwater = true;
                    //if (_P.playerHoldBreath == 20)
                    //{
                    //_P.playerVelocity.Y *= 0.2f;
                    //}
                    if (P.PlayerHoldBreath > 8)
                    {
                        P.ScreenEffect = ScreenEffect.Water;
                        P.ScreenEffectCounter = 0.5;
                    }
                    else
                    {
                        P.ScreenEffect = ScreenEffect.Drowning;
                        P.ScreenEffectCounter = 0.5;
                    }
                }
                else
                {
                    P.LastBreath = DateTime.Now;
                    P.Underwater = false;
                    P.PlayerHoldBreath = 20;
                }
            }
            else
            {
                P.Swimming = false;
                P.Underwater = false;
                P.LastBreath = DateTime.Now;
            }

            if (P.Swimming || P.Underwater)
            {
                TimeSpan timeSpan = DateTime.Now - P.LastBreath;
                P.PlayerVelocity.Y += (Gravity/8)*(float) gameTime.ElapsedGameTime.TotalSeconds;

                if (timeSpan.TotalMilliseconds > 1000)
                {
                    P.LastBreath = DateTime.Now;
                    P.PlayerHoldBreath -= 1;
                    if (P.PlayerHoldBreath <= 0)
                    {
                        P.SendPlayerHurt(20, false);
                    }
                    //_P.PlaySoundForEveryone(MineWorldSound.Death, _P.playerPosition);
                }
            }
            else
            {
                P.PlayerVelocity.Y += Gravity*(float) gameTime.ElapsedGameTime.TotalSeconds;
            }

            // Let the server know and then move on
            // Todo Dont really know if this is needed the playerupdate here
            P.SendPlayerUpdate();

            if (P.BlockEngine.SolidAtPointForPlayer(footPosition) || P.BlockEngine.SolidAtPointForPlayer(headPosition))
            {
                BlockType standingOnBlock = P.BlockEngine.BlockAtPoint(footPosition);
                BlockType hittingHeadOnBlock = P.BlockEngine.BlockAtPoint(headPosition);

                // If we"re hitting the ground with a high velocity, die!
                if (standingOnBlock != BlockType.None && P.PlayerVelocity.Y < 0)
                {
                    float fallDamage = Math.Abs(P.PlayerVelocity.Y)/Dievelocity;
                    if (fallDamage >= 0.5)
                    {
                        // Fall damage of 0.5 maps to a screenEffectCounter value of 2, meaning that the effect doesn't appear.
                        // Fall damage of 1.0 maps to a screenEffectCounter value of 0, making the effect very strong.
                        P.ScreenEffect = ScreenEffect.Fall;
                        P.ScreenEffectCounter = 2 - (fallDamage - 0.5)*4;
                        P.PlaySoundForEveryone(MineWorldSound.GroundHit, P.PlayerPosition);
                    }
                }
                else
                {
                    P.PlaySoundForEveryone(MineWorldSound.GroundHit, P.PlayerPosition);
                }

                if (P.BlockEngine.SolidAtPointForPlayer(midPosition))
                {
                    //_P.KillPlayer(Defines.deathByCrush);
                    P.SendPlayerHurt(100, false);
                }

                // If the player has their head stuck in a block, push them down.
                if (P.BlockEngine.SolidAtPointForPlayer(headPosition))
                {
                    int blockIn = (int) (headPosition.Y);
                    P.PlayerPosition.Y = (blockIn - 0.15f);
                }

                // If the player is stuck in the ground, bring them out.
                // This happens because we're standing on a block at -1.5, but stuck in it at -1.4, so -1.45 is the sweet spot.
                if (P.BlockEngine.SolidAtPointForPlayer(footPosition))
                {
                    int blockOn = (int) (footPosition.Y);
                    P.PlayerPosition.Y = (float) (blockOn + 1 + 1.45);
                }

                P.PlayerVelocity.Y = 0;

                // Logic for standing on a block.
                switch (standingOnBlock)
                {
                    case BlockType.Lava:
                        P.SendPlayerHurt(100, false);
                        break;
                }

                // Logic for bumping your head on a block.
                switch (hittingHeadOnBlock)
                {
                    case BlockType.Lava:
                        P.SendPlayerHurt(100, false);
                        break;
                }
            }
            if (
                !P.BlockEngine.SolidAtPointForPlayer(midPosition +
                                                      P.PlayerVelocity*(float) gameTime.ElapsedGameTime.TotalSeconds))
            {
                P.PlayerPosition += P.PlayerVelocity*(float) gameTime.ElapsedGameTime.TotalSeconds;
            }

            // Death by falling off the map.
            if (P.PlayerPosition.Y < 0)
            {
                P.SendPlayerHurt(100, false);
                //_P.KillPlayer(Defines.deathByMiss);
                return;
            }

            if (P.MoveVector.X != 0 || P.MoveVector.Z != 0)
            {
                // "Flatten" the movement vector so that we don"t move up/down.
                P.MoveVector.Y = 0;
                P.MoveVector.Normalize();
                P.MoveVector *= Movespeed*(float) gameTime.ElapsedGameTime.TotalSeconds;
                if (P.MovingOnRoad)
                    P.MoveVector *= 2;
                // Sprinting doubles speed, even if already on road
                if (P.Sprinting)
                    P.MoveVector *= 1.5f;
                if (P.Swimming)
                    P.MoveVector *= 0.5f;
                //if (_P.crouching)
                //_P.MoveVector.Y = -1;


                TryToMoveTo(P.MoveVector);
                // Attempt to move, doing collision stuff.
                if (TryToMoveTo(P.MoveVector))
                {
                }
                else
                {
                    if (!TryToMoveTo(new Vector3(0, 0, P.MoveVector.Z)))
                    {
                    }
                    if (!TryToMoveTo(new Vector3(P.MoveVector.X, 0, 0)))
                    {
                    }
                }
            }

            //Reset movement and flags
            P.MoveVector = Vector3.Zero;
            P.MovingOnRoad = false;
            P.Sprinting = false;
            P.Swimming = false;
            P.Underwater = false;
        }

        private bool TryToMoveTo(Vector3 moveVector)
        {
            // Build a "test vector" that is a little longer than the move vector.
            float moveLength = moveVector.Length();
            Vector3 testVector = moveVector;
            testVector.Normalize();
            testVector = testVector*(moveLength); // + 0.1f);

            // Apply this test vector.
            Vector3 movePosition = P.PlayerPosition + testVector;
            Vector3 midBodyPoint = movePosition + new Vector3(0, -0.7f, 0);
            Vector3 lowerBodyPoint = movePosition + new Vector3(0, -1.4f, 0);

            if (!P.BlockEngine.SolidAtPointForPlayer(movePosition) &&
                !P.BlockEngine.SolidAtPointForPlayer(lowerBodyPoint) &&
                !P.BlockEngine.SolidAtPointForPlayer(midBodyPoint))
            {
                testVector = moveVector;
                testVector.Normalize();
                testVector = testVector*(moveLength + 0.11f);
                    // Makes sure the camera doesnt move too close to the block ;)
                movePosition = P.PlayerPosition + testVector;
                midBodyPoint = movePosition + new Vector3(0, -0.7f, 0);
                lowerBodyPoint = movePosition + new Vector3(0, -1.4f, 0);

                if (!P.BlockEngine.SolidAtPointForPlayer(movePosition) &&
                    !P.BlockEngine.SolidAtPointForPlayer(lowerBodyPoint) &&
                    !P.BlockEngine.SolidAtPointForPlayer(midBodyPoint))
                {
                    P.PlayerPosition = P.PlayerPosition + moveVector;
                    return true;
                }
            }

            // It's solid there, so while we can't move we have officially collided with it.
            BlockType lowerBlock = P.BlockEngine.BlockAtPoint(lowerBodyPoint);
            BlockType midBlock = P.BlockEngine.BlockAtPoint(midBodyPoint);
            BlockType upperBlock = P.BlockEngine.BlockAtPoint(movePosition);

            // It's solid there, so see if it's a lava block. If so, touching it will kill us!
            if (upperBlock == BlockType.Lava || lowerBlock == BlockType.Lava || midBlock == BlockType.Lava)
            {
                P.SendPlayerHurt(100, false);
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

        public override void OnRenderAtUpdate(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            P.SkyEngine.Render(graphicsDevice);
            P.PlayerEngine.Render(graphicsDevice);
            P.PlayerEngine.RenderPlayerNames(graphicsDevice);
            P.BlockEngine.Render(graphicsDevice, gameTime);
            P.ParticleEngine.Render(graphicsDevice);
            P.InterfaceEngine.Render(graphicsDevice);
            P.DebugEngine.Render(graphicsDevice);
            Sm.Window.Title = Defines.MineworldclientVersion;
        }

        public override void OnCharEntered(CharacterEventArgs e)
        {
            if (e.Character < 32 || e.Character > 126) //From space to tilde
                return; //Do nothing
            if (P.Chatting)
            {
                //Chat delay to avoid entering the "start chat" key, an unfortunate side effect of the new key bind system
                TimeSpan diff = DateTime.Now - _startChat;
                if (diff.Milliseconds >= 2)
                    if (
                        !(Keyboard.GetState().IsKeyDown(Keys.LeftControl) ||
                          Keyboard.GetState().IsKeyDown(Keys.RightControl)))
                    {
                        P.ChatEntryBuffer += e.Character;
                    }
            }
        }

        public override void OnKeyDown(Keys key)
        {
            if (P.PlayerDead)
            {
                return;
            }
            else
            {
                // Exit!
                if (key == Keys.Y && Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    P.NetClient.Disconnect("Client disconnected.");
                    _nextState = "MineWorld.States.ServerBrowserState";
                }
                // Pixelcide!
                if (key == Keys.K && Keyboard.GetState().IsKeyDown(Keys.Escape) && !P.PlayerDead)
                {
                    P.SendPlayerHurt(100, false);
                    return;
                }
                if (P.Chatting)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.LeftControl) ||
                        Keyboard.GetState().IsKeyDown(Keys.RightControl))
                    {
                        if (key == Keys.V)
                        {
                            P.ChatEntryBuffer += Clipboard.GetText();
                            return;
                        }
                        else if (key == Keys.C)
                        {
                            Clipboard.SetText(P.ChatEntryBuffer);
                            return;
                        }
                        else if (key == Keys.X)
                        {
                            Clipboard.SetText(P.ChatEntryBuffer);
                            P.ChatEntryBuffer = "";
                            return;
                        }
                    }
                    // Put the characters in the chat buffer.
                    if (key == Keys.Enter)
                    {
                        // If we have an actual message to send, fire it off at the server.
                        if (P.ChatEntryBuffer.Length > 0)
                        {
                            SendChatOrCommandMessage();
                        }

                        P.ChatEntryBuffer = "";
                        P.Chatting = false;
                    }
                    else if (key == Keys.Back)
                    {
                        if (P.ChatEntryBuffer.Length > 0)
                            P.ChatEntryBuffer = P.ChatEntryBuffer.Substring(0, P.ChatEntryBuffer.Length - 1);
                    }
                    else if (key == Keys.Escape)
                    {
                        P.ChatEntryBuffer = "";
                        P.Chatting = false;
                    }
                    return;
                }
                else
                {
                    //If we arent typing lets move :P Everyday iam shuffling
                    switch ((Sm as MineWorldGame).KeyBinds.GetBoundKeyBoard(key))
                    {
                            /*
                        case CustomKeyBoardButtons.Forward:
                            {
                                _P.MoveVector += _P.playerCamera.GetLookVector();
                                break;
                            }
                        case CustomKeyBoardButtons.Backward:
                            {
                                _P.MoveVector -= _P.playerCamera.GetLookVector();
                                break;
                            }
                        case CustomKeyBoardButtons.Right:
                            {
                                _P.MoveVector += _P.playerCamera.GetRightVector();
                                break;
                            }
                        case CustomKeyBoardButtons.Left:
                            {
                                _P.MoveVector -= _P.playerCamera.GetRightVector();
                                break;
                            }
                             */
                        case CustomKeyBoardButtons.Sprint:
                            {
                                P.Sprinting = true;
                                break;
                            }
                        case CustomKeyBoardButtons.Jump:
                            {
                                Vector3 footPosition = P.PlayerPosition + new Vector3(0f, -1.5f, 0f);
                                Vector3 midPosition = P.PlayerPosition + new Vector3(0f, -0.7f, 0f);
                                if (P.BlockEngine.SolidAtPointForPlayer(footPosition) && P.PlayerVelocity.Y == 0)
                                {
                                    P.PlayerVelocity.Y = Jumpvelocity;
                                    float amountBelowSurface = ((int) footPosition.Y) + 1 - footPosition.Y;
                                    P.PlayerPosition.Y += amountBelowSurface + 0.01f;
                                }

                                if (P.BlockEngine.BlockAtPoint(midPosition) == BlockType.Water)
                                {
                                    P.PlayerVelocity.Y = Jumpvelocity*0.4f;
                                }
                                break;
                            }
                        case CustomKeyBoardButtons.Say:
                            {
                                P.Chatting = true;
                                _startChat = DateTime.Now;
                                break;
                            }
                    }
                }
            }
        }

        public override void OnKeyUp(Keys key)
        {
            if (P.PlayerDead)
            {
                return;
            }
            else
            {
                return;
            }
        }

        public override void OnMouseDown(MouseButtons button, int x, int y)
        {
            if (P.PlayerDead)
            {
                P.SendRespawnRequest();
            }
            else
            {
                switch ((Sm as MineWorldGame).KeyBinds.GetBoundMouse(button))
                {
                    case CustomMouseButtons.Fire:
                    case CustomMouseButtons.AltFire:
                        {
                            P.UseTool((Sm as MineWorldGame).KeyBinds.GetBoundMouse(button));
                            break;
                        }
                }
            }
        }

        public override void OnMouseUp(MouseButtons button, int x, int y)
        {
            if (P.PlayerDead)
            {
                return;
            }
            else
            {
                return;
            }
        }

        public override void OnMouseScroll(int scrollDelta)
        {
            if (P.PlayerDead)
            {
                return;
            }
            else
            {
                if (scrollDelta >= 120)
                {
                    // Unused at the moment
                    return;
                }
                else if (scrollDelta <= -120)
                {
                    // Unused at the moment
                    return;
                }
            }
        }

        private void SendChatOrCommandMessage()
        {
            if (P.NetClient.Status == NetConnectionStatus.Connected)
            {
                //Its a player command :D?
                if (P.ChatEntryBuffer.StartsWith("/"))
                {
                    NetBuffer msgBuffer = P.NetClient.CreateBuffer();
                    msgBuffer.Write((byte) MineWorldMessage.PlayerCommand);
                    msgBuffer.Write(P.ChatEntryBuffer);
                    P.NetClient.SendMessage(msgBuffer, NetChannel.ReliableInOrder6);
                }
                else
                {
                    NetBuffer msgBuffer = P.NetClient.CreateBuffer();
                    msgBuffer.Write((byte) MineWorldMessage.ChatMessage);
                    msgBuffer.Write((byte) ChatMessageType.PlayerSay);
                    msgBuffer.Write(P.ChatEntryBuffer);
                    msgBuffer.Write(P.PlayerHandle);
                    P.NetClient.SendMessage(msgBuffer, NetChannel.ReliableInOrder3);
                }
            }
        }
    }
}