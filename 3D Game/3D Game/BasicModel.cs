using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _3D_Game
{
    class BasicModel
    {
        public Model model { get; protected set; }
        public Matrix world = Matrix.Identity;

        // Costructor
        public BasicModel(Model m)
        {
            model = m;
        }

        //Updat method 
        public virtual void Update()
        {

        }
        public void Draw(Camera camera)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect be in mesh.Effects)
                {
                    be.EnableDefaultLighting();
                    be.Projection = camera.projection;
                    be.View = camera.view;
                    be.World = GetWorld() * mesh.ParentBone.Transform;
                }
                mesh.Draw();
            }
            
        }
        public virtual Matrix GetWorld()
        {
            return world;
        }

        public bool CollidesWith(Model otherModel, Matrix otherWorld)
        {
            //Loop through each modelmesh in both objects
            foreach (ModelMesh myModelMeshes in model.Meshes)
            {
                foreach (ModelMesh hisModelMeshes in otherModel.Meshes)
                {
                    if (myModelMeshes.BoundingSphere.Transform(
                        GetWorld()).Intersects(
                        hisModelMeshes.BoundingSphere.Transform(otherWorld)))
                        return true;
                }
            }
            return false;
        }
    }
}
