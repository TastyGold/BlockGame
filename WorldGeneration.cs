using System;
using SimplexNoise;
using static BlockGame.BlockGameMath;
using System.Collections.Generic;

namespace BlockGame
{
    public class WorldGeneration
    {
        private static Random rand = new Random();
        private const float worldScale = 200;
        private const float worldHeightOffset = 15;
        private const int noiseIterations = 5;

        public static int GetWorldHeight(float x, int seed)
        {
            // based on graph: https://www.desmos.com/calculator/x7lgzi99v5

            x /= worldScale;
            x += seed;
            float height = worldHeightOffset;
            for (int n = 1; n <= noiseIterations; n++)
            {
                float numerator = (float)(Math.Cos(2 * n * (x - (10 * n))) + Math.Cos(Math.PI * n * (x - 100)));
                float denominator = (float)((5 / worldScale) * n * (2 + Math.Cos(x)));
                height += numerator / denominator;
            }
            return 200 - (int)height;
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
                groundLevel[x] = GetWorldHeight(x + (chunkPos.x * 16), 0);
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
                    float simplex = GetSimplexPoint(x +(chunkPos.x * 16), y + (chunkPos.y * 16));
                    float width = 16;
                    float centre = 128;
                    if (worldY < 250 - worldHeightOffset)
                    {
                        width -= (250 - worldHeightOffset - worldY) / 3;
                        if (width < 3) width = 0;
                    }
                    if (simplex > centre - width && simplex < centre + width)
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
            for (int i = 1; i <= noiseIterations; i++)
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
            for (int i = 1; i <= noiseIterations; i++)
            {
                float pow = 1 << i;
                value += Noise.CalcPixel2D(x, y, 0.005f * pow) / pow;
            }
            return value;
        }
    }
}