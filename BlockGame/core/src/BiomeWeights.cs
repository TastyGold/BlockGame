namespace BlockGame
{
    public class BiomeWeights
    {
        private readonly int[] weights;

        public int GetMainBiome()
        {
            int maxIndex = 0;
            for (int i = 1; i < 8; i++)
            {
                if (weights[i] > weights[maxIndex])
                    maxIndex = i;
            }
            return maxIndex;
        }

        public Biome GetAverageValues()
        {
            Biome b = new Biome();

            int biomeIndex = GetMainBiome();
            b.biomeIndex = biomeIndex;
            b.palette = WorldBiomes.GetBiome(biomeIndex).palette;

            for (int i = 0; i < 8; i++)
            {
                for (int v = 0; v < weights[i]; v++)
                {
                    b.caveWidth += WorldBiomes.GetBiome(i).caveWidth;
                    b.groundHeight += WorldBiomes.GetBiome(i).groundHeight;
                    b.noiseAmplitude += WorldBiomes.GetBiome(i).noiseAmplitude;
                    b.caveStartHeight += WorldBiomes.GetBiome(i).caveStartHeight;
                    b.noiseLacunarity += WorldBiomes.GetBiome(i).noiseLacunarity;
                    b.primaryNoiseScale += WorldBiomes.GetBiome(i).primaryNoiseScale;
                }
            }

            b.caveWidth /= 256;
            b.groundHeight /= 256;
            b.noiseAmplitude /= 256;
            b.caveStartHeight /= 256;
            b.noiseLacunarity /= 256;
            b.primaryNoiseScale /= 256;

            return b;
        }

        public int this[int index]
        {
            get => weights[index];
            set => weights[index] = value;
        }

        public BiomeWeights()
        {
            weights = new int[8];
        }
    }
}