using System.Text;

namespace TownOfHost.Modules.GameEventHistory.Events;

public sealed class MeetingEndEvent() : Event
{
    public override string Bullet { get; } = ":green_circle:";
    public EventCommittedPlayer? Exiled { get; } = null;

    public MeetingEndEvent(EventCommittedPlayer exiled) : this()
    {
        Exiled = exiled;
    }

    public override void AppendDiscordString(StringBuilder builder)
    {
        builder.Append("**会議結果:**   追放者 ");
        if (Exiled.HasValue)
        {
            AppendPlayerWithEmoji(builder, Exiled.Value, false);
        }
        else
        {
            builder.Append("なし");
        }
    }
}
