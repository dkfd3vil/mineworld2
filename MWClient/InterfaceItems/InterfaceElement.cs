using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MineWorld.InterfaceItems
{
    internal class InterfaceElement
    {
        public PropertyBag P;
        public bool Enabled;
        public Rectangle Size = Rectangle.Empty;
        public string Text = "";
        public SpriteFont UiFont;
        public bool Visible;

        public InterfaceElement()
        {
        }

        public InterfaceElement(MineWorldGame gameInstance, PropertyBag pb)
        {
            UiFont = gameInstance.Content.Load<SpriteFont>("font_04b08");
            P = pb;
        }

        public virtual void OnCharEntered(CharacterEventArgs e)
        {
        }

        public virtual void OnKeyDown(Keys key)
        {
        }

        public virtual void OnKeyUp(Keys key)
        {
        }

        public virtual void OnMouseDown(MouseButtons button, int x, int y)
        {
        }

        public virtual void OnMouseUp(MouseButtons button, int x, int y)
        {
        }

        public virtual void OnMouseScroll(int scrollWheelValue)
        {
        }

        public virtual void Render(GraphicsDevice graphicsDevice)
        {
        }
    }
}