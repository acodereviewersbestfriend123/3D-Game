using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace _3D_Game
{
    class ParticleExplosion
    {
        //Particle array and x=vertix buffer
        VertexPositionTexture[] verts;
        Vector3[] vertexDirectionArray;
        Color[] vertexColorArray;
        VertexBuffer particleVertexBuffer;

        //Position
        Vector3 position;

        //Life
        int lifeLeft;

        //Rounds and particle count
        int numParticlesPerRound;
        int maxParticles;
        static Random rnd = new Random();
        int roundTime;
        int timeSinceLastRound = 0;

        //Vertex and Graphics info
        GraphicsDevice graphicsDevice;

        //setting
        ParticleSettings particleSettings;

        //Effects
        Effect particleEffect;

        //Textures
        Texture2D particleColorsTexture;

        //Array indices
        int endOfLiveParticlesIndex = 0;
        int endOfDeadParticlesIndex = 0;

        public bool IsDead
        {
            get { return endOfDeadParticlesIndex == maxParticles; }
        }

        public ParticleExplosion(GraphicsDevice graphicsDevice, Vector3 position,
            int lifeLeft, int roundTime, int numParticlesPerRound, int maxParticles,
            Texture2D particleColorsTexture, ParticleSettings particleSettings,
            Effect particleEffect)
        {
            this.position = position;
            this.lifeLeft = lifeLeft;
            this.numParticlesPerRound = numParticlesPerRound;
            this.maxParticles = maxParticles;
            this.roundTime = roundTime;
            this.graphicsDevice = graphicsDevice;
            this.particleSettings = particleSettings;
            this.particleEffect = particleEffect;
            this.particleColorsTexture = particleColorsTexture;

            InitializeParticleVertices();
        }

        private void InitializeParticleVertices()
        {
            //Instantiate all particle arrays
            verts = new VertexPositionTexture[maxParticles * 4];
            vertexDirectionArray = new Vector3[maxParticles];
            vertexColorArray = new Color[maxParticles];

            //Get Color data from colors texture
            Color[] colors = new Color[particleColorsTexture.Width *
                particleColorsTexture.Height];
            particleColorsTexture.GetData(colors);

            //Loop until max particles
            for (int i = 0; i < maxParticles; ++i)
            {
                float size = (float)rnd.NextDouble() * particleSettings.maxSize;

                //set position, direction, and size of particle
                verts[i * 4] = new VertexPositionTexture(position, new Vector2(0, 0));
                verts[(i * 4) + 1] = new VertexPositionTexture(new Vector3(position.X,
                    position.Y + size, position.Z), new Vector2(0, 1));
                verts[(i * 4) + 2] = new VertexPositionTexture(new Vector3(position.X + size,
                    position.Y, position.Z), new Vector2(1, 0));
                verts[(i * 4) + 3] = new VertexPositionTexture(new Vector3(position.X + size,
                    position.Y + size, position.Z), new Vector2(1, 1));

                // Create random velocity/ direction 
                Vector3 direction = new Vector3((float)rnd.NextDouble() * 2 - 1,
                    (float)rnd.NextDouble() * 2 - 1,
                    (float)rnd.NextDouble() * 2 - 1);
                direction.Normalize();

                //multiply by nextdouble to make sure all particles goes on random speed
                direction *= (float)rnd.NextDouble();

                //set direction of particle
                vertexDirectionArray[i] = direction;

                // set color of particle by getting random number from tecture
                vertexColorArray[i] = colors[(rnd.Next(0, particleColorsTexture.Height) *
                    particleColorsTexture.Width) + rnd.Next(0, particleColorsTexture.Width)];

            }

            // Istantiate vertex buffer
            particleVertexBuffer = new VertexBuffer(graphicsDevice,
                typeof(VertexPositionTexture), verts.Length, BufferUsage.None);

        }

        public void Update(GameTime gameTime)
        {
            //Decrement life left until its gone
            if (lifeLeft > 0)
                lifeLeft -= gameTime.ElapsedGameTime.Milliseconds;

            // Time for new round
            timeSinceLastRound += gameTime.ElapsedGameTime.Milliseconds;
            if (timeSinceLastRound > roundTime)
            {
                //New round add and remove particle
                timeSinceLastRound -= roundTime;

                //Increment end of life particles each
                //each round untile end of list is reached

                if (endOfLiveParticlesIndex < maxParticles)
                {

                    endOfLiveParticlesIndex += numParticlesPerRound;
                    if (endOfLiveParticlesIndex > maxParticles)
                        endOfLiveParticlesIndex = maxParticles;
                }

                if (lifeLeft <= 0)
                {
                    //Increment end of dead particles each
                    //round until end of list is reached

                    if (endOfDeadParticlesIndex < maxParticles)
                    {
                        endOfDeadParticlesIndex += numParticlesPerRound;
                        if (endOfDeadParticlesIndex > maxParticles)
                            endOfDeadParticlesIndex = maxParticles;
                    }
                }

            }
            for (int i = endOfDeadParticlesIndex;
                i < endOfLiveParticlesIndex; ++i)
            {
                verts[i * 4].Position += vertexDirectionArray[i];
                verts[(i * 4) + 1].Position += vertexDirectionArray[i];
                verts[(i * 4) + 2].Position += vertexDirectionArray[i];
                verts[(i * 4) + 3].Position += vertexDirectionArray[i];
            }
        }

        public void Drew(Camera camera)
        {
            graphicsDevice.SetVertexBuffer(particleVertexBuffer);

            //Only draw when there is a life particle
            if (endOfLiveParticlesIndex - endOfDeadParticlesIndex > 0)
            {
                for (int i = endOfDeadParticlesIndex; i < endOfLiveParticlesIndex; ++i)
                {
                    particleEffect.Parameters["WorldViewProjection"].SetValue(
                        camera.view * camera.projection);
                    particleEffect.Parameters["particleColor"].SetValue(
                        vertexColorArray[i].ToVector4());

                    //Draw particles
                    foreach (EffectPass pass in particleEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        graphicsDevice.DrawUserPrimitives<VertexPositionTexture>(
                            PrimitiveType.TriangleStrip, verts, i * 4, 2);
                    }
                }
            }

        }


    }
}
