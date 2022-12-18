#nullable enable
using System.Text.Json.Serialization;

namespace Kemono.Core.Models.JsonModel;

public class Embed
{
    [JsonPropertyName("description")]
    public string? Description
    {
        get;
        set;
    } = null;

    [JsonPropertyName("subject")]
    public string? Subject
    {
        get;
        set;
    } = null;

    [JsonPropertyName("url")]
    public string? Url
    {
        get;
        set;
    } = null;
}