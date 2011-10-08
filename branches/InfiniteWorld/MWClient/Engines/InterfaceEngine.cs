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

namespace MineWorld
{
    public class InterfaceEngine
    {
        MineWorldGame gameInstance;
        PropertyBag _P;
        SpriteBatch spriteBatch;
        public SpriteFont uiFont, radarFont;
        Rectangle drawRect;

        Texture2D texCrosshairs, texBlank, texHelpRed,texHelpBlue;

        Dictionary<BlockType, Texture2D> blockIcons = new Dictionary<BlockType, Texture2D>();

        public InterfaceEngine(MineWorldGame gameInstance)
        {
            this.gameInstance = gameInstance;
            spriteBatch = new SpriteBatch(gameInstance.GraphicsDevice);

            // Load textures.
            texCrosshairs = gameInstance.Content.Load<Texture2D>("ui/tex_ui_crosshair");
            texBlank = new Texture2D(gameInstance.GraphicsDevice, 1, 1);
            //texBlank.SetData(new int[1] { 2147483647 });
            texBlank.SetData(new uint[1] { 0xFFFFFFFF });
            texHelpRed = gameInstance.Content.Load<Texture2D>("menus/tex_menu_help_red");
            texHelpBlue = gameInstance.Content.Load<Texture2D>("menus/tex_menu_help_blue");

            drawRect = new Rectangle(gameInstance.GraphicsDevice.Viewport.Width / 2 - 1024 / 2,
                                     gameInstance.GraphicsDevice.Viewport.Height / 2 - 768 / 2,
                                     1024,
                                     1024);

            // Load icons.
            blockIcons[BlockType.Lava] = gameInstance.Content.Load<Texture2D>("icons/tex_icon_lava");
            blockIcons[BlockType.None] = gameInstance.Content.Load<Texture2D>("icons/tex_icon_deconstruction");

            // Load fonts.
            uiFont = gameInstance.Content.Load<SpriteFont>("font_04b08");
            radarFont = gameInstance.Content.Load<SpriteFont>("font_04b03b");
        }

        public void RenderPickAxe(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            int screenWidth = graphicsDevice.Viewport.Width;
            int screenHeight = graphicsDevice.Viewport.Height;
            graphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Point;
            int drawX = screenWidth / 2 - 60 * 3;
            int drawY = screenHeight - 91 * 3;
            //Texture2D toDraw;
            if (_P.Owncolor == Color.Red)
            {
                //toDraw = texToolPickaxeRed;
            }
            else
            {
                //toDraw = texToolPickaxeBlue;
            }
        }

        public void RenderMessageCenter(SpriteBatch spriteBatch, string text, Vector2 pointCenter, Color colorText, Color colorBackground)
        {
            Vector2 textSize = uiFont.MeasureString(text);
            spriteBatch.Draw(texBlank, new Rectangle((int)(pointCenter.X - textSize.X / 2 - 10), (int)(pointCenter.Y - textSize.Y / 2 - 10), (int)(textSize.X + 20), (int)(textSize.Y + 20)), colorBackground);
            spriteBatch.DrawString(uiFont, text, pointCenter - textSize / 2, colorText);
        }

        private static bool MessageExpired(ChatMessage msg)
        {
            return msg.TimeStamp <= 0;
        }

        public void Update(GameTime gameTime)
        {
            if (_P == null)
                return;

            foreach (ChatMessage msg in _P.chatBuffer)
                msg.TimeStamp -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            _P.chatBuffer.RemoveAll(MessageExpired);
        }

        public void drawChat(List<ChatMessage>messages, GraphicsDevice graphicsDevice)
        {
            int newlines = 0;
            for (int i = 0; i < messages.Count; i++)
            {
                newlines++;
                Color chatColor = Color.White;
                string chat = "";

                if (messages[i].Type == ChatMessageType.SayServer)
                    chatColor = Color.Blue;

                switch(messages[i].Type)
                {
                    case ChatMessageType.Say:
                        {
                            if (messages[i].Author == "" || messages[i].Author == null)
                            {
                                chat = "MESSAGE SAY WAS SENT WITHOUT OWNER";
                            }
                            chat = messages[i].Author + ": " + messages[i].Message;
                            break;
                        }
                    case ChatMessageType.SayServer:
                        {
                            chat = "Server: " + messages[i].Message;
                            break;
                        }
                    default:
                        {
                            chat = "Unkown Chatmessage Type : " + messages[i].Type.ToString();
                            break;
                        }
                }
                int y = graphicsDevice.Viewport.Height - 114;
                //newlines += messages[i].NewLines;
                y -= 16 * newlines;
                //y -= 16 * i;

                spriteBatch.DrawString(uiFont, chat, new Vector2(22, y), Color.Black);
                spriteBatch.DrawString(uiFont, chat, new Vector2(20, y - 2), chatColor);
            }
        }

        public void Render(GraphicsDevice graphicsDevice)
        {
            // If we don't have _P, grab it from the current gameInstance.
            // We can't do this in the constructor because we are created in the property bag's constructor!
            if (_P == null)
                _P = gameInstance.propertyBag;

            // Draw the UI.
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);

            // Draw the crosshair.
            spriteBatch.Draw(texCrosshairs, new Rectangle(graphicsDevice.Viewport.Width / 2 - texCrosshairs.Width / 2,
                                                            graphicsDevice.Viewport.Height / 2 - texCrosshairs.Height / 2,
                                                            texCrosshairs.Width,
                                                            texCrosshairs.Height), Color.White);

