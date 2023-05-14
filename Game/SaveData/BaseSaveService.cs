using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;
using System.Text.Json.Serialization.Metadata;

namespace GameCore;

public class BaseSaveService<T> where T : IGameSave
{
    private static readonly string s_saveFullPath = Config.SaveFullPath;
    private static readonly string s_saveNamePrefix = Config.SaveNamePrefix;
    private static readonly string[] s_ignoredPropertyNames = new string[]
    {
        nameof(GodotObject.NativeInstance),
        nameof(Resource.ResourceName),
        nameof(Resource.ResourcePath),
        nameof(Resource.ResourceLocalToScene)
    };

    private static void IgnoreBaseClass(JsonTypeInfo typeInfo)
    {
        if (!typeInfo.Type.IsAssignableTo(typeof(Resource)))
            return;
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;
        var props = typeInfo.Properties.Where(x => !s_ignoredPropertyNames.Contains(x.Name)).ToList();
        typeInfo.Properties.Clear();
        foreach (JsonPropertyInfo prop in props)
            typeInfo.Properties.Add(prop);
    }

    public static List<(string, T)> GetAllSaves()
    {
        return Directory
            .EnumerateFiles(s_saveFullPath, $"{s_saveNamePrefix}*")
            .Select(x =>
            {
                string fileName = Path.GetFileName(x);
                return (fileName, GetGameSave(fileName));
            })
            .OfType<(string, T)>()
            .OrderBy(x => x.Item2.LastModifiedUtc)
            .ToList();
    }

    public static T? GetGameSave(string fileName)
    {
        string content = File.ReadAllText(s_saveFullPath + fileName);
        return JsonSerializer.Deserialize<T>(content);
    }

    public static void SaveGame(T gameSave, string? fileName = null)
    {
        fileName ??= $"{s_saveNamePrefix}_{DateTime.UtcNow.Ticks}.json";
        JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { IgnoreBaseClass }
            }
        };
        string saveString = JsonSerializer.Serialize(gameSave, options);
        File.WriteAllText(s_saveFullPath + fileName, saveString);
    }
}
