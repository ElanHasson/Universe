using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Universe.Api.Converters;

public class Vector3JsonConverter : JsonConverter<Vector3>
{
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        float x = 0, y = 0, z = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new Vector3(x, y, z);
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString()!;
                reader.Read();

                switch (propertyName.ToLower())
                {
                    case "x":
                        x = reader.GetSingle();
                        break;
                    case "y":
                        y = reader.GetSingle();
                        break;
                    case "z":
                        z = reader.GetSingle();
                        break;
                }
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        // Handle NaN and Infinity values
        if (float.IsNaN(value.X) || float.IsInfinity(value.X))
            writer.WriteNumber("x", 0);
        else
            writer.WriteNumber("x", value.X);
            
        if (float.IsNaN(value.Y) || float.IsInfinity(value.Y))
            writer.WriteNumber("y", 0);
        else
            writer.WriteNumber("y", value.Y);
            
        if (float.IsNaN(value.Z) || float.IsInfinity(value.Z))
            writer.WriteNumber("z", 0);
        else
            writer.WriteNumber("z", value.Z);
            
        writer.WriteEndObject();
    }
}