using System;
using System.Collections.Generic;
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
using Microsoft.Xna.Framework.Design;
using MineWorld;

namespace InterfaceItems
{
    class InterfaceTextInput : InterfaceElement
    {
        public string value = "";
        private bool partialInFocus = false;
        private bool inFocus=false;

        public InterfaceTextInput()
        {
        }

        public InterfaceTextInput(MineWorldGame gameInstance)
        {
            uiFont = gameInstance.Content.Load<SpriteFont>("font_04b08");
        }

        public InterfaceTextInput(MineWorldGame gameInstance, PropertyBag pb)
        {
            uiFont = gameInstance.Content.Load<SpriteFont>("font_04b08");
            _P = pb;
        }

        public override void OnMouseDown(MouseButtons button, int x, int y)
        {
            if (enabled && size.Contains(x, y))
                partialInFocus = true;
            else
                inFocus = false;
        }

        public override void OnMouseUp(MouseButtons button, int x, int y)
        {
            if (enabled && partialInFocus && size.Contains(x, y))
            {
                inFocus = true;
                _P.PlaySound(MineWorldSound.ClickLow);
            }
            partialInFocus = false;
        }

        public override void OnCharEntered(EventInput.CharacterEventArgs e)
        {
            base.OnCharEntered(e);
            if ((int)e.Character < 32 || (int)e.Character > 126) //From space to tilde
                return; //Do nothing

            if (inFocus)
            {
                value += e.Character;
            }
        }

        public override void OnKeyDown(Keys key)
        {
            base.OnKeyDown(key);
            if (inFocus)
            {
                if (key == Keys.Enter)
                {
                    inFocus = false;
                    _P.PlaySound(MineWorldSound.ClickHigh);
                }
                else if (key == Keys.Back&&value.Length>0)
                    value = value.Substring(0, value.Length - 1);
            }
        }

        public override void Render(GraphicsDevice graphicsDevice)
        {
            if (visible && size.Width > 0 && size.Height > 0)
            {
                Color drawColour = new Color(1f, 1f, 1f);

                if (!enabled)
                    drawColour = new Color(.7f, .7f, .7f);
                else if (!inFocus)
                    drawColour = new Color(.85f, .85f, .85f);

                //Generate 1px white texture
                Texture2D shade = new Texture2D(graphicsDevice, 1, 1, true, SurfaceFormat.Color);
                shade.SetData(new Color[] { Color.White });

                //Draw base background
                SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
                spriteBatch.Begin();
                spriteBatch.Draw(shade, size, drawColour);

                spriteBatch.DrawString(uiFont, value, new Vector2(size.X + size.Width / 2 - uiFont.MeasureString(value).X / 2, size.Y + size.Height / 2 - 8), Color.Black);

                if (text != "")
                {
                    //Draw text
                    spriteBatch.DrawString(uiFont, text, new Vector2(size.X, size.Y - 20), enabled ? Color.White : new Color(.7f, .7f, .7f));//drawColour);
                }

                spriteBatch.End();
                shade.Dispose();
            }
        }
    }
}
