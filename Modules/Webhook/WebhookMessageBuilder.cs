using System.Text;

namespace TownOfHost.Modules.Webhook;

public sealed class WebhookMessageBuilder
{
    public StringBuilder ContentBuilder { get; } = new();
    public string UserName { get; init; } = DefaultUserName;
    public string AvatarUrl { get; init; } = DefaultAvatarUrl;

    private const string DefaultUserName = "TownOfHost-H";
    private const string DefaultAvatarUrl = "https://raw.githubusercontent.com/Hyz-sui/TownOfHost-H/images-H/Images/discord-avatar.png";
}
