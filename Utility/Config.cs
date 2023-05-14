using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;

namespace GameCore;

public static class Config
{
    static Config()
    {
        string fileText = File.ReadAllText("appsettings.json");
        Dictionary<string, string> settings = JsonSerializer.Deserialize<Dictionary<string, string>>(fileText)!;
        ProjectName = ProjectSettings.GetSetting("application/config/name").ToString();
        ProjectPath = "res://";
        ProjectFullPath = ProjectSettings.GlobalizePath("res://").Replace("/", "\\");
        SavePath = "user://";
        SaveFullPath = ProjectSettings.GlobalizePath("user://").Replace("/", "\\");
        SaveNamePrefix = settings.GetValueOrDefault(nameof(SaveNamePrefix)) ?? "gamesave";
        PortraitsPath = settings.GetOrEmpty(nameof(PortraitsPath));
        DialogPath = settings.GetOrEmpty(nameof(DialogPath));
        AudioPath = settings.GetOrEmpty(nameof(AudioPath));
        ItemPath = settings.GetOrEmpty(nameof(ItemPath));
    }

    public static string ProjectName { get; }
    public static string ProjectPath { get; }
    public static string ProjectFullPath { get; }
    public static string SavePath { get; }
    public static string SaveNamePrefix { get; }
    public static string SaveFullPath { get; }
    public static string PortraitsPath { get; }
    public static string PortraitsFullPath => $"{ProjectPath}{PortraitsPath}";
    public static string DialogPath { get; }
    public static string DialogFullPath => $"{ProjectPath}{DialogPath}";
    public static string AudioPath { get; }
    public static string AudioFullPath => $"{ProjectPath}{AudioPath}";
    public static string ItemPath { get; }
    public static string ItemFullPath => $"{ProjectPath}{ItemPath}";

    private static string GetOrEmpty(this Dictionary<string, string> dict, string key)
    {
        return dict.TryGetValue(key, out string? value) ? value : string.Empty;
    }
}
