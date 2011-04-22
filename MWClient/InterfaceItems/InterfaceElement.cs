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
using MineWorld;

namespace InterfaceItems
{
    class InterfaceElement
    {
        public bool visible = false;
        public bool enabled = false;
        public string text = "";
        public Rectangle size = Rectangle.Empty;
        public SpriteFont uiFont;
        public PropertyBag _P;

        public InterfaceElement()
        {
        }

        public InterfaceElement(MineWorldGame gameInstance, PropertyBag pb)
        {
            uiFont = gameInstance.Content.Load<SpriteFont>("font_04b08");
            _P = pb;
        }

        public virtual void OnCharEntered(EventInput.CharacterEventArgs e)
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
