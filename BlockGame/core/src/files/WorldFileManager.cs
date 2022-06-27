using System;
using System.Collections.Generic;
using System.IO;
using BlockGame.Worlds;

namespace BlockGame.Files
{
    public static class WorldFileManager
    {
        public const int ChunkSizeInBytes = 512;

        public static void WriteWorldRegionDataAll(World world)
        {
            Dictionary<int, FileStream> regionFiles = new Dictionary<int, FileStream>();

            foreach (WorldChunk chunk in world.chunks.Values)
            {
                int region = chunk.GetRegion();
                if (!regionFiles.ContainsKey(region))
                {
                    string filePath = $"{FileManager.SavesDir}TestWorld\\data\\region\\r.{region.ToString()}.bin";
                    if (!File.Exists(filePath))
                    {
                        regionFiles.Add(region, File.Create(filePath, 2 + (512 * 32 * 32)));
                    }
                    else 
                    {
                        regionFiles.Add(region, File.OpenWrite(filePath));
                    }

                    byte[] header = GetRegionHeader(region);
                    regionFiles[region].Write(header);
                    Console.WriteLine(((sbyte)header[1]).ToString());
                }
                Console.WriteLine($"ChunkPosX: {chunk.chunkPosX}, RegionLocalIndex: {chunk.GetRegionLocalIndex()}, Region: {region}");
                regionFiles[region].Seek(2 + (ChunkSizeInBytes * chunk.GetRegionLocalIndex()), SeekOrigin.Begin);
                WriteChunkData(regionFiles[region], chunk);
            }

            foreach (FileStream stream in regionFiles.Values)
            {
                stream.Close();
            }
        }

        public static World LoadWorldFromFile(string worldName)
        {
            World world = new World();

            string[] regionPaths = Directory.GetFiles($"{FileManager.SavesDir}{worldName}\\data\\region");

            for (int r = 0; r < regionPaths.Length; r++)
            {
                FileStream regionFile = File.OpenRead(regionPaths[r]);

                byte[] header = new byte[2];
                regionFile.Read(header, 0, 2);

                if (Convert.ToChar(header[0]) != 'r')
                {
                    Console.WriteLine($"Warning: File \"{regionPaths[r]}\" may be corrupt. Unable to load.");
                }
                else
                {
                    int region = Convert.ToInt32((sbyte)header[1]);
                    regionFile.Seek(2, SeekOrigin.Begin);

                    for (int i = 0; i < 1024; i++)
                    {
                        WorldChunk c = ReadChunkData(regionFile);
                        c.chunkPosX = (region * 32) + (i % 32);
                        c.chunkPosY = i >> 5;
                        world.AddChunk(c);
                    }
                }
            }

            return world;
        }

        public static byte[] GetRegionHeader(int region)
        {
            return new byte[2] { Convert.ToByte('r'), (byte)(sbyte)region };
        }

        public static void WriteChunkData(FileStream fs, WorldChunk c)
        {
            byte[] bs = new byte[ChunkSizeInBytes];

            for (int i = 0; i < 256; i++)
            {
                bs[i] = c.tiles[i & 0xf, i >> 4].FgTile;
            }
            for (int i = 0; i < 256; i++)
            {
                bs[i + 256] = c.tiles[i & 0xf, i >> 4].BgTile;
            }

            fs.Write(bs, 0, bs.Length);
        }

        public static WorldChunk ReadChunkData(FileStream fs)
        {
            WorldChunk c = new WorldChunk();

            byte[] bs = new byte[ChunkSizeInBytes];
            fs.Read(bs, 0, ChunkSizeInBytes);

            for (int i = 0; i < 256; i++)
            {
                c.tiles[i & 0xf, i >> 4].FgTile = bs[i];
            }
            for (int i = 0; i < 256; i++)
            {
                c.tiles[i & 0xf, i >> 4].BgTile = bs[i + 256];
            }

            return c;
        }
    }
}