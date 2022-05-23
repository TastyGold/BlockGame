using System;
using System.Runtime.InteropServices;
using Raylib_cs;

namespace BlockGame
{
    public static class SmoothLightmapGenerator
    {
        public const int lightmapQuality = 4; //2 = low, 3 = medium, 4 = high
        public const int lightmapResolution = 16 << lightmapQuality;

        public static Texture2D GetSmoothLightmap(byte[,] levels)
        {
            Texture2D tex = Raylib.LoadTextureFromImage(Raylib.GenImageColor(lightmapResolution, lightmapResolution, Color.BLACK));

            //function must be given an 18x18 byte array (a chunk and its neighbours)
            unsafe
            {
                //Color* colors = stackalloc Color[lightmapResolution * lightmapResolution];
                Color* colors = (Color*)Marshal.AllocHGlobal(lightmapResolution * lightmapResolution * 4);

                //tx, ty: tile coordinate within chunk
                for (int ty = 1; ty <= 16; ty++)
                {
                    for (int tx = 1; tx <= 16; tx++)
                    {
                        // c0 - c1
                        //  | V |
                        // c2 - c3

                        byte c0 = GetCornerAverage(levels, tx - 1, ty - 1);
                        byte c1 = GetCornerAverage(levels, tx, ty - 1);
                        byte c2 = GetCornerAverage(levels, tx - 1, ty);
                        byte c3 = GetCornerAverage(levels, tx, ty);

                        const int resolution = lightmapResolution / 16;

                        //px, py: pixel position within tile
                        for (int py = 0; py < resolution; py++)
                        {
                            for (int px = 0; px < resolution; px++)
                            {
                                byte v = BlockGameMath.Lerp(BlockGameMath.Lerp(c0, c1, px / (float)resolution), BlockGameMath.Lerp(c2, c3, px / (float)resolution), py / (float)resolution);

                                colors[px + (resolution * (tx - 1)) + (lightmapResolution * (py + (resolution * (ty - 1))))] = new Color(byte.MinValue, byte.MinValue, byte.MinValue, 255 - v);
                            }
                        }
                    }
                }

                Raylib.UpdateTexture(tex, (IntPtr)colors);

                Marshal.FreeHGlobal((IntPtr)colors);
            }

            return tex;
        }

        public static Image GetBasicLightmapImage(byte[,] levels)
        {
            Image map = Raylib.GenImageColor(18, 18, Color.BLACK);

            for (int y = 0; y < 18; y++)
            {
                for (int x = 0; x < 18; x++)
                {
                    Raylib.ImageDrawPixel(ref map, x, y, new Color(0, 0, 0, 255 - levels[x, y]));
                }
            }

            return map;
        }

        public static byte GetCornerAverage(byte[,] vs, int x, int y)
        {
            return GetAverageOf4(vs[x, y], vs[x + 1, y], vs[x, y + 1], vs[x + 1, y + 1]);
        }

        public static byte GetAverageOf4(byte a, byte b, byte c, byte d)
        {
            return (byte)((a + b + c + d) / 4);
        }
    }
}