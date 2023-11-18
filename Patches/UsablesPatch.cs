using HarmonyLib;
using UnityEngine;

using AmongUs.GameOptions;

using TownOfHost.Modules;

using TownOfHost.Roles.Core;
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
            PlayerControl playerControl = pc.Object;

            // 前半，Mod独自の処理

            // カスタムロールを元にベントを使えるか判定
            // エンジニアベースの役職は常にtrue
            couldUse = playerControl.CanUseImpostorVentButton() || pc.Role.Role == RoleTypes.Engineer;

            canUse = couldUse;
            // カスタムロールが使えなかったら使用不可
            if (!canUse)
            {
                return false;
            }

            // ここまでMod独自の処理
            // ここからバニラ処理の置き換え

            IUsable usableVent = __instance.Cast<IUsable>();
            // ベントとプレイヤーの間の距離
            float actualDistance = float.MaxValue;

            couldUse =
                // クラシックではtrue 多分バニラHnS用
                GameManager.Instance.LogicUsables.CanUse(usableVent, playerControl) &&
                // pc.Role.CanUse(usableVent) &&  バニラロールではなくカスタムロールを元に判定するので無視
                // 対象のベントにベントタスクがない もしくは今自分が対象のベントに入っている
                (!playerControl.MustCleanVent(__instance.Id) || (playerControl.inVent && Vent.currentVent == __instance)) &&
                playerControl.IsAlive() &&
                (playerControl.CanMove || playerControl.inVent);

            // ベント掃除のチェック
            if (ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Ventilation, out var systemType))
            {
                VentilationSystem ventilationSystem = systemType.TryCast<VentilationSystem>();
                // 誰かがベント掃除をしていたらそのベントには入れない
                if (ventilationSystem != null && ventilationSystem.IsVentCurrentlyBeingCleaned(__instance.Id))
                {
                    couldUse = false;
                }
            }

            canUse = couldUse;
            if (canUse)
            {
                Vector3 center = playerControl.Collider.bounds.center;
                Vector3 ventPosition = __instance.transform.position;
                actualDistance = Vector2.Distance(center, ventPosition);
                canUse &= actualDistance <= __instance.UsableDistance && !PhysicsHelpers.AnythingBetween(playerControl.Collider, center, ventPosition, Constants.ShipOnlyMask, false);
            }
            __result = actualDistance;
            return false;
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
            var player = PlayerControl.LocalPlayer;

            //   task_cams: Airship
            //  Surv_Panel: Polus
            // SurvConsole: Skeld
            if (player.IsAlive())
            {
                if (DeviceTimer.CamerasRanOut && systemConsole.name is "task_cams" or "Surv_Panel" or "SurvConsole")
                {
                    return true;
                }
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
