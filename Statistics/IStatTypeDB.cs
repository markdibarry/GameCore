using Godot;
using GCol = Godot.Collections;

namespace GameCore.Statistics;

public interface IStatTypeDB
{
    string[] GetTypeNames();
    string[]? GetValueEnumOptions(int statType);

    /// <summary>
    /// Gets display properties for editor
    /// </summary>
    /// <param name="statType"></param>
    /// <returns></returns>
    public GCol.Array<GCol.Dictionary> GetStatPropertyList(int statType)
    {
        GCol.Array<GCol.Dictionary> properties = new();
        string[]? valueOptions = GetValueEnumOptions(statType);

        properties.Add(new()
        {
            { "name", "StatType" },
            { "type", (int)Variant.Type.Int },
            { "usage", (int)PropertyUsageFlags.Default },
            { "hint", (int)PropertyHint.Enum },
            { "hint_string", GetTypeNames().Join(",") }
        });
        GCol.Dictionary valueProp = new()
        {
            { "name", "Value" },
            { "type", (int)Variant.Type.Int },
            { "usage", (int)PropertyUsageFlags.Default }
        };
        if (valueOptions != null)
        {
            valueProp.Add("hint", (int)PropertyHint.Enum);
            valueProp.Add("hint_string", valueOptions.Join(","));
        }
        properties.Add(valueProp);

        return properties;
    }
}
