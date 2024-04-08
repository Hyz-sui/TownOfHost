using System.Text;

namespace TownOfHost.Modules.GameEventHistory.Events;

public sealed class GameEndEvent(string winsText) : Event
{
    public override string Bullet { get; } = ":green_circle:";
    public string WinsText { get; } = winsText;

    public override void AppendDiscordString(StringBuilder builder)
    {
        builder.Append("**ゲーム終了:**   ");
        builder.Append(WinsText);
    }
}
