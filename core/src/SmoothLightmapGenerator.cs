using System;
using Raylib_cs;

namespace BlockGame
{
    public static class SmoothLightmapGenerator
    {
        public static Image GetSmoothLightmap(byte[,] levels)
        {
            //function must be given an 18x18 byte array (a chunk and its neighbours)
            Image map = Raylib.GenImageColor(256, 256, Color.BLACK);

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

                    //px, py: pixel position within tile
                    for (int py = 0; py < 16; py++)
                    {
                        for (int px = 0; px < 16; px++)
                        {
                            byte v = BlockGameMath.Lerp(BlockGameMath.Lerp(c0, c1, px / 16f), BlockGameMath.Lerp(c2, c3, px / 16f), py / 16f);

                            int cx = ((tx - 1) * 16) + px;
                            int cy = ((ty - 1) * 16) + py;
                            Raylib.ImageDrawPixel(ref map, cx, cy, new Color(byte.MinValue, byte.MinValue, byte.MinValue, 255 - v));
                        }
                    }
                }
            }

            return map;
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