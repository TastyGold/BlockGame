using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Raylib_cs;
using System.Runtime.InteropServices;
using BlockGame.Maths;

namespace BlockGame.Rendering
{
    static class TextureGenerationDiagnostics
    {
        const int chunkSize = 16;

        public static void Test()
        {
            Stopwatch s = Stopwatch.StartNew();
            byte[,] values = new byte[16 * chunkSize, 16 * chunkSize];

            for (int ty = 0; ty < values.GetLength(1) / 16; ty++)
            {
                for (int tx = 0; tx < values.GetLength(0) / 16; tx++)
                {
                    byte c0 = SmoothLightmapGenerator.GetAverageOf4(0, 16, 22, 41);
                    byte c1 = SmoothLightmapGenerator.GetAverageOf4(100, 16, 22, 41);
                    byte c2 = SmoothLightmapGenerator.GetAverageOf4(41, 2, 37, 82);
                    byte c3 = SmoothLightmapGenerator.GetAverageOf4(2, 6, 72, 82);

                    for (int py = 0; py < 16; py++)
                    {
                        for (int px = 0; px < 16; px++)
                        {
                            byte v = BlockGameMath.Lerp(BlockGameMath.Lerp(c0, c1, px / (float)16), BlockGameMath.Lerp(c2, c3, px / (float)16), py / (float)16);
                            values[px + (tx * 16), py + (ty * 16)] = v;
                        }
                    }
                }
            }
            Console.WriteLine($"Interpolated values calculated in: {s.ElapsedMilliseconds}ms");

            s = Stopwatch.StartNew();
            Image img = Raylib.GenImageColor(16 * chunkSize, 16 * chunkSize, Color.BLACK);

            for (int y = 0; y < 16 * chunkSize; y++)
            {
                for (int x = 0; x < 16 * chunkSize; x++)
                {
                    byte v = values[x, y];
                    byte m = byte.MinValue;
                    Raylib.ImageDrawPixel(ref img, x, y, new Color(m, m, m, v));
                }
            }
            Console.WriteLine($"Image created in: {s.ElapsedMilliseconds}ms");
        }

        public static void TestWithPointer()
        {
            unsafe
            {
                Color* colors = stackalloc Color[256 * chunkSize * chunkSize];
                Stopwatch s = Stopwatch.StartNew();

                for (int ty = 0; ty < chunkSize; ty++)
                {
                    for (int tx = 0; tx < chunkSize; tx++)
                    {
                        byte c0 = SmoothLightmapGenerator.GetAverageOf4(0, 16, 22, 41);
                        byte c1 = SmoothLightmapGenerator.GetAverageOf4(100, 16, 22, 41);
                        byte c2 = SmoothLightmapGenerator.GetAverageOf4(41, 2, 37, 82);
                        byte c3 = SmoothLightmapGenerator.GetAverageOf4(2, 6, 72, 82);

                        for (int py = 0; py < 16; py++)
                        {
                            for (int px = 0; px < 16; px++)
                            {
                                byte v = BlockGameMath.Lerp(BlockGameMath.Lerp(c0, c1, px / (float)16), BlockGameMath.Lerp(c2, c3, px / (float)16), py / (float)16);
                                colors[px + (tx * 16) + ((py + (ty * 16)) * chunkSize * 16)] = new Color(byte.MinValue, byte.MinValue, byte.MinValue, 256- v);
                            }
                        }
                    }
                }
                Console.WriteLine($"Interpolated values calculated in: {s.ElapsedMilliseconds}ms");
                s = Stopwatch.StartNew();
                Image img = Raylib.GenImageColor(256, 256, Color.BLACK);
                Marshal.FreeHGlobal(img.data);
                img.data = (IntPtr)colors;
                
                Console.WriteLine($"Image generated in: {s.ElapsedMilliseconds}ms");
                Raylib.ExportImage(img, "..//..//..//testExport.png");
            }
        }
    }
}
