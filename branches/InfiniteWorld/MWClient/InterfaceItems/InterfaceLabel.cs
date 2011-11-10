using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MineWorld.InterfaceItems
{
    internal class InterfaceLabel : InterfaceElement
    {
        public InterfaceLabel()
        {
        }

        public InterfaceLabel(MineWorldGame gameInstance)
        {
            UiFont = gameInstance.Content.Load<SpriteFont>("font_04b08");
        }

        public InterfaceLabel(MineWorldGame gameInstance, PropertyBag pb)
        {
            UiFont = gameInstance.Content.Load<SpriteFont>("font_04b08");
            P = pb;
        }

        public override void Render(GraphicsDevice graphicsDevice)
        {
            if (Visible && Text != "")
            {
                SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
                spriteBatch.Begin();

                spriteBatch.DrawString(UiFont, Text, new Vector2(Size.X, Size.Y), Color.White);
                spriteBatch.End();
            }
        }
    }
}