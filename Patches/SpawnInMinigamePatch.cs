using HarmonyLib;
using TownOfHost.Roles.Core;
using TownOfHost.Roles.Impostor;

namespace TownOfHost.Patches;

[HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.SpawnAt))]
public static class SpawnInMinigameSpawnAtPatch
{
    public static void Postfix()
    {
        if (AmongUsClient.Instance.AmHost)
        {
            (PlayerControl.LocalPlayer.GetRoleClass() as Penguin)?.OnSpawnAirship();
            PlayerControl.LocalPlayer.RpcResetAbilityCooldown();
            if (Options.FixFirstKillCooldown.GetBool() && !MeetingStates.MeetingCalled)
            {
                PlayerControl.LocalPlayer.SetKillCooldown(Main.AllPlayerKillCooldown[PlayerControl.LocalPlayer.PlayerId]);
            }
            if (Options.RandomSpawn.GetBool())
            {
                new RandomSpawn.AirshipSpawnMap().RandomTeleport(PlayerControl.LocalPlayer);
            }
        }
    }
}
