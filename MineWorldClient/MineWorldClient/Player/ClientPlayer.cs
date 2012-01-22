using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace MineWorld
{
    public class ClientPlayer
    {
        Model playermodel;

        public long ID;
        public string name;
        public Vector3 position;
        public Vector3 heading;
        public float temprot = 0.0f;
        public float scale = 0.1f;

        public ClientPlayer(ContentManager conmanager)
        {
            //Model
            playermodel = conmanager.Load<Model>("Models/player");
        }

        public void Draw(Matrix view, Matrix projection)
        {
            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[playermodel.Bones.Count];
            playermodel.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in playermodel.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effectmodel in mesh.Effects)
                {
                    effectmodel.EnableDefaultLighting();
                    effectmodel.World = transforms[mesh.ParentBone.Index] * 
                        Matrix.CreateScale(scale) *
                        Matrix.CreateRotationY(temprot)
                        * Matrix.CreateTranslation(position);
                    effectmodel.View = view;
                    effectmodel.Projection = projection;
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
        }
    }
}
