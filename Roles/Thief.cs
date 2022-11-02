using System.Collections.Generic;
using UnityEngine;

namespace TownOfHost
{
    public static class Thief
    {
        public static readonly int Id = 51000;
        public static List<byte> playerIdList = new();

        public static CustomOption ThiefCooldown;
        public static CustomOption ThiefHasImpostorVision;
        public static CustomOption ThiefCanVent;

        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Thief);
            ThiefCooldown = CustomOption.Create(Id + 10, TabGroup.NeutralRoles, Color.white, "ThiefCooldown", 30f, 2.5f, 180f, 2.5f, Options.CustomRoleSpawnChances[CustomRoles.Thief]);
            ThiefHasImpostorVision = CustomOption.Create(Id + 11, TabGroup.NeutralRoles, Color.white, "ThiefHasImpostorVision", false, Options.CustomRoleSpawnChances[CustomRoles.Thief]);
            ThiefCanVent = CustomOption.Create(Id + 12, TabGroup.NeutralRoles, Color.white, "ThiefCanVent", true, Options.CustomRoleSpawnChances[CustomRoles.Thief]);
        }
        public static void Init()
        {
            playerIdList = new();
        }
        public static void Add(byte playerId)
        {
            playerIdList.Add(playerId);
            if (!Main.ResetCamPlayerList.Contains(playerId))
            {
                Main.ResetCamPlayerList.Add(playerId);
            }
            if (playerId == 0 && PlayerControl.LocalPlayer.PlayerId == 0)
            {
                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                {
                    pc.Data.Role.CanBeKilled = true;
                }
            }
        }
        public static bool IsEnable() => playerIdList.Count > 0;

        ///<summary>戻り値はスチールロールの成否です 成功した場合はtrueになります</summary>
        public static bool TrySteal(PlayerControl thief, PlayerControl target)
        {
            var targetRole = target.GetCustomRole();
            var succeeded = targetRole.IsImpostor() || (targetRole is
                CustomRoles.Sheriff or
                CustomRoles.Egoist);
            // 相手がキル役職でなければ自爆
            if (!succeeded)
            {
                Logger.Info($"{target.GetNameWithRole()}はスチールロールできない役職でした", "Thief");
                PlayerState.SetDeathReason(thief.PlayerId, PlayerState.DeathReason.Misfire);
                thief.RpcMurderPlayer(thief);
            }
            // 以下スチールロール処理
            else
            {
                Logger.Info($"{thief.Data.PlayerName}のロールを{targetRole}に変更", "Thief");
                thief.RpcSetCustomRole(targetRole);
                switch (targetRole)
                {
                    case CustomRoles.BountyHunter:
                        BountyHunter.Add(thief.PlayerId);
                        break;
                    case CustomRoles.FireWorks:
                        FireWorks.Add(thief.PlayerId);
                        break;
                    case CustomRoles.Sniper:
                        Sniper.Add(thief.PlayerId);
                        break;
                    case CustomRoles.TimeThief:
                        TimeThief.Add(thief.PlayerId);
                        break;
                    case CustomRoles.EvilTracker:
                        EvilTracker.Add(thief.PlayerId);
                        break;
                    case CustomRoles.Sheriff:
                        Sheriff.Add(thief.PlayerId);
                        break;
                    case CustomRoles.Egoist:
                        Egoist.Add(thief.PlayerId);
                        break;
                    default:
                        break;
                }
                Utils.NotifyRoles();
                thief.CustomSyncSettings();
                // ホストはこれをしないとサボボタンなどが出てこないor押せない
                if (thief.PlayerId == 0)
                {
                    DestroyableSingleton<HudManager>.Instance.SetHudActive(true);
                    if (targetRole.IsImpostor() || targetRole == CustomRoles.Egoist)
                    {
                        thief.Data.Role.TeamType = RoleTeamTypes.Impostor;
                        thief.Data.Role.CanVent = true;
                    }
                }
            }
            return succeeded;
        }
        ///<summary>純粋なインポスターと元シーフのインポスターは互いにキルできてしまうため，その判定をします．
        ///キル者が元シーフでターゲットがインポスターの場合とキル者がインポスターでターゲットが元シーフの場合にfalseを返します</summary>
        public static bool CanKill(PlayerControl killer, PlayerControl target) =>  // キル者は確定でインポスター(=シーフではない)
            !(killer == target ||
            (playerIdList.Contains(killer.PlayerId) && target.GetCustomRole().IsImpostor()) ||
            (playerIdList.Contains(target.PlayerId) && !target.Is(CustomRoles.Thief)));
        public static void ApplyGameOptions(GameOptionsData opt, byte playerId)
        {
            var pc = Utils.GetPlayerById(playerId);
            opt.RoleOptions.ShapeshifterCooldown = 255f;
            opt.RoleOptions.ShapeshifterDuration = 1f;
            opt.SetVision(pc, ThiefHasImpostorVision.GetBool());
        }
    }
}
