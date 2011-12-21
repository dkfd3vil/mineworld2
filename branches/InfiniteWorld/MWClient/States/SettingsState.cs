using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineWorld.InterfaceItems;
using MineWorld.StateMasher;

namespace MineWorld.States
{
    public class SettingsState : State
    {
        private const int BaseHeight = 14;
        private const int SliderFullWidth = 200;

        private readonly ClickRegion[] _clkMenuSettings = new[]
                                                              {
                                                                  new ClickRegion(new Rectangle(0, 713, 255, 42),
                                                                                  "cancel"),
                                                                  new ClickRegion(new Rectangle(524, 713, 500, 42),
                                                                                  "accept")
                                                              };

        private readonly List<InterfaceElement> _elements = new List<InterfaceElement>();

        protected string NextState;
        private Vector2 _currentPos = new Vector2(0, 0);
        private Rectangle _drawRect;
        private int _originalY;
        private Texture2D _texSettings;

        public void AddSpace(int amount)
        {
            _currentPos.Y += amount;
        }

        public void ShiftColumn()
        {
            ShiftColumn(350);
        }

        public void ShiftColumn(int amount)
        {
            _currentPos.X += amount;
            _currentPos.Y = _originalY;
        }

        public void AddSliderAutomatic(string text, float minVal, float maxVal, float initVal, bool integerOnly)
        {
            int height = 38; //basic
            if (text != "")
                height += 20;
            _currentPos.Y += height;
            AddSlider(new Rectangle((int) _currentPos.X, (int) _currentPos.Y, SliderFullWidth, BaseHeight), true, true,
                      text, minVal, maxVal, initVal, integerOnly);
        }

        public void AddSlider(Rectangle size, bool enabled, bool visible, string text, float minVal, float maxVal,
                              float initVal, bool integerOnly)
        {
            InterfaceSlider temp = new InterfaceSlider((Sm as MineWorldGame), P);
            temp.Size = size;
            temp.Enabled = enabled;
            temp.Visible = visible;
            temp.Text = text;
            temp.MinVal = minVal;
            temp.MaxVal = maxVal;
            temp.SetValue(initVal);
            temp.Integers = integerOnly;
            _elements.Add(temp);
        }

        public void AddButtonAutomatic(string text, string onText, string offText, bool clicked)
        {
            int height = 22; //basic
            if (text != "")
                height += 20;
            _currentPos.Y += height;
            AddButton(new Rectangle((int) _currentPos.X, (int) _currentPos.Y, SliderFullWidth, BaseHeight), true, true,
                      text, onText, offText, clicked);
        }

        public void AddButton(Rectangle size, bool enabled, bool visible, string text, string onText, string offText,
                              bool clicked)
        {
            InterfaceButtonToggle temp = new InterfaceButtonToggle((Sm as MineWorldGame), P);
            temp.Size = size;
            temp.Enabled = enabled;
            temp.Visible = visible;
            temp.Text = text;
            temp.OnText = onText;
            temp.OffText = offText;
            temp.Clicked = clicked;
            _elements.Add(temp);
        }

        public void AddTextInputAutomatic(string text, string initVal)
        {
            int height = 22; //basic
            if (text != "")
                height += 20;
            _currentPos.Y += height;
            AddTextInput(new Rectangle((int) _currentPos.X, (int) _currentPos.Y, SliderFullWidth, BaseHeight), true,
                         true, text, initVal);
        }

        public void AddTextInput(Rectangle size, bool enabled, bool visible, string text, string initVal)
        {
            InterfaceTextInput temp = new InterfaceTextInput((Sm as MineWorldGame), P);
            temp.Size = size;
            temp.Enabled = enabled;
            temp.Visible = visible;
            temp.Text = text;
            temp.Value = initVal;
            _elements.Add(temp);
        }

        public void AddLabelAutomatic(string text)
        {
            _currentPos.Y += 20;
            AddLabel(new Rectangle((int) _currentPos.X - 100, (int) _currentPos.Y, 0, 0), true, text);
        }

        public void AddLabel(Rectangle size, bool visible, string text)
        {
            InterfaceLabel temp = new InterfaceLabel((Sm as MineWorldGame), P);
            temp.Size = size;
            temp.Visible = visible;
            temp.Text = text;
            _elements.Add(temp);
        }

