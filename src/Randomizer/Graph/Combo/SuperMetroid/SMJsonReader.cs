namespace Randomizer.Graph.Combo.SuperMetroid;

using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Text.Json;
using Combo.SuperMetroid.Model;

internal class SMJsonReader
{
    private List<Room> _rooms = new();

    private List<T> LoadFiles<T>(string path)
    {
        var jsonData = new List<T>();
        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();

        foreach (var fileName in Directory.GetFiles(path, "*.json", SearchOption.AllDirectories))
        {
            if (fileName.Contains("roomDiagram"))
            {
                continue;
            }

            var fileContents = File.ReadAllText(fileName);
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                var data = JsonSerializer.Deserialize<T>(fileContents, options);
                if (data != null)
                {
                    jsonData.Add(data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while deserializing: {fileName}, {e.Message} ({e.InnerException?.Source ?? ""}");
            }
        }

        return jsonData;
    }

    private T? LoadFile<T>(string path)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var data = JsonSerializer.Deserialize<T>(File.ReadAllText(path), options);
            return data ?? default;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while deserializing: {path}, {e.Message} ({e.InnerException?.Source ?? ""}");
            return default;
        }
    }

    // Load all room data into a rooms list from json files
    public void Load()
    {
        var path = Path.Combine(YamlReader.DataRoot, "../Combo/Data/SuperMetroid/sm-json-data/");
        var rooms = LoadFiles<Room>(Path.Combine(path, "region"));
        var connections = LoadFiles<ConnectionCollection>(Path.Combine(path, "connection"));
        var techs = LoadFile<TechCollection>(Path.Combine(path, "tech.json"));
    }
}
