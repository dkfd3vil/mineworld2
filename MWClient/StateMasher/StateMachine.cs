using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MineWorld.StateMasher
{
    /// <summary>
    ///   This is the main type for your game
    /// </summary>
    public class StateMachine : Game
    {
        private readonly FpsCounter _fpsCounter = new FpsCounter();
        public GraphicsDeviceManager GraphicsDeviceManager;

        private State _currentState;
        private string _currentStateType = "";

        private MouseState _msOld;
        private bool _needToRenderOnEnter;
        public PropertyBag PropertyBag;

        public StateMachine()
        {
            Content.RootDirectory = "Content";
            GraphicsDeviceManager = new GraphicsDeviceManager(this);
            EventInput.Initialize(Window);
            EventInput.CharEntered += EventInputCharEntered;
            EventInput.KeyDown += EventInputKeyDown;
            EventInput.KeyUp += EventInputKeyUp;
        }

        public string CurrentStateType
        {
            get { return _currentStateType; }
        }

        public double FrameRate
        {
            get { return _fpsCounter.Fps(); }
        }

        [DllImport("user32.dll")]
        public static extern int GetForegroundWindow();

        protected void ChangeState(string newState)
        {
            Debug.Assert(newState != "", "Newstate string at changestate was empty\n\r Report at dev");

            // Call OnLeave for the old state.
            if (_currentState != null)
                _currentState.OnLeave(newState);

            // Instantiate and set the new state.
            Assembly a = Assembly.GetExecutingAssembly();
            Type t = a.GetType(newState);
            _currentState = Activator.CreateInstance(t) as State;

            // Set up the new state.
            if (_currentState != null)
            {
                _currentState.P = PropertyBag;
                _currentState.Sm = this;
                _currentState.OnEnter(_currentStateType);
            }
            _currentStateType = newState;
            _needToRenderOnEnter = true;
        }

        public bool WindowHasFocus()
        {
            return GetForegroundWindow() == (int) Window.Handle;
        }

        //Keyboard input
        public void EventInputCharEntered(object sender, CharacterEventArgs e)
        {
            if (_currentState != null)
                _currentState.OnCharEntered(e);
        }

        public void EventInputKeyDown(object sender, KeyEventArgs e)
        {
            if (_currentState != null)
                _currentState.OnKeyDown(e.KeyCode);
        }

        public void EventInputKeyUp(object sender, KeyEventArgs e)
        {
            if (_currentState != null)
                _currentState.OnKeyUp(e.KeyCode);
        }

        protected override void LoadContent()
        {
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            //If we dont got a property bag or state then return silent
            if (_currentState == null && PropertyBag == null)
            {
                return;
            }

            // Call OnUpdate.
            if (_currentState != null)
            {
                string newState = _currentState.OnUpdate(gameTime, Keyboard.GetState(), Mouse.GetState());
                if (newState != null)
                    ChangeState(newState);
            }

            // Check for mouse events.
            MouseState msNew = Mouse.GetState();
            if (WindowHasFocus())
            {
                if (_msOld.LeftButton == ButtonState.Released && msNew.LeftButton == ButtonState.Pressed)
                    _currentState.OnMouseDown(MouseButtons.LeftButton, msNew.X, msNew.Y);
                if (_msOld.MiddleButton == ButtonState.Released && msNew.MiddleButton == ButtonState.Pressed)
                    _currentState.OnMouseDown(MouseButtons.MiddleButton, msNew.X, msNew.Y);
                if (_msOld.RightButton == ButtonState.Released && msNew.RightButton == ButtonState.Pressed)
                    _currentState.OnMouseDown(MouseButtons.RightButton, msNew.X, msNew.Y);
                if (_msOld.LeftButton == ButtonState.Pressed && msNew.LeftButton == ButtonState.Released)
                    _currentState.OnMouseUp(MouseButtons.LeftButton, msNew.X, msNew.Y);
                if (_msOld.MiddleButton == ButtonState.Pressed && msNew.MiddleButton == ButtonState.Released)
                    _currentState.OnMouseUp(MouseButtons.MiddleButton, msNew.X, msNew.Y);
                if (_msOld.RightButton == ButtonState.Pressed && msNew.RightButton == ButtonState.Released)
                    _currentState.OnMouseUp(MouseButtons.RightButton, msNew.X, msNew.Y);
                if (_msOld.ScrollWheelValue != msNew.ScrollWheelValue)
                    _currentState.OnMouseScroll(msNew.ScrollWheelValue - _msOld.ScrollWheelValue);
            }
            _msOld = msNew;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            //If we dont got a property bag or state then return silent
            if (_currentState == null && PropertyBag == null)
            {
                return;
            }

            // TODO Find a better place for fps counter
            // I tottaly dont like the way how FPS is implented
            StateMachine sm = _currentState.Sm;
            if ((sm as MineWorldGame).Csettings.DrawFrameRate)
            {
                _fpsCounter.Frame(gameTime.TotalRealTime.Milliseconds + gameTime.TotalRealTime.Seconds*1000 +
                                 gameTime.TotalRealTime.Minutes*60000 + gameTime.TotalRealTime.Hours*3600000);
            }

            // Call OnRenderAtUpdate.
            _currentState.OnRenderAtUpdate(GraphicsDevice, gameTime);

            // If we have one queued, call OnRenderAtEnter.
            if (_needToRenderOnEnter)
            {
                _needToRenderOnEnter = false;
                _currentState.OnRenderAtEnter(GraphicsDevice);
            }

            base.Draw(gameTime);
        }
    }
}