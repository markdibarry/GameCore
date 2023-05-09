using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace GameCore;

public static class Config
{
    static Config()
    {
        string fileText = File.ReadAllText("appsettings.json");
        var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(fileText)!;
        ProjectDirName = settings[nameof(ProjectDirName)];
        PortraitsPath = settings[nameof(PortraitsPath)];
        DialogPath = settings[nameof(DialogPath)];
        AudioPath = settings[nameof(AudioPath)];
        ItemPath = settings[nameof(ItemPath)];
        SavePath = settings[nameof(SavePath)];
        SavePrefix = settings[nameof(SavePrefix)];
    }

    public static string ProjectDirName { get; }
    public static string ProjectPrefix => $"res://{ProjectDirName}";
    public static string PortraitsPath { get; }
    public static string PortraitsFullPath => $"{ProjectPrefix}{PortraitsPath}";
    public static string DialogPath { get; }
    public static string DialogFullPath => $"{ProjectPrefix}{DialogPath}";
    public static string AudioPath { get; }
    public static string AudioFullPath => $"{ProjectPrefix}{AudioPath}";
    public static string ItemPath { get; }
    public static string ItemFullPath => $"{ProjectPrefix}{ItemPath}";
    public static string SavePath { get; }
    public static string SavePrefix { get; }

    public static string GodotRoot => GetGodotRoot();
    private static string GetGodotRoot([CallerFilePath] string rootResourcePath = "")
    {
        int stopIndex = rootResourcePath.LastIndexOf(ProjectDirName) + ProjectDirName.Length;
        return rootResourcePath[..stopIndex];
    }
}
