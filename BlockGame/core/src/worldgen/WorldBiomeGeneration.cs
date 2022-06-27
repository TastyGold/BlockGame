using System;
using System.Collections.Generic;
using System.Text;
using Raylib_cs;
using System.Diagnostics;
using SimplexNoise;
using static BlockGame.Maths.BlockGameMath;
using BlockGame.Worlds;
using BlockGame.Maths;

namespace BlockGame.Worlds.Generation
{
    public static class WorldBiomeGeneration
    {
        public static Image biomeImage = Raylib.LoadImage("..//..//..//core//assets//biomeTexture.png");
        public static Image biomeValuesImage = Raylib.LoadImage("..//..//..//core//assets//biomeValues.png");
        public static Color[] biomeColors = {
            new Color(156, 219, 67, 255), new Color(20, 160, 46, 255),
            new Color(255, 213, 65, 255), new Color(219, 164, 99, 255),
            new Color(36, 159, 222, 255), new Color(40, 92, 196, 255),
            new Color(109, 117, 141, 255), new Color (218, 224, 234, 255)
        };

        public static float worldBiomeScale = 0.002f;

        private static IntPtr colors;
        public static void LoadColors()
        {
            colors = Raylib.LoadImageColors(biomeImage);
        }

        private static IntPtr valueSheet;
        public static void LoadValueSheet()
        {
            valueSheet = Raylib.LoadImageColors(biomeValuesImage);
        }

        public static BiomeWeights GetBiomeWeights(int worldX)
        {
            BiomeWeights weights = new BiomeWeights();

            int texX = (int)Noise.CalcPixel1D(worldX, worldBiomeScale) * 240 / 256 + 8;
            int texY = (int)Noise.CalcPixel1D(worldX + 381257, worldBiomeScale) * 240 / 256 + 8;

            unsafe
            {
                Color* col = (Color*)valueSheet;
                int imgWidth = 8 * 256;

                for (int i = 0; i < 8; i++)
                {
                    int ix = 256 * i;

                    weights[i] = col[ix + texX + (imgWidth * texY)].r;
                }
            }

            return weights;
        }

        public static BiomeWeights GetBiomeWeightsLerped(int worldX, out float modX, out float modY)
        {
            BiomeWeights weights = new BiomeWeights();

            float exactX = Noise.CalcPixel1D(worldX, worldBiomeScale) * 240 / 256 + 8;
            float exactY = Noise.CalcPixel1D(worldX + 381257, worldBiomeScale) * 240 / 256 + 8;

            int minX = (exactX - 0.5f).FloorToInt();
            int minY = (exactY - 0.5f).FloorToInt();

            int maxX = minX + 1;
            int maxY = minY + 1;

            minX = Math.Clamp(minX, 0, 255);
            minY = Math.Clamp(minY, 0, 255);
            maxX = Math.Clamp(maxX, 0, 255);
            maxY = Math.Clamp(maxY, 0, 255);

            modX = (exactX - 0.5f) % 1;
            modY = (exactY - 0.5f) % 1;

            unsafe
            {
                Color* col = (Color*)valueSheet;

                for (int i = 0; i < 8; i++)
                {
                    byte c00 = col[minX + (256 * i) + (256 * 8 * minY)].r;
                    byte c01 = col[minX + (256 * i) + (256 * 8 * maxY)].r;
                    byte c10 = col[maxX + (256 * i) + (256 * 8 * minY)].r;
                    byte c11 = col[maxX + (256 * i) + (256 * 8 * maxY)].r;

                    weights[i] = Lerp(Lerp(c00, c10, modX), Lerp(c01, c11, modX), modY);
                }
            }

            return weights;
        }

        public static BiomeWeights GetBlurredColorValues(int texX, int texY)
        {
            BiomeWeights weights = new BiomeWeights();

            unsafe 
            {
                Color* col = (Color*)colors;

                for (int i = 0; i < 256; i++)
                {
                    const float size = 1f;
                    int x = Math.Clamp((int)((PixelCircle.x[i] - 9) * size) + texX, 0, 255);
                    int y = Math.Clamp((int)((PixelCircle.y[i] - 9) * size) + texY, 0, 255);

                    for (int c = 0; c < 8; c++)
                    {
                        if (CompareColors(col[x + (y * 256)], biomeColors[c]))
                        {
                            weights[c]++;
                            c = 8;
                        }
                    }
                }
            }

            return weights;
        }

        public static Image Test()
        {
            Stopwatch s = Stopwatch.StartNew();
            LoadColors();
            Image output = Raylib.GenImageColor(256 * 8, 256, Color.BLACK);
            PixelCircle.LoadArrays();
            BiomeWeights[,] levels = new BiomeWeights[256,256];

            Console.WriteLine($"Starting: {s.ElapsedMilliseconds}ms");
            for (int y = 0; y < 256; y++)
            {
                for (int x = 0; x < 256; x++)
                {
                    levels[x, y] = GetBlurredColorValues(x, y);
                }
            }
            Console.WriteLine($"Values calculated: {s.ElapsedMilliseconds}ms");

            for (int i = 0; i < 8; i++)
            {
                int ix = 256 * i;

                for (int y = 0; y < 256; y++)
                {
                    for (int x = 0; x < 256; x++)
                    {
                        int white = Math.Min(255, levels[x, y][i]);
                        Raylib.ImageDrawPixel(ref output, ix + x, y, new Color(white, white, white, 255));
                    }
                }
            }
            Console.WriteLine($"Image generated: {s.ElapsedMilliseconds}ms");
            Raylib.ExportImage(output, "..//..//..//biomeValues.png");

            return output;
        }

        public static bool CompareColors(Color a, Color b)
        {
            return !(a.r != b.r || a.g != b.g || a.b != b.b || a.a != b.a);
        }
    }
}
