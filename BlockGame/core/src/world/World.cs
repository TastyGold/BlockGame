﻿using System;
using System.Collections.Generic;
using BlockGame.Rendering;
using Raylib_cs;
using BlockGame.Maths;
using BlockGame.Worlds.Generation;
using static BlockGame.Maths.BlockGameMath;

namespace BlockGame.Worlds
{
    public class World
    {
        public WorldLightingManager lightingManager;

        public const int worldChunkHeight = 32;
        public static bool enableMapLighting = true;

        public static Random rand = new Random();
        public static int seed;

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
                return null;
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
                Image i = Raylib.LoadImage("..//..//..//core//assets//tilemapcolors.png");
                Color* colors = (Color*)Raylib.LoadImageColors(i);
                Raylib.UnloadImage(i);

                i = Raylib.LoadImage("..//..//..//core//assets//bgtilecolors.png");
                Color* bgcolors = (Color*)Raylib.LoadImageColors(i);
                Raylib.UnloadImage(i);

                //Raylib.ImageDrawRectangle(ref img, 0, WorldGeneration.seaLevel, mapWidth, worldChunkHeight * 16, Color.BLUE);
                foreach (KeyValuePair<VecInt2, WorldChunk> pair in chunks)
                {
                    for (int y = 0; y < 16; y++)
                    {
                        for (int x = 0; x < 16; x++)
                        {
                            bool hasFg = pair.Value.tiles[x, y].FgTile != 0;
                            bool hasBg = pair.Value.tiles[x, y].BgTile != 0;

                            if (hasFg || hasBg)
                            {
                                Color pixelColor = hasFg
                                    ? colors[pair.Value.tiles[x, y].FgTile]
                                    : bgcolors[pair.Value.tiles[x, y].BgTile];

                                if (!hasFg)
                                {
                                    float tint = 0.6f;
                                    pixelColor = new Color((int)(pixelColor.r * tint), (int)(pixelColor.g * tint), (int)(pixelColor.b * tint), 255);
                                }

                                if (!enableMapLighting)
                                {
                                    int light = pair.Value.lightManager.localSkylightLevels[x, y];
                                    int tempR = pixelColor.r * light;
                                    int tempG = pixelColor.g * light;
                                    int tempB = pixelColor.b * light;
                                    tempR /= 255;
                                    tempG /= 255;
                                    tempB /= 255;
                                    pixelColor = new Color(tempR, tempG, tempB, 255);
                                }
                                Raylib.ImageDrawPixel(ref img, x + (pair.Key.x - leftmost) * 16, y + pair.Key.y * 16, pixelColor);
                            }
                            else Raylib.ImageDrawPixel(ref img, x + (pair.Key.x - leftmost) * 16, y + pair.Key.y * 16, Color.SKYBLUE);
                        }
                    }
                }
                Raylib.UnloadImageColors((IntPtr)colors);
                Raylib.UnloadImageColors((IntPtr)bgcolors);
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
                for (int x = -32; x < 32; x++)
                {
                    VecInt2 v = new VecInt2(x, y);
                    WorldChunk c = WorldGeneration.GenerateChunk(v);
                    AddChunk(c);
                }
            }
            //CalculateLighting();
        }

        public void CalculateLighting()
        {
            foreach (WorldChunk chunk in chunks.Values)
            {
                chunk.lightManager.UpdateLightmap();
            }
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

        public World()
        {
            seed = rand.Next(-100000, 100000);
            SimplexNoise.Noise.Seed = seed;
            lightingManager = new WorldLightingManager(this);
        }
    }
}