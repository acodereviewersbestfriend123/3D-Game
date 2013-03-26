

namespace _3D_Game
{
    class ParticleSettings
    {
        //size of particle
        public int maxSize = 2;
    }

    class ParticleExplosionSettings
    {
        // Life of particle
        public int minLife = 1000;
        public int maxLife = 2000;

        //Particles per round
        public int minParticlePerRound = 100;
        public int maxParticlePreRound = 600;

        //Round time
        public int minRoundTime = 16;
        public int maxRoundTime = 50;

        //Number of particles
        public int minParticles = 2000;
        public int maxParticles = 3000;
    }
}
