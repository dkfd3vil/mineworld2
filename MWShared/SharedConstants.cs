﻿using Microsoft.Xna.Framework.Graphics;

namespace MineWorld
{
    public class Defines
    {
        public const double MINEWORLD_VER = 5.0;
        public const string MINEWORLDSERVER_VERSION = "Mine World Beta 5.0";
        public const string MINEWORLDCLIENT_VERSION = "Mine World Beta 5.0";

        public const int MINEWORLD_PORT = 5565;

        public const string MASTERSERVER_BASE_URL = "http://humorco.nl/mineworld/";

        //Mapsize % Packetsize MUST EQUAL 0!!!
        public const int MAPSIZE = 64;
        public const int PACKETSIZE = 32;

        public const int GROUND_LEVEL = MAPSIZE / 8;

        public const byte MAXLIGHT = 10;
        public const byte MINLIGHT = 1;
        public const float SIDESHADOWS = 0.0f; //0.2f

        public const float NEARPLANE = 0.01f;
        public const float FARPLANE = 140;

        // Todo is this really needed? what could the client send that upsets the server?
        // TODO Test stuff xD
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