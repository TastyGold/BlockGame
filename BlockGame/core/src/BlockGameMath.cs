using Raylib_cs;

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

        public static byte Lerp(byte a, byte b, float t)
        {
            if (a == b) return a;
            else return (byte)(((1 - t) * (float)a) + (t * (float)b));
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

        public static T[,] Duplicate<T>(this T[,] arr)
        {
            int w = arr.GetLength(0);
            int h = arr.GetLength(1);
            T[,] newArray = new T[w, h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    newArray[x, y] = arr[x, y];
                }
            }

            return newArray;
        }

        //Stops texture bleeding by slightly decreasing the height/width
        public static Rectangle FixBleedingEdge(this Rectangle rec)
        {
            return new Rectangle(rec.x + 0.0002f, rec.y + 0.0002f, rec.width - 0.0004f, rec.height - 0.0004f);
        }
    }
}