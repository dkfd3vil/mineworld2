using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MineWorld
{
    internal class GeometryDebugger
    {
        private readonly Effect _effect;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly VertexDeclaration _vertexDeclaration;
        public Matrix ProjectionMatrix = Matrix.Identity;
        public Matrix ViewMatrix = Matrix.Identity;

        public GeometryDebugger(GraphicsDevice graphicsDevice, Effect effect)
        {
            _graphicsDevice = graphicsDevice;
            _vertexDeclaration = new VertexDeclaration(graphicsDevice, VertexPositionColor.VertexElements);
            _effect = effect;
        }

        public void DrawSphere(Vector3 position, float radius, Color color)
        {
            VertexPositionColor[] sphereVertices = ConstructSphereVertices(position, radius, color);
            _effect.CurrentTechnique = _effect.Techniques["Colored"];
            _effect.Parameters["World"].SetValue(Matrix.Identity);
            _effect.Parameters["View"].SetValue(ViewMatrix);
            _effect.Parameters["Projection"].SetValue(ProjectionMatrix);
            _effect.Begin();
            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                _graphicsDevice.RenderState.CullMode = CullMode.None;
                _graphicsDevice.VertexDeclaration = _vertexDeclaration;
                _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, sphereVertices, 0, sphereVertices.Length/3);
                pass.End();
            }
            _effect.End();
        }

        public void DrawLine(Vector3 posStart, Vector3 posEnd, Color color)
        {
        }

        public VertexPositionColor[] ConstructSphereVertices(Vector3 position, float radius, Color color)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[3*8];
            VertexPositionColor top = new VertexPositionColor(Vector3.Up*radius + position, color);
            VertexPositionColor bottom = new VertexPositionColor(Vector3.Down*radius + position, color);
            VertexPositionColor left = new VertexPositionColor(Vector3.Left*radius + position, color);
            VertexPositionColor right = new VertexPositionColor(Vector3.Right*radius + position, color);
            VertexPositionColor back = new VertexPositionColor(Vector3.Backward*radius + position, color);
            VertexPositionColor front = new VertexPositionColor(Vector3.Forward*radius + position, color);

            // top
            vertices[0] = back;
            vertices[1] = top;
            vertices[2] = right;

            vertices[3] = right;
            vertices[4] = top;
            vertices[5] = front;

            vertices[6] = front;
            vertices[7] = top;
            vertices[8] = left;

            vertices[9] = left;
            vertices[10] = top;
            vertices[11] = back;

            // bottom
            vertices[12] = back;
            vertices[13] = right;
            vertices[14] = bottom;

            vertices[15] = right;
            vertices[16] = front;
            vertices[17] = bottom;

            vertices[18] = front;
            vertices[19] = left;
            vertices[20] = bottom;

            vertices[21] = left;
            vertices[22] = back;
            vertices[23] = bottom;

            return vertices;
        }
    }
}