using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;
using Hazel;
using static TownOfHost.Options;
using static TownOfHost.Translator;

namespace TownOfHost
{
    public static class EvilHacker
    {
        private static readonly int Id = 3100;
        private static List<byte> playerIdList = new();

        private static OptionItem OptionCanSeeDeadPos;
        private static OptionItem OptionCanSeeOtherImp;
        private static OptionItem OptionCanSeeKillFlash;
        private static OptionItem OptionCanSeeMurderScene;
        private static OptionItem OptionCanSeeImpArrow;

        private static bool CanSeeDeadPos;
        private static bool CanSeeOtherImp;
        private static bool CanSeeKillFlash;
        private static bool CanSeeMurderScene;
        private static bool CanSeeImpArrow;

        private static Dictionary<SystemTypes, int> PlayerCount = new();
        private static Dictionary<SystemTypes, int> DeadCount = new();
        private static List<SystemTypes> ImpRooms = new();
        // (キルしたインポスター, 殺害現場の部屋)
        private static List<(byte killerId, SystemTypes room)> KillerIdsAndRooms = new();

        public static void SetupCustomOption()
        {
            SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.EvilHacker);
            OptionCanSeeDeadPos = BooleanOptionItem.Create(Id + 10, "CanSeeDeadPos", true, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.EvilHacker]);
            OptionCanSeeOtherImp = BooleanOptionItem.Create(Id + 11, "CanSeeOtherImp", true, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.EvilHacker]);
            OptionCanSeeKillFlash = BooleanOptionItem.Create(Id + 12, "CanSeeKillFlash", true, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.EvilHacker]);
            OptionCanSeeMurderScene = BooleanOptionItem.Create(Id + 13, "CanSeeMurderScene", true, TabGroup.ImpostorRoles, false).SetParent(OptionCanSeeKillFlash);
            OptionCanSeeImpArrow = BooleanOptionItem.Create(Id + 14, "CanSeeImpArrow", true, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.EvilHacker]);
        }
        public static void Init()
        {
            playerIdList = new();
            PlayerCount = new();
            DeadCount = new();
            ImpRooms = new();
            KillerIdsAndRooms = new();

            CanSeeDeadPos = OptionCanSeeDeadPos.GetBool();
            CanSeeOtherImp = OptionCanSeeOtherImp.GetBool();
            CanSeeKillFlash = OptionCanSeeKillFlash.GetBool();
            CanSeeMurderScene = OptionCanSeeMurderScene.GetBool();
            CanSeeImpArrow = OptionCanSeeImpArrow.GetBool();
        }
        public static void Add(byte playerId) => playerIdList.Add(playerId);
        public static bool IsEnable => playerIdList.Count > 0;
        public static void InitDeadCount()
        {
            if (ShipStatus.Instance == null)
            {
                Logger.Warn("死者カウントの初期化時にShipStatus.Instanceがnullでした", "EvilHacker");
                return;
            }
            foreach (var room in ShipStatus.Instance.AllRooms)
            {
                DeadCount[room.RoomId] = 0;
            }
        }

        public static void OnReportDeadbody()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            // 全生存プレイヤーの位置を取得
            foreach (var room in ShipStatus.Instance.AllRooms)
            {
                PlayerCount[room.RoomId] = 0;
            }
            foreach (var pc in Main.AllAlivePlayerControls)
            {
                var room = Main.PlayerStates[pc.PlayerId].LastRoom?.RoomId ?? default;
                PlayerCount[room]++;
                if (CanSeeOtherImp && pc.GetCustomRole().IsImpostor() && !ImpRooms.Contains(room))
                {
                    ImpRooms.Add(room);
                }
            }
            PlayerCount.Remove(SystemTypes.Hallway);
            DeadCount.Remove(SystemTypes.Hallway);
            // 送信するメッセージを生成
            StringBuilder messageBuilder = new();
            foreach (var kvp in PlayerCount)
            {
                var roomName = GetString(kvp.Key.ToString());
                if (ImpRooms.Contains(kvp.Key))
                {
                    messageBuilder.Append("★");
                }
                messageBuilder.AppendFormat("{0}: {1}", roomName, kvp.Value + DeadCount[kvp.Key]);
                if (DeadCount[kvp.Key] > 0 && CanSeeDeadPos)
                {
                    messageBuilder.AppendFormat("({0}\u00d7{1})", GetString("Deadbody"), DeadCount[kvp.Key]);
                }
                messageBuilder.AppendLine();
            }
            // 生存イビルハッカーに送信
            var aliveEvilHackerIds = playerIdList.Where(player => Utils.GetPlayerById(player).IsAlive()).ToArray();
            var message = messageBuilder.ToString();
            aliveEvilHackerIds.Do(id => Utils.SendMessage(
                message,
                id,
                Utils.ColorString(Color.green, $"{GetString("Message.LastAdminInfo")}")));

            InitDeadCount();
            ImpRooms = new();
        }
        public static void OnMurder(PlayerControl killer, PlayerControl target)
        {
            var room = target.GetPlainShipRoom()?.RoomId ?? default;
            DeadCount[room]++;
            if (CanSeeOtherImp && target.GetCustomRole().IsImpostor() && !ImpRooms.Contains(room))
            {
                ImpRooms.Add(room);
            }
            if (CanSeeMurderScene && Utils.IsImpostorKill(killer, target))
            {
                var realKiller = target.GetRealKiller() ?? killer;
                KillerIdsAndRooms.Add((realKiller.PlayerId, room));
                RpcSyncMurderScenes();
                new LateTask(() =>
                {
                    if (!GameStates.IsInGame)
                    {
                        Logger.Info("待機中にゲームが終了したためキャンセル", "EvilHacker");
                        return;
                    }
                    KillerIdsAndRooms.Remove((realKiller.PlayerId, room));
                    RpcSyncMurderScenes();
                    var aliveEvilHackers = (
                        from id in playerIdList
                        let player = Utils.GetPlayerById(id)
                        where player.IsAlive()
                        select player).ToArray();
                    foreach (var player in aliveEvilHackers)
                    {
                        Utils.NotifyRoles(false, player);
                    }
                }, 10f, "Remove EvilHacker KillerIdsAndRooms");
            }
        }
        public static string UtilsGetTargetArrow(bool isMeeting, PlayerControl seer)
        {
            //ミーティング以外では矢印表示
            if (isMeeting || !CanSeeImpArrow) return "";
            string SelfSuffix = "";
            foreach (var arrow in Main.targetArrows)
            {
                var target = Utils.GetPlayerById(arrow.Key.Item2);
                if (arrow.Key.Item1 == seer.PlayerId && !Main.PlayerStates[arrow.Key.Item2].IsDead && target.GetCustomRole().IsImpostor())
                    SelfSuffix += arrow.Value;
            }
            return SelfSuffix;
        }
        public static string PCGetTargetArrow(PlayerControl seer, PlayerControl target)
        {
            if (!CanSeeImpArrow) return "";
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
        public static void RpcSyncMurderScenes()
        {
            // タプルの数，キル者ID1，キル現場1，キル者ID2，キル現場2，......
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)CustomRPC.SyncEvilHackerScenes,
                SendOption.Reliable, -1);
            writer.Write(KillerIdsAndRooms.Count);
            foreach (var (killerId, room) in KillerIdsAndRooms)
            {
                writer.Write(killerId);
                writer.Write((byte)room);
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void ReceiveRPC(MessageReader reader)
        {
            int count = reader.ReadInt32();
            List<(byte, SystemTypes)> rooms = new(count);
            for (int i = 0; i < count; i++)
            {
                rooms.Add((reader.ReadByte(), (SystemTypes)reader.ReadByte()));
            }
            KillerIdsAndRooms = rooms;
        }
        public static string GetMurderSceneText(PlayerControl seer)
        {
            if (!seer.IsAlive()) return "";
            var roomNames = (
                from tuple in KillerIdsAndRooms
                    // 自身がキルしたものは除外
                where tuple.killerId != seer.PlayerId
                select GetString(tuple.room.ToString())).ToArray();
            if (roomNames.Length < 1) return "";
            return $"{GetString("EvilHackerMurderOccurred")}: {string.Join(", ", roomNames)}";
        }
        public static bool KillFlashCheck(PlayerControl killer, PlayerControl target) =>
            CanSeeKillFlash && Utils.IsImpostorKill(killer, target);
    }
}
