using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class DataManager<T>
{
    private readonly string _filePath;

    public DataManager(string fileName)
    {
        _filePath = fileName;
    }

    public void SaveData(List<T> data)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(data, options);
        File.WriteAllText(_filePath, json);
    }

    public List<T> LoadData()
    {
        if (!File.Exists(_filePath))
        {
            return new List<T>();
        }

        string json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
    }
}