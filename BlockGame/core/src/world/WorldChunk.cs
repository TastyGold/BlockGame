using System;
using BlockGame.Rendering;
using BlockGame.Maths;
using BlockGame.Worlds.Generation;

namespace BlockGame.Worlds
{
    public class WorldChunk
    {
        public World world;

        public int chunkPosX;
        public int chunkPosY;
        public VecInt2 ChunkPosVec => new VecInt2(chunkPosX, chunkPosY);
        public Tile[,] tiles = new Tile[16, 16];

        public WorldChunkLightingManager lightManager;
        public WorldChunkBiomeManager biomeManager;
        public WorldChunkRenderer renderer;

        public void PopulateTileArray()
        {
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    tiles[x, y] = new Tile(0, 0);
                }
            }
        }
        public bool[,] GetFgTileMapWithSeam()
        {
            bool[,] map = new bool[18, 18];

            // inner
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    map[x + 1, y + 1] = tiles[x, y].FgTile != 0;
                }
            }

            // edges
            WorldChunk chunk = world.GetChunk(new VecInt2(chunkPosX, chunkPosY - 1), out bool found);
            if (found)
            {
                for (int i = 0; i < 16; i++)
                {
                    map[i + 1, 0] = chunk.tiles[i, 15].FgTile != 0;
                }
            }
            chunk = world.GetChunk(new VecInt2(chunkPosX, chunkPosY + 1), out found);
            if (found)
            {
                for (int i = 0; i < 16; i++)
                {
                    map[i + 1, 17] = chunk.tiles[i, 0].FgTile != 0;
                }
            }
            chunk = world.GetChunk(new VecInt2(chunkPosX + 1, chunkPosY), out found);
            if (found)
            {
                for (int i = 0; i < 16; i++)
                {
                    map[17, i + 1] = chunk.tiles[0, i].FgTile != 0;
                }
            }
            chunk = world.GetChunk(new VecInt2(chunkPosX - 1, chunkPosY), out found);
            if (found)
            {
                for (int i = 0; i < 16; i++)
                {
                    map[0, i + 1] = chunk.tiles[15, i].FgTile != 0;
                }
            }

            // corners
            chunk = world.GetChunk(new VecInt2(chunkPosX - 1, chunkPosY - 1), out found);
            if (found)
            {
                map[0, 0] = chunk.tiles[15, 15].FgTile != 0;
            }
            chunk = world.GetChunk(new VecInt2(chunkPosX + 1, chunkPosY - 1), out found);
            if (found)
            {
                map[17, 0] = chunk.tiles[0, 15].FgTile != 0;
            }
            chunk = world.GetChunk(new VecInt2(chunkPosX - 1, chunkPosY + 1), out found);
            if (found)
            {
                map[0, 17] = chunk.tiles[15, 0].FgTile != 0;
            }
            chunk = world.GetChunk(new VecInt2(chunkPosX + 1, chunkPosY + 1), out found);
            if (found)
            {
                map[17, 17] = chunk.tiles[0, 0].FgTile != 0;
            }

            return map;
        }

        public int GetRegion()
        {
            if (chunkPosX >= 0) return chunkPosX / 32;
            else return (chunkPosX + 1) / 32 - 1;
        }
        public int GetRegionLocalIndex()
        {
            return BlockGameMath.Modulo(chunkPosX, 32) + (chunkPosY * 32);
        }

        public WorldChunk GetAdjacentChunk(int offsetX, int offsetY, out bool exists)
        {
            WorldChunk chunk = world.GetChunk(ChunkPosVec + new VecInt2(offsetX, offsetY), out exists);
            return chunk;
        }

        public static void Print2DBoolArray(bool[,] arr)
        {
            for (int y = 0; y < arr.GetLength(1); y++)
            {
                Console.Write("\n");
                for (int x = 0; x < arr.GetLength(0); x++)
                {
                    Console.Write(arr[x, y] ? "1 " : "0 ");
                }
            }
        }

        public WorldChunk()
        {
            PopulateTileArray();
            lightManager = new WorldChunkLightingManager() { chunk = this };
            biomeManager = new WorldChunkBiomeManager() { chunk = this };
            renderer = new WorldChunkRenderer() { chunk = this } ;
        }
        public WorldChunk(VecInt2 chunkPos)
        {
            chunkPosX = chunkPos.x;
            chunkPosY = chunkPos.y;

            PopulateTileArray();
            lightManager = new WorldChunkLightingManager() { chunk = this };
            biomeManager = new WorldChunkBiomeManager() { chunk = this };
            renderer = new WorldChunkRenderer() { chunk = this };
        }
    }
}