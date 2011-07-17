﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Diagnostics;
using StateMasher;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace MineWorld.States
{
    public class ServerBrowserState : State
    {
        Texture2D texMenu;
        Rectangle drawRect;
        string nextState = null;
        List<ServerInformation> serverList = new List<ServerInformation>();
        List<int> descWidths;
        SpriteFont uiFont;
        bool directConnectIPEnter = false;
        string directConnectIP = "";
        //KeyMap keyMap;

        ClickRegion[] clkMenuServer = new ClickRegion[3] {
            new ClickRegion(new Rectangle(0,713,425,42), "direct"),
            new ClickRegion(new Rectangle(456,713,262,42),"settings"),
	        new ClickRegion(new Rectangle(763,713,243,42), "refresh")
        };

        public override void OnEnter(string oldState)
        {
            _SM.IsMouseVisible = true;
            (_SM as MineWorldGame).ResetPropertyBag();
            _P = _SM.propertyBag;

            texMenu = _SM.Content.Load<Texture2D>("menus/tex_menu_server");

            drawRect = new Rectangle(_SM.GraphicsDevice.Viewport.Width / 2 - 1024 / 2,
                                     _SM.GraphicsDevice.Viewport.Height / 2 - 768 / 2,
                                     1024,
                                     1024);

            uiFont = _SM.Content.Load<SpriteFont>("font_04b08");
            //keyMap = new KeyMap();
            
            serverList = (_SM as MineWorldGame).EnumerateServers(0.5f);
        }

        public override void OnLeave(string newState)
        {

        }

        public override string OnUpdate(GameTime gameTime, KeyboardState keyState, MouseState mouseState)
        {
            return nextState;
        }

        public override void OnRenderAtEnter(GraphicsDevice graphicsDevice)
        {

        }

        public override void OnRenderAtUpdate(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            //check for IP in command line and skip server browser
            if ((_SM as MineWorldGame).IPargument != null)
            {
                IPAddress IPtoJoin = (_SM as MineWorldGame).IPargument;
                (_SM as MineWorldGame).IPargument = null;
                (_SM as MineWorldGame).propertyBag.serverName = IPtoJoin.ToString();
                (_SM as MineWorldGame).JoinGame(new IPEndPoint(IPtoJoin, Defines.MINEWORLD_PORT));

                nextState = "MineWorld.States.LoadingState";
            }

            //begin server browser
            descWidths = new List<int>();
            SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.Draw(texMenu, drawRect, Color.White);

            int drawY = 80;
            foreach (ServerInformation server in serverList)
            {
                if (drawY < 660)
                {
                    int textWidth = (int)(uiFont.MeasureString(server.GetServerDesc()).X);
                    descWidths.Add(textWidth+30);
                    spriteBatch.DrawString(uiFont, server.GetServerDesc(), new Vector2(_SM.GraphicsDevice.Viewport.Width / 2 - textWidth / 2, drawRect.Y + drawY), !server.lanServer && server.numPlayers == server.maxPlayers ? new Color(0.7f, 0.7f, 0.7f) : Color.White);
                    drawY += 25;
                }
            }

            spriteBatch.DrawString(uiFont, Defines.MINEWORLDCLIENT_VERSION, new Vector2(10, _SM.GraphicsDevice.Viewport.Height - 20), Color.White);

            if (directConnectIPEnter)
                spriteBatch.DrawString(uiFont, "ENTER IP: " + directConnectIP, new Vector2(drawRect.X + 30, drawRect.Y + 690), Color.White);

            spriteBatch.End();
        }

        public override void OnCharEntered(EventInput.CharacterEventArgs e)
        {
            if ((int)e.Character < 32 || (int)e.Character > 126) //From space to tilde
                return; //Do nothing

            //Only respond if entering an ip and control is not pressed
            if (directConnectIPEnter && !(Keyboard.GetState().IsKeyDown(Keys.LeftControl) || Keyboard.GetState().IsKeyDown(Keys.RightControl)))
            {
                directConnectIP += e.Character;
            }
        }

        public override void OnKeyDown(Keys key)
        {
            if (directConnectIPEnter)
            {
                if (key == Keys.Escape)
                {
                    directConnectIPEnter = false;
                    directConnectIP = "";
                }

                if (key == Keys.Back && directConnectIP.Length > 0)
                {
                    directConnectIP = directConnectIP.Substring(0, directConnectIP.Length - 1);
                }

                if (key == Keys.Enter)
                {
                    // Try what was entered first as an IP, and then second as a host name.
                    directConnectIPEnter = false;
                    _P.PlaySound(MineWorldSound.ClickHigh);
                    IPAddress connectIp = null;
                    if (!IPAddress.TryParse(directConnectIP, out connectIp))
                    {
                        connectIp = null;
                        try
                        {
                            IPAddress[] resolveResults = Dns.GetHostAddresses(directConnectIP);
                            for (int i = 0; i < resolveResults.Length; i++)
                                if (resolveResults[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                {
                                    connectIp = resolveResults[i];
                                    break;
                                }
                        }
                        catch (Exception)
                        {
                            // So, GetHostAddresses() might fail, but we don't really care. Just leave connectIp as null.
                            // WTF or you inform the user....
                        }
                    }
                    if (connectIp != null)                   
                    {
                        (_SM as MineWorldGame).propertyBag.serverName = directConnectIP;
                        (_SM as MineWorldGame).JoinGame(new IPEndPoint(connectIp, Defines.MINEWORLD_PORT));
                        nextState = "MineWorld.States.LoadingState";
                    }
                    directConnectIP = "";
                }

                if (key == Keys.V && (Keyboard.GetState().IsKeyDown(Keys.LeftControl) || Keyboard.GetState().IsKeyDown(Keys.RightControl)))
                {
                    try
                    {
                        directConnectIP += System.Windows.Forms.Clipboard.GetText();
                    }
                    catch { }
                }
            }
            else
            {
                if (key == Keys.Escape)
                {
                    _SM.Exit();
                }
            }
        }

        public override void OnKeyUp(Keys key)
        {

        }

        public override void OnMouseDown(MouseButtons button, int x, int y)
        {
            if (directConnectIPEnter == false)
            {
                int serverIndex = (y - drawRect.Y - 75) / 25;
                if (serverIndex >= 0 && serverIndex < serverList.Count)
                {
                    int distanceFromCenter = Math.Abs(_SM.GraphicsDevice.Viewport.Width / 2 - x);
                    if (distanceFromCenter < descWidths[serverIndex] / 2)
                    {
                        (_SM as MineWorldGame).propertyBag.serverName = serverList[serverIndex].serverName;
                        (_SM as MineWorldGame).JoinGame(serverList[serverIndex].ipEndPoint);
                        nextState = "MineWorld.States.LoadingState";
                        _P.PlaySound(MineWorldSound.ClickHigh);
                    }
                }

                x -= drawRect.X;
                y -= drawRect.Y;
                switch (ClickRegion.HitTest(clkMenuServer, new Point(x, y)))
                {
                    case "refresh":
                        _P.PlaySound(MineWorldSound.ClickHigh);
                        serverList = (_SM as MineWorldGame).EnumerateServers(0.5f);
                        break;

                    case "direct":
                        directConnectIPEnter = true;
                        _P.PlaySound(MineWorldSound.ClickHigh);
                        break;
                    case "settings":
                        nextState = "MineWorld.States.SettingsState";
                        _P.PlaySound(MineWorldSound.ClickHigh);
                        break;
                }
            }
        }

        public override void OnMouseUp(MouseButtons button, int x, int y)
        {

        }

        public override void OnMouseScroll(int scrollDelta)
        {

        }
    }
}
