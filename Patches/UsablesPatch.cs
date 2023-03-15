using HarmonyLib;
using UnityEngine;

using AmongUs.GameOptions;

using TownOfHost.Modules;

namespace TownOfHost
{
    [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
    class CanUsePatch
    {
        public static bool Prefix(ref float __result, Console __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            canUse = couldUse = false;
            //こいつをfalseでreturnしても、タスク(サボ含む)以外の使用可能な物は使えるまま(ボタンなど)
            return __instance.AllowImpostor || Utils.HasTasks(PlayerControl.LocalPlayer.Data, false);
        }
    }
    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
    class EmergencyMinigamePatch
    {
        public static void Postfix(EmergencyMinigame __instance)
        {
            if (Options.CurrentGameMode == CustomGameMode.HideAndSeek) __instance.Close();
        }
    }
    [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
    class CanUseVentPatch
    {
        public static bool Prefix(Vent __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc,
            [HarmonyArgument(1)] ref bool canUse,
            [HarmonyArgument(2)] ref bool couldUse,
            ref float __result)
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //#######################################
            //     ==ベント処理==
            //#######################################
            //参考:https://github.com/Eisbison/TheOtherRoles/blob/main/TheOtherRoles/Patches/UsablesPatch.cs

            bool VentForTrigger = false;
            float num = float.MaxValue;

            var usableDistance = __instance.UsableDistance;

            if (pc.IsDead) return false; //死んでる人は強制的にfalseに。

            canUse = couldUse = pc.Object.CanUseImpostorVentButton();
            switch (pc.GetCustomRole())
            {
                case CustomRoles.Arsonist:
                    if (pc.Object.IsDouseDone())
                        VentForTrigger = true;
                    break;
                default:
                    if (pc.Role.Role == RoleTypes.Engineer) // インポスター陣営ベースの役職とエンジニアベースの役職は常にtrue
                        canUse = couldUse = true;
                    break;
            }
            if (!canUse) return false;

            canUse = couldUse = (pc.Object.inVent || canUse) && (pc.Object.CanMove || pc.Object.inVent);

            if (VentForTrigger && pc.Object.inVent)
            {
                canUse = couldUse = false;
                return false;
            }
            if (canUse)
            {
                Vector2 truePosition = pc.Object.GetTruePosition();
                Vector3 position = __instance.transform.position;
                num = Vector2.Distance(truePosition, position);
                canUse &= num <= usableDistance && !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipOnlyMask, false);
            }
            __result = num;
            return false;
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
    }

    [HarmonyPatch]
    public static class BlockUseConsoles
    {
        private static bool IsUnusable(IUsable target)
        {
            // targetがConsole(タスク系，縁取りが黄色)
            if (target.TryCast<Console>() is Console console)
            {
                return IsUnusable(console);
            }

            // targetがSystemConsole(それ以外，縁取りが白)
            if (target.TryCast<SystemConsole>() is SystemConsole systemConsole)
            {
                return IsUnusable(systemConsole);
            }

            return false;
        }
        private static bool IsUnusable(Console console)
        {
            var player = PlayerControl.LocalPlayer;

            if (player.Is(CustomRoleTypes.Madmate))
            {
                if (!Options.ModdedMadmateCantOpenSabConsoles.GetBool())
                {
                    return false;
                }

                var taskType = console.FindTask(player)?.TaskType;
                if (
                    (taskType == TaskTypes.FixLights && !Options.MadmateCanFixLightsOut.GetBool()) ||
                    (taskType == TaskTypes.FixComms && !Options.MadmateCanFixComms.GetBool()))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool IsUnusable(SystemConsole systemConsole)
        {
            //   task_cams: Airship
            //  Surv_Panel: Polus
            // SurvConsole: Skeld
            if (DeviceTimer.CamerasRanOut && systemConsole.name is "task_cams" or "Surv_Panel" or "SurvConsole")
            {
                return true;
            }
            return false;
        }

        [HarmonyPatch(typeof(UseButton), nameof(UseButton.SetTarget))]
        public static class UseButtonSetTargetPatch
        {
            public static bool Prefix(UseButton __instance, [HarmonyArgument(0)] IUsable target)
            {
                if (!GameStates.IsInTask || target == null)
                {
                    return true;
                }

                if (IsUnusable(target))
                {
                    __instance.SetDisabled();
                    __instance.SetTarget(null);
                    __instance.OverrideText("使用不可");
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Console), nameof(Console.Use))]
        public static class ConsoleUsePatch
        {
            public static bool Prefix(Console __instance)
            {
                if (IsUnusable(__instance))
                {
                    return false;
                }

                return true;
            }
        }
        [HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.Use))]
        public static class SystemConsoleUsePatch
        {
            public static bool Prefix(SystemConsole __instance)
            {
                if (IsUnusable(__instance))
                {
                    return false;
                }

                return true;
            }
        }
    }
}
