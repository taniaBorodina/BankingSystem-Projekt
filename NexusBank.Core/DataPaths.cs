
using System;
using System.IO;

public static class DataPaths
{
    public static string GetPath(string fileName)
    {
        string folder = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "NexusBank",
            "Data"
        );

        Directory.CreateDirectory(folder);

        return Path.Combine(folder, fileName);
    }
}