        public override void OnEnter(string oldState)
        {
            Sm.IsMouseVisible = true;

            //Load the background
            _texSettings = Sm.Content.Load<Texture2D>("menus/tex_menu_settings");
            _drawRect = new Rectangle(Sm.GraphicsDevice.Viewport.Width/2 - 1024/2,
                                      Sm.GraphicsDevice.Viewport.Height/2 - 768/2,
                                      1024,
                                      1024);

            //Read the data from file
            Datafile dw = new Datafile((Sm as MineWorldGame).Csettings.Directory + "/client.config.txt");

            _currentPos = new Vector2(200, 100);
            _originalY = (int) _currentPos.Y;

            AddLabelAutomatic("User Settings");
            AddTextInputAutomatic("Username", dw.Data.ContainsKey("handle") ? dw.Data["handle"] : "Player");
            AddSpace(16);

            AddLabelAutomatic("Screen Settings");
            AddTextInputAutomatic("Scrn  Width", dw.Data.ContainsKey("width") ? dw.Data["width"] : "1024");
            AddTextInputAutomatic("Scrn Height", dw.Data.ContainsKey("height") ? dw.Data["height"] : "780");
            AddButtonAutomatic("Screen Mode", "Fullscreen", "Windowed",
                               dw.Data.ContainsKey("fullscreen") ? bool.Parse(dw.Data["fullscreen"]) : false);
            AddSpace(16);

            AddLabelAutomatic("Sound Settings");
            AddSliderAutomatic("Volume", 1f, 100f,
                               dw.Data.ContainsKey("volume") ? float.Parse(dw.Data["volume"])*100 : 100f, true);
            AddButtonAutomatic("Enable Sound", "On", "NoSound",
                               dw.Data.ContainsKey("nosound") ? !bool.Parse(dw.Data["nosound"]) : true);
            AddSpace(16);

            ShiftColumn();

            AddLabelAutomatic("Mouse Settings");
            AddButtonAutomatic("Invert Mouse", "Yes", "No",
                               dw.Data.ContainsKey("yinvert") ? bool.Parse(dw.Data["yinvert"]) : false);
            AddSliderAutomatic("Mouse Sensitivity", 1f, 10f,
                               dw.Data.ContainsKey("sensitivity") ? float.Parse(dw.Data["sensitivity"]) : 5f, true);
            AddSpace(16);

            AddLabelAutomatic("Misc Settings");
            AddButtonAutomatic("Bloom", "Pretty", "Boring",
                               dw.Data.ContainsKey("pretty") ? bool.Parse(dw.Data["pretty"]) : true);
            AddButtonAutomatic("Show FPS", "Yes", "No",
                               dw.Data.ContainsKey("showfps") ? bool.Parse(dw.Data["showfps"]) : true);
            AddSpace(16);
        }

        public override void OnMouseDown(MouseButtons button, int x, int y)
        {
            base.OnMouseDown(button, x, y);
            foreach (InterfaceElement element in _elements)
            {
                element.OnMouseDown(button, x, y);
            }
            switch (ClickRegion.HitTest(_clkMenuSettings, new Point(x, y)))
            {
                case "cancel":
                    NextState = "MineWorld.States.ServerBrowserState";
                    break;
                case "accept":
                    if (SaveSettings())
                        NextState = "MineWorld.States.ServerBrowserState";
                    else
                    {
                        ErrorManager.ErrorMsg = "Error: Problem while saving";
                        ErrorManager.NewState = "MineWorld.States.ServerBrowserState";
                        NextState = "MineWorld.States.ErrorState";
                    }
                    break;
            }
        }

        public override void OnMouseUp(MouseButtons button, int x, int y)
        {
            base.OnMouseUp(button, x, y);
            foreach (InterfaceElement element in _elements)
            {
                element.OnMouseUp(button, x, y);
            }
        }

