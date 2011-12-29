using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MineWorldData;

namespace MineWorld
{
    public enum BlockModel
    {
        Cube = 0,
        Cross = 1,
        Slab = 2,
    }

    //This is the baseblock other block are derived from this class
    public class BaseBlock
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
        public BlockModel Model;
        public BlockTypes Type;

        public BaseBlock(Vector2 top, Vector2 forward, Vector2 backward, Vector2 right, Vector2 left, Vector2 bottom, BlockModel modelIn, bool SolidIn, bool TransparentIn, bool AimSolidIn, BlockTypes typeIn)
        {
            UVMapTop = top / 16f;
            UVMapForward = forward / 16f;
            UVMapBackward = backward / 16f;
            UVMapLeft = left / 16f;
            UVMapRight = right / 16f;
            UVMapBottom = bottom / 16f;
            Model = modelIn;
            Transparent = TransparentIn;
            Solid = SolidIn;
            AimSolid = AimSolidIn;
            Type = typeIn;
        }

        public BaseBlock(Vector2 UVMapIn, BlockModel modelIn, bool SolidIn, bool TransparentIn, bool AimSolidIn, BlockTypes typeIn)
        {
            UVMapTop = UVMapIn / 16f;
            UVMapForward = UVMapIn / 16f;
            UVMapBackward = UVMapIn / 16f;
            UVMapLeft = UVMapIn / 16f;
            UVMapRight = UVMapIn / 16f;
            UVMapBottom = UVMapIn / 16f;
            Model = modelIn;
            Transparent = TransparentIn;
            Solid = SolidIn;
            AimSolid = AimSolidIn;
            Type = typeIn;
        }

        public virtual void OnUse()
        {
            return;
        }
    }
}
