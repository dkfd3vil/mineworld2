using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace MineWorld
{
    public class KeyBindHandler
    {
        //Cases where there are multiple keys under the same name
        enum SpecialKeys
        {
            Control,
            Alt,
            Shift
        }

        Dictionary<Keys, KeyBoardButtons> keyBinds = new Dictionary<Keys, KeyBoardButtons>();
        Dictionary<MouseButtons, KeyBoardButtons> mouseBinds = new Dictionary<MouseButtons, KeyBoardButtons>();
        Dictionary<SpecialKeys, KeyBoardButtons> specialKeyBinds = new Dictionary<SpecialKeys, KeyBoardButtons>();

        public bool IsBound(KeyBoardButtons button, Keys theKey)
        {
            if (keyBinds.ContainsKey(theKey) && keyBinds[theKey] == button)
                return true;
            else if ((theKey == Keys.LeftAlt || theKey == Keys.RightAlt) && specialKeyBinds.ContainsKey(SpecialKeys.Alt) && specialKeyBinds[SpecialKeys.Alt] == button)
                return true;
            else if ((theKey == Keys.LeftShift || theKey == Keys.RightShift) && specialKeyBinds.ContainsKey(SpecialKeys.Shift) && specialKeyBinds[SpecialKeys.Shift] == button)
                return true;
            else if ((theKey == Keys.LeftControl || theKey == Keys.RightControl) && specialKeyBinds.ContainsKey(SpecialKeys.Control) && specialKeyBinds[SpecialKeys.Control] == button)
                return true;
            return false;
        }

        public bool IsPressed(KeyBoardButtons button)
        {
            KeyboardState state = Keyboard.GetState();
            foreach (Keys key in keyBinds.Keys)
            {
                if (keyBinds[key] == button)
                {
                    if (state.IsKeyDown(key))
                        return true;
                }
            }
            MouseState ms = Mouse.GetState();
            foreach (MouseButtons mb in mouseBinds.Keys)
            {
                if (mouseBinds[mb] == button)
                {
                    switch (mb)
                    {
                        case MouseButtons.LeftButton:
                            if (ms.LeftButton == ButtonState.Pressed)
                                return true;
                            break;
                        case MouseButtons.MiddleButton:
                            if (ms.MiddleButton == ButtonState.Pressed)
                                return true;
                            break;
                        case MouseButtons.RightButton:
                            if (ms.RightButton == ButtonState.Pressed)
                                return true;
                            break;
                    }
                }
            }
            foreach (SpecialKeys key in specialKeyBinds.Keys)
            {
                if (specialKeyBinds[key] == button)
                {
                    switch (key)
                    {
                        case SpecialKeys.Alt:
                            if (state.IsKeyDown(Keys.LeftAlt) || state.IsKeyDown(Keys.RightAlt))
                                return true;
                            break;
                        case SpecialKeys.Control:
                            if (state.IsKeyDown(Keys.LeftControl) || state.IsKeyDown(Keys.RightControl))
                                return true;
                            break;
                        case SpecialKeys.Shift:
                            if (state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift))
                                return true;
                            break;
                    }
                }
            }
            return false;
        }

        public bool IsBound(KeyBoardButtons button, MouseButtons mb)
        {
            if (mouseBinds.ContainsKey(mb) && mouseBinds[mb] == button)
                return true;
            return false;
        }

        public KeyBoardButtons GetBound(Keys theKey)
        {
            if (keyBinds.ContainsKey(theKey))
                return keyBinds[theKey];
            else if ((theKey == Keys.LeftAlt || theKey == Keys.RightAlt) && specialKeyBinds.ContainsKey(SpecialKeys.Alt))
                return specialKeyBinds[SpecialKeys.Alt];
            else if ((theKey == Keys.LeftShift || theKey == Keys.RightShift) && specialKeyBinds.ContainsKey(SpecialKeys.Shift))
                return specialKeyBinds[SpecialKeys.Shift];
            else if ((theKey == Keys.LeftControl || theKey == Keys.RightControl) && specialKeyBinds.ContainsKey(SpecialKeys.Control))
                return specialKeyBinds[SpecialKeys.Control];
            return KeyBoardButtons.None;
        }

        public KeyBoardButtons GetBound(MouseButtons theButton)
        {
            if (mouseBinds.ContainsKey(theButton))
                return mouseBinds[theButton];
            return KeyBoardButtons.None;
        }

        //If overwrite is true then the previous entry for that button will be removed
        public bool BindKey(KeyBoardButtons button, string key, bool overwrite)
        {
            try
            {
                //Key bind
                Keys actualKey = (Keys)Enum.Parse(typeof(Keys), key, true);
                if (Enum.IsDefined(typeof(Keys), actualKey))
                {
                    keyBinds.Add(actualKey, (KeyBoardButtons)button);
                    return true;
                }
            }
            catch { }
            try
            {
                //Mouse bind
                MouseButtons actualMB = (MouseButtons)Enum.Parse(typeof(MouseButtons), key, true);
                if (Enum.IsDefined(typeof(MouseButtons), actualMB))
                {
                    mouseBinds.Add(actualMB, (KeyBoardButtons)button);
                    return true;
                }
            }
            catch { }
            //Special cases
            if (key.Equals("control", StringComparison.OrdinalIgnoreCase) || key.Equals("ctrl", StringComparison.OrdinalIgnoreCase))
            {
                specialKeyBinds.Add(SpecialKeys.Control, (KeyBoardButtons)button);
                return true;
            }
            if (key.Equals("shift", StringComparison.OrdinalIgnoreCase))
            {
                specialKeyBinds.Add(SpecialKeys.Shift, (KeyBoardButtons)button);
                return true;
            }
            if (key.Equals("alt", StringComparison.OrdinalIgnoreCase))
            {
                specialKeyBinds.Add(SpecialKeys.Alt, (KeyBoardButtons)button);
                return true;
            }
            return false;
        }

        //Note that multiple binds to the same key won't work right now due to the way DatafileWriter handles input and how Dictionary works
        //Macro support is a future goal
        public void SaveBinds(Datafile output, string filename)
        {
            foreach (Keys key in keyBinds.Keys)
            {
                output.Data[key.ToString()] = keyBinds[key].ToString();
            }
            foreach (MouseButtons button in mouseBinds.Keys)
            {
                output.Data[button.ToString()] = mouseBinds[button].ToString();
            }
            foreach (SpecialKeys key in specialKeyBinds.Keys)
            {
                output.Data[key.ToString()] = specialKeyBinds[key].ToString();
            }
            output.WriteChanges(filename);
        }

        public void CreateDefaultSet()
        {
            mouseBinds.Add(MouseButtons.LeftButton, KeyBoardButtons.Fire);
            mouseBinds.Add(MouseButtons.RightButton, KeyBoardButtons.AltFire);

            keyBinds.Add(Keys.W, KeyBoardButtons.Forward);
            keyBinds.Add(Keys.S, KeyBoardButtons.Backward);
            keyBinds.Add(Keys.A, KeyBoardButtons.Left);
            keyBinds.Add(Keys.D, KeyBoardButtons.Right);
            specialKeyBinds.Add(SpecialKeys.Shift, KeyBoardButtons.Sprint);
            specialKeyBinds.Add(SpecialKeys.Control, KeyBoardButtons.Crouch);
            keyBinds.Add(Keys.Space, KeyBoardButtons.Jump);

            keyBinds.Add(Keys.Y, KeyBoardButtons.Say);
        }
    }
}
