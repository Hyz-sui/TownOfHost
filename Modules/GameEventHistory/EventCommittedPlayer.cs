using TownOfHost.Roles.Core;

namespace TownOfHost.Modules.GameEventHistory;

public readonly struct EventCommittedPlayer(string name, byte playerId, int colorId, CustomRoles roleId)
{
    public string Name { get; init; } = name;
    public byte PlayerId { get; init; } = playerId;
    public int ColorId { get; init; } = colorId;
    public CustomRoles RoleId { get; init; } = roleId;

    public EventCommittedPlayer(PlayerControl playerControl) : this(playerControl.GetRealName(), playerControl.PlayerId, playerControl.Data.DefaultOutfit.ColorId, playerControl.GetCustomRole()) { }
}
