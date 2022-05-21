using System;

namespace BlockGame
{
    public class WorldChunkBiomeManager
    {
        public WorldChunk chunk;

        public Biome[] biomeData = new Biome[16];

        public void GenerateBiomeData()
        {
            for (int i = 0; i < 16; i++)
            {
                biomeData[i] = WorldBiomeGeneration.GetBiomeWeights((chunk.chunkPosX * 16) + i).GetAverageValues();
            }
        }
    }
}
