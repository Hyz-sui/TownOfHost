using System.Text;

namespace TownOfHost.Modules.GameEventHistory.Events;

public sealed class GameEndEvent() : Event
{
    public override string Bullet { get; } = ":green_circle:";

    public override void AppendDiscordString(StringBuilder builder)
    {
        builder.Append("**ゲーム終了:**   ");
        builder.Append(SetEverythingUpPatch.LastWinsText);
    }
}
