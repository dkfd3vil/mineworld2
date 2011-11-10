using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace MineWorld
{
    public class KeyBindHandler
    {
        //Cases where there are multiple keys under the same name

        private readonly Dictionary<Keys, CustomKeyBoardButtons> _keyBinds =
            new Dictionary<Keys, CustomKeyBoardButtons>();

        private readonly Dictionary<MouseButtons, CustomMouseButtons> _mouseBinds =
            new Dictionary<MouseButtons, CustomMouseButtons>();

        private readonly Dictionary<SpecialKeys, CustomKeyBoardButtons> _specialKeyBinds =
            new Dictionary<SpecialKeys, CustomKeyBoardButtons>();

        public bool IsBound(CustomKeyBoardButtons button, Keys theKey)
        {
            if (_keyBinds.ContainsKey(theKey) && _keyBinds[theKey] == button)
                return true;
            else if ((theKey == Keys.LeftAlt || theKey == Keys.RightAlt) && _specialKeyBinds.ContainsKey(SpecialKeys.Alt) &&
                     _specialKeyBinds[SpecialKeys.Alt] == button)
                return true;
            else if ((theKey == Keys.LeftShift || theKey == Keys.RightShift) &&
                     _specialKeyBinds.ContainsKey(SpecialKeys.Shift) && _specialKeyBinds[SpecialKeys.Shift] == button)
                return true;
            else if ((theKey == Keys.LeftControl || theKey == Keys.RightControl) &&
                     _specialKeyBinds.ContainsKey(SpecialKeys.Control) &&
                     _specialKeyBinds[SpecialKeys.Control] == button)
                return true;
            return false;
        }

        public bool IsPressedKeyBoardButton(CustomKeyBoardButtons button)
        {
            KeyboardState state = Keyboard.GetState();
            foreach (Keys key in _keyBinds.Keys)
            {
                if (_keyBinds[key] == button)
                {
                    if (state.IsKeyDown(key))
                        return true;
                }
            }
            foreach (SpecialKeys key in _specialKeyBinds.Keys)
            {
                if (_specialKeyBinds[key] == button)
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

        public bool IsPressedMouseButton(CustomMouseButtons button)
        {
            MouseState ms = Mouse.GetState();
            foreach (MouseButtons mb in _mouseBinds.Keys)
            {
                if (_mouseBinds[mb] == button)
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
            return false;
        }

        public bool IsBoundMouse(MouseButtons mb)
        {
            if (_mouseBinds.ContainsKey(mb))
                return true;
            return false;
        }

        public bool IsBoundKeyboard(Keys button)
        {
            if (_keyBinds.ContainsKey(button))
                return true;
            return false;
        }

        public CustomKeyBoardButtons GetBoundKeyBoard(Keys theKey)
        {
            if (_keyBinds.ContainsKey(theKey))
                return _keyBinds[theKey];
            else if ((theKey == Keys.LeftAlt || theKey == Keys.RightAlt) && _specialKeyBinds.ContainsKey(SpecialKeys.Alt))
                return _specialKeyBinds[SpecialKeys.Alt];
            else if ((theKey == Keys.LeftShift || theKey == Keys.RightShift) &&
                     _specialKeyBinds.ContainsKey(SpecialKeys.Shift))
                return _specialKeyBinds[SpecialKeys.Shift];
            else if ((theKey == Keys.LeftControl || theKey == Keys.RightControl) &&
                     _specialKeyBinds.ContainsKey(SpecialKeys.Control))
                return _specialKeyBinds[SpecialKeys.Control];
            return CustomKeyBoardButtons.None;
        }

        public CustomMouseButtons GetBoundMouse(MouseButtons theButton)
        {
            if (_mouseBinds.ContainsKey(theButton))
                return _mouseBinds[theButton];
            return CustomMouseButtons.None;
        }

        //If overwrite is true then the previous entry for that button will be removed
        public bool BindKey(CustomKeyBoardButtons button, string key, bool overwrite)
        {
            try
            {
                //Key bind
                Keys actualKey = (Keys) Enum.Parse(typeof (Keys), key, true);
                if (Enum.IsDefined(typeof (Keys), actualKey))
                {
                    _keyBinds.Add(actualKey, button);
                    return true;
                }
            }
            catch
            {
            }
            try
            {
                //Mouse bind
                MouseButtons actualMb = (MouseButtons) Enum.Parse(typeof (MouseButtons), key, true);
                if (Enum.IsDefined(typeof (MouseButtons), actualMb))
                {
                    _mouseBinds.Add(actualMb, (CustomMouseButtons) button);
                    return true;
                }
            }
            catch
            {
            }
            //Special cases
            if (key.Equals("control", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("ctrl", StringComparison.OrdinalIgnoreCase))
            {
                _specialKeyBinds.Add(SpecialKeys.Control, button);
                return true;
            }
            if (key.Equals("shift", StringComparison.OrdinalIgnoreCase))
            {
                _specialKeyBinds.Add(SpecialKeys.Shift, button);
                return true;
            }
            if (key.Equals("alt", StringComparison.OrdinalIgnoreCase))
            {
                _specialKeyBinds.Add(SpecialKeys.Alt, button);
                return true;
            }
            return false;
        }

        //Note that multiple binds to the same key won't work right now due to the way DatafileWriter handles input and how Dictionary works
        //Macro support is a future goal
        public void SaveBinds(Datafile output, string filename)
        {
            foreach (Keys key in _keyBinds.Keys)
            {
                output.Data[key.ToString()] = _keyBinds[key].ToString();
            }
            foreach (MouseButtons button in _mouseBinds.Keys)
            {
                output.Data[button.ToString()] = _mouseBinds[button].ToString();
            }
            foreach (SpecialKeys key in _specialKeyBinds.Keys)
            {
                output.Data[key.ToString()] = _specialKeyBinds[key].ToString();
            }
            output.WriteChanges(filename);
        }

        public void CreateDefaultSet()
        {
            _mouseBinds.Add(MouseButtons.LeftButton, CustomMouseButtons.Fire);
            _mouseBinds.Add(MouseButtons.RightButton, CustomMouseButtons.AltFire);

            _keyBinds.Add(Keys.W, CustomKeyBoardButtons.Forward);
            _keyBinds.Add(Keys.S, CustomKeyBoardButtons.Backward);
            _keyBinds.Add(Keys.A, CustomKeyBoardButtons.Left);
            _keyBinds.Add(Keys.D, CustomKeyBoardButtons.Right);
            _specialKeyBinds.Add(SpecialKeys.Shift, CustomKeyBoardButtons.Sprint);
            _keyBinds.Add(Keys.Space, CustomKeyBoardButtons.Jump);

            _keyBinds.Add(Keys.Y, CustomKeyBoardButtons.Say);
        }

        private enum SpecialKeys
        {
            Control,
            Alt,
            Shift
        }
    }
}