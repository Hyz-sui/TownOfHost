using System.Collections.Generic;
using System.Text;

using Hazel;
using UnityEngine;

namespace TownOfHost.Modules;

public static class DeviceTimer
{
    public static bool CamerasRanOut
    {
        get => camerasRanOut && isEnabled;
        private set => camerasRanOut = value;
    }
    private static bool camerasRanOut;
    private static int NumPlayersWatchingCamera => playersWatchingCamera.Count;
    private static HashSet<byte> playersWatchingCamera;
    private static bool isEnabled;
    private static float camerasRemaining;
    private static string notifyText;
    private static HashSet<byte> unusableNotifyTargets;

    public static void Init()
    {
        CamerasRanOut = false;
        playersWatchingCamera = new();
        isEnabled = Options.CamerasTimer.GetBool();
        camerasRemaining = Options.CamerasMaxTimer.GetInt();
        UpdateNotifyText();
        unusableNotifyTargets = new();
    }
    public static void HandleRepairSystem(SystemTypes systemType, PlayerControl player, byte amount)
    {
        if (!AmongUsClient.Instance.AmHost || !isEnabled)
        {
            return;
        }
        if (systemType != SystemTypes.Security || !player.IsAlive())
        {
            return;
        }

        switch (amount)
        {
            case 1: BeginCamera(player); break;
            case 2: CloseCamera(player); break;
        }
    }
    public static void BeginCamera(PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost || !isEnabled)
        {
            return;
        }
        playersWatchingCamera.Add(player.PlayerId);
        Logger.Info($"Begin: {System.DateTime.Now:HH:mm:ss}", nameof(DeviceTimer));
    }
    public static void CloseCamera(PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost || !isEnabled)
        {
            return;
        }
        if (playersWatchingCamera.Contains(player.PlayerId))
        {
            playersWatchingCamera.Remove(player.PlayerId);
            Logger.Info($"Close: {System.DateTime.Now:HH:mm:ss}", nameof(DeviceTimer));
        }
    }
    public static void ConsumeCamera()
    {
        if (!AmongUsClient.Instance.AmHost || !isEnabled)
        {
            return;
        }
        camerasRemaining -= Time.fixedDeltaTime * NumPlayersWatchingCamera;
        if (camerasRemaining <= 0f && !CamerasRanOut)
        {
            RpcUpdateCamerasUsable(camerasRemaining);
        }
    }
    public static void RpcUpdateCamerasUsable(float time)
    {
        if (AmongUsClient.Instance.AmHost)
        {
            Logger.Info("【RpcUpdateCamerasUsable】", nameof(DeviceTimer));
            var writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)CustomRPC.UpdateCamerasUsable,
                SendOption.Reliable);
            writer.Write(time);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        UpdateCamerasUsable(time);
    }
    public static void UpdateCamerasUsable(float time)
    {
        if (!isEnabled || CamerasRanOut)
        {
            return;
        }

        if (time <= 0f)
        {
            Logger.Info($"Destroy: {System.DateTime.Now:HH:mm:ss}", nameof(DeviceTimer));
            CamerasRanOut = true;
        }
    }
    public static void UpdateNotifyText()
    {
        if (!isEnabled)
        {
            return;
        }
        var notifyBuilder = new StringBuilder();

        notifyBuilder.Append("カメラ: ");
        if (CamerasRanOut)
        {
            notifyBuilder.AppendLine("時間切れ");
        }
        else
        {
            notifyBuilder.AppendFormat("あと {0:0.00} 秒", camerasRemaining).AppendLine();
        }

        notifyText = notifyBuilder.ToString();
    }
    public static void SendNotifyText()
    {
        if (!isEnabled)
        {
            return;
        }
        Utils.SendMessage(notifyText, title: Utils.ColorString(Color.cyan, "デバイスの残り時間"));
    }
    private static bool IsNearCamera(PlayerControl player)
    {
        var usableDistance = DisableDevice.UsableDistance();
        var playerPosition = player.GetTruePosition();
        var devicePositions = DisableDevice.DevicePos;
        var devicePosition = (MapNames)Main.NormalOptions.MapId switch
        {
            MapNames.Airship => devicePositions["AirshipCamera"],
            MapNames.Polus => devicePositions["PolusCamera"],
            MapNames.Skeld => devicePositions["SkeldCamera"],
            _ => Vector2.zero
        };
        if (devicePosition == Vector2.zero)
        {
            return false;
        }
        var distance = Vector2.Distance(playerPosition, devicePosition);

        return distance <= usableDistance;
    }
    public static string GetNameNotifyText(PlayerControl player)
    {
        if (player.IsModClient() || !player.IsAlive())
        {
            return string.Empty;
        }
        if (!CamerasRanOut)
        {
            return string.Empty;
        }

        if (unusableNotifyTargets.Contains(player.PlayerId))
        {
            return Utils.ColorString(Color.cyan, "使用不可");
        }
        return string.Empty;
    }
    public static void UpdateUnusableNotify(PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost || !isEnabled || !CamerasRanOut)
        {
            return;
        }
        if (player.IsModClient())
        {
            return;
        }

        if (!IsNearCamera(player))
        {
            if (unusableNotifyTargets.Contains(player.PlayerId))
            {
                unusableNotifyTargets.Remove(player.PlayerId);
                Utils.NotifyRoles(SpecifySeer: player);
            }
            return;
        }

        if (unusableNotifyTargets.Add(player.PlayerId))
        {
            Utils.NotifyRoles(SpecifySeer: player);
        }
    }
}
