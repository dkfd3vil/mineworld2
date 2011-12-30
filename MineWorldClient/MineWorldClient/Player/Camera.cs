using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace MineWorld
{
    public class Camera
    {
        // Properties
        Vector3 pos, ang;
        Matrix view, proj;

        const float PiOver2 = MathHelper.PiOver2;
        const float TwoPi = MathHelper.TwoPi;

        /// <summary>
        /// Creates a 3D camera object
        /// </summary>
        /// <param name="game">Game reference</param>
        /// <param name="pos">Camera position</param>
        /// <param name="ang">Camera angle</param>
        public Camera(PropertyBag game, Vector3 pos, Vector3 ang)
        {
            this.pos = pos;
            this.ang = ang;
            this.SetPerspective(MathHelper.ToRadians(90),game.GameManager.device.Viewport.AspectRatio, 0.01f, 100000.0f);
        }

        public void Update()
        {
            // Use modulus on angles to keep values between (0 - 2PI) radians
            ang.X = MathHelper.Clamp(ang.X, -PiOver2 + 0.01f, PiOver2 - 0.01f);// Clamp pitch
            ang.Y %= TwoPi;
            ang.Z %= TwoPi;

            // Create view matrix
            view = Matrix.Identity *
                   Matrix.CreateTranslation(-pos) *
                   Matrix.CreateRotationZ(ang.Z) *
                   Matrix.CreateRotationY(ang.Y) *
                   Matrix.CreateRotationX(ang.X);
        }

        /// <summary>
        /// Gets or sets the position of the camera in 3D space
        /// </summary>
        public Vector3 Position
        {
            get { return pos; }
            set { pos = value; }
        }

        /// <summary>
        /// Gets or sets the local angle of the camera
        /// </summary>
        public Vector3 Angle
        {
            get { return ang; }
            set { ang = value; }
        }

        /// <summary>
        /// Gets or sets the camera's pitch
        /// </summary>
        public float Pitch
        {
            get { return ang.X; }
            set
            {
                ang.X = value;
            }
        }

        /// <summary>
        /// Gets or sets the camera's yaw
        /// </summary>
        public float Yaw
        {
            get { return ang.Y; }
            set { ang.Y = value; }
        }

        /// <summary>
        /// Gets or sets the camera's roll
        /// </summary>
        public float Roll
        {
            get { return ang.Z; }
            set { ang.Z = value; }
        }

        /// <summary>
        /// Gets the forward direction of the camera
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                return Vector3.Normalize(
                    new Vector3(
                        -(float)(Math.Sin(ang.Z) * Math.Sin(ang.X) + Math.Cos(ang.Z) * Math.Sin(ang.Y) * Math.Cos(ang.X)),
                        -(float)(-Math.Cos(ang.Z) * Math.Sin(ang.X) + Math.Sin(ang.Z) * Math.Sin(ang.Y) * Math.Cos(ang.X)),
                        (float)(Math.Cos(ang.Y) * Math.Cos(ang.X))
                    )
                );
            }
        }

        /// <summary>
        /// Gets the right direction of the camera
        /// </summary>
        public Vector3 Right
        {
            get
            {
                return Vector3.Normalize(
                    Vector3.Cross(Vector3.Up, this.Forward)
                );
            }
        }

        /// <summary>
        /// Gets the view matrix
        /// </summary>
        public Matrix View
        {
            get { return view; }
        }

        /// <summary>
        /// Gets the projection matrix
        /// </summary>
        public Matrix Projection
        {
            get { return proj; }
        }

        /// <summary>
        /// Sets the perspective for the camera
        /// </summary>
        /// <param name="fov">Field of view</param>
        /// <param name="aspratio">Aspect ratio</param>
        /// <param name="znear">Near clipping plane</param>
        /// <param name="zfar">Far clipping plane</param>
        public void SetPerspective(float fov, float aspratio, float znear, float zfar)
        {
            // Create projection matrix
            proj = Matrix.CreatePerspectiveFieldOfView(fov, aspratio, znear, zfar);
        }

        /// <summary>
        /// Adds offset to the camera angle each time this is called
        /// </summary>
        /// <param name="pitch">Pitch</param>
        /// <param name="yaw">Yaw</param>
        /// <param name="roll">Roll</param>
        public void Rotate(float pitch, float yaw, float roll)
        {
            ang.X += pitch;
            ang.Y += yaw;
            ang.Z += roll;
        }

        /// <summary>
        /// Retrieves a string representation of the current object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("Position: [{0}, {1}, {2}]\nAngle: [{3}, {4}, {5}]",
                Math.Floor(pos.X), Math.Floor(pos.Y), Math.Floor(pos.Z),
                Math.Round(ang.X, 2), Math.Round(ang.Y, 2), Math.Round(ang.Z, 2)
            );
        }

    }
}


