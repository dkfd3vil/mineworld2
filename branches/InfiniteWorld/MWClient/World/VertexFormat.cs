﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector; //I included this because it contains the Byte4 type

namespace MineWorld
{
    public struct VertexFormat
    {
        //This is my own custom vertex format, which uses bytes for a position rather than vectors composed of floats
        private Byte4 Position;
        private Vector3 Normal;
        private Vector2 TexCoord;

        //The constructor takes a 3 byte position, a vector3 normal, and a vector2 UV map coordinate
        public VertexFormat(byte x, byte y, byte z, Vector3 normal, Vector2 UV)
        {
            this.Position = new Byte4(x, y, z, 1);
            this.Normal = normal;
            this.TexCoord = UV;
        }


        //This helps pass info to the effect file
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Byte4, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(Byte) * 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(Byte)*4 + sizeof(float)*3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );
    }
}
