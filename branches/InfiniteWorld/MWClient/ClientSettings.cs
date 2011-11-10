 //Contains assorted client settings

namespace MineWorld
{
    public struct ClientSettings
    {
        public string Directory;
        public bool DrawFrameRate;
        public bool Fullscreen;
        public int Height;
        public bool InvertMouseYAxis;
        public bool NoSound;
        public bool RenderPretty;
        public bool Vsync;
        public int Width;

        public int MapSeed;
        public int MapX;
        public int MapY;
        public int MapZ;
        public float MouseSensitivity;
        public string PlayerHandle;
        public float VolumeLevel;
    }
}