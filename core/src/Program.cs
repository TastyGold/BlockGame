using System;
using System.Numerics;
using System.Diagnostics;
using Raylib_cs;
using static BlockGame.BlockGameMath;
using SimplexNoise;
using System.Collections.Generic;

namespace BlockGame
{
    static class Program
    {
        private static int camSpeed = 750;

        private static Texture2D biomes;
        private static Texture2D biomeTexture;
        private static Texture2D heightTexture;
        private static Texture2D ampTexture;
        private static Texture2D circle18x;

        public static void Main()
        {
            BiomeBlockPalette b = WorldBiomes.defaultPalette;
            Console.WriteLine((int)b.GetFgPaletteTile(100));
            Raylib.InitWindow(1600, 900, "Hello World");
            World world = new World();
            WorldBiomeGeneration.LoadValueSheet();
            WorldBiomes.Load();
            Stopwatch s = Stopwatch.StartNew();
            world.GenerateChunks();
            s.Stop();
            Console.WriteLine($"{s.ElapsedMilliseconds}ms");
            Camera2D cam = new Camera2D() { target = new Vector2(0, 256*16), offset = new Vector2(Raylib.GetScreenWidth() / 2, Raylib.GetScreenHeight() / 2), rotation = 0, zoom = 2.5f };
            
            Texture2D map = world.LoadMapTexture();
            bool showMap = false;
            biomes = GenerateBiomeTexture();
            biomeTexture = Raylib.LoadTexture("..//..//..//core//assets//biomeTexture.png");
            heightTexture = Raylib.LoadTexture("..//..//..//core//assets//groundHeightMap.png");
            ampTexture = Raylib.LoadTexture("..//..//..//core//assets//amplitudeMap.png");
            circle18x = Raylib.LoadTexture("..//..//..//core//assets//18x18circle.png");

            WorldChunk target = world.chunks[new VecInt2(0, 9)];
            target.lightManager = new WorldChunkLightingManager();
            target.lightManager.chunk = target;
            target.lightManager.CalculateSkylight();

            Stopwatch stopwatch = Stopwatch.StartNew();

            VecInt2 lastCamChunk = new VecInt2(-1, -1);

            while (!Raylib.WindowShouldClose())
            {
                VecInt2 camChunk = world.GetWorldToChunkPosition(cam.target);

                if (lastCamChunk != camChunk)
                {
                    lastCamChunk = camChunk;
                    Stopwatch st = Stopwatch.StartNew();
                    WorldRenderer.SetLoadedRange(world, camChunk.x - Settings.renderDistanceX, camChunk.y - Settings.renderDistanceY, camChunk.x + Settings.renderDistanceX, camChunk.y + Settings.renderDistanceY);
                    Console.WriteLine(st.ElapsedMilliseconds);
                    st.Stop();
                }

                float c = camSpeed / cam.zoom;
                if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_ALT)) c /= 5;
                if (Raylib.IsKeyDown(KeyboardKey.KEY_A)) cam.target += new Vector2(Raylib.GetFrameTime() * -c, 0);
                if (Raylib.IsKeyDown(KeyboardKey.KEY_D)) cam.target += new Vector2(Raylib.GetFrameTime() * c, 0);
                if (Raylib.IsKeyDown(KeyboardKey.KEY_S)) cam.target += new Vector2(0, Raylib.GetFrameTime() * c);
                if (Raylib.IsKeyDown(KeyboardKey.KEY_W)) cam.target += new Vector2(0, Raylib.GetFrameTime() * -c);
                if (Raylib.GetMouseWheelMove() < 0 && cam.zoom > 0.2f) cam.zoom -= 0.1f;
                if (Raylib.GetMouseWheelMove() > 0 && cam.zoom < 8.5f) cam.zoom += 0.1f;

