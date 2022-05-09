using System.Numerics;
using Raylib_cs;

namespace BlockGame
{
    public static class WorldRenderer
    {
        public static Texture2D tileTexture = Raylib.LoadTexture("..//..//..//core//assets//tiles.png");
        public static bool enableLighting = true;

        public static void DrawChunk(WorldChunk chunk)
        {
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    // background
                    if (chunk.tiles[x, y].BgTile != 0 && chunk.tiles[x, y].FgTile == 0)
                    {
                        Tile t = chunk.tiles[x, y];
                        int m8 = t.BgTile % 8;
                        Rectangle srec = new Rectangle(m8 * 16, (t.BgTile / 8) * 16, 16, 16);
                        Rectangle drec = new Rectangle(((chunk.chunkPosX * 16) + x) * 16, ((chunk.chunkPosY * 16) + y) * 16, 16 + 0.002f, 16 + 0.002f);
                        byte light = enableLighting ? (byte)(chunk.lightManager.skylightLevels[x, y] * 8 / 10) : (byte)204;
                        Raylib.DrawTexturePro(tileTexture, srec, drec, Vector2.Zero, 0, new Color(light, light, light, (byte)255));

                    }

                    // foreground
                    if (chunk.tiles[x, y].FgTile != 0)
                    {
                        Tile t = chunk.tiles[x, y];
                        int m8 = t.FgTile % 8;
                        Rectangle srec = new Rectangle(m8 * 16, (t.FgTile / 8) * 16, 16, 16);
                        Rectangle drec = new Rectangle(((chunk.chunkPosX * 16) + x) * 16, ((chunk.chunkPosY * 16) + y) * 16, 16+0.002f, 16 + 0.002f);
                        byte light = enableLighting ? (byte)(chunk.lightManager.skylightLevels[x, y]) : (byte)255;
                        Raylib.DrawTexturePro(tileTexture, srec, drec, Vector2.Zero, 0, new Color(light, light, light, (byte)255));
                    }
                }
            }

            //ambient occlusion
            WorldAmbientOcclusion.DrawAmbientOcclusion(chunk);
        }
    }
}