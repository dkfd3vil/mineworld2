using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Net;
using StateMasher;
using InterfaceItems;
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
    public class LoginState : State
    {
        Texture2D texSettings;
        List<InterfaceElement> elements = new List<InterfaceElement>();
        Rectangle drawRect;
        int baseHeight = 14;
        //int sliderFullHeight = 52;
        int sliderFullWidth = 200;
        /*int buttonFullHeight = 36;
        int textFullHeight = 36;*/
        Vector2 currentPos = new Vector2(0, 0);
        int originalY = 0;

        ClickRegion[] clkMenuSettings = new ClickRegion[2] {
            new ClickRegion(new Rectangle(41,638,191 - 41,698 - 638),"cancel"),
            new ClickRegion(new Rectangle(868,642,1010 - 868,702 - 642),"login")
        };

        protected string nextState = null;

        public void addSpace(int amount)
        {
            currentPos.Y += amount;
        }

        public void shiftColumn()
        {
            shiftColumn(350);
        }

        public void shiftColumn(int amount)
        {
            currentPos.X += amount;
            currentPos.Y = originalY;
        }

        public void addSliderAutomatic(string text, float minVal, float maxVal, float initVal, bool integerOnly)
        {
            int height = 38; //basic
            if (text != "")
                height += 20;
            currentPos.Y += height;
            addSlider(new Rectangle((int)currentPos.X, (int)currentPos.Y, sliderFullWidth, baseHeight), true, true, text, minVal, maxVal, initVal, integerOnly);
        }

        public void addSlider(Rectangle size, bool enabled, bool visible, string text, float minVal, float maxVal, float initVal, bool integerOnly)
        {
            InterfaceSlider temp = new InterfaceSlider((_SM as MineWorldGame), _P);
            temp.size = size;
            temp.enabled = enabled;
            temp.visible = visible;
            temp.text = text;
            temp.minVal = minVal;
            temp.maxVal = maxVal;
            temp.setValue(initVal);
            temp.integers = integerOnly;
            elements.Add(temp);
        }

        public void addButtonAutomatic(string text, string onText, string offText, bool clicked)
        {
            int height = 22; //basic
            if (text != "")
                height += 20;
            currentPos.Y += height;
            addButton(new Rectangle((int)currentPos.X, (int)currentPos.Y, sliderFullWidth, baseHeight), true, true, text, onText, offText, clicked);
        }

        public void addButton(Rectangle size, bool enabled, bool visible, string text, string onText, string offText, bool clicked)
        {
            InterfaceButtonToggle temp = new InterfaceButtonToggle((_SM as MineWorldGame), _P);
            temp.size = size;
            temp.enabled = enabled;
            temp.visible = visible;
            temp.text = text;
            temp.onText = onText;
            temp.offText = offText;
            temp.clicked = clicked;
            elements.Add(temp);
        }

        public void addTextInputAutomatic(string text, string initVal)
        {
            int height = 22; //basic
            if (text != "")
                height += 20;
            currentPos.Y += height;
            addTextInput(new Rectangle((int)currentPos.X, (int)currentPos.Y, sliderFullWidth, baseHeight), true, true, text, initVal);
        }

        public void addTextInput(Rectangle size, bool enabled, bool visible, string text, string initVal)
        {
            InterfaceTextInput temp = new InterfaceTextInput((_SM as MineWorldGame), _P);
            temp.size = size;
            temp.enabled = enabled;
            temp.visible = visible;
            temp.text = text;
            temp.value = initVal;
            elements.Add(temp);
        }

        public void addLabelAutomatic(string text)
        {
            currentPos.Y += 20;
            addLabel(new Rectangle((int)currentPos.X - 100, (int)currentPos.Y, 0, 0), true, text);
        }

        public void addLabel(Rectangle size, bool visible, string text)
        {
            InterfaceLabel temp = new InterfaceLabel((_SM as MineWorldGame), _P);
            temp.size = size;
            temp.visible = visible;
            temp.text = text;
            elements.Add(temp);
        }

        public override void OnEnter(string oldState)
        {
            _SM.IsMouseVisible = true;

            //Load the background
            //TODO Create own backgrounds for loginstate
            texSettings = _SM.Content.Load<Texture2D>("menus/tex_menu_login");
            drawRect = new Rectangle(_SM.GraphicsDevice.Viewport.Width / 2 - 1024 / 2,
                                                 _SM.GraphicsDevice.Viewport.Height / 2 - 768 / 2,
                                                 1024,
                                                 1024);

            addTextInput(new Rectangle(359, 322, 678 - 359, 370 - 322), true, true, "username", (_SM as MineWorldGame).Csettings.playerHandle);
            addTextInput(new Rectangle(363, 449, 676 - 363,504 - 449), true, true, "password", "");
        }

        public override void OnLeave(string newState)
        {
            base.OnLeave(newState);
        }

        public override void OnMouseDown(MouseButtons button, int x, int y)
        {
            base.OnMouseDown(button, x, y);
            string username = "";
            string password = "";
            foreach (InterfaceElement element in elements)
            {
                element.OnMouseDown(button, x, y);
            }
            switch (ClickRegion.HitTest(clkMenuSettings, new Point(x, y)))
            {
                case "cancel":
                    nextState = "MineWorld.States.TitleState";
                    break;
                case "login":
                    {
                        foreach (InterfaceElement element in elements)
                        {
                            switch (element.text)
                            {
                                case "username":
                                    {
                                        username = (element as InterfaceTextInput).value;
                                        //globalvalues.Add("username", username);
                                        break;
                                    }
                                case "password":
                                    {
                                        password = (element as InterfaceTextInput).value;
                                        break;
                                    }
                            }
                        }
                    Dictionary<String, String> LoginInfo = new Dictionary<string, string>();
                    LoginInfo.Add("u", username);
                    LoginInfo.Add("p", password);
                    bool correct =HttpRequest.Get(Defines.MASTERSERVER_BASE_URL + "login.php",LoginInfo) == "OK";
                    if (correct)
                    {
                        nextState = "MineWorld.States.ServerBrowserState";
                    }
                    else
                    {
                        ErrorManager.ErrorMsg = "Wrong username or/and password";
                        ErrorManager.NewState = "MineWorld.States.LoginState";
                        nextState = "MineWorld.States.ErrorState";
                    }
                    break;
                    }
            }
        }

        public override void OnMouseUp(MouseButtons button, int x, int y)
        {
            base.OnMouseUp(button, x, y);
            foreach (InterfaceElement element in elements)
            {
                element.OnMouseUp(button, x, y);
            }
        }

        public override void OnCharEntered(EventInput.CharacterEventArgs e)
        {
            base.OnCharEntered(e);
            foreach (InterfaceElement element in elements)
            {
                element.OnCharEntered(e);
            }
        }

        public override void OnKeyDown(Keys key)
        {
            base.OnKeyDown(key);
            if (key == Keys.Escape)
                nextState = "MineWorld.States.TitleState";
            else
            {
                foreach (InterfaceElement element in elements)
                {
                    element.OnKeyDown(key);
                }
            }
        }

        public override void OnKeyUp(Keys key)
        {
            base.OnKeyUp(key);
            foreach (InterfaceElement element in elements)
            {
                element.OnKeyUp(key);
            }
        }

        public override string OnUpdate(GameTime gameTime, KeyboardState keyState, MouseState mouseState)
        {
            return nextState;
        }

        public override void OnRenderAtUpdate(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            spriteBatch.Draw(texSettings, drawRect, Color.White);
            spriteBatch.End();
            foreach (InterfaceElement element in elements)
            {
                element.Render(graphicsDevice);
            }
        }
    }
}