            if (gameInstance.Csettings.DrawFrameRate)
                RenderMessageCenter(spriteBatch, string.Format("FPS: {0:000}", gameInstance.FrameRate), new Vector2(60, graphicsDevice.Viewport.Height - 20), Color.Gray, Color.Black);

            // Show the altimeter.
            int altitude = (int)(_P.playerPosition.Y - Defines.MAPSIZE + Defines.GROUND_LEVEL);
            RenderMessageCenter(spriteBatch, string.Format("ALTITUDE: {0:00}", altitude), new Vector2(graphicsDevice.Viewport.Width - 90, graphicsDevice.Viewport.Height - 20), altitude >= 0 ? Color.Gray : Color.White, Color.Black);

            // Draw the text-based information panel.
            int textStart = (graphicsDevice.Viewport.Width - 1024) / 2;
            spriteBatch.Draw(texBlank, new Rectangle(0, 0, graphicsDevice.Viewport.Width, 20), Color.Black);
            RenderMessageCenter(spriteBatch, "Health: " + _P.playerHealth.ToString() + "/" + _P.playerHealthMax.ToString(), new Vector2(graphicsDevice.Viewport.Width - 300, graphicsDevice.Viewport.Height - 20),Color.Green, Color.Black);

            // Draw player information.
            if ((Keyboard.GetState().IsKeyDown(Keys.Tab) && _P.screenEffect == ScreenEffect.None))
            {
                spriteBatch.Draw(texBlank, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), new Color(Color.Black, 0.7f));

                //Server name
                RenderMessageCenter(spriteBatch, _P.serverName, new Vector2(graphicsDevice.Viewport.Width / 2, 32), Color.White, Color.Black);

                //TODO Make sure we splitt this up if the left side is full
                int drawY = 200;
                foreach (ClientPlayer p in _P.playerList.Values)
                {
                    RenderMessageCenter(spriteBatch, p.Name, new Vector2(graphicsDevice.Viewport.Width / 4, drawY), Color.White, new Color(0, 0, 0, 0));
                    drawY += 35;
                }
            }

            // Draw the chat buffer.
            if (_P.chatMode == ChatMessageType.Say)
            {
                drawChat(_P.chatBuffer, graphicsDevice);
                spriteBatch.DrawString(uiFont, "Say> " + _P.chatEntryBuffer, new Vector2(22, graphicsDevice.Viewport.Height - 98), Color.Black);
                spriteBatch.DrawString(uiFont, "Say> " + _P.chatEntryBuffer, new Vector2(20, graphicsDevice.Viewport.Height - 100), Color.White);
            }
            else
            {
                drawChat(_P.chatBuffer, graphicsDevice);
            }

            // Draw escape message.
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                RenderMessageCenter(spriteBatch, "PRESS Y TO CONFIRM THAT YOU WANT TO QUIT.", new Vector2(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2 + 30), Color.White, Color.Black);
                RenderMessageCenter(spriteBatch, "PRESS K TO COMMIT PIXELCIDE.", new Vector2(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2 + 80), Color.White, Color.Black);
            }

            // Draw the current screen effect.
            if (_P.screenEffect == ScreenEffect.Death)
            {
                Color drawColor = new Color(1 - (float)_P.screenEffectCounter * 0.5f, 0f, 0f);
                spriteBatch.Draw(texBlank, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), drawColor);
                if (_P.screenEffectCounter >= 2)
                    RenderMessageCenter(spriteBatch, "You have died. Click to respawn.", new Vector2(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2), Color.White, Color.Black);
            }
            if (_P.screenEffect == ScreenEffect.Teleport || _P.screenEffect == ScreenEffect.Explosion)
            {
                Color drawColor = new Color(1, 1, 1, 1 - (float)_P.screenEffectCounter * 0.5f);
                spriteBatch.Draw(texBlank, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), drawColor);
                if (_P.screenEffectCounter > 2)
                    _P.screenEffect = ScreenEffect.None;
            }
            if (_P.screenEffect == ScreenEffect.Fall)
            {
                Color drawColor = new Color(1, 0, 0, 1 - (float)_P.screenEffectCounter * 0.5f);
                spriteBatch.Draw(texBlank, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), drawColor);
                if (_P.screenEffectCounter > 2)
                    _P.screenEffect = ScreenEffect.None;
            }
            if (_P.screenEffect == ScreenEffect.Water)
            {
                Color drawColor = new Color(0.0f, 0.5f, 1.0f, 1.0f - (float)_P.screenEffectCounter);
                spriteBatch.Draw(texBlank, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), drawColor);
                if (_P.screenEffectCounter > 2)
                    _P.screenEffect = ScreenEffect.None;
            }
            if (_P.screenEffect == ScreenEffect.Drowning)
            {
                Color drawColor = new Color(0.5f, 0, 0.8f, 0.25f + (float)_P.screenEffectCounter * 0.2f);
                spriteBatch.Draw(texBlank, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), drawColor);
                if (_P.screenEffectCounter > 2)
                {
                    _P.screenEffect = ScreenEffect.Water;
                    _P.screenEffectCounter = 1;
                }
            }


            // Draw the help screen.
            if (Keyboard.GetState().IsKeyDown(Keys.F1))
            {
                spriteBatch.Draw(texBlank, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.Black);
                if (_P.Owncolor == Color.Red)
                {
                    spriteBatch.Draw(texHelpRed, drawRect, Color.White);
                }
                else
                {
                    spriteBatch.Draw(texHelpBlue, drawRect, Color.White);
                }
            }

            spriteBatch.End();
        }
    }
}