        public bool SaveSettings()
        {
            Datafile dw = new Datafile((Sm as MineWorldGame).Csettings.Directory + "/client.config.txt");
            foreach (InterfaceElement element in _elements)
            {
                switch (element.Text)
                {
                    case "Username":
                        dw.Data["handle"] = (element as InterfaceTextInput).Value;
                        (Sm as MineWorldGame).Csettings.PlayerHandle = (element as InterfaceTextInput).Value;
                        break;
                    case "Scrn  Width":
                        dw.Data["width"] = (element as InterfaceTextInput).Value;
                        (Sm as MineWorldGame).Csettings.Width = int.Parse((element as InterfaceTextInput).Value);
                        break;
                    case "Scrn Height":
                        dw.Data["height"] = (element as InterfaceTextInput).Value;
                        (Sm as MineWorldGame).Csettings.Height = int.Parse((element as InterfaceTextInput).Value);
                        break;
                    case "Screen Mode":
                        dw.Data["fullscreen"] = (element as InterfaceButtonToggle).Clicked.ToString().ToLower();
                        (Sm as MineWorldGame).Csettings.Fullscreen =
                            bool.Parse((element as InterfaceButtonToggle).Clicked.ToString().ToLower());
                        break;
                    case "Volume":
                        dw.Data["volume"] = ((element as InterfaceSlider).Value/100).ToString();
                        (Sm as MineWorldGame).Csettings.VolumeLevel =
                            float.Parse(((element as InterfaceSlider).Value/100).ToString());
                        break;
                    case "Enable Sound":
                        dw.Data["nosound"] = (!(element as InterfaceButtonToggle).Clicked).ToString().ToLower();
                        (Sm as MineWorldGame).Csettings.NoSound =
                            bool.Parse((!(element as InterfaceButtonToggle).Clicked).ToString().ToLower());
                        break;
                    case "Invert Mouse":
                        dw.Data["yinvert"] = (element as InterfaceButtonToggle).Clicked.ToString().ToLower();
                        (Sm as MineWorldGame).Csettings.InvertMouseYAxis =
                            bool.Parse((element as InterfaceButtonToggle).Clicked.ToString().ToLower());
                        break;
                    case "Mouse Sensitivity":
                        dw.Data["sensitivity"] = (element as InterfaceSlider).Value.ToString();
                        (Sm as MineWorldGame).Csettings.MouseSensitivity = Math.Max(0.001f,
                                                                                     Math.Min(0.05f,
                                                                                              float.Parse(
                                                                                                  (element as
                                                                                                   InterfaceSlider).
                                                                                                      Value.ToString())/
                                                                                              1000f));
                        break;
                    case "Bloom":
                        dw.Data["pretty"] = (element as InterfaceButtonToggle).Clicked.ToString().ToLower();
                        (Sm as MineWorldGame).Csettings.RenderPretty =
                            bool.Parse((element as InterfaceButtonToggle).Clicked.ToString().ToLower());
                        break;
                    case "Show FPS":
                        dw.Data["showfps"] = (element as InterfaceButtonToggle).Clicked.ToString().ToLower();
                        (Sm as MineWorldGame).Csettings.DrawFrameRate =
                            bool.Parse((element as InterfaceButtonToggle).Clicked.ToString().ToLower());
                        break;
                }
            }
            (Sm as MineWorldGame).GraphicsDeviceManager.IsFullScreen = (Sm as MineWorldGame).Csettings.Fullscreen;
            (Sm as MineWorldGame).GraphicsDeviceManager.PreferredBackBufferWidth =
                (Sm as MineWorldGame).Csettings.Width;
            (Sm as MineWorldGame).GraphicsDeviceManager.PreferredBackBufferHeight =
                (Sm as MineWorldGame).Csettings.Height;
            (Sm as MineWorldGame).GraphicsDeviceManager.ApplyChanges();
            if (dw.WriteChanges((Sm as MineWorldGame).Csettings.Directory + "/client.config.txt") >= 1)
            {
                return true;
            }
            return false;
        }

        public override void OnCharEntered(CharacterEventArgs e)
        {
            base.OnCharEntered(e);
            foreach (InterfaceElement element in _elements)
            {
                element.OnCharEntered(e);
            }
        }

        public override void OnKeyDown(Keys key)
        {
            base.OnKeyDown(key);
            if (key == Keys.Escape)
                NextState = "MineWorld.States.ServerBrowserState";
            else
            {
                foreach (InterfaceElement element in _elements)
                {
                    element.OnKeyDown(key);
                }
            }
        }

        public override void OnKeyUp(Keys key)
        {
            base.OnKeyUp(key);
            foreach (InterfaceElement element in _elements)
            {
                element.OnKeyUp(key);
            }
        }

        public override string OnUpdate(GameTime gameTime, KeyboardState keyState, MouseState mouseState)
        {
            return NextState;
        }

        public override void OnRenderAtUpdate(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            spriteBatch.Draw(_texSettings, _drawRect, Color.White);
            spriteBatch.End();
            foreach (InterfaceElement element in _elements)
            {
                element.Render(graphicsDevice);
            }
        }
    }
}