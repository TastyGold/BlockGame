using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;
using static BlockGame.Maths.BlockGameMath;
using BlockGame.Worlds;
using BlockGame.Runtime;
using BlockGame.Maths;

namespace BlockGame.Rendering
{
    public static class WorldRenderer
    {
        public static Texture2D tileTexture = Raylib.LoadTexture("..//..//..//core//assets//tiles.png");
        public static Texture2D bgTileTexture = Raylib.LoadTexture("..//..//..//core//assets//bgtiles.png");

        public static List<WorldChunk> loadedChunks = new List<WorldChunk>();

        public static void LoadChunk(WorldChunk chunk)
        {
            loadedChunks.Add(chunk);
            chunk.renderer.LoadSmoothLightmap();
        }
        public static void UnloadChunk(WorldChunk chunk)
        {
            loadedChunks.Remove(chunk);
            chunk.renderer.UnloadLightmapTexture();
        }
        public static void SetLoadedRange(World world, int startX, int startY, int endX, int endY)
        {
            int i = 0;
            while (i < loadedChunks.Count)
            {
                WorldChunk c = loadedChunks[i];
                if (c.chunkPosX < startX || c.chunkPosX > endX || c.chunkPosY < startY || c.chunkPosY > endY)
                {
                    UnloadChunk(c);
                }
                else i++;
            }

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    WorldChunk chunk = world.GetChunk(new VecInt2(x, y), out bool found);
                    if (found)
                    {
                        if (!loadedChunks.Contains(chunk))
                        {
                            LoadChunk(chunk);
                        }
                    }
                }
            }
        }

        public static void DrawLoadedChunks()
        {
            for (int i = 0; i < loadedChunks.Count; i++)
            {
                DrawChunk(loadedChunks[i]);
            }
        }

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
                        Rectangle srec = new Rectangle(m8 * 16, (t.BgTile / 8) * 16, 16, 16).FixBleedingEdge();
                        Rectangle drec = new Rectangle(((chunk.chunkPosX * 16) + x) * 16, ((chunk.chunkPosY * 16) + y) * 16, 16 + 0.002f, 16 + 0.002f);
                        byte light = (Settings.enableLighting && !Settings.enableSmoothLighting) ? (byte)(chunk.lightManager.localSkylightLevels[x, y] * 8 / 10) : (byte)204;
                        Raylib.DrawTexturePro(bgTileTexture, srec, drec, Vector2.Zero, 0, new Color(light, light, light, (byte)255));

                    }

                    // foreground
                    if (chunk.tiles[x, y].FgTile != 0)
                    {
                        Tile t = chunk.tiles[x, y];
                        int m8 = t.FgTile % 8;
                        Rectangle srec = new Rectangle(m8 * 16, (t.FgTile / 8) * 16, 16, 16).FixBleedingEdge();
                        Rectangle drec = new Rectangle(((chunk.chunkPosX * 16) + x) * 16, ((chunk.chunkPosY * 16) + y) * 16, 16+0.002f, 16 + 0.002f);
                        byte light = (Settings.enableLighting && !Settings.enableSmoothLighting) ? (byte)(chunk.lightManager.localSkylightLevels[x, y]) : (byte)255;
                        Raylib.DrawTexturePro(tileTexture, srec, drec, Vector2.Zero, 0, new Color(light, light, light, (byte)255));
                    }
                }
            }

            //ambient occlusion
            WorldAmbientOcclusion.DrawAmbientOcclusion(chunk);

            //Lighting
            if (Settings.enableLighting && Settings.enableSmoothLighting) Raylib.DrawTextureEx(chunk.renderer.lightmapTexture, new Vector2(chunk.chunkPosX * 256, chunk.chunkPosY * 256), 0, 256 / SmoothLightmapGenerator.lightmapResolution, Color.WHITE);
        }
    }
}