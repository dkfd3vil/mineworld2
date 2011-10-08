using Microsoft.Xna.Framework.Graphics;

namespace MineWorld
{
    public class Defines
    {
        public const string MINEWORLDSERVER_VERSION = "Mine World Alpha 1";
        public const string MINEWORLDCLIENT_VERSION = "Mine World Alpha 1";
        public const string MINEWORLD_PHASE = "ALPHA";
        public const double MINEWORLD_BUILD = 1;

        public const int MAPSIZE = 64;
        //Mapsize % Packetsize MUST EQUAL 0!!!
        public const int PACKETSIZE = 32;

        public const int GROUND_LEVEL = MAPSIZE / 8;

        public const string deathByLava = "WAS INCINERATED BY LAVA!";
        public const string deathByElec = "WAS ELECTROCUTED!";
        public const string deathByExpl = "WAS KILLED IN AN EXPLOSION!";
        public const string deathByFall = "WAS KILLED BY GRAVITY!";
        public const string deathByMiss = "WAS KILLED BY MISADVENTURE!";
        public const string deathByCrush = "WAS CRUSHED!";
        public const string deathByDrown = "WAS DROWNED!";
        public const string deathBySuic = "HAS COMMITED PIXELCIDE!";


        public const byte MAXLIGHT = 10;
        public const byte MINLIGHT = 1;
        public const float SIDESHADOWS = 0.0f; //0.2f

        public const float NEARPLANE = 0.01f;
        public const float FARPLANE = 140;

        public static string Sanitize(string input)
        {
            string output = "";
            for (int i = 0; i < input.Length; i++)
            {
                char c = (char)input[i];
                if (c >= 32 && c <= 126)
                    output += c;
            }
            return output;
        }
    }
}