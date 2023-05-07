using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameCore.Statistics;

public class ConditionConverter : JsonConverter<Condition>
{
    public override Condition? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Utf8JsonReader readerClone = reader;
        int val = 0;
        while (readerClone.Read())
        {
            if (readerClone.TokenType == JsonTokenType.PropertyName && readerClone.GetString() == nameof(Condition.ConditionType))
            {
                readerClone.Read();
                val = readerClone.GetInt32();
                break;
            }
        }
        Type conditionType = StatsLocator.ConditionLookup.GetConditionType(val);
        return JsonSerializer.Deserialize(ref reader, conditionType) as Condition;
    }

    public override void Write(Utf8JsonWriter writer, Condition value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
