using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MineWorld
{
    public class Camera
    {
        private const float NearPlane = 0.01f;
        private const float FarPlane = 140f;
        public float Pitch;
        public Vector3 Position;
        public Matrix ProjectionMatrix = Matrix.Identity;
        public Matrix ViewMatrix = Matrix.Identity;
        public float Yaw;

        public Camera(GraphicsDevice device)
        {
            Pitch = 0;
            Yaw = 0;
            Position = Vector3.Zero;

            float aspectRatio = device.Viewport.AspectRatio;
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(70), aspectRatio, NearPlane,
                                                                   FarPlane);
        }

        // Returns a unit vector pointing in the direction that we're looking.
        public Vector3 GetLookVector()
        {
            Matrix rotation = Matrix.CreateRotationX(Pitch)*Matrix.CreateRotationY(Yaw);
            return Vector3.Transform(Vector3.Forward, rotation);
        }

        public Vector3 GetRightVector()
        {
            Matrix rotation = Matrix.CreateRotationX(Pitch)*Matrix.CreateRotationY(Yaw);
            return Vector3.Transform(Vector3.Right, rotation);
        }

        public void Update()
        {
            Vector3 target = Position + GetLookVector();
            ViewMatrix = Matrix.CreateLookAt(Position, target, Vector3.Up);
        }
    }
}