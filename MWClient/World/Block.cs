using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MineWorld
{
    public enum BlockModel
    {
        Cube = 0,
        Plant,
        //Slab,
    }

    public class Block
    {
        //This class only contains UV map coordinates, and solid/transparent bools
        //The only constructor available requires all fields

        public Vector2 UVMapTop;
        public Vector2 UVMapRight;
        public Vector2 UVMapLeft;
        public Vector2 UVMapForward;
        public Vector2 UVMapBackward;
        public Vector2 UVMapBottom;
        public bool Solid;
        public bool Transparent;
        public bool AimSolid;
        public int Model = 0;

        public Block(Vector2 top, Vector2 forward, Vector2 backward, Vector2 right, Vector2 left, Vector2 bottom, bool SolidIn, bool TransparentIn, bool AimSolidIn)
        {
            UVMapTop = top / 16f;
            UVMapForward = forward / 16f;
            UVMapBackward = backward / 16f;
            UVMapLeft = left / 16f;
            UVMapRight = right / 16f;
            UVMapBottom = bottom / 16f;
            Transparent = TransparentIn;
            Solid = SolidIn;
            AimSolid = AimSolidIn;
        }

        public Block(Vector2 UVMapIn, bool SolidIn, bool TransparentIn, bool AimSolidIn)
        {
            UVMapTop = UVMapIn / 16f;
            UVMapForward = UVMapIn / 16f;
            UVMapBackward = UVMapIn / 16f;
            UVMapLeft = UVMapIn / 16f;
            UVMapRight = UVMapIn / 16f;
            UVMapBottom = UVMapIn / 16f;
            Transparent = TransparentIn;
            Solid = SolidIn;
            AimSolid = AimSolidIn;
        }

        public Block(Vector2 top, Vector2 forward, Vector2 backward, Vector2 right, Vector2 left, Vector2 bottom, bool SolidIn, bool TransparentIn, bool AimSolidIn, int ModelIn)
        {
            UVMapTop = top / 16f;
            UVMapForward = forward / 16f;
            UVMapBackward = backward / 16f;
            UVMapLeft = left / 16f;
            UVMapRight = right / 16f;
            UVMapBottom = bottom / 16f;
            Transparent = TransparentIn;
            Solid = SolidIn;
            AimSolid = AimSolidIn;
            Model = ModelIn;
        }

        public Block(Vector2 UVMapIn, bool SolidIn, bool TransparentIn, bool AimSolidIn, int ModelIn)
        {
            UVMapTop = UVMapIn / 16f;
            UVMapForward = UVMapIn / 16f;
            UVMapBackward = UVMapIn / 16f;
            UVMapLeft = UVMapIn / 16f;
            UVMapRight = UVMapIn / 16f;
            UVMapBottom = UVMapIn / 16f;
            Transparent = TransparentIn;
            Solid = SolidIn;
            AimSolid = AimSolidIn;
            Model = ModelIn;
        }
    }
}
