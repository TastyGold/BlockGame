namespace BlockGame
{
    public struct Biome
    {
        public int biomeIndex;

        public float groundHeight;

        public float noiseAmplitude;
        public float primaryNoiseScale;
        public int noiseIterations;
        public float noiseLacunarity;

        public float caveStartHeight;
        public float caveWidth;

        public BiomeBlockPalette palette;
    }
}