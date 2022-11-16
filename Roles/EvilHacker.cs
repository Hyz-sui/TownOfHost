using System.Collections.Generic;
using System.Linq;
using static TownOfHost.Options;
using static TownOfHost.Translator;
using UnityEngine;

namespace TownOfHost
{
    public static class EvilHacker
    {
        public static readonly int Id = 3100;
        public static List<byte> playerIdList = new();

        public static CustomOption CanSeeDeadPos;
        public static CustomOption CanSeeOtherImp;
        public static CustomOption CanSeeKillFlash;
        public static CustomOption CanSeeImpArrow;

        public static Dictionary<SystemTypes, int> PlayerCount = new();
        public static Dictionary<SystemTypes, int> DeadCount = new();
        public static List<SystemTypes> ImpRooms = new();

        public static void SetupCustomOption()
        {
            SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.EvilHacker);
            CanSeeDeadPos = CustomOption.Create(Id + 10, TabGroup.ImpostorRoles, Color.white, "CanSeeDeadPos", true, CustomRoleSpawnChances[CustomRoles.EvilHacker]);
            CanSeeOtherImp = CustomOption.Create(Id + 11, TabGroup.ImpostorRoles, Color.white, "CanSeeOtherImp", true, CustomRoleSpawnChances[CustomRoles.EvilHacker]);
            CanSeeKillFlash = CustomOption.Create(Id + 12, TabGroup.ImpostorRoles, Color.white, "CanSeeKillFlash", true, CustomRoleSpawnChances[CustomRoles.EvilHacker]);
            CanSeeImpArrow = CustomOption.Create(Id + 13, TabGroup.ImpostorRoles, Color.white, "CanSeeImpArrow", true, CustomRoleSpawnChances[CustomRoles.EvilHacker]);
        }
        public static void Init()
        {
            playerIdList = new();
            PlayerCount = new();
            DeadCount = new();
            ImpRooms = new();
        }
        public static void Add(byte playerId)
        {
            playerIdList.Add(playerId);
        }
        public static bool IsEnable() => playerIdList.Count > 0;

        // ShipStatus.Instanceがないときに呼んじゃだめ
        public static void InitDeadCount() => ShipStatus.Instance.AllRooms.ToList().ForEach(room => DeadCount[room.RoomId] = 0);
        public static void OnStartMeeting()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            ShipStatus.Instance.AllRooms.ToList().ForEach(room => PlayerCount[room.RoomId] = 0);
            foreach (var room in ShipStatus.Instance.AllRooms)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!pc.IsAlive()) continue;
                    if (pc.Collider.IsTouching(room.roomArea))
                    {
                        PlayerCount[room.RoomId]++;
                        if (CanSeeOtherImp.GetBool() && pc.GetCustomRole().IsImpostor() && !ImpRooms.Contains(room.RoomId))
                            ImpRooms.Add(room.RoomId);
                    }
                }
            }
            PlayerCount.Remove(SystemTypes.Hallway);
            DeadCount.Remove(SystemTypes.Hallway);
            var alivePlayerIds = playerIdList.Where(x => Utils.GetPlayerById(x).IsAlive()).ToList();
            string message = $"=={GetString("Message.LastAdminInfo")}==\n";
            foreach (var kvp in PlayerCount)
            {
                if (ImpRooms.Contains(kvp.Key)) message += '★';
                var roomName = DestroyableSingleton<TranslationController>.Instance.GetString(kvp.Key);
                if (CanSeeDeadPos.GetBool())
                {
                    message = $"{message}{roomName}: {kvp.Value + DeadCount[kvp.Key]}";
                    message += DeadCount[kvp.Key] > 0 ? $"({GetString("Deadbody")}\u00d7{DeadCount[kvp.Key]})\n" : '\n';
                }
                else
                {
                    message = $"{message}{roomName}: {kvp.Value + DeadCount[kvp.Key]}\n";
                }
            }
            alivePlayerIds.ForEach(id => Utils.SendMessage(message, id));
            InitDeadCount();
            ImpRooms = new();
        }
        public static void OnMurder(PlayerControl target)
        {
            foreach (var room in ShipStatus.Instance.AllRooms)
            {
                if (target.Collider.IsTouching(room.roomArea))
                {
                    DeadCount[room.RoomId]++;
                    if (CanSeeOtherImp.GetBool() && target.GetCustomRole().IsImpostor() && !ImpRooms.Contains(room.RoomId))
                        ImpRooms.Add(room.RoomId);
                }
            }
        }
        public static bool KillFlashCheck(PlayerControl killer, PlayerState.DeathReason deathReason)
            => CanSeeKillFlash.GetBool() && Utils.IsImpostorKill(killer, deathReason);
        public static string UtilsGetTargetArrow(bool isMeeting, PlayerControl seer)
        {
            //ミーティング以外では矢印表示
            if (isMeeting || !CanSeeImpArrow.GetBool()) return "";
            string SelfSuffix = "";
            foreach (var arrow in Main.targetArrows)
            {
                var target = Utils.GetPlayerById(arrow.Key.Item2);
                if (arrow.Key.Item1 == seer.PlayerId && !PlayerState.isDead[arrow.Key.Item2] && target.GetCustomRole().IsImpostor())
                    SelfSuffix += arrow.Value;
            }
            return SelfSuffix;
        }
        public static string PCGetTargetArrow(PlayerControl seer, PlayerControl target)
        {
            if (!CanSeeImpArrow.GetBool()) return "";
            var update = false;
            string Suffix = "";
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                bool foundCheck =
                    pc != target && pc.GetCustomRole().IsImpostor();

                //発見対象じゃ無ければ次
                if (!foundCheck) continue;

                update = FixedUpdatePatch.CheckArrowUpdate(target, pc, update, pc.GetCustomRole().IsImpostor());
                var key = (target.PlayerId, pc.PlayerId);
                var arrow = Main.targetArrows[key];
                if (target.AmOwner)
                {
                    //MODなら矢印表示
                    Suffix += arrow;
                }
            }
            if (AmongUsClient.Instance.AmHost && seer.PlayerId != target.PlayerId && update)
            {
                //更新があったら非Modに通知
                Utils.NotifyRoles(SpecifySeer: target);
            }
            return Suffix;
        }
    }
}
