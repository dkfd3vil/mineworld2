using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineWorld.StateMasher;

namespace MineWorld.States
{
    public class ErrorState : State
    {
        private Rectangle _drawRect;
        private string _nextState;
        private Texture2D _texMenu;
        private SpriteFont _uiFont;

        public override void OnEnter(string oldState)
        {
            Sm.IsMouseVisible = false;

            //TODO Replace this errorstate placeholder
            _texMenu = Sm.Content.Load<Texture2D>("menus/tex_menu_error");

            _drawRect = new Rectangle(Sm.GraphicsDevice.Viewport.Width/2 - 1024/2,
                                     Sm.GraphicsDevice.Viewport.Height/2 - 768/2,
                                     1024,
                                     1024);

            _uiFont = Sm.Content.Load<SpriteFont>("font_04b08");
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
            SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            spriteBatch.Draw(_texMenu, _drawRect, Color.White);
            spriteBatch.DrawString(_uiFont, ErrorManager.ErrorMsg,
                                   new Vector2(
                                       ((int)
                                        (Sm.GraphicsDevice.Viewport.Width/2 -
                                         _uiFont.MeasureString(ErrorManager.ErrorMsg).X/2)), _drawRect.Y + 430),
                                   Color.White);
            spriteBatch.End();
        }

        public override void OnKeyDown(Keys key)
        {
            if (key == Keys.Escape)
            {
                _nextState = ErrorManager.NewState;
            }
        }

        public override void OnKeyUp(Keys key)
        {
        }

        public override void OnMouseDown(MouseButtons button, int x, int y)
        {
            if (button == MouseButtons.LeftButton)
            {
                _nextState = ErrorManager.NewState;
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