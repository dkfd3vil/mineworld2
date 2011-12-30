using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using System.Threading;
using MineWorldData;
using Microsoft.Xna.Framework;

namespace MineWorldServer
{
    public class MineWorldServer
    {
        //Server variables
        public NetServer Server;
        public GameWorld GameWorld;
        public Thread Listener;
        public ServerListener ServerListener;
        public ServerSender ServerSender;
        public bool KeepServerRunning = true;
        public bool RestartServer = false;

        //Console part
        public MineWorldConsole console;
        private DateTime _lastKeyAvaible = DateTime.Now;


        public Dictionary<NetConnection, ServerPlayer> PlayerList = new Dictionary<NetConnection,ServerPlayer>();
        public Vector3 WorldMapSize = new Vector3(64,64,64);
        public BlockTypes[, ,] WorldMap;

        public MineWorldServer()
        {
            NetPeerConfiguration netConfig = new NetPeerConfiguration("MineWorld");
            netConfig.Port = Constants.MINEWORLD_PORT;
            netConfig.MaximumConnections = 2;
            netConfig.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            Server = new NetServer(netConfig);
            GameWorld = new GameWorld(this);
            console = new MineWorldConsole(this);
            ServerListener = new ServerListener(Server,this);
            ServerSender = new ServerSender(Server, this);
            Listener = new Thread(ServerListener.Start);
            WorldMap = new BlockTypes[(int)WorldMapSize.X, (int)WorldMapSize.Y, (int)WorldMapSize.Z];
        }

        public void Start()
        {
            console.SetTitle("MineWorldServer v" + Constants.MINEWORLDSERVER_VERSION);
            Server.Start();
            Listener.Start();

            GameWorld.GenerateSimpleMap(BlockTypes.Dirt);

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
    }
}
