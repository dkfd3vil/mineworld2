﻿using System;
using System.Collections.Generic;
using System.Text;
using MineWorld;
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

namespace InterfaceItems
{
    class InterfaceLabel : InterfaceElement
    {
        public InterfaceLabel()
        {
        }

        public InterfaceLabel(MineWorldGame gameInstance)
        {
            uiFont = gameInstance.Content.Load<SpriteFont>("font_04b08");
        }

        public InterfaceLabel(MineWorldGame gameInstance, PropertyBag pb)
        {
            uiFont = gameInstance.Content.Load<SpriteFont>("font_04b08");
            _P = pb;
        }

        public override void Render(GraphicsDevice graphicsDevice)
        {
            if (visible&&text!="")
            {
                SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
                spriteBatch.Begin();

                spriteBatch.DrawString(uiFont, text, new Vector2(size.X, size.Y), Color.White);
                spriteBatch.End();
            }
        }
    }
}
