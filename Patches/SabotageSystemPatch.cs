using HarmonyLib;
using Hazel;
using TownOfHost.Attributes;
using TownOfHost.Roles.Core;
using TownOfHost.Roles.Neutral;
using UnityEngine;

namespace TownOfHost
{
    //参考
    //https://github.com/Koke1024/Town-Of-Moss/blob/main/TownOfMoss/Patches/MeltDownBoost.cs

    [HarmonyPatch(typeof(ReactorSystemType), nameof(ReactorSystemType.Deteriorate))]
    public static class ReactorSystemTypePatch
    {
        public static void Prefix(ReactorSystemType __instance)
        {
            if (!__instance.IsActive || !Options.SabotageTimeControl.GetBool())
                return;
            if (ShipStatus.Instance.Type == ShipStatus.MapType.Pb)
            {
                if (__instance.Countdown >= Options.PolusReactorTimeLimit.GetFloat())
                    __instance.Countdown = Options.PolusReactorTimeLimit.GetFloat();
                return;
            }
            return;
        }
    }
    [HarmonyPatch(typeof(HeliSabotageSystem), nameof(HeliSabotageSystem.Deteriorate))]
    public static class HeliSabotageSystemPatch
    {
        public static void Prefix(HeliSabotageSystem __instance)
        {
            if (!__instance.IsActive || !Options.SabotageTimeControl.GetBool())
                return;
            if (AirshipStatus.Instance != null)
                if (__instance.Countdown >= Options.AirshipReactorTimeLimit.GetFloat())
                    __instance.Countdown = Options.AirshipReactorTimeLimit.GetFloat();
        }
    }
    [HarmonyPatch(typeof(HudOverrideSystemType), nameof(HudOverrideSystemType.UpdateSystem))]
    public static class HudOverrideSystemTypeUpdateSystemPatch
    {
        public static bool Prefix([HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] MessageReader msgReader)
        {
            var reader = MessageReader.Get(msgReader);
            var amount = reader.ReadByte();
            var isMadmate =
                player.Is(CustomRoleTypes.Madmate) ||
                // マッド属性化時に削除
                (player.GetRoleClass() is SchrodingerCat schrodingerCat && schrodingerCat.AmMadmate);
            if (isMadmate)
            {
                //直せてしまったらキャンセル
                return !(!Options.MadmateCanFixComms.GetBool() && amount is 0 or 16 or 17);
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(SwitchSystem), nameof(SwitchSystem.UpdateSystem))]
    public static class SwitchSystemRepairDamagePatch
    {
        public static bool Prefix(SwitchSystem __instance, [HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] MessageReader msgReader)
        {
            if (!AmongUsClient.Instance.AmHost)
            {
                return true;
            }

            var reader = MessageReader.Get(msgReader);
            var amount = reader.ReadByte();

            // 停電サボタージュが鳴らされた場合は関係なし(ホスト名義で飛んでくるため誤爆注意)
            if (amount.HasBit(SwitchSystem.DamageSystem))
            {
                return true;
            }

            var isMadmate =
                player.Is(CustomRoleTypes.Madmate) ||
                // マッド属性化時に削除
                (player.GetRoleClass() is SchrodingerCat schrodingerCat && schrodingerCat.AmMadmate);
            if (isMadmate && !Options.MadmateCanFixLightsOut.GetBool())
            {
                return false;
            }

            //Airshipの特定の停電を直せないならキャンセル
            if (Main.NormalOptions.MapId == 4)
            {
                var truePosition = player.GetTruePosition();
                if (Options.DisableAirshipViewingDeckLightsPanel.GetBool() && Vector2.Distance(truePosition, new(-12.93f, -11.28f)) <= 2f) return false;
                if (Options.DisableAirshipGapRoomLightsPanel.GetBool() && Vector2.Distance(truePosition, new(13.92f, 6.43f)) <= 2f) return false;
                if (Options.DisableAirshipCargoLightsPanel.GetBool() && Vector2.Distance(truePosition, new(30.56f, 2.12f)) <= 2f) return false;
            }

            // サボタージュによる破壊ではない && 配電盤を下げられなくするオプションがオン
            if (!amount.HasBit(SwitchSystem.DamageSystem) && Options.BlockDisturbancesToSwitches.GetBool())
            {
                // amount分だけ1を左にずらす
                // 各桁が各ツマミに対応する
                // 一番左のツマミが操作されたら(amount: 0) 00001
                // 一番右のツマミが操作されたら(amount: 4) 10000
                // ref: SwitchSystem.RepairDamage, SwitchMinigame.FixedUpdate
                var switchedKnob = (byte)(0b_00001 << amount);
                // ExpectedSwitches: すべてONになっているときのスイッチの上下状態
                // ActualSwitches: 実際のスイッチの上下状態
                // 操作されたツマミについて，ExpectedとActualで同じならそのツマミは既に直ってる
                if ((__instance.ActualSwitches & switchedKnob) == (__instance.ExpectedSwitches & switchedKnob))
                {
                    return false;
                }
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(ElectricTask), nameof(ElectricTask.Initialize))]
    public static class ElectricTaskInitializePatch
    {
        public static void Postfix()
        {
            Utils.MarkEveryoneDirtySettings();
            if (!GameStates.IsMeeting)
                Utils.NotifyRoles(ForceLoop: true);
        }
    }
    [HarmonyPatch(typeof(ElectricTask), nameof(ElectricTask.Complete))]
    public static class ElectricTaskCompletePatch
    {
        public static void Postfix()
        {
            Utils.MarkEveryoneDirtySettings();
            if (!GameStates.IsMeeting)
                Utils.NotifyRoles(ForceLoop: true);
        }
    }

