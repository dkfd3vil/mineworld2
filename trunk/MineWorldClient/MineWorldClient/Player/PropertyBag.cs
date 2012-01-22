using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Lidgren.Network.Xna;
using MineWorldData;
using System.IO;
using Microsoft.Xna.Framework;

namespace MineWorld
{
    public class PropertyBag
    {
        public MineWorldClient Game;
        public GameStateManager GameManager;
        public WorldManager WorldManager;
        public Player Player;
        public Debug Debugger;
        public NetClient Client;
        public ClientListener ClientListener;
        public ClientSender ClientSender;
        public NetIncomingMessage _msgBuffer;
        public BlockTypes[, ,] Tempblockmap;

        public PropertyBag(MineWorldClient Gamein,GameStateManager GameManagerin)
        {
            Game = Gamein;
            GameManager = GameManagerin;
            NetPeerConfiguration netconfig = new NetPeerConfiguration("MineWorld");
            Client = new NetClient(netconfig);
            Client.Start();
            Player = new Player(this);
            WorldManager = new WorldManager(GameManager, Player);
            ClientListener = new ClientListener(Client, this);
            ClientSender = new ClientSender(Client, this);
            Debugger = new Debug(this);
        }
    }
}
