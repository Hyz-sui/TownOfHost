using System.Text.Json.Serialization;

namespace TownOfHost.Modules.Webhook;

public readonly record struct WebhookRequest(
    [property: JsonPropertyName("content")]
    string Content,
    [property: JsonPropertyName("username")]
    string UserName,
    [property: JsonPropertyName("avatar_url")]
    string AvatarUrl);
