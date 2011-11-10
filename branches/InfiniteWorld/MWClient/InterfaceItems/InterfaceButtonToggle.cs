using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MineWorld.InterfaceItems
{
    internal class InterfaceButtonToggle : InterfaceElement
    {
        public bool Clicked;
        private bool _midClick;
        public string OffText = "Off";
        public string OnText = "On";

        public InterfaceButtonToggle()
        {
        }

        public InterfaceButtonToggle(MineWorldGame gameInstance)
        {
            UiFont = gameInstance.Content.Load<SpriteFont>("font_04b08");
        }

        public InterfaceButtonToggle(MineWorldGame gameInstance, PropertyBag pb)
        {
            UiFont = gameInstance.Content.Load<SpriteFont>("font_04b08");
            P = pb;
        }

        public override void OnMouseDown(MouseButtons button, int x, int y)
        {
            if (Enabled && Size.Contains(x, y))
            {
                _midClick = true;
            }
            else
                _midClick = false;
        }

        public override void OnMouseUp(MouseButtons button, int x, int y)
        {
            if (Enabled && _midClick && Size.Contains(x, y))
            {
                Clicked = !Clicked;
                P.PlaySound(MineWorldSound.ClickLow);
            }
            _midClick = false;
        }

        public override void Render(GraphicsDevice graphicsDevice)
        {
            if (Visible && Size.Width > 0 && Size.Height > 0)
            {
                Color drawColour = new Color(1f, 1f, 1f);

                if (!Enabled)
                    drawColour = new Color(.7f, .7f, .7f);
                else if (_midClick)
                    drawColour = new Color(.85f, .85f, .85f);

                //Generate 1px white texture
                Texture2D shade = new Texture2D(graphicsDevice, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
                shade.SetData(new[] {Color.White});

                //Draw base button
                SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
                spriteBatch.Begin();
                spriteBatch.Draw(shade, Size, drawColour);

                //Draw button text
                string dispText = OffText;
                if (Clicked)
                    dispText = OnText;

                spriteBatch.DrawString(UiFont, dispText,
                                       new Vector2(Size.X + Size.Width/2 - UiFont.MeasureString(dispText).X/2,
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