using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

//Contains assorted server settings

namespace MineWorld
{
    public struct ClientSettings
    {
        public string Directory;
        public bool Fullscreen;
        public int Width;
        public int Height;
        public bool Vsync;
        public string playerHandle;
        public float volumeLevel;
        public bool RenderPretty;
        public bool DrawFrameRate;
        public bool InvertMouseYAxis;
        public bool NoSound;
        public float mouseSensitivity;
        public Color color;
    }
}