    // サボタージュを発生させたときに呼び出されるメソッド
    [HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
    public static class SabotageSystemTypeRepairDamagePatch
    {
        private static bool isCooldownModificationEnabled;
        private static float modifiedCooldownSec;

        [GameModuleInitializer]
        public static void Initialize()
        {
            isCooldownModificationEnabled = Options.ModifySabotageCooldown.GetBool();
            modifiedCooldownSec = Options.SabotageCooldown.GetFloat();
        }

        public static bool Prefix(SabotageSystemType __instance, [HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] MessageReader msgReader)
        {

            var newReader = MessageReader.Get(msgReader);
            var amount = newReader.ReadByte();
            var nextSabotage = (SystemTypes)amount;
            Logger.Info("Sabotage" + ", PlayerName: " + player.GetNameWithRole() + ", SabotageType: " + nextSabotage.ToString(), "RepairSystem");
            //HASモードではサボタージュ不可
            if (Options.CurrentGameMode == CustomGameMode.HideAndSeek || Options.IsStandardHAS) return false;
            var roleClass = player.GetRoleClass();
            if (roleClass != null)
            {
                return roleClass.OnInvokeSabotage(nextSabotage);
            }
            else
            {
                return CanSabotage(player, nextSabotage);
            }
        }
        private static bool CanSabotage(PlayerControl player, SystemTypes systemType)
        {
            //サボタージュ出来ないキラー役職はサボタージュ自体をキャンセル
            if (!player.Is(CustomRoleTypes.Impostor))
            {
                return false;
            }
            return true;
        }
        public static void Postfix(SabotageSystemType __instance)
        {
            if (!isCooldownModificationEnabled || !AmongUsClient.Instance.AmHost)
            {
                return;
            }
            __instance.Timer = modifiedCooldownSec;
            __instance.IsDirty = true;
        }
    }

    [HarmonyPatch(typeof(SecurityCameraSystemType), nameof(SecurityCameraSystemType.UpdateSystem))]
    public static class SecurityCameraSystemTypeUpdateSystemPatch
    {
        public static bool Prefix([HarmonyArgument(1)] MessageReader msgReader)
        {
            var newReader = MessageReader.Get(msgReader);
            var amount = newReader.ReadByte();
            // カメラ無効時，バニラプレイヤーはカメラを開けるので点滅させない
            if (amount == SecurityCameraSystemType.IncrementOp)
            {
                var camerasDisabled = (MapNames)Main.NormalOptions.MapId switch
                {
                    MapNames.Skeld => Options.DisableSkeldCamera.GetBool(),
                    MapNames.Polus => Options.DisablePolusCamera.GetBool(),
                    MapNames.Airship => Options.DisableAirshipCamera.GetBool(),
                    _ => false,
                };
                return !camerasDisabled;
            }
            return true;
        }
        public static void Postfix([HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] MessageReader msgReader)
        {
            var newReader = MessageReader.Get(msgReader);
            var amount = newReader.ReadByte();
        }
    }
}