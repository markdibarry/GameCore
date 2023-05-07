using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameCore.Statistics;

/// <summary>
/// A custom converter for Dictionary&lt;List&lt;Modifier&gt;&gt;.
/// Removes Modifiers with a Dependent Source Type.
/// </summary>
public class ModifierLookupConverter : JsonConverter<Dictionary<int, List<Modifier>>>
{
    public override Dictionary<int, List<Modifier>>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<Dictionary<int, List<Modifier>>>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<int, List<Modifier>> value, JsonSerializerOptions options)
    {
        Dictionary<int, IEnumerable<Modifier>> newMods = new();
        foreach (var pair in value)
            newMods.Add(pair.Key, pair.Value.Where(x => x.SourceType != SourceType.Dependent));
        JsonSerializer.Serialize(writer, newMods, options);
    }
}
