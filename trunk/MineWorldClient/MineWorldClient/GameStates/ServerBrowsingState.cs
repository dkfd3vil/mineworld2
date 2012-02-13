using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TomShane.Neoforce.Controls;
using MineWorldData;

namespace MineWorld
{
    public class ServerBrowsingState : BaseState
    {
        GameStateManager gamemanager;
        Manager guiman;
        Window serverbrowsingmenu;
        ListBox serversbox;
        Dictionary<string, ServerInformation> servers = new Dictionary<string,ServerInformation>();
        Button join;
        Button refresh;
        Button back;

        public ServerBrowsingState(GameStateManager manager, GameStates associatedState)
            : base(manager, associatedState)
        {
            gamemanager = manager;
        }

        public override void LoadContent(ContentManager contentloader)
        {
            guiman = new Manager(gamemanager.game, gamemanager.graphics, "Default");
            guiman.Initialize();

            serverbrowsingmenu = new Window(guiman);
            serverbrowsingmenu.Init();
            serverbrowsingmenu.Resizable = false;
            serverbrowsingmenu.Movable = false;
            serverbrowsingmenu.CloseButtonVisible = false;
            serverbrowsingmenu.Text = "Server Browser";
            serverbrowsingmenu.Width = 300;
            serverbrowsingmenu.Height = 400;
            serverbrowsingmenu.Center();
            serverbrowsingmenu.Visible = true;
            serverbrowsingmenu.BorderVisible = true;

            serversbox = new ListBox(guiman);
            serversbox.Init();
            //servers.SetPosition(50, 50);
            serversbox.Left = 50;
            serversbox.Top = 150;
            serversbox.Width = 200;
            serversbox.Height = 200;
            serversbox.Anchor = Anchors.Bottom;
            serversbox.Parent = serverbrowsingmenu;

            join = new Button(guiman);
            join.Init();
            join.Text = "Join";
            join.Width = 200;
            join.Height = 50;
            join.Left = 50;
            join.Top = 0;
            join.Anchor = Anchors.Bottom;
            join.Parent = serverbrowsingmenu;

            refresh = new Button(guiman);
            refresh.Init();
            refresh.Text = "Refresh";
            refresh.Width = 200;
            refresh.Height = 50;
            refresh.Left = 50;
            refresh.Top = 50;
            refresh.Anchor = Anchors.Bottom;
            refresh.Parent = serverbrowsingmenu;

            back = new Button(guiman);
            back.Init();
            back.Text = "Back";
            back.Width = 200;
            back.Height = 50;
            back.Left = 50;
            back.Top = 100;
            back.Anchor = Anchors.Bottom;
            back.Parent = serverbrowsingmenu;

            guiman.Cursor = guiman.Skin.Cursors["Default"].Resource;
            guiman.Add(serverbrowsingmenu);

            gamemanager.game.IsMouseVisible = true;
        }

        public override void Unload()
        {
        }

        public override void Update(GameTime gameTime, InputHelper input)
        {
            guiman.Update(gameTime);
            if (gamemanager.game.IsActive)
            {
                if (join.Pushed)
                {
                    join.Pushed = false;
                    string selectedserver;
                    selectedserver = serversbox.Items[serversbox.ItemIndex].ToString();

                    foreach (ServerInformation server in servers.Values)
                    {
                        if (selectedserver == server.servername)
                        {
                            gamemanager.Pbag.ClientSender.SendJoinGame(server.ipaddress);
                            gamemanager.SwitchState(GameStates.LoadingState);
                        }
                    }
                }
                if (refresh.Pushed)
                {
                    refresh.Pushed = false;
                    serversbox.Items.Clear();
                    servers.Clear();
                    gamemanager.Pbag.ClientSender.DiscoverLocalServers(Constants.MINEWORLD_PORT);
                }
                if (back.Pushed)
                {
                    back.Pushed = false;
                    gamemanager.SwitchState(GameStates.MainMenuState);
                }
            }
        }

        public void AddServer(ServerInformation server)
        {
            servers.Add(server.servername, server);
            serversbox.Items.Add(server.servername);
        }

        public void RemoveServer(ServerInformation server)
        {
            servers.Remove(server.servername);
            serversbox.Items.Remove(server.servername);
        }

        public override void Draw(GameTime gameTime, GraphicsDevice gDevice, SpriteBatch sBatch)
        {
            guiman.BeginDraw(gameTime);
            guiman.EndDraw();
        }
    }
}