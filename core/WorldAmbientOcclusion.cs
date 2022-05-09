using Raylib_cs;
using System;
using System.Numerics;

namespace BlockGame
{
    public static class WorldAmbientOcclusion
    {
        private static int[] neighbourOffsetX = { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static int[] neighbourOffsetY = { -1, -1, -1, 0, 0, 1, 1, 1 };
        private static Texture2D ambientOcclusionTexture = Raylib.LoadTexture("..//..//..//core//assets//ambientOcclusion.png");

        public static RenderTexture2D GetChunkOcclusionMap(RenderTexture2D target, bool[,] tiles)
        {
            //tiles texture
            Raylib.BeginTextureMode(target);
            for (int y = 1; y < 17; y++)
            {
                for (int x = 1; x < 17; x++)
                {
                    if (tiles[x, y] == false)
                    {

                    }
                }
            }
            Raylib.EndTextureMode();

            return target;
        }

        public static void DrawAmbientOcclusion(WorldChunk chunk)
        {
            bool[,] tiles = chunk.GetFgTileMapWithSeam();

            for (int y = 1; y < 17; y++)
            {
                for (int x = 1; x < 17; x++)
                {
                    if (tiles[x, y] == false && chunk.tiles[x - 1, y - 1].BgTile != 0)
                    {
                        int texId = 0;
                        for (int n = 0; n < 8; n++)
                        {
                            if (tiles[x + neighbourOffsetX[n], y + neighbourOffsetY[n]] == true)
                            {
                                texId += 1 << n;
                            }
                        }

                        if (texId != 0)
                        {
                            int texX = texId % 16;
                            int texY = texId / 16;

                            Rectangle srec = new Rectangle(texX * 16, texY * 16, 16, 16);
                            Rectangle drec = new Rectangle((chunk.chunkPosX * 16 + (x - 1)) * 16, (chunk.chunkPosY * 16 + (y - 1)) * 16, 16, 16);

                            Raylib.DrawTexturePro(ambientOcclusionTexture, srec, drec, Vector2.Zero, 0, new Color(0, 0, 0, 40));
                        }
                    }
                }
            }
        }
    }
}