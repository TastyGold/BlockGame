using System;
using System.Collections.Generic;

namespace BlockGame
{
    public class WorldChunkLightingManager
    {
        public WorldChunk chunk;

        public byte[,] lightLevels = new byte[16, 16];
        public byte[,] skylightLevels = new byte[16, 16];

        public const int fgTileFalloff = 36;
        public const int bgTileFalloff = 16;

        private static readonly int[] neighborX = { 0, 1, 0, -1 };
        private static readonly int[] neighborY = { -1, 0, 1, 0 };

        private static readonly int[] cornerNeighborX = { -1, 1, -1, 1 };
        private static readonly int[] cornerNeighborY = { -1, -1, 1, 1 };

        public List<WorldLight> lights = new List<WorldLight>();

        public void AddLight(float posX, float posY, int luminosity)
        {
            lights.Add(new WorldLight() { positionX = posX, positionY = posY, luminosity = (byte)luminosity });
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
                        float v = l.luminosity;
                        v -= (float)Math.Pow(Math.Abs(l.positionX - x), 2);
                        v -= (float)Math.Pow(Math.Abs(l.positionY - y), 2);
                        byte value = (byte)v;
                        lightLevels[x, y] = Math.Max(lightLevels[x, y], value);
                    }
                }
            }
        }

        public void CallSeamCheckOnNeighbors()
        {
            for (int i = 0; i < 8; i++)
            {
                VecInt2 key = chunk.ChunkPosVec + new VecInt2(i < 4 ? neighborX[i] : cornerNeighborX[i - 4], i < 4 ? neighborY[i] : cornerNeighborY[i - 4]);
                if (chunk.world.chunks.ContainsKey(key))
                {
                    chunk.world.chunks[key]?.lightManager.FixSkylightSeams(out _);
                }
            }
        }

        public void CalculateSkylight()
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
        
        public void FixSkylightSeams(out bool changed)
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

        public void ApplyNeighborSkylight(ref bool[,] solved, ref bool complete, int x, int y)
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
        public byte luminosity = 255;
        public float positionX;
        public float positionY;
    }
}