using System.Text;
using TownOfHost.Roles.Core;

namespace TownOfHost.Modules.GameEventHistory.Events;

public sealed class MurderEvent(EventCommittedPlayer killer, EventCommittedPlayer victim, SystemTypes room) : Event
{
    public override string Bullet { get; } = killer.RoleId is CustomRoles.Sheriff ? ":yellow_square:" : ":red_square:";
    public EventCommittedPlayer Killer { get; } = killer;
    public EventCommittedPlayer Victim { get; } = victim;
    public SystemTypes Room { get; } = room;

    public override void AppendDiscordString(StringBuilder builder)
    {
        if (Killer.PlayerId == Victim.PlayerId)
        {
            AppendDiscordSuicide(builder);
        }
        else
        {
            AppendDiscordMurder(builder);
        }
    }
    private void AppendDiscordSuicide(StringBuilder builder)
    {
        builder.Append("**自爆:**   ");
        AppendPlayerWithEmoji(builder, Killer, false);
        builder.Append("   @");
        builder.Append(DestroyableSingleton<TranslationController>.Instance.GetString(Room));
    }
    private void AppendDiscordMurder(StringBuilder builder)
    {
        builder.Append("**キル:**   ");
        AppendPlayerWithEmoji(builder, Killer, true);
        builder.Append("   →   ");
        AppendPlayerWithEmoji(builder, Victim, false);
        builder.Append("   @");
        builder.Append(DestroyableSingleton<TranslationController>.Instance.GetString(Room));
    }
}
