using System.Text;

namespace TownOfHost.Modules.GameEventHistory.Events;

public sealed class MeetingCallEvent(EventCommittedPlayer reporter) : Event
{
    public override string Bullet { get; } = ":green_circle:";
    public EventCommittedPlayer Reporter { get; } = reporter;
    public EventCommittedPlayer? Dead { get; } = null;

    public MeetingCallEvent(EventCommittedPlayer reporter, EventCommittedPlayer dead) : this(reporter)
    {
        Dead = dead;
    }

    public override void AppendDiscordString(StringBuilder builder)
    {
        if (Dead.HasValue)
        {
            AppendDiscordReport(builder);
        }
        else
        {
            AppendDiscordEmergency(builder);
        }
    }
    private void AppendDiscordReport(StringBuilder builder)
    {
        builder.Append("**通報:**   ");
        AppendPlayerWithEmoji(builder, Reporter, true);
        builder.Append("   →   ");
        AppendPlayerWithEmoji(builder, Dead.Value, false);
    }
    private void AppendDiscordEmergency(StringBuilder builder)
    {
        builder.Append("**緊急会議:**   ");
        AppendPlayerWithEmoji(builder, Reporter, true);
    }
}
