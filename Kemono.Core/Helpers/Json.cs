using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kemono.Core.Models;

namespace Kemono.Core.Helpers;

public static class Json
{
    public static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        Converters =
        {
            new DateTimeOffsetJsonConverter()
        }
    };

    public static async Task<T> ToObjectAsync<T>(string value) =>
        await Task.Run(() => JsonSerializer.Deserialize<T>(value));

    public static T ToObject<T>(string value) => JsonSerializer.Deserialize<T>(value);

    public static async Task<string> StringifyAsync(object value) =>
        await Task.Run(() => JsonSerializer.Serialize(value));

    public static string Stringify(object value) => JsonSerializer.Serialize(value);
}

public class DateTimeOffsetFromNumberJsonConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        DateTimeOffset.FromUnixTimeSeconds((long)reader.GetDouble());

    public override void Write(
        Utf8JsonWriter writer,
        DateTimeOffset dateTimeValue,
        JsonSerializerOptions options) => writer.WriteNumber("", dateTimeValue.ToUnixTimeSecondsAccurateTick());
}

public class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        DateTimeOffset.ParseExact(reader.GetString()!,
            "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.CreateSpecificCulture("en-US"));

    public override void Write(
        Utf8JsonWriter writer,
        DateTimeOffset dateTimeValue,
        JsonSerializerOptions options) =>
        writer.WriteStringValue(dateTimeValue.ToString(
            "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.CreateSpecificCulture("en-US")));
}