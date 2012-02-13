﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using System.Threading;
using MineWorldData;
using Microsoft.Xna.Framework;
using EasyConfig;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace MineWorldServer
{
    public class MineWorldServer
    {
        //Server variables
        public NetServer Server;
        public GameWorld GameWorld;
        public MapManager MapManager;
        public PlayerManager PlayerManager;

        public Thread Listener;
        public ServerListener ServerListener;
        public ServerSender ServerSender;

        public bool KeepServerRunning = true;
        public bool RestartServer = false;

        public string ServerName;

        //Console part
        public MineWorldConsole console;
        private DateTime _lastKeyAvaible = DateTime.Now;

        public ConfigFile Configloader;

        public byte[] Terrain;

        public MineWorldServer()
        {
            NetPeerConfiguration netConfig = new NetPeerConfiguration("MineWorld");
            netConfig.Port = Constants.MINEWORLD_PORT;
            netConfig.MaximumConnections = 2;
            netConfig.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            netConfig.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            netConfig.DisableMessageType(NetIncomingMessageType.UnconnectedData);
            Server = new NetServer(netConfig);
            GameWorld = new GameWorld(this);
            console = new MineWorldConsole(this);
            ServerListener = new ServerListener(Server,this);
            ServerSender = new ServerSender(Server, this);
            Listener = new Thread(ServerListener.Start);
            MapManager = new MapManager();
            PlayerManager = new PlayerManager();
            Configloader = new ConfigFile("Data/Settings.ini");
        }

        public void Start()
        {
            console.SetTitle("MineWorldServer v" + Constants.MINEWORLDSERVER_VERSION.ToString());

            LoadSettings();
            MapManager.GenerateCubeMap(BlockTypes.Dirt);

            Server.Start();
            Listener.Start();

            while (KeepServerRunning)
            {
                // Handle console keypresses.
                while (console.KeyAvailable())
                {
                    // What if there is constant keyavaible ?
                    // This code makes sure the rest of the program can also run
                    TimeSpan timeSpanLastKeyAvaible = DateTime.Now - _lastKeyAvaible;
                    if (timeSpanLastKeyAvaible.Milliseconds < 2000)
                    {
                        string input = console.ReadLine();
                        console.ProcessInput(input);
                        _lastKeyAvaible = DateTime.Now;
                        break;
                    }
                }
            }
        }

        public void LoadSettings()
        {
            ServerName = Configloader.SettingGroups["Server"].Settings["Name"].GetValueAsString();
            Server.Configuration.MaximumConnections = Configloader.SettingGroups["Server"].Settings["Maxplayers"].GetValueAsInt();
            int size = Configloader.SettingGroups["Map"].Settings["Mapsize"].GetValueAsInt();
            MapManager.SetMapSize(size);
        }

        public bool VersionMatch(int version)
        {
            if (Constants.MINEWORLDSERVER_VERSION == version)
            {
                return true;
            }

            return false;
        }
    }
}