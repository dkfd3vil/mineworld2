using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineWorld.StateMasher;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace MineWorld.States
{
    public class ServerBrowserState : State
    {
        private readonly ClickRegion[] _clkMenuServer = new[]
                                                           {
                                                               new ClickRegion(new Rectangle(0, 713, 425, 42), "direct")
                                                               ,
                                                               new ClickRegion(new Rectangle(456, 713, 262, 42),
                                                                               "settings"),
                                                               new ClickRegion(new Rectangle(763, 713, 243, 42),
                                                                               "refresh")
                                                           };

        private List<int> _descWidths;
        private string _directConnectIp = "";
        private bool _directConnectIpEnter;

        private Rectangle _drawRect;
        private string _nextState;
        private List<ServerInformation> _serverList = new List<ServerInformation>();
        private Texture2D _texMenu;
        private SpriteFont _uiFont;

        public override void OnEnter(string oldState)
        {
            Sm.IsMouseVisible = true;
            (Sm as MineWorldGame).ResetPropertyBag();
            P = Sm.PropertyBag;

            _texMenu = Sm.Content.Load<Texture2D>("menus/tex_menu_server");

            _drawRect = new Rectangle(Sm.GraphicsDevice.Viewport.Width/2 - 1024/2,
                                     Sm.GraphicsDevice.Viewport.Height/2 - 768/2,
                                     1024,
                                     1024);

            _uiFont = Sm.Content.Load<SpriteFont>("font_04b08");
            //keyMap = new KeyMap();

            _serverList = (Sm as MineWorldGame).EnumerateServers(0.5f);
        }

        public override void OnLeave(string newState)
        {
        }

        public override string OnUpdate(GameTime gameTime, KeyboardState keyState, MouseState mouseState)
        {
            return _nextState;
        }

        public override void OnRenderAtEnter(GraphicsDevice graphicsDevice)
        {
        }

        public override void OnRenderAtUpdate(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            //check for IP in command line and skip server browser
            if ((Sm as MineWorldGame).Pargument != null)
            {
                IPAddress ptoJoin = (Sm as MineWorldGame).Pargument;
                (Sm as MineWorldGame).Pargument = null;
                (Sm as MineWorldGame).PropertyBag.ServerName = ptoJoin.ToString();
                (Sm as MineWorldGame).JoinGame(new IPEndPoint(ptoJoin, 5565));

                _nextState = "MineWorld.States.LoadingState";
            }

            //begin server browser
            _descWidths = new List<int>();
            SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            spriteBatch.Draw(_texMenu, _drawRect, Color.White);

            int drawY = 80;
            foreach (ServerInformation server in _serverList)
            {
                if (drawY < 660)
                {
                    int textWidth = (int) (_uiFont.MeasureString(server.GetServerDesc()).X);
                    _descWidths.Add(textWidth + 30);
                    spriteBatch.DrawString(_uiFont, server.GetServerDesc(),
                                           new Vector2(Sm.GraphicsDevice.Viewport.Width/2 - textWidth/2,
                                                       _drawRect.Y + drawY),
                                           !server.LanServer && server.NumPlayers == server.MaxPlayers
                                               ? new Color(0.7f, 0.7f, 0.7f)
                                               : Color.White);
                    drawY += 25;
                }
            }

            spriteBatch.DrawString(_uiFont, Defines.MineworldclientVersion,
                                   new Vector2(10, Sm.GraphicsDevice.Viewport.Height - 20), Color.White);

            if (_directConnectIpEnter)
                spriteBatch.DrawString(_uiFont, "ENTER IP: " + _directConnectIp,
                                       new Vector2(_drawRect.X + 30, _drawRect.Y + 690), Color.White);

            spriteBatch.End();
        }

        public override void OnCharEntered(CharacterEventArgs e)
        {
            if (e.Character < 32 || e.Character > 126) //From space to tilde
                return; //Do nothing

            //Only respond if entering an ip and control is not pressed
            if (_directConnectIpEnter &&
                !(Keyboard.GetState().IsKeyDown(Keys.LeftControl) || Keyboard.GetState().IsKeyDown(Keys.RightControl)))
            {
                _directConnectIp += e.Character;
            }
        }

        public override void OnKeyDown(Keys key)
        {
            if (_directConnectIpEnter)
            {
                if (key == Keys.Escape)
                {
                    _directConnectIpEnter = false;
                    _directConnectIp = "";
                }

                if (key == Keys.Back && _directConnectIp.Length > 0)
                {
                    _directConnectIp = _directConnectIp.Substring(0, _directConnectIp.Length - 1);
                }

                if (key == Keys.Enter)
                {
                    // Try what was entered first as an IP, and then second as a host name.
                    _directConnectIpEnter = false;
                    P.PlaySound(MineWorldSound.ClickHigh);
                    IPAddress connectIp;
                    if (!IPAddress.TryParse(_directConnectIp, out connectIp))
                    {
                        connectIp = null;
                        try
                        {
                            IPAddress[] resolveResults = Dns.GetHostAddresses(_directConnectIp);
                            for (int i = 0; i < resolveResults.Length; i++)
                                if (resolveResults[i].AddressFamily == AddressFamily.InterNetwork)
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
                        (Sm as MineWorldGame).PropertyBag.ServerName = _directConnectIp;
                        (Sm as MineWorldGame).JoinGame(new IPEndPoint(connectIp, 5565));
                        _nextState = "MineWorld.States.LoadingState";
                    }
                    _directConnectIp = "";
                }

                if (key == Keys.V &&
                    (Keyboard.GetState().IsKeyDown(Keys.LeftControl) || Keyboard.GetState().IsKeyDown(Keys.RightControl)))
                {
                    try
                    {
                        _directConnectIp += Clipboard.GetText();
                    }
                    catch (Exception)
                    {
                    }
                }
                /*else if (keyMap.IsKeyMapped(key))
                {
                    directConnectIP += keyMap.TranslateKey(key, false);
                }*/
            }
            else
            {
                if (key == Keys.Escape)
                {
                    Sm.Exit();
                }
            }
        }

        public override void OnKeyUp(Keys key)
        {
        }

        public override void OnMouseDown(MouseButtons button, int x, int y)
        {
            if (_directConnectIpEnter == false)
            {
                int serverIndex = (y - _drawRect.Y - 75)/25;
                if (serverIndex >= 0 && serverIndex < _serverList.Count)
                {
                    int distanceFromCenter = Math.Abs(Sm.GraphicsDevice.Viewport.Width/2 - x);
                    if (distanceFromCenter < _descWidths[serverIndex]/2)
                    {
                        (Sm as MineWorldGame).PropertyBag.ServerName = _serverList[serverIndex].ServerName;
                        (Sm as MineWorldGame).JoinGame(_serverList[serverIndex].IpEndPoint);
                        _nextState = "MineWorld.States.LoadingState";
                        P.PlaySound(MineWorldSound.ClickHigh);
                    }
                }

                x -= _drawRect.X;
                y -= _drawRect.Y;
                switch (ClickRegion.HitTest(_clkMenuServer, new Point(x, y)))
                {
                    case "refresh":
                        P.PlaySound(MineWorldSound.ClickHigh);
                        _serverList = (Sm as MineWorldGame).EnumerateServers(0.5f);
                        break;

                    case "direct":
                        _directConnectIpEnter = true;
                        P.PlaySound(MineWorldSound.ClickHigh);
                        break;
                    case "settings":
                        _nextState = "MineWorld.States.SettingsState";
                        P.PlaySound(MineWorldSound.ClickHigh);
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