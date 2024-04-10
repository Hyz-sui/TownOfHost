using System.Text;

namespace TownOfHost.Modules.GameEventHistory.Events;

public sealed class CrewTaskFinishEvent(EventCommittedPlayer player) : Event
{
    public override string Bullet { get; } = ":blue_circle:";
    public EventCommittedPlayer Player { get; } = player;

    public override void AppendDiscordString(StringBuilder builder)
    {
        builder.Append("**タスク完了:**   ");
        AppendPlayerWithEmoji(builder, Player, true);
    }
}
