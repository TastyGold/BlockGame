using System;
using System.Collections.Generic;
using System.IO;

namespace BlockGame.Files
{
    public static class FileManager
    {
        public static string AppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BlockGame\\";
        public static string SavesDir = AppDataDir + "saves\\";

        public static void CreateWorldFolder(string worldName)
        {
            Directory.CreateDirectory($"{SavesDir}{worldName}\\data\\region");
            Directory.CreateDirectory($"{SavesDir}{worldName}\\data\\entity");
        }
    }
}