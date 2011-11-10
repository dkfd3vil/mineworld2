using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MineWorld.InterfaceItems
{
    internal class InterfaceSlider : InterfaceElement
    {
        public bool Integers;
        public float MaxVal = 1f;
        public float MinVal;
        private bool _sliding;
        public float Value;

        public InterfaceSlider()
        {
        }

        public InterfaceSlider(MineWorldGame gameInstance)
        {
            UiFont = gameInstance.Content.Load<SpriteFont>("font_04b08");
        }

        public InterfaceSlider(MineWorldGame gameInstance, PropertyBag pb)
        {
            UiFont = gameInstance.Content.Load<SpriteFont>("font_04b08");
            P = pb;
        }

        public void SetValue(float newVal)
        {
            if (Integers)
                Value = (int) Math.Round(newVal);
            else
                Value = newVal;
        }

        public float GetPercent()
        {
            return (Value - MinVal)/(MaxVal - MinVal);
        }

        public override void OnMouseDown(MouseButtons button, int x, int y)
        {
            if (Size.Contains(x, y))
            {
                _sliding = true;
                Update(x, y);
            }
        }

        public void Update()
        {
            MouseState ms = Mouse.GetState();
            if (ms.LeftButton == ButtonState.Pressed)
                Update(ms.X, ms.Y);
            else
                _sliding = false;
        }

        public void Update(int x, int y)
        {
            if (_sliding)
            {
                MouseState ms = Mouse.GetState();
                if (ms.LeftButton == ButtonState.Released)
                    _sliding = false;
                else
                {
                    if (x < Size.X + Size.Height)
                        Value = MinVal;
                    else if (x > Size.X + Size.Width - Size.Height)
                        Value = MaxVal;
                    else
                    {
                        int xMouse = x - Size.X - Size.Height;
                        int xMax = Size.Width - 2*Size.Height;
                        float sliderPercent = xMouse/(float) xMax;
                        if (Integers)
                            Value = (int) Math.Round((sliderPercent*(MaxVal - MinVal)) + MinVal);
                        else
                            Value = sliderPercent*(MaxVal - MinVal) + MinVal;
                        if (Value < MinVal)
                            Value = MinVal;
                        else if (Value > MaxVal)
                            Value = MaxVal;
                    }
                }
            }
        }

        public override void Render(GraphicsDevice graphicsDevice)
        {
            Update();

            if (Visible && Size.Width > 0 && Size.Height > 0)
            {
                Color drawColour = new Color(1f, 1f, 1f);

                if (!Enabled)
                {
                    drawColour = new Color(.5f, .5f, .5f);
                }
                //Generate 1px white texture
                Texture2D shade = new Texture2D(graphicsDevice, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
                shade.SetData(new[] {Color.White});
                //Draw end boxes
                SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                spriteBatch.Draw(shade, new Rectangle(Size.X, Size.Y, Size.Height, Size.Height), drawColour);
                spriteBatch.Draw(shade,
                                 new Rectangle(Size.X + Size.Width - Size.Height, Size.Y, Size.Height, Size.Height),
                                 drawColour);

                //Draw line
                float sliderPercent = GetPercent();
                int sliderPartialWidth = Size.Height/4;
                int midHeight = (Size.Height/2) - 1;
                int actualWidth = Size.Width - 2*Size.Height;
                int actualPosition = (int) (sliderPercent*actualWidth);
                spriteBatch.Draw(shade, new Rectangle(Size.X, Size.Y + midHeight, Size.Width, 1), drawColour);

                //Draw slider
                spriteBatch.Draw(shade,
                                 new Rectangle(Size.X + Size.Height + actualPosition - sliderPartialWidth,
                                               Size.Y + midHeight - sliderPartialWidth, Size.Height/2, Size.Height/2),
                                 drawColour);
                if (Text != "")
                {
                    //Draw text
                    spriteBatch.DrawString(UiFont, Text, new Vector2(Size.X, Size.Y - 36), drawColour);
                }
                //Draw amount
                spriteBatch.DrawString(UiFont, (((float) (int) (Value*10))/10).ToString(),
                                       new Vector2(Size.X, Size.Y - 20), drawColour);

                spriteBatch.End();
                shade.Dispose();
            }
        }
    }
}