using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class IResultJsonConverter : JsonConverter<IResult>
{
    public override IResult Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            JsonElement root = doc.RootElement;
            if (!root.TryGetProperty("$type", out JsonElement typeElement))
                throw new JsonException("Missing $type property");

            string typeName = typeElement.GetString();
            Type type = Type.GetType(typeName);
            if (type == null)
                throw new JsonException($"Unknown type: {typeName}");

            return (IResult)JsonSerializer.Deserialize(root.GetRawText(), type, options);
        }
    }

    public override void Write(Utf8JsonWriter writer, IResult value, JsonSerializerOptions options)
    {
        var json = JsonSerializer.Serialize(value, value.GetType(), options);
        using (JsonDocument doc = JsonDocument.Parse(json))
        {
            writer.WriteStartObject();

            foreach (JsonProperty prop in doc.RootElement.EnumerateObject())
            {
                prop.WriteTo(writer);
            }

            writer.WriteString("$type", value.GetType().AssemblyQualifiedName);

            writer.WriteEndObject();
        }
    }
}
