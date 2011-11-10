using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MineWorld.InterfaceItems
{
    internal class InterfaceTextInput : InterfaceElement
    {
        private bool _inFocus;
        private bool _partialInFocus;
        public string Value = "";

        public InterfaceTextInput()
        {
        }

        public InterfaceTextInput(MineWorldGame gameInstance)
        {
            UiFont = gameInstance.Content.Load<SpriteFont>("font_04b08");
        }

        public InterfaceTextInput(MineWorldGame gameInstance, PropertyBag pb)
        {
            UiFont = gameInstance.Content.Load<SpriteFont>("font_04b08");
            P = pb;
        }

        public override void OnMouseDown(MouseButtons button, int x, int y)
        {
            if (Enabled && Size.Contains(x, y))
                _partialInFocus = true;
            else
                _inFocus = false;
        }

        public override void OnMouseUp(MouseButtons button, int x, int y)
        {
            if (Enabled && _partialInFocus && Size.Contains(x, y))
            {
                _inFocus = true;
                P.PlaySound(MineWorldSound.ClickLow);
            }
            _partialInFocus = false;
        }

        public override void OnCharEntered(CharacterEventArgs e)
        {
            base.OnCharEntered(e);
            if (e.Character < 32 || e.Character > 126) //From space to tilde
                return; //Do nothing

            if (_inFocus)
            {
                Value += e.Character;
            }
        }

        public override void OnKeyDown(Keys key)
        {
            base.OnKeyDown(key);
            if (_inFocus)
            {
                if (key == Keys.Enter)
                {
                    _inFocus = false;
                    P.PlaySound(MineWorldSound.ClickHigh);
                }
                else if (key == Keys.Back && Value.Length > 0)
                    Value = Value.Substring(0, Value.Length - 1);
            }
        }

        public override void Render(GraphicsDevice graphicsDevice)
        {
            if (Visible && Size.Width > 0 && Size.Height > 0)
            {
                Color drawColour = new Color(1f, 1f, 1f);

                if (!Enabled)
                    drawColour = new Color(.7f, .7f, .7f);
                else if (!_inFocus)
                    drawColour = new Color(.85f, .85f, .85f);

                //Generate 1px white texture
                Texture2D shade = new Texture2D(graphicsDevice, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
                shade.SetData(new[] {Color.White});

                //Draw base background
                SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
                spriteBatch.Begin();
                spriteBatch.Draw(shade, Size, drawColour);

                spriteBatch.DrawString(UiFont, Value,
                                       new Vector2(Size.X + Size.Width/2 - UiFont.MeasureString(Value).X/2,
                                                   Size.Y + Size.Height/2 - 8), Color.Black);

                if (Text != "")
                {
                    //Draw text
                    spriteBatch.DrawString(UiFont, Text, new Vector2(Size.X, Size.Y - 20),
                                           Enabled ? Color.White : new Color(.7f, .7f, .7f)); //drawColour);
                }

                spriteBatch.End();
                shade.Dispose();
            }
        }
    }
}