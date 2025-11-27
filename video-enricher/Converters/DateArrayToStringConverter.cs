using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VideoEnricher.Converters
{
    public class DateArrayToStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString() ?? string.Empty;
            }

            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read(); // Move to first element (Year)
                int year = reader.GetInt32();
                
                reader.Read(); // Move to second element (Month)
                int month = reader.GetInt32();
                
                reader.Read(); // Move to third element (Day)
                int day = reader.GetInt32();
                
                reader.Read(); // EndArray

                return new DateTime(year, month, day).ToString("yyyy-MM-dd");
            }

            throw new JsonException($"Unexpected token type {reader.TokenType} for DateArrayToStringConverter.");
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
