using System.Text.Json.Serialization;

namespace Kemono.Core.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Service
{
    fanbox,
    fantia,
    dlsite,
    subscribestar,
    patreon,
    gumroad,
    discord,
    boosty,
    afdian
}
