﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Lidgren.Network;
using Lidgren.Network.Xna;
using Microsoft.Xna.Framework;

//Contains assorted server settings

namespace MineWorld
{
    public partial class MineWorldServer
    {
        public struct ServerSettings
        {
            public string Servername;
            public int Maxplayers;
            public bool Public;
            public bool Proxy;
            public string LevelName;
            public bool Autoload;
            public bool AutoAnnouce;
            public string MOTD;
            public bool Includelava;
            public int Autosavetimer;

            //Not used by configs
            public bool StopFluids;
            public string Directory;
        }
        int lavaBlockCount = 0;
        uint oreFactor = 10;

        uint teamCashRed = 0;
        uint teamCashBlue = 0;
        uint teamOreRed = 0;
        uint teamOreBlue = 0;

        uint winningCashAmount = 10000;
        PlayerTeam winningTeam = PlayerTeam.None;
    }
}