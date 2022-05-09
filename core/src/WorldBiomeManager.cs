using System;
using System.Collections.Generic;
using System.Text;
using Raylib_cs;
using System.Diagnostics;

namespace BlockGame
{
    public static class WorldBiomeManager
    {
        public static Image textureImage = Raylib.LoadImage("..//..//..//core//assets//biomeTexture.png");
        public static Color[] biomeColors = {
            new Color(156, 219, 67, 255), new Color(20, 160, 46, 255),
            new Color(255, 213, 65, 255), new Color(219, 164, 99, 255),
            new Color(36, 159, 222, 255), new Color(40, 92, 196, 255),
            new Color(109, 117, 141, 255), new Color (218, 224, 234, 255)
        };

        public static int[] GetBiomeValues(int biomeX, int biomeY)
        {
            int[] values = new int[8];

            unsafe 
            {
                Color* colors = (Color*)Raylib.LoadImageColors(textureImage);

                for (int i = 0; i < 256; i++)
                {
                    const float size = 2f;
                    int x = Math.Clamp((int)((PixelCircle.x[i] - 9) * size) + biomeX, 0, 255);
                    int y = Math.Clamp((int)((PixelCircle.y[i] - 9) * size) + biomeY, 0, 255);

                    for (int c = 0; c < 8; c++)
                    {
                        if (CompareColors(colors[x + (y * 256)], biomeColors[c]))
                        {
                            values[c]++;
                            c = 8;
                        }
                    }
                }
            }

            return values;
        }

        public static Image Test()
        {
            Stopwatch s = Stopwatch.StartNew();
            Image output = Raylib.GenImageColor(256 * 4, 256 * 2, Color.BLACK);
            PixelCircle.LoadArrays();
            int[,][] levels = new int[256,256][];

            Console.WriteLine($"Starting: {s.ElapsedMilliseconds}ms");
            for (int y = 0; y < 256; y++)
            {
                for (int x = 0; x < 256; x++)
                {
                    levels[x, y] = GetBiomeValues(x, y);
                }
            }
            Console.WriteLine($"Values calculated: {s.ElapsedMilliseconds}ms");

            for (int i = 0; i < 8; i++)
            {
                int ix = 256 * (i % 4);
                int iy = 256 * (i / 4);

                for (int y = 0; y < 256; y++)
                {
                    for (int x = 0; x < 256; x++)
                    {
                        int white = Math.Min(255, levels[x, y][i]);
                        Raylib.ImageDrawPixel(ref output, ix + x, iy + y, new Color(white, white, white, 255));
                    }
                }
            }
            Console.WriteLine($"Image generated: {s.ElapsedMilliseconds}ms");
            Raylib.ExportImage(output, "..//..//..//biomeLevels.png");

            return output;
        }

        public static bool CompareColors(Color a, Color b)
        {
            return !(a.r != b.r || a.g != b.g || a.b != b.b || a.a != b.a);
        }
    }
}
