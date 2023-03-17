using HarmonyLib;
using TMPro;
using UnityEngine;

using TownOfHost.Modules;

namespace TownOfHost.Patches
{
    [HarmonyPatch]
    public static class CameraPatch
    {
        // バニラプレイヤーはコミュサボになるため，閉じるのではなくコミュサボ状態にする
        private static bool ShouldScramble() => DeviceTimer.CamerasRanOut && PlayerControl.LocalPlayer.IsAlive();

        [HarmonyPatch]
        public static class PlanetSurveillanceMinigamePatch
        {
            private static TextMeshPro runOutText;

            [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Update)), HarmonyPrefix]
            public static bool UpdatePrefix(PlanetSurveillanceMinigame __instance)
            {
                if (ShouldScramble())
                {
                    __instance.isStatic = true;
                    __instance.ViewPort.sharedMaterial = __instance.StaticMaterial;
                    if (runOutText == null)
                    {
                        var sabText = __instance.SabText;
                        sabText.gameObject.SetActive(false);
                        runOutText = Object.Instantiate(sabText, sabText.transform.parent);
                        runOutText.text = "時間切れ";
                        runOutText.gameObject.SetActive(true);
                    }

                    return false;
                }
                return true;
            }
            [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.NextCamera)), HarmonyPrefix]
            public static bool NextCameraPrefix(PlanetSurveillanceMinigame __instance)
            {
                if (ShouldScramble())
                {
                    if (Constants.ShouldPlaySfx())
                    {
                        SoundManager.Instance.PlaySound(__instance.CloseSound, false);
                    }
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch]
        public static class SurveillanceMinigamePatch
        {
            private static TextMeshPro[] runOutTexts;

            [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Update)), HarmonyPrefix]
            public static bool UpdatePrefix(SurveillanceMinigame __instance)
            {
                if (ShouldScramble())
                {
                    if (runOutTexts == null)
                    {
                        runOutTexts = new TextMeshPro[__instance.ViewPorts.Length];
                    }
                    __instance.isStatic = true;
                    for (int j = 0; j < __instance.ViewPorts.Length; j++)
                    {
                        __instance.ViewPorts[j].sharedMaterial = __instance.StaticMaterial;
                        if (runOutTexts[j] == null)
                        {
                            var sabText = __instance.SabText[j];
                            sabText.gameObject.SetActive(false);
                            runOutTexts[j] = Object.Instantiate(sabText, sabText.transform.parent);
                            runOutTexts[j].text = "時間切れ";
                            runOutTexts[j].gameObject.SetActive(true);
                        }
                    }

                    return false;
                }
                return true;
            }
        }
    }
}
