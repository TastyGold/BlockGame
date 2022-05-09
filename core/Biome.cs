namespace BlockGame
{
    public struct Biome
    {
        public float groundHeight;

        public float noiseAmplitude;
        public float primaryNoiseScale;
        public int noiseIterations;
        public float noiseLacunarity;

        public float caveStartHeight;
        public float caveWidth;
    }

    public static class WorldBiomes
    {
        public static Biome plains, forest, desert, plateau, ocean, deepocean, mountains, peaks;

        public static void Load()
        {
            plains = new Biome()
            {
                groundHeight = 15,
                primaryNoiseScale = 3,
                noiseAmplitude = 0.05f,
                noiseIterations = 2,
            };
        }
    }
}