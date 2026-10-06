using System.Text.Json;
using System.Text.Json.Serialization;
using Soenneker.Blazor.Rrweb.Replay.Configuration;

namespace Soenneker.Blazor.Rrweb.Replay;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(RrwebReplayOptions))]
[JsonSerializable(typeof(JsonElement[]))]
internal partial class LibraryJsonContext : JsonSerializerContext;
