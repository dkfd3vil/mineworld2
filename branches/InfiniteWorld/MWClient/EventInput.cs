using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MineWorld
{
    public class CharacterEventArgs : EventArgs
    {
        private readonly char _character;
        private readonly int _lParam;

        public CharacterEventArgs(char character, int lParam)
        {
            _character = character;
            _lParam = lParam;
        }

        public char Character
        {
            get { return _character; }
        }

        public int Param
        {
            get { return _lParam; }
        }

        public int RepeatCount
        {
            get { return _lParam & 0xffff; }
        }

        public bool ExtendedKey
        {
            get { return (_lParam & (1 << 24)) > 0; }
        }

        public bool AltPressed
        {
            get { return (_lParam & (1 << 29)) > 0; }
        }

        public bool PreviousState
        {
            get { return (_lParam & (1 << 30)) > 0; }
        }

        public bool TransitionState
        {
            get { return (_lParam & (1 << 31)) > 0; }
        }
    }

    public class KeyEventArgs : EventArgs
    {
        private readonly Keys _keyCode;

        public KeyEventArgs(Keys keyCode)
        {
            _keyCode = keyCode;
        }

        public Keys KeyCode
        {
            get { return _keyCode; }
        }
    }

    public delegate void CharEnteredHandler(object sender, CharacterEventArgs e);

    public delegate void KeyEventHandler(object sender, KeyEventArgs e);

    public static class EventInput
    {
        //various Win32 constants that we need
        private const int GwlWndproc = -4;
        private const int WmKeydown = 0x100;
        private const int WmKeyup = 0x101;
        private const int WmChar = 0x102;
        private const int WmImeSetcontext = 0x0281;
        private const int WmInputlangchange = 0x51;
        private const int WmGetdlgcode = 0x87;
        private const int DlgcWantallkeys = 4;
        private static bool _initialized;
        private static IntPtr _prevWndProc;
        private static WndProc _hookProcDelegate;
        private static IntPtr _hImc;

        /// <summary>
        ///   Event raised when a character has been entered.
        /// </summary>
        public static event CharEnteredHandler CharEntered;

        /// <summary>
        ///   Event raised when a key has been pressed down. May fire multiple times due to keyboard repeat.
        /// </summary>
        public static event KeyEventHandler KeyDown;

        /// <summary>
        ///   Event raised when a key has been released.
        /// </summary>
        public static event KeyEventHandler KeyUp;

        //Win32 functions that we're using
        [DllImport("Imm32.dll")]
        private static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("Imm32.dll")]
        private static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hImc);

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, int msg, IntPtr wParam,
                                                    IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        ///   Initialize the TextInput with the given GameWindow.
        /// </summary>
        /// <param name = "window">The XNA window to which text input should be linked.</param>
        public static void Initialize(GameWindow window)
        {
            if (_initialized)
                throw new InvalidOperationException("TextInput.Initialize can only be called once!");

            _hookProcDelegate = HookProc;
            _prevWndProc = (IntPtr) SetWindowLong(window.Handle, GwlWndproc,
                                                 (int) Marshal.GetFunctionPointerForDelegate(_hookProcDelegate));

            _hImc = ImmGetContext(window.Handle);
            _initialized = true;
        }

        private static IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            IntPtr returnCode = CallWindowProc(_prevWndProc, hWnd, msg, wParam, lParam);

            switch (msg)
            {
                case WmGetdlgcode:
                    returnCode = (IntPtr) (returnCode.ToInt32() | DlgcWantallkeys);
                    break;

                case WmKeydown:
                    if (KeyDown != null)
                        KeyDown(null, new KeyEventArgs((Keys) wParam));
                    break;

                case WmKeyup:
                    if (KeyUp != null)
                        KeyUp(null, new KeyEventArgs((Keys) wParam));
                    break;

                case WmChar:
                    if (CharEntered != null)
                        CharEntered(null, new CharacterEventArgs((char) wParam, lParam.ToInt32()));
                    break;

                case WmImeSetcontext:
                    if (wParam.ToInt32() == 1)
                        ImmAssociateContext(hWnd, _hImc);
                    break;

                case WmInputlangchange:
                    ImmAssociateContext(hWnd, _hImc);
                    returnCode = (IntPtr) 1;
                    break;
            }

            return returnCode;
        }

        private delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}