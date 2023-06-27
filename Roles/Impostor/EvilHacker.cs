using System.Collections.Generic;
using System.Linq;
using System.Text;

using HarmonyLib;
using UnityEngine;

using AmongUs.GameOptions;
using Hazel;

using TownOfHost.Extensions;
using TownOfHost.Roles.Core;
using TownOfHost.Roles.Core.Interfaces;
using static TownOfHost.Translator;

namespace TownOfHost.Roles.Impostor;

public sealed class EvilHacker : RoleBase, IImpostor
{
    public EvilHacker(PlayerControl player) : base(RoleInfo, player)
    {
    }

    public static readonly SimpleRoleInfo RoleInfo = SimpleRoleInfo.Create(
        typeof(EvilHacker),
        player => new EvilHacker(player),
        CustomRoles.EvilHacker,
        () => RoleTypes.Impostor,
        CustomRoleTypes.Impostor,
        3100,
        SetupCustomOption,
        "eh");

    private static List<byte> playerIdList = new();

    private static OptionItem OptionCanSeeDeadPos;
    private static OptionItem OptionCanSeeOtherImp;
    private static OptionItem OptionCanSeeKillFlash;
    private static OptionItem OptionCanSeeMurderScene;
    private static OptionItem OptionCanSeeImpArrow;
    private static OptionItem OptionInheritAbility;

    private static bool CanSeeDeadPos;
    private static bool CanSeeOtherImp;
    private static bool CanSeeKillFlash;
    private static bool CanSeeMurderScene;
    private static bool CanSeeImpArrow;
    private static bool InheritAbility;

    private static Dictionary<SystemTypes, int> PlayerCount = new();
    private static Dictionary<SystemTypes, int> DeadCount = new();
    private static List<SystemTypes> ImpRooms = new();
    // (キルしたインポスター, 殺害現場の部屋)
    private static List<(byte killerId, SystemTypes room)> KillerIdsAndRooms = new();
    private static Dictionary<byte, byte[]> ImpostorIds = new();

    private enum OptionName { EvilHackerCanSeeDeadPos, EvilHackerCanSeeOtherImp, EvilHackerCanSeeKillFlash, EvilHackerCanSeeMurderScene, EvilHackerCanSeeImpArrow, EvilHackerInheritAbility, }
    public static void SetupCustomOption()
    {
        OptionCanSeeDeadPos = BooleanOptionItem.Create(RoleInfo, 10, OptionName.EvilHackerCanSeeDeadPos, true, false);
        OptionCanSeeOtherImp = BooleanOptionItem.Create(RoleInfo, 11, OptionName.EvilHackerCanSeeOtherImp, true, false);
        OptionCanSeeKillFlash = BooleanOptionItem.Create(RoleInfo, 12, OptionName.EvilHackerCanSeeKillFlash, true, false);
        OptionCanSeeMurderScene = BooleanOptionItem.Create(RoleInfo, 13, OptionName.EvilHackerCanSeeMurderScene, true, false).SetParent(OptionCanSeeKillFlash);
        OptionCanSeeImpArrow = BooleanOptionItem.Create(RoleInfo, 14, OptionName.EvilHackerCanSeeImpArrow, true, false);
        OptionInheritAbility = BooleanOptionItem.Create(RoleInfo, 15, OptionName.EvilHackerInheritAbility, true, false);
    }
    public static void Init()
    {
        playerIdList = new();
        PlayerCount = new();
        DeadCount = new();
        ImpRooms = new();
        KillerIdsAndRooms = new();
        ImpostorIds = new();

        CanSeeDeadPos = OptionCanSeeDeadPos.GetBool();
        CanSeeOtherImp = OptionCanSeeOtherImp.GetBool();
        CanSeeKillFlash = OptionCanSeeKillFlash.GetBool();
        CanSeeMurderScene = OptionCanSeeMurderScene.GetBool();
        CanSeeImpArrow = OptionCanSeeImpArrow.GetBool();
        InheritAbility = OptionInheritAbility.GetBool();
    }
    public static void Add(byte playerId)
    {
        playerIdList.Add(playerId);

        var impostorIds = (
            from player in Main.AllAlivePlayerControls
            where player.PlayerId != playerId
            where player.Is(CustomRoleTypes.Impostor)
            select player.PlayerId).ToArray();
        foreach (var impostorId in impostorIds)
        {
            TargetArrow.Add(playerId, impostorId);
        }
        ImpostorIds[playerId] = impostorIds;
    }
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

    public static void OnMeeting()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        // 全生存プレイヤーの位置を取得
        foreach (var room in ShipStatus.Instance.AllRooms)
        {
            PlayerCount[room.RoomId] = 0;
        }
        foreach (var pc in Main.AllAlivePlayerControls)
        {
            var room = PlayerState.GetByPlayerId(pc.PlayerId).LastRoom?.RoomId ?? default;
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
    // 暇なときに書き直したい
    public static string GetArrow(PlayerControl seer, bool isMeeting)
    {
        if (isMeeting || !CanSeeImpArrow)
        {
            return string.Empty;
        }
        var impostorIds = GetArrowTargets(seer.PlayerId);
        if (impostorIds == null)
        {
            return "?";
        }
        return TargetArrow.GetArrows(seer, impostorIds);
    }
    private static byte[] GetArrowTargets(byte seerId)
    {
        if (!ImpostorIds.TryGetValue(seerId, out var impostorIds))
        {
            Logger.Warn($"{Utils.GetPlayerById(seerId)?.GetRealName() ?? $"(null: {seerId})"}からの矢印ターゲットの取得に失敗", "EvilHacker");
            return null;
        }
        var aliveImpostorIds = (
            from impostorId in impostorIds
            let impostor = Utils.GetPlayerById(impostorId)
            where impostor != null
            where impostor.IsAlive()
            select impostorId).ToArray();
        return aliveImpostorIds;
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
        return Utils.ColorString(Color.green, $"{GetString("EvilHackerMurderOccurred")}: {string.Join(", ", roomNames)}");
    }
    public static bool KillFlashCheck(PlayerControl killer, PlayerControl target) =>
        CanSeeKillFlash && Utils.IsImpostorKill(killer, target);
    public static void Inherit()
    {
        if (!InheritAbility)
        {
            return;
        }

        // 生存素インポスター
        var candidates = Main.AllAlivePlayerControls.Where(player => player.Is(CustomRoles.Impostor)).ToArray();
        if (candidates.Length <= 0)
        {
            return;
        }

        var target = candidates.PickRandom();
        Logger.Info($"継承: {target.GetNameWithRole()}", "EvilHacker");
        target.RpcChangeMainRole(CustomRoles.EvilHacker);
        Add(target.PlayerId);
        Utils.NotifyRoles(SpecifySeer: target);
    }
}
