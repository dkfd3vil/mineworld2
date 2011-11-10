using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MineWorld.Engines
{
    public class PlayerEngine
    {
        private readonly MineWorldGame _gameInstance;
        private PropertyBag _p;

        public PlayerEngine(MineWorldGame gameInstance)
        {
            _gameInstance = gameInstance;
        }

        public void Update(GameTime gameTime)
        {
            if (_p == null)
                return;

            foreach (ClientPlayer p in _p.PlayerList.Values)
            {
                p.StepInterpolation(gameTime.TotalGameTime.TotalSeconds);
                p.SpriteModel.Update(gameTime);
            }
        }

        public void Render(GraphicsDevice graphicsDevice)
        {
            // If we don't have _P, grab it from the current gameInstance.
            // We can't do this in the constructor because we are created in the property bag's constructor!
            if (_p == null)
                _p = _gameInstance.PropertyBag;

            foreach (ClientPlayer p in _p.PlayerList.Values)
            {
                if (p.Alive && p.ID != _p.PlayerMyId)
                {
                    p.SpriteModel.Draw(_p.PlayerCamera.ViewMatrix,
                                       _p.PlayerCamera.ProjectionMatrix,
                                       _p.PlayerCamera.Position,
                                       _p.PlayerCamera.GetLookVector(),
                                       p.Position - Vector3.UnitY*1.5f,
                                       p.Heading,
                                       2);
                }
            }
        }

        public void RenderPlayerNames(GraphicsDevice graphicsDevice)
        {
            // If we don't have _P, grab it from the current gameInstance.
            // We can't do this in the constructor because we are created in the property bag's constructor!
            if (_p == null)
                _p = _gameInstance.PropertyBag;

            foreach (ClientPlayer p in _p.PlayerList.Values)
            {
                if (p.Alive && p.ID != _p.PlayerMyId)
                {
                    // Figure out what text we should draw on the player - only for teammates and nearby enemies
                    bool continueDraw = false;
                    if (p.ID != _p.PlayerMyId)
                        continueDraw = true;
                    else
                    {
                        Vector3 diff = (p.Position - _p.PlayerPosition);
                        float len = diff.Length();
                        diff.Normalize();
                        if (len <= 15)
                        {
                            Vector3 hit = Vector3.Zero;
                            Vector3 build = Vector3.Zero;
                            _gameInstance.PropertyBag.BlockEngine.RayCollision(
                                _p.PlayerPosition + new Vector3(0f, 0.1f, 0f), diff, len, 25, ref hit, ref build);
                            if (hit == Vector3.Zero) //Why is this reversed?
                                continueDraw = true;
                        }
                    }
                    if (continueDraw)
                    {
                        string playerText = p.Name;
                        playerText = "*** " + playerText + " ***";

                        p.SpriteModel.DrawText(_p.PlayerCamera.ViewMatrix,
                                               _p.PlayerCamera.ProjectionMatrix,
                                               p.Position - Vector3.UnitY*1.5f,
                                               playerText, Color.White);
                    }
                }
            }
        }
    }
}