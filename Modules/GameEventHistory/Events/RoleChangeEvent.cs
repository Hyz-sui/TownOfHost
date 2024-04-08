using System.Text;
using TownOfHost.Roles.Core;

namespace TownOfHost.Modules.GameEventHistory.Events;

public sealed class RoleChangeEvent(EventCommittedPlayer player, CustomRoles to) : Event
{
    public override string Bullet { get; } = ":green_circle:";
    public EventCommittedPlayer Player { get; } = player;
    public CustomRoles To { get; } = to;

    public override void AppendDiscordString(StringBuilder builder)
    {
        builder.Append("**ロール変更:**   ");
        AppendPlayerWithEmoji(builder, Player, true);
        builder.Append("   ");
        builder.Append(Translator.GetRoleString(Player.RoleId.ToString()));
        builder.Append("   →   ");
        builder.Append(Translator.GetRoleString(To.ToString()));
    }
}
