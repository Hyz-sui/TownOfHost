using HarmonyLib;

namespace TownOfHost.Patches;

[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
public static class GetBroadcastVersionPatch
{
    public static bool Prefix(ref int __result)
    {
        __result = Constants.GetVersion(2222, 0, 0, 0);
        return false;
    }
}
