using System;
using System.Collections.Generic;
using System.Text;

namespace BlockGame.Maths
{
    static class PixelCircle
    {
        // first number is x offset, second number is width of line
        public static (int, int)[] lines = {
            (6, 6), (4, 10), (3, 12), (2, 14), (1, 16), (1, 16), (0, 18), (0, 18), (0, 18),
            (0, 18), (0, 18), (0, 18), (1, 16), (1, 16), (2, 14), (3, 12), (4, 10), (6, 6), };

        public static int[] x;
        public static int[] y;

        public static void LoadArrays()
        {
            x = new int[256];
            y = new int[256];
            int i = 0;

            for (int line = 0; line < lines.Length; line++)
            {
                for (int lineIndex = 0; lineIndex < lines[line].Item2; lineIndex++)
                {
                    y[i] = line;
                    x[i] = lineIndex + lines[line].Item1;
                    i++;
                }
            }
        }

        public static void Test()
        {
            LoadArrays();
            bool[,] tiles = new bool[18, 18];

            for (int i = 0; i < 256; i++)
            {
                tiles[x[i], y[i]] = true;
            }

            for (int y = 0; y < 18; y++)
            {
                for (int x = 0; x < 18; x++)
                {
                    Console.Write(tiles[x, y] ? "0 " : "- ");
                }
                Console.Write("\n");
            }
        }
    }
}
