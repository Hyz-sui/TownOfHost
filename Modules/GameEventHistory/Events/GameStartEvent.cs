using System.Text;

namespace TownOfHost.Modules.GameEventHistory.Events;

public sealed class GameStartEvent : Event
{
    public override string Bullet { get; } = ":green_circle:";

    public override void AppendDiscordString(StringBuilder builder)
    {
        builder.Append("**ゲーム開始**");
    }
}
