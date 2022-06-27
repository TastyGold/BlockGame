using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RandomAccessFilesTesting
{
    public static class Program
    {
        public static string path = "test.bin";

        static void Main(string[] args)
        {
            FileManager.RunOverwriteTest();
        }
    }

    public static class FileManager
    {
        public const string DataDir = "..\\..\\..\\Files\\";

        public static void RunRegionTest()
        {
            float average = 0;
            for (int i = 0; i < 20; i++)
            {
                Stopwatch s = Stopwatch.StartNew();
                for (int j = 0; j < 100; j++)
                {
                    OverwriteTest(Program.path);
                }
                Console.WriteLine($"Time taken: {s.ElapsedMilliseconds}ms");
                s.Stop();
                average += s.ElapsedMilliseconds * 0.05f;
            }
            Console.WriteLine($"Average: {Math.Round(average)}ms");
        }

        public static async void RunOverwriteTest()
        {
            await TestOverwriteAsync(Program.path);
        }

        public static void TestRegionFile(string filePath)
        {
            FileStream stream = File.Create(DataDir + filePath);
            byte[] bs = new byte[256 * 2 * 32 * 32];
            stream.Write(bs, 0, bs.Length);
            stream.Close();
        }

        public static void OverwriteTest(string filePath)
        {
            FileStream stream = File.OpenWrite(DataDir + filePath);
            stream.Seek(256 * 2 * 197, SeekOrigin.Begin);
            byte[] bs = new byte[256 * 2];
            stream.Write(bs, 0, bs.Length);
            stream.Close();
        }

        public static async Task TestOverwriteAsync(string filePath)
        {
            FileStream stream = File.OpenWrite(DataDir + filePath);

            for (int n = 0; n < 100; n++)
            {
                List<Task> tasks = new List<Task>();
                for (int i = n * 100; i < 100 * (n + 2); i++)
                {
                    tasks.Add(OverwriteTestAsync(stream, i));
                }

                Stopwatch s = Stopwatch.StartNew();
                Console.WriteLine($"Starting {n}");
                await Task.WhenAll(tasks);
                Console.WriteLine($"Time taken: {s.ElapsedMilliseconds}ms ({n})");
            }

            await Task.Delay(100000);
        }

        public static async Task OverwriteTestAsync(FileStream stream, int i)
        {
            stream.Seek(256 * 2 * i, SeekOrigin.Begin);
            byte[] bs = new byte[256 * 2];
            for (int j = 0; j < bs.Length; j++)
            {
                bs[j] = (byte)(i % 256);
            }
            await stream.WriteAsync(bs, 0, bs.Length);
        }

        public static void CreateFile(string filePath)
        {
            FileStream stream = File.Create(DataDir + filePath);

            byte[] bs = Encoding.ASCII.GetBytes("Hello world, this is a test");

            stream.Write(bs, 0, bs.Length);

            stream.Close();
        }

        public static void OpenFile(string filePath)
        {
            FileStream stream = File.OpenRead(DataDir + filePath);

            byte[] bs = new byte[stream.Length];
            stream.Seek(4, SeekOrigin.Begin);
            stream.Read(bs, 0, 5);

            Console.WriteLine(Encoding.ASCII.GetString(bs));

            stream.Close();
        }
    }
}
