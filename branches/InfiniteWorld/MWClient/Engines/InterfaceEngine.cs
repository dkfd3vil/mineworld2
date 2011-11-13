using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MineWorld.Engines
{
    public class InterfaceEngine
    {
        private readonly Dictionary<BlockType, Texture2D> _blockIcons = new Dictionary<BlockType, Texture2D>();
        private readonly Rectangle _drawRect;
        private readonly MineWorldGame _gameInstance;
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _texBlank;
        private readonly Texture2D _texCrosshairs;
        private readonly Texture2D _texHelpBlue;
        private PropertyBag _p;
        public SpriteFont UiFont;

        public InterfaceEngine(MineWorldGame gameInstance)
        {
            _gameInstance = gameInstance;
            _spriteBatch = new SpriteBatch(gameInstance.GraphicsDevice);

            // Load textures.
            _texCrosshairs = gameInstance.Content.Load<Texture2D>("ui/tex_ui_crosshair");
            _texBlank = new Texture2D(gameInstance.GraphicsDevice, 1, 1);
            //texBlank.SetData(new int[1] { 2147483647 });
            _texBlank.SetData(new[] {0xFFFFFFFF});
            gameInstance.Content.Load<Texture2D>("menus/tex_menu_help_red");
            _texHelpBlue = gameInstance.Content.Load<Texture2D>("menus/tex_menu_help_blue");

            _drawRect = new Rectangle(gameInstance.GraphicsDevice.Viewport.Width/2 - 1024/2,
                                     gameInstance.GraphicsDevice.Viewport.Height/2 - 768/2,
                                     1024,
                                     1024);

            // Load icons.
            _blockIcons[BlockType.Lava] = gameInstance.Content.Load<Texture2D>("icons/tex_icon_lava");
            _blockIcons[BlockType.None] = gameInstance.Content.Load<Texture2D>("icons/tex_icon_deconstruction");

            // Load fonts.
            UiFont = gameInstance.Content.Load<SpriteFont>("font_04b08");
        }

        public void RenderMessageCenter(SpriteBatch spriteBatch, string text, Vector2 pointCenter, Color colorText,
                                        Color colorBackground)
        {
            Vector2 textSize = UiFont.MeasureString(text);
            spriteBatch.Draw(_texBlank,
                             new Rectangle((int) (pointCenter.X - textSize.X/2 - 10),
                                           (int) (pointCenter.Y - textSize.Y/2 - 10), (int) (textSize.X + 20),
                                           (int) (textSize.Y + 20)), colorBackground);
            spriteBatch.DrawString(UiFont, text, pointCenter - textSize/2, colorText);
        }

        public void Update(GameTime gameTime)
        {
            if (_p == null)
                return;

            foreach (ChatMessage msg in _p.ChatBuffer)
            {
                msg.TimeStamp -= (float) gameTime.ElapsedGameTime.TotalSeconds;
                if(msg.MessageExpired())
                {
                    _p.ChatBuffer.Remove(msg);
                }
            }
        }

        public void DrawChat(List<ChatMessage> messages, GraphicsDevice graphicsDevice)
        {
            int newlines = 0;
            for (int i = 0; i < messages.Count; i++)
            {
                newlines++;
                Color chatColor = Color.White;
                string chat;

                if (messages[i].Type == ChatMessageType.Server)
                    chatColor = Color.Blue;

                switch (messages[i].Type)
                {
                    case ChatMessageType.PlayerSay:
                        {
                            if (string.IsNullOrEmpty(messages[i].Author))
                            {
                                chat = "MESSAGE SAY WAS SENT WITHOUT OWNER";
                                break;
                            }
                            chat = messages[i].Author + ": " + messages[i].Message;
                            break;
                        }
                    case ChatMessageType.Server:
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
                y -= 16*newlines;
                //y -= 16 * i;

                _spriteBatch.DrawString(UiFont, chat, new Vector2(22, y), Color.Black);
                _spriteBatch.DrawString(UiFont, chat, new Vector2(20, y - 2), chatColor);
            }
        }

        public void Render(GraphicsDevice graphicsDevice)
        {
            // If we don't have _P, grab it from the current gameInstance.
            // We can't do this in the constructor because we are created in the property bag's constructor!
            if (_p == null)
                _p = _gameInstance.PropertyBag;

            // Draw the UI.
            _spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);

            // Draw the crosshair.
            _spriteBatch.Draw(_texCrosshairs, new Rectangle(graphicsDevice.Viewport.Width/2 - _texCrosshairs.Width/2,
                                                          graphicsDevice.Viewport.Height/2 - _texCrosshairs.Height/2,
                                                          _texCrosshairs.Width,
                                                          _texCrosshairs.Height), Color.White);

            if (_gameInstance.Csettings.DrawFrameRate)
                RenderMessageCenter(_spriteBatch, string.Format("FPS: {0:000}", _gameInstance.FrameRate),
                                    new Vector2(60, graphicsDevice.Viewport.Height - 20), Color.Gray, Color.Black);

            // Show the altimeter.
            //int altitude = (int)(_p.playerPosition.Y - + Defines.GROUND_LEVEL);
            //RenderMessageCenter(spriteBatch, string.Format("ALTITUDE: {0:00}", altitude), new Vector2(graphicsDevice.Viewport.Width - 90, graphicsDevice.Viewport.Height - 20), altitude >= 0 ? Color.Gray : Color.White, Color.Black);

            // Draw the text-based information panel.
            _spriteBatch.Draw(_texBlank, new Rectangle(0, 0, graphicsDevice.Viewport.Width, 20), Color.Black);
            RenderMessageCenter(_spriteBatch,
                                "Health: " + _p.PlayerHealth.ToString() + "/" + _p.PlayerHealthMax.ToString(),
                                new Vector2(graphicsDevice.Viewport.Width - 300, graphicsDevice.Viewport.Height - 20),
                                Color.Green, Color.Black);

            // Draw player information.
            if ((Keyboard.GetState().IsKeyDown(Keys.Tab) && _p.ScreenEffect == ScreenEffect.None))
            {
                _spriteBatch.Draw(_texBlank,
                                 new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                                 new Color(Color.Black, 0.7f));

                //Server name
                RenderMessageCenter(_spriteBatch, _p.ServerName, new Vector2(graphicsDevice.Viewport.Width/2, 32),
                                    Color.White, Color.Black);

                //TODO Make sure we splitt this up if the left side is full
                int drawY = 200;
                foreach (ClientPlayer p in _p.PlayerList.Values)
                {
                    RenderMessageCenter(_spriteBatch, p.Name, new Vector2(graphicsDevice.Viewport.Width/4, drawY),
                                        Color.White, new Color(0, 0, 0, 0));
                    drawY += 35;
                }
            }

            // Draw the chat buffer.
            if (_p.Chatting)
            {
                DrawChat(_p.ChatBuffer, graphicsDevice);
                _spriteBatch.DrawString(UiFont, "Say> " + _p.ChatEntryBuffer,
                                       new Vector2(22, graphicsDevice.Viewport.Height - 98), Color.Black);
                _spriteBatch.DrawString(UiFont, "Say> " + _p.ChatEntryBuffer,
                                       new Vector2(20, graphicsDevice.Viewport.Height - 100), Color.White);
            }
            else
            {
                DrawChat(_p.ChatBuffer, graphicsDevice);
            }

            // Draw escape message.
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                RenderMessageCenter(_spriteBatch, "PRESS Y TO CONFIRM THAT YOU WANT TO QUIT.",
                                    new Vector2(graphicsDevice.Viewport.Width/2, graphicsDevice.Viewport.Height/2 + 30),
                                    Color.White, Color.Black);
                RenderMessageCenter(_spriteBatch, "PRESS K TO COMMIT PIXELCIDE.",
                                    new Vector2(graphicsDevice.Viewport.Width/2, graphicsDevice.Viewport.Height/2 + 80),
                                    Color.White, Color.Black);
            }

            // Draw the current screen effect.
            if (_p.ScreenEffect == ScreenEffect.Death)
            {
                Color drawColor = new Color(1 - (float) _p.ScreenEffectCounter*0.5f, 0f, 0f);
                _spriteBatch.Draw(_texBlank,
                                 new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                                 drawColor);
                if (_p.ScreenEffectCounter >= 2)
                    RenderMessageCenter(_spriteBatch, "You have died. Click to respawn.",
                                        new Vector2(graphicsDevice.Viewport.Width/2, graphicsDevice.Viewport.Height/2),
                                        Color.White, Color.Black);
            }
            if (_p.ScreenEffect == ScreenEffect.Explosion)
            {
                Color drawColor = new Color(1, 1, 1, 1 - (float) _p.ScreenEffectCounter*0.5f);
                _spriteBatch.Draw(_texBlank,
                                 new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                                 drawColor);
                if (_p.ScreenEffectCounter > 2)
                    _p.ScreenEffect = ScreenEffect.None;
            }
            if (_p.ScreenEffect == ScreenEffect.Fall)
            {
                Color drawColor = new Color(1, 0, 0, 1 - (float) _p.ScreenEffectCounter*0.5f);
                _spriteBatch.Draw(_texBlank,
                                 new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                                 drawColor);
                if (_p.ScreenEffectCounter > 2)
                    _p.ScreenEffect = ScreenEffect.None;
            }
            if (_p.ScreenEffect == ScreenEffect.Water)
            {
                Color drawColor = new Color(0.0f, 0.5f, 1.0f, 1.0f - (float) _p.ScreenEffectCounter);
                _spriteBatch.Draw(_texBlank,
                                 new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                                 drawColor);
                if (_p.ScreenEffectCounter > 2)
                    _p.ScreenEffect = ScreenEffect.None;
            }
            if (_p.ScreenEffect == ScreenEffect.Drowning)
            {
                Color drawColor = new Color(0.5f, 0, 0.8f, 0.25f + (float) _p.ScreenEffectCounter*0.2f);
                _spriteBatch.Draw(_texBlank,
                                 new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                                 drawColor);
                if (_p.ScreenEffectCounter > 2)
                {
                    _p.ScreenEffect = ScreenEffect.Water;
                    _p.ScreenEffectCounter = 1;
                }
            }


            // Draw the help screen.
            if (Keyboard.GetState().IsKeyDown(Keys.F1))
            {
                _spriteBatch.Draw(_texBlank,
                                 new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                                 Color.Black);
                _spriteBatch.Draw(_texHelpBlue, _drawRect, Color.White);
            }

            _spriteBatch.End();
        }
    }
}