using System;
using System.Collections.Generic;

namespace BlockGame
{
    public class WorldChunkLightingManager
    {
        //Chunk reference
        public WorldChunk chunk;

        //Light value arrays
        //public Byte3[,] localLightLevels = new Byte3[16, 16];
        public byte[,] localSkylightLevels = new byte[16, 16];

        //public BorrowedLight borrowedLight = new BorrowedLight();
        public BorrowedSkylight borrowedSkylight = new BorrowedSkylight();

        //Constants
        public const int fgTileFalloff = 32;
        public const int bgTileFalloff = 16;

        private static readonly int[] neighborX = { 0, 1, 0, -1 };
        private static readonly int[] neighborY = { -1, 0, 1, 0 };

        private static readonly int[] cornerNeighborX = { -1, 1, -1, 1 };
        private static readonly int[] cornerNeighborY = { -1, -1, 1, 1 };

        //Point lights
        public List<WorldLight> lights = new List<WorldLight>();

        public byte[,] Get18x18Lightmap()
        {
            byte[,] map = new byte[18, 18];

            //Corners
            WorldChunk c = chunk.GetAdjacentChunk(-1, -1, out bool exists);
            map[0, 0] = exists ? c.lightManager.localSkylightLevels[15, 15] : localSkylightLevels[0, 0];

            c = chunk.GetAdjacentChunk(+1, -1, out exists);
            map[17, 0] = exists ? c.lightManager.localSkylightLevels[0, 15] : localSkylightLevels[15, 0];

            c = chunk.GetAdjacentChunk(-1, +1, out exists);
            map[0, 17] = exists ? c.lightManager.localSkylightLevels[15, 0] : localSkylightLevels[0, 15];

            c = chunk.GetAdjacentChunk(+1, +1, out exists);
            map[17, 17] = exists ? c.lightManager.localSkylightLevels[0, 0] : localSkylightLevels[15, 15];

            //Edges
            c = chunk.GetAdjacentChunk(0, -1, out exists);
            for (int i = 0; i < 16; i++)
            {
                map[i + 1, 0] = exists ? c.lightManager.localSkylightLevels[i, 15] : localSkylightLevels[i, 0];
            }

            c = chunk.GetAdjacentChunk(0, +1, out exists);
            for (int i = 0; i < 16; i++)
            {
                map[i + 1, 17] = exists ? c.lightManager.localSkylightLevels[i, 0] : localSkylightLevels[i, 15];
            }

            c = chunk.GetAdjacentChunk(-1, 0, out exists);
            for (int i = 0; i < 16; i++)
            {
                map[0, i + 1] = exists ? c.lightManager.localSkylightLevels[15, i] : localSkylightLevels[0, i];
            }

            c = chunk.GetAdjacentChunk(+1, 0, out exists);
            for (int i = 0; i < 16; i++)
            {
                map[17, i + 1] = exists ? c.lightManager.localSkylightLevels[0, i] : localSkylightLevels[15, i];
            }

            //Centre
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    map[x + 1, y + 1] = localSkylightLevels[x, y];
                }
            }

