using Godot;
using System.Runtime.CompilerServices;

namespace GameCore.Utility;

public static class GDEx
{
    public static T[] GetEnums<T>() => (T[])Enum.GetValues(typeof(T));

    /// <summary>
    /// If no value is found at key, a new value is added and returned
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="key"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns>The value at the key's location</returns>
    public static TValue GetOrAddNew<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
    {
        if (dict.TryGetValue(key, out TValue? stat))
            return stat;
        dict[key] = new TValue();
        return dict[key];
    }

    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? source)
    {
        return source ?? Array.Empty<T>();
    }

    public static string GetScenePath([CallerFilePath] string csPath = "")
    {
        string godotRoot = Config.GodotRoot;
        if (!csPath.EndsWith(".cs"))
            throw new Exception($"Caller '{csPath}' is not cs file.");
        if (!csPath.StartsWith(godotRoot))
            throw new Exception($"Caller '{csPath}' is outside '{godotRoot}'.");

        string resPath = csPath[godotRoot.Length..];
        resPath = Path.ChangeExtension(resPath, ".tscn");
        resPath = resPath.TrimStart('/', '\\').Replace("\\", "/");
        return $"res://{resPath}";
    }

    public static T Instantiate<T>(string path) where T : GodotObject
    {
        return GD.Load<PackedScene>(path).Instantiate<T>();
    }

    public static Godot.Collections.Array<T> ToGArray<[MustBeVariant] T>(this IEnumerable<T> source)
    {
        return new Godot.Collections.Array<T>(source);
    }

    /// <summary>
    /// Translates with a shortened id if in the editor context.<br/>
    /// The TranslationServer doesn't run in the Godot editor context.
    /// Useful for when translation ids are very long.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="message"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string TrS(this GodotObject obj, string message, StringName? context = null)
    {
        if (Engine.IsEditorHint())
        {
            string[] strArr = message.Split("_");
            strArr = strArr.Select(x => x.Length > 2 ? x[..3] : x).ToArray();
            if (strArr.Length > 2)
                message = strArr[1] + strArr[2];
            else if (strArr.Length == 2)
                message = strArr[0] + strArr[1];
        }
        return obj.Tr(message, context);
    }
}
