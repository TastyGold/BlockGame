using System;
using System.Collections.Generic;
using Raylib_cs;
using static BlockGame.BlockGameMath;

namespace BlockGame
{
    public class World
    {
        public const int worldChunkHeight = 32;

        public static Random rand = new Random();
        public int seed = rand.Next(-10000, 10000);

        public Dictionary<VecInt2, WorldChunk> chunks = new Dictionary<VecInt2, WorldChunk>();

        public void SetTile(VecInt2 position, bool fg, byte id)
        {
            VecInt2 chunkPos = new VecInt2(TileToChunkCoord(position.x), TileToChunkCoord(position.y));
            int x = Modulo(position.x, 16);
            int y = Modulo(position.y, 16);

            if (chunks.ContainsKey(chunkPos))
            {
                if (fg)
                {
                    chunks[chunkPos].tiles[x, y].FgTile = id;
                }
                else
                {
                    chunks[chunkPos].tiles[x, y].BgTile = id;
                }
            }
        }

        public void AddChunk(WorldChunk c)
        {
            c.world = this;
            chunks.Add(new VecInt2(c.chunkPosX, c.chunkPosY), c);
        }
        public WorldChunk GetChunk(VecInt2 position, out bool found)
        {
            if (chunks.ContainsKey(position))
            {
                found = true;
                return chunks[position];
            }
            else
            {
                found = false;
                return new WorldChunk();
            }
        }

        public Texture2D LoadMapTexture()
        {
            int leftmost = 0, rightmost = 0;
            
            foreach (KeyValuePair<VecInt2, WorldChunk> pair in chunks)
            {
                if (pair.Key.x < leftmost) leftmost = pair.Key.x;
                if (pair.Key.x > rightmost) rightmost = pair.Key.x;
            }

            int mapWidth = 16 * (rightmost - leftmost + 1);
            Image img = Raylib.GenImageColor(mapWidth, worldChunkHeight * 16, Color.WHITE);

            unsafe
            {
                Image i = Raylib.LoadImage("..//..//..//tilemapcolors.png");
                Color* colors = (Color*)Raylib.LoadImageColors(i);

                foreach (KeyValuePair<VecInt2, WorldChunk> pair in chunks)
                {
                    for (int y = 0; y < 16; y++)
                    {
                        for (int x = 0; x < 16; x++)
                        {
                            Raylib.ImageDrawPixel(ref img, x + (pair.Key.x - leftmost) * 16, y + pair.Key.y * 16, colors[pair.Value.tiles[x, y].FgTile]);
                        }
                    }
                }
                Raylib.UnloadImageColors((IntPtr)colors);
                Raylib.UnloadImage(i);
            }

            Texture2D texture = Raylib.LoadTextureFromImage(img);
            Raylib.UnloadImage(img);

            return texture;
        }

        public VecInt2 GetWorldToChunkPosition(System.Numerics.Vector2 v)
        {
            return new VecInt2((int)(v.X / 256 - (v.X < 0 ? 1 : 0)), (int)(v.Y / 256 - (v.Y < 0 ? 1 : 0)));
        }

        public void GenerateRandomChunks()
        {
            for (int i = 0; i < 4; i++)
            {
                VecInt2 v = new VecInt2(i % 2, i / 2);
                AddChunk(WorldGeneration.RandomChunk(v));
            }
        }
        public void GenerateChunks()
        {
            for (int y = 0; y < worldChunkHeight; y++)
            {
                for (int x = -24; x < 24; x++)
                {
                    VecInt2 v = new VecInt2(x, y);
                    WorldChunk c = WorldGeneration.GenerateChunk(v);
                    AddChunk(c);
                    c.lightManager = new WorldChunkLightingManager() { chunk = c };
                    c.lightManager.CalculateSkylight();
                }
            }
            //for (int i = 0; i < 2; i++)
            //{
                foreach (WorldChunk chunk in chunks.Values)
                {
                    chunk.lightManager.FixSkylightSeams(out _);
                }
            //}
        }

        public void DrawAll()
        {
            foreach (WorldChunk c in chunks.Values)
            {
                WorldRenderer.DrawChunk(c);
            }
        }
        public void OutlineChunk(int x, int y)
        {
            Raylib.DrawLine(x * 256, y * 256, (x + 1) * 256, y * 256, Color.RED);
            Raylib.DrawLine(x * 256, y * 256, x * 256, (y + 1) * 256, Color.RED);
            Raylib.DrawLine((x + 1) * 256, y * 256, (x + 1) * 256, (y + 1) * 256, Color.RED);
            Raylib.DrawLine(x * 256, (y + 1) * 256, (x + 1) * 256, (y + 1) * 256, Color.RED);
        }

        public void DrawRange(int x, int y, int w, int h)
        {
            for (int i = x; i < x + w; i++)
            {
                for (int j = y; j < y + h; j++)
                {
                    if (chunks.ContainsKey(new VecInt2(i, j)))
                    {
                        WorldRenderer.DrawChunk(chunks[new VecInt2(i, j)]);
                    }
                }
            }
        }
    }
}