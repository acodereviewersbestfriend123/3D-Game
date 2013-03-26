using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _3D_Game
{
    class ParticleStarSheet
    {
        //Particle arrays and vertex buffer
        VertexPositionTexture[] verts;
        Color[] vertexColorArray;
        VertexBuffer particleVertexBuffer;

        //Behaivor variables
        Vector3 maxPosition;
        int maxParticles;
        static Random rnd = new Random();

        //Vertex and Graphics info
        GraphicsDevice graphicsDevice;

        //setting
        ParticleSettings particleSettings;

        //Effect
        Effect particleEffect;

        //Textures
        Texture2D particleColorsTexture;

        public ParticleStarSheet(GraphicsDevice graphicsDevice, Vector3 maxPosition, int maxParticles,
            Texture2D particleColorsTexture, ParticleSettings particleSettings, Effect particleEffect)
        {
            this.maxParticles = maxParticles;
            this.graphicsDevice = graphicsDevice;
            this.particleSettings = particleSettings;
            this.particleEffect = particleEffect;
            this.particleColorsTexture = particleColorsTexture;
            this.maxPosition = maxPosition;

            InitiliazeParticleVertices();

        }

        private void InitiliazeParticleVertices()
        {
            //instantiate all particle arrays
            verts = new VertexPositionTexture[maxParticles * 4];
            vertexColorArray = new Color[maxParticles];

            //Get color data from colors Texture
            Color[] colors = new Color[particleColorsTexture.Width *
                particleColorsTexture.Height];
            particleColorsTexture.GetData(colors);

            //Loop untile max particle
            for (int i = 0; i < maxParticles; ++i)
            {
                float size = (float)rnd.NextDouble() * particleSettings.maxSize;

                Vector3 position = new Vector3(
                    rnd.Next(-(int)maxPosition.X, (int)maxPosition.X),
                    rnd.Next(-(int)maxPosition.Y, (int)maxPosition.Y),
                    maxPosition.Z);
                
                //Set position and sice of partice
                verts[i * 4] = new VertexPositionTexture(position, new Vector2(0, 0));
                verts[(i * 4) + 1] = new VertexPositionTexture(
                    new Vector3(position.X, position.Y + size, position.Z),
                    new Vector2(0, 1));
                verts[(i * 4) + 2] = new VertexPositionTexture(
                    new Vector3(position.X + size, position.Y, position.Z),
                    new Vector2(1, 0));
                verts[(i * 4) + 3] = new VertexPositionTexture(
                    new Vector3(position.X + size, position.Y + size, position.Z),
                    new Vector2(1, 1));

                //Set color of particle by getting a random color from texture
                vertexColorArray[i] = colors[(rnd.Next(0,
                    particleColorsTexture.Height) * particleColorsTexture.Width) +
                    rnd.Next(0, particleColorsTexture.Width)];


            }

            //Instantiate vertex buffer
            particleVertexBuffer = new VertexBuffer(graphicsDevice,
                typeof(VertexPositionTexture), verts.Length, BufferUsage.None);
        }

        public void Drew(Camera camera)
        {
            graphicsDevice.SetVertexBuffers(particleVertexBuffer);

            for (int i = 0; i < maxParticles; ++i)
            {
                particleEffect.Parameters["WorldViewProjection"].SetValue(
                    camera.view * camera.projection);
                particleEffect.Parameters["particleColor"].SetValue(
                    vertexColorArray[i].ToVector4());

                //Drew particles
                foreach (EffectPass pass in particleEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserPrimitives<VertexPositionTexture>(
                        PrimitiveType.TriangleStrip,
                        verts, i * 4, 2);
                }
            }
        }



    }
}
