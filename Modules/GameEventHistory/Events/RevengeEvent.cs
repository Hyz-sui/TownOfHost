using System.Text;

namespace TownOfHost.Modules.GameEventHistory.Events;

public sealed class RevengeEvent(EventCommittedPlayer cat, EventCommittedPlayer victim) : Event
{
    public override string Bullet { get; } = ":red_circle:";
    public EventCommittedPlayer Cat { get; } = cat;
    public EventCommittedPlayer Victim { get; } = victim;

    public override void AppendDiscordString(StringBuilder builder)
    {
        builder.Append("**道連れ:**   ");
        AppendPlayerWithEmoji(builder, Cat, true);
        builder.Append("   →   ");
        AppendPlayerWithEmoji(builder, Victim, false);
    }
}
