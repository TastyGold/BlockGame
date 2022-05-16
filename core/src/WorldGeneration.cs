using System;
using SimplexNoise;
using static BlockGame.BlockGameMath;
using System.Collections.Generic;

namespace BlockGame
{
    public class WorldGeneration
    {
        private static readonly Random rand = new Random();
        private const int oceanWaterLevel = 0;
        private const int caveNoiseIterations = 5;
        public static int Seed => World.seed;
        public static int seaLevel = 256;

        public static int GetWorldHeight(int x, Biome b)
        {
            return seaLevel - (int)b.groundHeight - ((int)GetBiomeSimplex(x, b) / 128);
        }

        public static bool GetCavePresence(int x, int y)
        {
            float simplex = GetSimplexPoint(x, y);
            float width = 16;
            float centre = 128;
            if (y < seaLevel)
            {
                width -= (seaLevel - y) / 3;
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
                chunk.biomeManager.biomeData[x] = WorldBiomeGeneration.GetBiomeWeightsLerped(x + (chunkPos.x * 16), out _, out _).GetAverageValues();
                groundLevel[x] = GetWorldHeight(x + (chunkPos.x * 16), chunk.biomeManager.biomeData[x]);
            }
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    int worldY = y + (chunkPos.y * 16);

                    //if (worldY < groundLevel[x])
                    //{
                    //    chunk.tiles[x, y].FgTile = 0;
                    //}
                    //else if (worldY == groundLevel[x])
                    //{
                    //    chunk.tiles[x, y].FgTile = 1;
                    //    chunk.tiles[x, y].BgTile = 2;
                    //}
                    //else if (worldY < groundLevel[x] + 7)
                    //{
                    //    chunk.tiles[x, y].FgTile = 2; 
                    //    chunk.tiles[x, y].BgTile = 2;
                    //}
                    //else
                    //{
                    //    chunk.tiles[x, y].FgTile = 3;
                    //    chunk.tiles[x, y].BgTile = 9;
                    //}
                    chunk.tiles[x, y].FgTile = (byte)chunk.biomeManager.biomeData[x].palette.GetFgPaletteTile(worldY - groundLevel[x] - 10);
                    chunk.tiles[x, y].BgTile = (byte)chunk.biomeManager.biomeData[x].palette.GetBgPaletteTile(worldY - groundLevel[x] - 10);

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