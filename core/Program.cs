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

        public static void Main()
        {
            Raylib.InitWindow(1600, 900, "Hello World");
            World world = new World();
            Stopwatch s = Stopwatch.StartNew();
            world.GenerateChunks();
            s.Stop();
            Console.WriteLine($"{s.ElapsedMilliseconds}ms");
            Console.WriteLine(WorldGeneration.GetWorldHeight(0));
            Camera2D cam = new Camera2D() { target = new Vector2(0, 256*10), offset = new Vector2(Raylib.GetScreenWidth() / 2, Raylib.GetScreenHeight() / 2), rotation = 0, zoom = 2.5f };
            
            Texture2D map = world.LoadMapTexture();
            bool showMap = false;
            Texture2D biomes = GenerateBiomeTexture();
            Texture2D biomeTexture = Raylib.LoadTexture("..//..//..//core//assets//biomeTexture.png");
            Texture2D heightTexture = Raylib.LoadTexture("..//..//..//core//assets//groundHeightMap.png");
            Texture2D ampTexture = Raylib.LoadTexture("..//..//..//core//assets//amplitudeMap.png");

            WorldChunk target = world.chunks[new VecInt2(0, 9)];
            target.lightManager = new WorldChunkLightingManager();
            target.lightManager.chunk = target;
            target.lightManager.CalculateSkylight();

            Stopwatch stopwatch = Stopwatch.StartNew();

            while (!Raylib.WindowShouldClose())
            {
                float c = camSpeed / cam.zoom;
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
                }
                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON))
                {
                    WorldChunkLightingManager l = world.chunks[new VecInt2(TileToChunkCoord(mousePos.x), TileToChunkCoord(mousePos.y))].lightManager;
                    Console.WriteLine(l.skylightLevels[Modulo(mousePos.x, 16), Modulo(mousePos.y, 16)]);
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_M))
                {
                    if (!showMap)
                    {
                        Raylib.UnloadTexture(map);
                        map = world.LoadMapTexture();
                    }
                    showMap = !showMap;
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_L))
                {
                    WorldRenderer.enableLighting = !WorldRenderer.enableLighting;
                }

                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(164, 224, 252, 255));
                Raylib.DrawRectangleGradientV(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), new Color(164, 224, 252, 255), new Color(202, 238, 252, 255));
                Raylib.BeginMode2D(cam);
                //Raylib.DrawRectangle(-100000, WorldGeneration.seaLevel * 16, 1000000, 100000, Color.BLUE);

                VecInt2 camChunk = world.GetWorldToChunkPosition(cam.target);
                //Raylib.DrawTextureTiled(WorldRenderer.tileTexture, new Rectangle(16*7, 0, 16, 16), new Rectangle(camChunk.x * 256, camChunk.y * 256, 256, 256), Vector2.Zero, 0, 1, Color.LIGHTGRAY);
                world.DrawRange(camChunk.x - Settings.renderDistanceX, camChunk.y - Settings.renderDistanceY, 1 + (Settings.renderDistanceX * 2), 1 + (Settings.renderDistanceY * 2));
                //world.OutlineChunk(0, 9);
                //world.OutlineChunk(camChunk.x, camChunk.y);
                //WorldAmbientOcclusion.DrawAmbientOcclusion(camChunk, world.GetChunk(camChunk, out _).GetFgTileMapWithSeam());
                Raylib.DrawRectangle(mousePos.x * 16, mousePos.y * 16, 16, 16, Color.RED);

                Raylib.EndMode2D();

                if (showMap) Raylib.DrawTexture(map, 0, 256, Color.WHITE);
                Raylib.DrawTexture(biomes, 0, 0, Color.WHITE);
                Raylib.DrawTexture(biomeTexture, map.width, 256, Color.WHITE);
                Raylib.DrawTexture(heightTexture, map.width + 256, 256, Color.WHITE);
                Raylib.DrawTexture(ampTexture, map.width + 512, 256, Color.WHITE);
                int texX = (int)Noise.CalcPixel1D((int)stopwatch.ElapsedMilliseconds, 0.0007f);
                int texY = (int)Noise.CalcPixel1D((int)stopwatch.ElapsedMilliseconds + 7549823, 0.0007f);
                Raylib.DrawRectangle(texX + map.width, texY + 254, 4, 4, Color.BLACK);
                //Raylib.DrawTexture(noisemap, 800, 0, Color.WHITE);
                Raylib.DrawFPS(10, 10);
                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
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
                    int texX = (int)Noise.CalcPixel1D(x, 0.008f) * 240 / 256 + 8;
                    int texY = (int)Noise.CalcPixel1D(x + 381257, 0.008f) * 240 / 256 + 8;
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

    public static class Settings
    {
        public static int renderDistanceX = 3;
        public static int renderDistanceY = 2;
    }
}