using System;
using SimplexNoise;
using static BlockGame.BlockGameMath;
using System.Collections.Generic;

namespace BlockGame
{
    public class WorldGeneration
    {
        private static readonly Random rand = new Random();
        private const float worldScale = 100;
        private const int worldHeightOffset = 196;
        private const float worldHeightScale = 0.05f;
        private const int caveNoiseIterations = 5;
        private const int groundNoiseInterations = 2;
        public static int seed;
        public static int seaLevel = 128;

        public static int GetWorldHeight(float x)
        {
            float height = 0;
            int factor = 1;
            for (int n = 1; n <= groundNoiseInterations; n++)
            {
                height += Noise.CalcPixel1D((int)x * factor, factor / (worldScale + x)) / factor;
                factor <<= 1;
            }
            height += Noise.CalcPixel1D((int)x, 1/worldScale);
            return worldHeightOffset - (int)(height * worldHeightScale);
        }

        public static int GetWorldHeight(int x, Biome b)
        {
            return seaLevel - (int)b.groundHeight - ((int)GetBiomeSimplex(x, b) / 128);
        }

        public static bool GetCavePresence(int x, int y)
        {
            float simplex = GetSimplexPoint(x, y);
            float width = 16;
            float centre = 128;
            if (y < 250 - worldHeightOffset)
            {
                width -= (250 - worldHeightOffset - y) / 3;
                if (width < 3) width = 0;
            }

            return simplex > centre - width && simplex < centre + width;
        }

        public static WorldChunk RandomChunk(VecInt2 chunkPos)
        {
            WorldChunk c = new WorldChunk(chunkPos);

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    c.tiles[x, y] = new Tile(rand.Next(0, 8));
                }
            }
            return c;
        }

        public static WorldChunk GenerateChunk(VecInt2 chunkPos)
        {
            WorldChunk chunk = new WorldChunk(chunkPos);

            int[] groundLevel = new int[16];
            for (int x = 0; x < 16; x++)
            {
                //groundLevel[x] = GetWorldHeight(x + (chunkPos.x * 16) + World.seed);
                groundLevel[x] = GetWorldHeight(x + (chunkPos.x * 16));
            }
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    int worldY = y + (chunkPos.y * 16);

                    if (worldY < groundLevel[x])
                    {
                        chunk.tiles[x, y].FgTile = 0;
                    }
                    else if (worldY == groundLevel[x])
                    {
                        chunk.tiles[x, y].FgTile = 1;
                        chunk.tiles[x, y].BgTile = 2;
                    }
                    else if (worldY < groundLevel[x] + 7)
                    {
                        chunk.tiles[x, y].FgTile = 2; 
                        chunk.tiles[x, y].BgTile = 2;
                    }
                    else
                    {
                        chunk.tiles[x, y].FgTile = 3;
                        chunk.tiles[x, y].BgTile = 9;
                    }

                    //Caves
                    //float simplex = GetSimplexPoint(x +(chunkPos.x * 16) + World.seed, y + (chunkPos.y * 16));
                    //float width = 16;
                    //float centre = 128;
                    //if (worldY < 250 - worldHeightOffset)
                    //{
                    //    width -= (250 - worldHeightOffset - worldY) / 3;
                    //    if (width < 3) width = 0;
                    //}
                    bool cave = GetCavePresence(x + (chunkPos.x * 16), y + (chunkPos.y * 16));
                    if (cave)
                    {
                        chunk.tiles[x, y].FgTile = 0;
                    }

                }
            }
            return chunk;
        }

        public static float[,] GetSimplex()
        {
            int size = 400;

            float[,] vs = new float[size, size];
            for (int i = 1; i <= caveNoiseIterations; i++)
            {
                float pow = 1 << i;
                float[,] ns = Noise.Calc2D(size, size, 0.005f * pow);
                vs.Add(ns, 1 / pow);
            }

            return vs; 
        }

        public static float GetSimplexPoint(int x, int y)
        {
            float value = 0;
            for (int i = 1; i <= caveNoiseIterations; i++)
            {
                float pow = 1 << i;
                value += Noise.CalcPixel2D(x, y, 0.005f * pow) / pow;
            }
            return value;
        }

        public static float GetBiomeSimplex(int x, Biome biome)
        {
            float value = 0;
            for (int i = 1; i <= biome.noiseIterations; i++)
            {
                float pow = (float)Math.Pow(biome.noiseLacunarity, i - 1);
                value += Noise.CalcPixel1D(x, biome.primaryNoiseScale * pow) / pow * biome.noiseAmplitude;
            }
            return value;
        }
    }
}