                Vector2 mouse = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), cam);
                VecInt2 mousePos = new VecInt2((mouse.X / 16).FloorToInt(), (mouse.Y / 16).FloorToInt());
                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
                {
                    if (!Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT)) world.SetTile(mousePos, false, 0);
                    else world.SetTile(mousePos, true, Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_ALT) ? (byte)8 : (byte)0);
                    WorldChunkLightingManager l = world.chunks[new VecInt2(TileToChunkCoord(mousePos.x), TileToChunkCoord(mousePos.y))].lightManager;
                    l.CalculateSkylight();
                    l.CallNeighborSkylightCalc();
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int x = -1; x <= 1; x++)
                        {
                            WorldRenderer.UnloadChunk(world.GetChunk(l.chunk.ChunkPosVec + new VecInt2(x, y), out _));
                            WorldRenderer.LoadChunk(world.GetChunk(l.chunk.ChunkPosVec + new VecInt2(x, y), out _));
                        }
                    }
                }
                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON))
                {
                    WorldChunkLightingManager l = world.chunks[new VecInt2(TileToChunkCoord(mousePos.x), TileToChunkCoord(mousePos.y))].lightManager;
                    Console.WriteLine(l.skylightLevels[Modulo(mousePos.x, 16), Modulo(mousePos.y, 16)]);
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_M))
                {
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL))
                    {
                        Raylib.ExportImage(Raylib.GetTextureData(map), "..//..//..//mapExport.png");
                    }
                    else
                    {
                        if (!showMap)
                        {
                            Raylib.UnloadTexture(map);
                            map = world.LoadMapTexture();
                        }
                        showMap = !showMap;
                    }
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_L))
                {
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL))
                    {
                        Raylib.ExportImage(SmoothLightmapGenerator.GetSmoothLightmap(world.GetChunk(camChunk, out _).lightManager.Get18x18Lightmap()), "..//..//..//chunkLightmapExport.png");
                        Raylib.ExportImage(SmoothLightmapGenerator.GetBasicLightmapImage(world.GetChunk(camChunk, out _).lightManager.Get18x18Lightmap()), "..//..//..//chunkLightmapBasicExport.png");
                    }
                    else
                    {
                        WorldRenderer.enableLighting = !WorldRenderer.enableLighting;
                        World.enableMapLighting = !World.enableMapLighting;
                    }
                }

                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(164, 224, 252, 255));
                Raylib.DrawRectangleGradientV(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), new Color(164, 224, 252, 255), new Color(202, 238, 252, 255));
                Raylib.BeginMode2D(cam);

                //world.DrawRange(camChunk.x - Settings.renderDistanceX, camChunk.y - Settings.renderDistanceY, 1 + (Settings.renderDistanceX * 2), 1 + (Settings.renderDistanceY * 2));
                WorldRenderer.DrawLoadedChunks();
                //world.OutlineChunk(camChunk.x, camChunk.y);
                Raylib.DrawRectangle(mousePos.x * 16, mousePos.y * 16, 16, 16, Color.RED);
                //Raylib.DrawRectangle((cam.target.X / 16).FloorToInt() * 16, 0, 16, 512 * 16, new Color(255, 0, 0, 100));

                //Console.WriteLine((cam.target.X / 16).FloorToInt());
                Raylib.EndMode2D();

                if (showMap) Raylib.DrawTexture(map, 0, 256, Color.WHITE);
                //Raylib.DrawTexture(biomes, 0, 0, Color.WHITE);
                //Raylib.DrawTexture(biomeTexture, map.width, 256, Color.WHITE);
                //Raylib.DrawTexture(heightTexture, map.width + 256, 256, Color.WHITE);
                //Raylib.DrawTexture(ampTexture, map.width + 512, 256, Color.WHITE);
                int texX = (int)Noise.CalcPixel1D((int)stopwatch.ElapsedMilliseconds, 0.0007f);
                int texY = (int)Noise.CalcPixel1D((int)stopwatch.ElapsedMilliseconds + 7549823, 0.0007f);
                //Raylib.DrawRectangle(texX + map.width, texY + 254, 4, 4, Color.BLACK);
                //DrawBiomeValues((cam.target.X / 16).FloorToInt(), biomeTexture);
                //Raylib.DrawTexture(noisemap, 800, 0, Color.WHITE);
                Raylib.DrawFPS(10, 10);
                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }

        public static void DrawBiomeValues(int camX, Texture2D biomeTexture)
        {
            int w = Raylib.GetScreenWidth();
            int h = Raylib.GetScreenHeight();

            Raylib.DrawTexture(biomeTexture, w - 256, h - 512, Color.WHITE);
            int texX = (int)Noise.CalcPixel1D(camX, WorldBiomeGeneration.worldBiomeScale) * 240 / 256 + 8;
            int texY = (int)Noise.CalcPixel1D(camX + 381257, WorldBiomeGeneration.worldBiomeScale) * 240 / 256 + 8;
            Raylib.DrawCircle(w - 256 + texX, h - 512 + texY, 9, Color.DARKGRAY);

            Rectangle srcRec = new Rectangle(texX - 16, texY - 16, 32, 32);
            Rectangle destRec = new Rectangle(w - 256, h - 768, 256, 256);
            Raylib.DrawTexturePro(biomeTexture, srcRec, destRec, Vector2.Zero, 0, Color.WHITE);
            Raylib.DrawTextureEx(circle18x, new Vector2(w - 256 + (7 * 8), h - 768 + (7 * 8)), 0, 8, new Color(255, 255, 255, 100));

            Raylib.DrawRectangle(w - 256, h - 256, 256, 256, Color.DARKGRAY);
            
            for (int i = 0; i < 8; i++)
            {
                int value = WorldBiomeGeneration.GetBiomeWeightsLerped(camX, out _, out _)[i];
                Raylib.DrawRectangle(w - 256 + (32 * i), h - value, 32, value, WorldBiomeGeneration.biomeColors[i]);
                Raylib.DrawText(value.ToString(), w - 250 + (32 * i), h - 15, 10, Color.WHITE);
            }
        }

        public static Texture2D GenerateCaveTexture(float[,] noiseMap)
        {
            int size = noiseMap.GetLength(0);
            Image img = Raylib.GenImageColor(size, size, Color.WHITE);
            for (int y = 0; y < noiseMap.GetLength(1); y++)
            {
                for (int x = 0; x < noiseMap.GetLength(0); x++)
                {
                    int f = 255;
                    const int width = 16;
                    const int centre = 128;
                    if (noiseMap[x, y] > centre - width && noiseMap[x, y] < centre + width)
                    {
                        f = 0;
                    }
                    Raylib.ImageDrawPixel(ref img, x, y, new Color(f, f, f, 255));
                }
            }
            Texture2D tex = Raylib.LoadTextureFromImage(img);
            Raylib.UnloadImage(img);

            return tex;
        }
        public static Texture2D GenerateBiomeTexture()
        {
            Image img = Raylib.GenImageColor(1600, 256, Color.WHITE);
            Noise.Seed = World.seed;
            unsafe
            {
                Image c = Raylib.LoadImage("..//..//..//core//assets//biomeTexture.png");
                Color* colors = (Color*)Raylib.LoadImageColors(c);
                Image gh = Raylib.LoadImage("..//..//..//core//assets//groundHeightMap.png");
                Color* heights = (Color*)Raylib.LoadImageColors(gh);
                Image amp = Raylib.LoadImage("..//..//..//core//assets//amplitudeMap.png");
                Color* amplidutdes = (Color*)Raylib.LoadImageColors(amp);
                for (int x = 0; x < 1600; x++)
                {
                    int texX = (int)Noise.CalcPixel1D(x, WorldBiomeGeneration.worldBiomeScale) * 240 / 256 + 8;
                    int texY = (int)Noise.CalcPixel1D(x + 381257, WorldBiomeGeneration.worldBiomeScale) * 240 / 256 + 8;
                    //BiomeValues b = WorldBiomeGeneration.GetBiomeValues(x);

                    Color col = colors[(texX % 256) + (texY * 256)];
                    Raylib.ImageDrawRectangle(ref img, x, 0, 1, 256, col);
                    int hill = (int)((Math.Sin((float)x / 5) * amplidutdes[(texX % 256) + (texY * 256)].r) / 20);
                    Raylib.ImageDrawPixel(ref img, x, 256 - heights[(texX % 256) + (texY * 256)].r - hill, Color.BLACK);
                }
                Raylib.UnloadImage(c);
                Raylib.UnloadImageColors((IntPtr)colors);
            }
            Texture2D tex = Raylib.LoadTextureFromImage(img);
            Raylib.UnloadImage(img);

            return tex;
        }
    }
}