namespace BlockGame
{
    public static class BlockGameMath
    {
        public static int FloorToInt(this float f)
        {
            return (int)f + (f < 0 ? -1 : 0);
        }

        public static int Modulo(int x, int m)
        {
            return (x % m + m) % m;
        }

        public static int TileToChunkCoord(int v)
        {
            if (v >= 0) return v / 16;
            else
            {
                v++;
                v /= 16;
                v--;
                return v;
            }
        }

        public static void Add(this float[,] arr1, float[,] arr2, float multiplier)
        {
            for (int y = 0; y < arr1.GetLength(1); y++)
            {
                for (int x = 0; x < arr1.GetLength(0); x++)
                {
                    arr1[x, y] += arr2[x, y] * multiplier;
                }
            }
        }
    }
}