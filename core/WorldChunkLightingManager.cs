using System;
using System.Collections.Generic;

namespace BlockGame
{
    public class WorldChunkLightingManager
    {
        public WorldChunk chunk;

        public byte[,] lightLevels = new byte[16, 16];
        public byte[,] skylightLevels = new byte[16, 16];

        public const int fgTileFalloff = 32;
        public const int bgTileFalloff = 16;

        private static readonly int[] neighborX = { 0, 1, 0, -1 };
        private static readonly int[] neighborY = { -1, 0, 1, 0 };

        private static readonly int[] cornerNeighborX = { -1, 1, -1, 1 };
        private static readonly int[] cornerNeighborY = { -1, -1, 1, 1 };

        public List<WorldLight> lights = new List<WorldLight>();

        public void AddLight(float posX, float posY, int luminosity)
        {
            lights.Add(new WorldLight() { positionX = posX, positionY = posY, r = (byte)luminosity });
        }

        public void CalculateLightLevels()
        {
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    for (int i = 0; i < lights.Count; i++)
                    {
                        WorldLight l = lights[i];
                        float v = l.r;
                        v -= (float)Math.Pow(Math.Abs(l.positionX - x), 2);
                        v -= (float)Math.Pow(Math.Abs(l.positionY - y), 2);
                        byte value = (byte)v;
                        lightLevels[x, y] = Math.Max(lightLevels[x, y], value);
                    }
                }
            }
        }

        public void CallNeighborSkylightCalc()
        {
            for (int i = 0; i < 8; i++)
            {
                VecInt2 key = chunk.ChunkPosVec + new VecInt2(i < 4 ? neighborX[i] : cornerNeighborX[i - 4], i < 4 ? neighborY[i] : cornerNeighborY[i - 4]);
                if (chunk.world.chunks.ContainsKey(key))
                {
                    chunk.world.chunks[key].lightManager.CalculateSkylight();
                }
            }
        }

        public void CalculateSkylight()
        {
            bool complete = true;
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    if (chunk.tiles[x, y].FgTile == 0 && chunk.tiles[x, y].BgTile == 0)
                    {
                        complete = false;
                        skylightLevels[x, y] = 255;
                    }
                }
            }

            VecInt2[] chunkOffsets = { VecInt2.Up, VecInt2.Right, VecInt2.Down, VecInt2.Left };

            byte[,] skylightNeighbors = new byte[16, 4]; //1x16 strips that represent the light levels on the 4 chunk seams
            bool[,] skylightNeighborFg = new bool[16, 4]; //same thing but for FG/BG: true = fg, false = bg

            WorldChunk neighbor;
            for (int i = 0; i < 4; i++)
            {
                if (chunk.world.chunks.ContainsKey(chunk.ChunkPosVec + chunkOffsets[i]))
                {
                    neighbor = chunk.world.chunks[chunk.ChunkPosVec + chunkOffsets[i]];
                    for (int j = 0; j < 16; j++)
                    {
                        int x = i switch
                        {
                            0 => j,
                            1 => 0,
                            2 => j,
                            3 => 15,
                            _ => -1,
                        };
                        int y = i switch
                        {
                            0 => 15,
                            1 => j,
                            2 => 0,
                            3 => j,
                            _ => -1,
                        };
                        skylightNeighbors[j, i] = neighbor.lightManager.skylightLevels[x, y];
                        skylightNeighborFg[j, i] = neighbor.tiles[x, y].FgTile != 0;
                        if (skylightNeighbors[j, i] != 0)
                        {
                            complete = false;
                        }
                    }
                }
            }

            while (!complete)
            {
                complete = true;
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        int maxBrightness = skylightLevels[x, y];
                        int newValue;

                        // left
                        if (x > 0) newValue = skylightLevels[x - 1, y] - GetFalloffFrom(x - 1, y);
                        else newValue = skylightNeighbors[y, 3] - (skylightNeighborFg[y, 3] ? fgTileFalloff : bgTileFalloff);
                        if (newValue > maxBrightness) maxBrightness = newValue;

                        // right
                        if (x < 15) newValue = skylightLevels[x + 1, y] - GetFalloffFrom(x + 1, y);
                        else newValue = skylightNeighbors[y, 1] - (skylightNeighborFg[y, 1] ? fgTileFalloff : bgTileFalloff);
                        if (newValue > maxBrightness) maxBrightness = newValue;

                        // up
                        if (y > 0) newValue = skylightLevels[x, y - 1] - GetFalloffFrom(x, y - 1);
                        else newValue = skylightNeighbors[x, 0] - (skylightNeighborFg[x, 0] ? fgTileFalloff : bgTileFalloff);
                        if (newValue > maxBrightness) maxBrightness = newValue;

                        // down
                        if (y < 15) newValue = skylightLevels[x, y + 1] - GetFalloffFrom(x, y + 1);
                        else newValue = skylightNeighbors[x, 2] - (skylightNeighborFg[x, 2] ? fgTileFalloff : bgTileFalloff);
                        if (newValue > maxBrightness) maxBrightness = newValue;

                        if (maxBrightness > skylightLevels[x, y])
                        {
                            complete = false;
                            skylightLevels[x, y] = (byte)maxBrightness;
                        }
                    }
                }
            }
        }

        public int GetFalloffFrom(int x, int y)
        {
            if (x < 0 || x > 15 || y < 0 || y > 15) throw new IndexOutOfRangeException($"Coordinate <{x}, <{y}> is invalid");
            else return chunk.tiles[x, y].FgTile != 0 ? fgTileFalloff : bgTileFalloff;
        }

        public void _CalculateSkylight()
        {
            bool complete = false;
            bool[,] solved = new bool[16, 16];
            bool[,] solvedThisPass = new bool[16, 16];

            while (!complete)
            {
                complete = true;
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        if (!solved[x, y])
                        {
                            if (chunk.tiles[x, y].FgTile == 0 && chunk.tiles[x, y].BgTile == 0)
                            {
                                skylightLevels[x, y] = 255;
                                solved[x, y] = true;
                                complete = false;
                            }
                            else
                            {
                                byte maxLightValue = 0;
                                for (int i = 0; i < 4; i++)
                                {
                                    if (!(x + neighborX[i] < 0 || x + neighborX[i] >= 16 || y + neighborY[i] < 0 || y + neighborY[i] >= 16))
                                    {
                                        if (solved[x + neighborX[i], y + neighborY[i]])
                                        {
                                            solvedThisPass[x, y] = true;
                                            int value = skylightLevels[x + neighborX[i], y + neighborY[i]];
                                            value -= chunk.tiles[x + neighborX[i], y + neighborY[i]].FgTile != 0 ? fgTileFalloff : bgTileFalloff;
                                            if (value > maxLightValue) maxLightValue = (byte)value;
                                            complete = false;
                                        }
                                    }
                                }
                                if (solvedThisPass[x, y] == true)
                                {
                                    skylightLevels[x, y] = maxLightValue;
                                    complete = false;


                                }
                            }
                        }
                    }
                }
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        solved[x, y] = solved[x, y] || solvedThisPass[x, y];
                        solvedThisPass[x, y] = false;
                    }
                }
            }
        }
        public void _FixSkylightSeams(out bool changed)
        {
            bool[,] solved = new bool[16, 16];
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    if (chunk.tiles[x, y].FgTile == 0 && chunk.tiles[x, y].BgTile == 0)
                    {
                        skylightLevels[x, y] = 255;
                        solved[x, y] = true;
                    }
                }

            }
            bool[,] solvedThisPass = new bool[16, 16];
            VecInt2[] chunkOffsets = { VecInt2.Up, VecInt2.Right, VecInt2.Down, VecInt2.Left };

            byte[,] skylightNeighbors = new byte[16, 4]; //1x16 strips that represent the light levels on the 4 chunk seams
            bool[,] skylightNeighborFg = new bool[16, 4]; //same thing but for FG/BG: true = fg, false = bg

            WorldChunk neighbor;
            for (int i = 0; i < 4; i++)
            {
                if (chunk.world.chunks.ContainsKey(chunk.ChunkPosVec + chunkOffsets[i]))
                {
                    neighbor = chunk.world.chunks[chunk.ChunkPosVec + chunkOffsets[i]];
                    for (int j = 0; j < 16; j++)
                    {
                        int x = i switch
                        {
                            0 => j,
                            1 => 0,
                            2 => j,
                            3 => 15,
                            _ => -1,
                        }; 
                        int y = i switch
                        {
                            0 => 15,
                            1 => j,
                            2 => 0,
                            3 => j,
                            _ => -1,
                        };
                        skylightNeighbors[j, i] = neighbor.lightManager.skylightLevels[x, y];
                        skylightNeighborFg[j, i] = neighbor.tiles[x, y].FgTile != 0;
                    }
                }
            }

            // iterate over chunk edges
            changed = false;
            for (int y = 0; y <= 15; y += 15)
            {
                for (int x = 0; x < 16; x++)
                {
                    int newValue = skylightNeighbors[x, y == 0 ? 0 : 2];
                    newValue -= skylightNeighborFg[x, y == 0 ? 0 : 2] ? fgTileFalloff : bgTileFalloff;
                    if (newValue > skylightLevels[x, y])
                    {
                        skylightLevels[x, y] = (byte)newValue;
                        if (newValue != 0) solved[x, y] = true;
                        changed = true;
                    }
                }
            }
            for (int x = 0; x <= 15; x += 15)
            {
                for (int y = 0; y < 16; y++)
                {
                    int newValue = skylightNeighbors[y, x == 0 ? 3 : 1];
                    newValue -= skylightNeighborFg[y, x == 0 ? 3 : 1] ? fgTileFalloff : bgTileFalloff;
                    if (newValue > skylightLevels[x, y])
                    {
                        skylightLevels[x, y] = (byte)newValue;
                        if (newValue != 0) solved[x, y] = true;
                        changed = true;
                    }
                }
            }

            bool complete = false;
            while (!complete)
            {
                complete = true;
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        if (!solved[x, y])
                        {
                            byte maxLightValue = skylightLevels[x, y];
                            for (int i = 0; i < 4; i++)
                            {
                                if (!(x + neighborX[i] < 0 || x + neighborX[i] >= 16 || y + neighborY[i] < 0 || y + neighborY[i] >= 16))
                                {
                                    if (solved[x + neighborX[i], y + neighborY[i]])
                                    {
                                        int value = skylightLevels[x + neighborX[i], y + neighborY[i]];
                                        value -= chunk.tiles[x + neighborX[i], y + neighborY[i]].FgTile != 0 ? fgTileFalloff : bgTileFalloff;
                                        if (value > maxLightValue)
                                        {
                                            maxLightValue = (byte)value;
                                            complete = false;
                                            solvedThisPass[x, y] = true;
                                        }
                                    }
                                }
                            }
                            if (solvedThisPass[x, y])
                            {
                                skylightLevels[x, y] = maxLightValue;
                            }
                        }
                    }
                }
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        solved[x, y] = solved[x, y] || solvedThisPass[x, y];
                        solvedThisPass[x, y] = false;
                    }
                }
            }
        }
        public void _ApplyNeighborSkylight(ref bool[,] solved, ref bool complete, int x, int y)
        {
            byte maxLightValue = 0;
            for (int i = 0; i < 4; i++)
            {
                if (!(x + neighborX[i] < 0 || x + neighborX[i] >= 16 || y + neighborY[i] < 0 || y + neighborY[i] >= 16))
                {
                    if (solved[x + neighborX[i], y + neighborY[i]])
                    {
                        solved[x, y] = true;
                        int value = skylightLevels[x + neighborX[i], y + neighborY[i]];
                        value -= chunk.tiles[x + neighborX[i], y + neighborY[i]].FgTile != 0 ? fgTileFalloff : bgTileFalloff;
                        if (value > maxLightValue) maxLightValue = (byte)value;
                        complete = false;
                    }
                }
            }
            if (solved[x, y] == true)
            {
                skylightLevels[x, y] = maxLightValue;
                complete = false;
            }
        }
    }

    public class WorldLight
    {
        public byte r = 255;
        public byte g = 255;
        public byte b = 255;
        public float positionX;
        public float positionY;
    }
}