            return map;
        }

        public void DisplayLightmapInConsole()
        {
            string output = "\n     ";
            for (int i = 0; i < 16; i++)
            {
                output += GetTableValueText(borrowedSkylight[0, i]) + " ";
            }
            Console.WriteLine(output + "\n");

            for (int y = 0; y < 16; y++)
            {
                output = GetTableValueText(borrowedSkylight[3, y]) + "  ";

                for (int x = 0; x < 16; x++)
                {
                    output += GetTableValueText(localSkylightLevels[x, y]) + " ";
                }

                output += " " + GetTableValueText(borrowedSkylight[1, y]);

                Console.WriteLine(output);
            }

            output = "\n     ";
            for (int i = 0; i < 16; i++)
            {
                output += GetTableValueText(borrowedSkylight[2, i]) + " ";
            }
            Console.WriteLine(output);
        }
        public string GetTableValueText(byte v)
        {
            string output = v.ToString();
            while (output.Length < 3)
            {
                output = " " + output;
            }
            return output;
        }

        public void CalculateLocalSkylightLevels()
        {
            localSkylightLevels = new byte[16, 16];
            bool complete = true;
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    if (chunk.tiles[x, y].FgTile == 0 && chunk.tiles[x, y].BgTile == 0)
                    {
                        complete = false;
                        localSkylightLevels[x, y] = 255;
                    }
                }
            }

            BlendValueMap(localSkylightLevels, ref complete);
        }
        public void CalculateLocalSkylightLevelsWithBorrow(bool borrowNorth, bool borrowEast, bool borrowSouth, bool borrowWest)
        {
            localSkylightLevels = new byte[16, 16];
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    if (chunk.tiles[x, y].FgTile == 0 && chunk.tiles[x, y].BgTile == 0)
                    {
                        localSkylightLevels[x, y] = 255;
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
                        int maxBrightness = localSkylightLevels[x, y];

                        for (int n = 0; n < 4; n++)
                        {
                            int nx = x + neighborX[n];
                            int ny = y + neighborY[n];

                            int newValue;

                            if (!(nx < 0 || nx > 15 || ny < 0 || ny > 15))
                            {
                                newValue = localSkylightLevels[nx, ny] - GetFalloffFrom(nx, ny);
                            }
                            else if (!(!borrowNorth || n != 0) || !(!borrowEast || n != 1) || !(!borrowSouth || n != 2) || !(!borrowWest || n != 3))
                            {
                                newValue = GetSkylightFromBorrowedWithFalloff(nx, ny);
                            }
                            else newValue = 0;
                                
                            if (newValue > maxBrightness) maxBrightness = newValue;
                        }

                        if (maxBrightness > localSkylightLevels[x, y])
                        {
                            complete = false;
                            localSkylightLevels[x, y] = (byte)maxBrightness;
                        }
                    }
                }
            }
        }
        private void BlendValueMap(byte[,] levels, ref bool complete)
        {
            while (!complete)
            {
                complete = true;
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        int maxBrightness = levels[x, y];

                        for (int n = 0; n < 4; n++)
                        {
                            int nx = x + neighborX[n];
                            int ny = y + neighborY[n];

                            if (!(nx < 0 || nx > 15 || ny < 0 || ny > 15))
                            {
                                int newValue = levels[nx, ny] - GetFalloffFrom(nx, ny);
                                if (newValue > maxBrightness) maxBrightness = newValue;
                            }
                        }

                        if (maxBrightness > levels[x, y])
                        {
                            complete = false;
                            levels[x, y] = (byte)maxBrightness;
                        }
                    }
                }
            }
        }

        public void LendSkylightTo(int side, out bool changed, out WorldChunk neighbor)
        {
            changed = false;
            neighbor = chunk.GetAdjacentChunk(neighborX[side], neighborY[side], out bool exists);

            if (exists)
            {
                for (int i = 0; i < 16; i++)
                {
                    // side | lx | ly | ns
                    // -------------------
                    //    0 |  n |  0 |  2
                    //    1 | 15 |  n |  3
                    //    2 |  n | 15 |  0
                    //    3 |  0 |  n |  1

                    int lx = side % 2 == 0 ? i : (side == 1 ? 15 : 0); //local x
                    int ly = side % 2 == 1 ? i : (side == 0 ? 0 : 15); //local y
                    int ns = (side + 2) % 4; //neighbor side

                    BorrowedSkylight b = neighbor.lightManager.borrowedSkylight;
                    bool hasFg = chunk.tiles[lx, ly].FgTile != 0;
                    bool hasBg = chunk.tiles[lx, ly].BgTile != 0;

                    if (b[ns, i] != localSkylightLevels[lx, ly] ||
                        b.sky[ns, i] != !(hasFg || hasBg) ||
                        b.foreground[ns, i] != hasFg)
                    {
                        changed = true;
                        b[ns, i] = localSkylightLevels[lx, ly];
                        b.sky[ns, i] = !(hasFg || hasBg);
                        b.foreground[ns, i] = hasFg;
                    }
                }
            }
        }
        public void UpdateLightmap()
        {
            bool[] changedDiag = new bool[4];
            WorldChunk[] chunksDiag = new WorldChunk[4];

            BorrowedSkylight borrowedSkylightBackup = borrowedSkylight.GetDeepCopy();

            for (int i = 0; i < 4; i++) //prepare adj borrowing
            {
                WorldChunk adj = chunk.GetAdjacentChunk(neighborX[i], neighborY[i], out bool exists);
                if (exists)
                {
                    adj.lightManager.CalculateLocalSkylightLevelsWithBorrow(false, false, false, false);
                    adj.lightManager.LendSkylightTo((i + 2) % 4, out _, out _);
                }
            }

            CalculateLocalSkylightLevelsWithBorrow(false, false, false, false); //calculate mid without borrow

            for (int i = 0; i < 4; i++) //prepare diag borrowing
            {
                LendSkylightTo(i, out bool changed, out WorldChunk adj);

                if (adj != null)
                {
                    bool vertical = i % 2 == 0;
                    adj.lightManager.CalculateLocalSkylightLevelsWithBorrow(vertical, !vertical, vertical, !vertical);

                    adj.lightManager.LendSkylightTo(RotAnticlockwise(i), out changedDiag[i], out chunksDiag[i]);
                    adj.lightManager.LendSkylightTo(RotClockwise(i), out changedDiag[RotClockwise(i)], out chunksDiag[RotClockwise(i)]);
                }
            }

            for (int i = 0; i < 2; i++) //calculate adj
            {
                CalculateLocalSkylightLevelsWithBorrow(i == 1, i == 0, i == 1, i == 0);

                LendSkylightTo(0 + i, out bool changed, out WorldChunk adj);
                if (adj != null) adj.lightManager.CalculateLocalSkylightLevelsWithBorrow(true, true, true, true);

                LendSkylightTo(2 + i, out changed, out adj);
                if (adj != null) adj.lightManager.CalculateLocalSkylightLevelsWithBorrow(true, true, true, true);
            }

            borrowedSkylight = borrowedSkylightBackup;
            CalculateLocalSkylightLevelsWithBorrow(true, true, true, true); //calculate mid

            for (int i = 0; i < 4; i++) //calculate diag
            {
                if (changedDiag[i])
                {
                    chunksDiag[i].lightManager.CalculateLocalSkylightLevelsWithBorrow(true, true, true, true);
                }
            }
        }
        public void ResetLentSkylight()
        {
            for (int i = 0; i < 4; i++)
            {
                WorldChunk adj = chunk.GetAdjacentChunk(neighborX[i], neighborY[i], out bool exists);
                if (exists)
                {
                    adj.lightManager.ResetBorrowedSkylightSide((i + 2) % 4);

                    int iLeft = i == 0 ? 3 : i - 1;
                    WorldChunk diag = adj.GetAdjacentChunk(neighborX[iLeft], neighborY[iLeft], out exists);
                    if (exists) diag.lightManager.ResetBorrowedSkylightSide((iLeft + 2) % 4);

                    int iRight = i == 3 ? 0 : i + 1;
                    diag = adj.GetAdjacentChunk(neighborX[iRight], neighborY[iRight], out exists);
                    if (exists) diag.lightManager.ResetBorrowedSkylightSide((iRight + 2) % 4);
                }
            }
        }

        public void ResetBorrowedSkylightSide(int side)
        {
            for (int i = 0; i < 16; i++)
            {
                borrowedSkylight[side, i] = 0;
            }
        }

        public int GetFalloffFrom(int x, int y)
        {
            if (x < 0 || x > 15 || y < 0 || y > 15) throw new IndexOutOfRangeException($"Coordinate <{x}, <{y}> is invalid");
            else if (chunk.tiles[x, y].FgTile == 0 && chunk.tiles[x, y].BgTile == 0) return 0;
            else return chunk.tiles[x, y].FgTile != 0 ? fgTileFalloff : bgTileFalloff;
        }
        public int GetFalloffFromNeighbor(WorldChunk neighbor, int nx, int ny)
        {
            if (nx < 0 || nx > 15 || ny < 0 || ny > 15) throw new IndexOutOfRangeException($"Coordinate <{nx}, <{ny}> is invalid");
            else if (neighbor.tiles[nx, ny].FgTile == 0 && chunk.tiles[nx, ny].BgTile == 0) return 0;
            else return neighbor.tiles[nx, ny].FgTile != 0 ? fgTileFalloff : bgTileFalloff;
        }
        public int GetSkylightFromBorrowedWithFalloff(int nx, int ny)
        {
            int side, index;

            if (ny == -1)
            {
                side = 0;
                index = nx;
            }
            else if (nx == 16)
            {
                side = 1;
                index = ny;
            }
            else if (ny == 16)
            {
                side = 2;
                index = nx;
            }
            else if (nx == -1)
            {
                side = 3;
                index = ny;
            }
            else throw new IndexOutOfRangeException();

            return borrowedSkylight[side, index] - (borrowedSkylight.sky[side, index] ? 0 : (borrowedSkylight.foreground[side, index] ? fgTileFalloff : bgTileFalloff));
        }

        public int RotClockwise(int side) => side == 3 ? 0 : side + 1;
        public int RotAnticlockwise(int side) => side == 0 ? 3 : side - 1;

        //deprecated methods
        public void _CallNeighborSkylightCalc()
        {
            for (int i = 0; i < 8; i++)
            {
                VecInt2 key = chunk.ChunkPosVec + new VecInt2(i < 4 ? neighborX[i] : cornerNeighborX[i - 4], i < 4 ? neighborY[i] : cornerNeighborY[i - 4]);
                if (chunk.world.chunks.ContainsKey(key))
                {
                    chunk.world.chunks[key].lightManager._CalculateSkylight();
                }
            }
        }
        public void _CalculateSkylight()
        {
            bool complete = true;
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    if (chunk.tiles[x, y].FgTile == 0 && chunk.tiles[x, y].BgTile == 0)
                    {
                        complete = false;
                        blendedSkylightLevels[x, y] = 255;
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
                        skylightNeighbors[j, i] = neighbor.lightManager.blendedSkylightLevels[x, y];
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
                        int maxBrightness = blendedSkylightLevels[x, y];
                        int newValue;

                        // left
                        if (x > 0) newValue = blendedSkylightLevels[x - 1, y] - GetFalloffFrom(x - 1, y);
                        else newValue = skylightNeighbors[y, 3] - (skylightNeighborFg[y, 3] ? fgTileFalloff : bgTileFalloff);
                        if (newValue > maxBrightness) maxBrightness = newValue;

                        // right
                        if (x < 15) newValue = blendedSkylightLevels[x + 1, y] - GetFalloffFrom(x + 1, y);
                        else newValue = skylightNeighbors[y, 1] - (skylightNeighborFg[y, 1] ? fgTileFalloff : bgTileFalloff);
                        if (newValue > maxBrightness) maxBrightness = newValue;

                        // up
                        if (y > 0) newValue = blendedSkylightLevels[x, y - 1] - GetFalloffFrom(x, y - 1);
                        else newValue = skylightNeighbors[x, 0] - (skylightNeighborFg[x, 0] ? fgTileFalloff : bgTileFalloff);
                        if (newValue > maxBrightness) maxBrightness = newValue;

                        // down
                        if (y < 15) newValue = blendedSkylightLevels[x, y + 1] - GetFalloffFrom(x, y + 1);
                        else newValue = skylightNeighbors[x, 2] - (skylightNeighborFg[x, 2] ? fgTileFalloff : bgTileFalloff);
                        if (newValue > maxBrightness) maxBrightness = newValue;

                        if (maxBrightness > blendedSkylightLevels[x, y])
                        {
                            complete = false;
                            blendedSkylightLevels[x, y] = (byte)maxBrightness;
                        }
                    }
                }
            }
        }
        public Byte3[,] blendedLightLevels = new Byte3[16, 16];
        public byte[,] blendedSkylightLevels = new byte[16, 16];
        public void _BlendSkylightLevelsRecursive(int iteration)
        {
            if (iteration == 0)
            {
                for (int y = 0; y < 16; y++) //copy light levels from local
                {
                    for (int x = 0; x < 16; x++)
                    {
                        blendedSkylightLevels[x, y] = localSkylightLevels[x, y];
                    }
                }
            }
            else blendedSkylightLevels = new byte[16, 16];

            VecInt2[] chunkOffsets = { VecInt2.Up, VecInt2.Right, VecInt2.Down, VecInt2.Left };
            bool[] callNeighbors = new bool[4];

            for (int i = 0; i < 4; i++) //neighbor index
            {
                WorldChunk neighbor = chunk.world.GetChunk(chunk.ChunkPosVec + chunkOffsets[i], out bool found);
                if (found)
                {
                    for (int n = 0; n < 16; n++) //strip index
                    {
                        //Borrows light from adjacent chunks
                        //Only affects the edge tiles of the local chunk

                        //LUT for conditional expressions below
                        //   |  local  | neighbor
                        // i | lx | ly | nx | ny
                        // ---------------------
                        // 0 |  n |  0 |  n | 15
                        // 1 | 15 |  n |  0 |  n
                        // 2 |  n | 15 |  n |  0
                        // 3 |  0 |  n | 15 |  n

                        int lx = i % 2 == 0 ? n : (i == 1 ? 15 : 0);
                        int ly = i % 2 == 1 ? n : (i == 0 ? 0 : 15);
                        int nx = i % 2 == 0 ? n : (i == 1 ? 0 : 15);
                        int ny = i % 2 == 1 ? n : (i == 0 ? 15 : 0);

                        byte[,] neighborMap = iteration == 0 ? neighbor.lightManager.localSkylightLevels : neighbor.lightManager.blendedSkylightLevels;

                        int newValue = neighborMap[nx, ny] - GetFalloffFromNeighbor(neighbor, nx, ny);
                        if (blendedSkylightLevels[lx, ly] < newValue)
                        {
                            blendedSkylightLevels[lx, ly] = (byte)newValue;
                        }
                    }
                }
            }

            bool complete = false;
            BlendValueMap(blendedSkylightLevels, ref complete);
        }
    }

    public enum Side
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3,
    }

    public class Array4x16<T>
    { 
        public T[,] ts = new T[4, 16];

        public Array4x16<T> GetDeepCopy()
        {
            Array4x16<T> arr = new Array4x16<T>();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    arr[i, j] = ts[i, j];
                }
            }
            return arr;
        }

        public T this[int side, int index]
        {
            get { return ts[side, index]; }
            set { ts[side, index] = value; }
        }
    }
    public class BorrowedLight : Array4x16<Byte3>
    {
        public readonly bool[,] foreground = new bool[4, 16];

    }
    public class BorrowedSkylight : Array4x16<byte>
    {
        public readonly bool[,] foreground = new bool[4, 16];
        public readonly bool[,] sky = new bool[4, 16];

        public new BorrowedSkylight GetDeepCopy()
        {
            return new BorrowedSkylight(base.GetDeepCopy().ts, BlockGameMath.Duplicate(foreground), BlockGameMath.Duplicate(sky));
        }

        public BorrowedSkylight() { }

        public BorrowedSkylight(byte[,] ts, bool[,] foreground, bool[,] sky)
        {
            this.ts = ts;
            this.foreground = foreground;
            this.sky = sky;
        }
    }

    public struct Byte3
    {
        public byte r;
        public byte g;
        public byte b;
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