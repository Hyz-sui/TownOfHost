using System;
using System.Text;

namespace TownOfHost.Modules.GameEventHistory;

public abstract class Event : IHistoryEvent
{
    public DateTime UtcTime { get; }
    public abstract string Bullet { get; }
    protected Event()
    {
        UtcTime = DateTime.UtcNow;
    }

    public abstract void AppendDiscordString(StringBuilder builder);

    protected void AppendPlayerWithEmoji(StringBuilder builder, EventCommittedPlayer player, bool isAlive)
    {
        builder.Append(Utils.ColorIdToDiscordEmoji(player.ColorId, isAlive));
        builder.Append(" **");
        builder.Append(player.Name);
        builder.Append("**");
    }
}
