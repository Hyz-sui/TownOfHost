using System.Text.RegularExpressions;

using TMPro;
using UnityEngine;

namespace TownOfHost
{
    public static class LobbySummary
    {
        private static TextMeshPro _PrevSummaryTMP;
        private static TextMeshPro PrevSummaryTMP
        {
            get
            {
                if (_PrevSummaryTMP != null)
                {
                    return _PrevSummaryTMP;
                }
                var settingsTMP = DestroyableSingleton<HudManager>.Instance.GameSettings;
                _PrevSummaryTMP = Object.Instantiate(settingsTMP, settingsTMP.transform.parent);
                _PrevSummaryTMP.name = "PrevSummary_TMP";
                _PrevSummaryTMP.GetComponent<AspectPosition>().enabled = false;
                _PrevSummaryTMP.transform.localPosition = new(-2.3f, 3f, -1f);
                _PrevSummaryTMP.fontSize = _PrevSummaryTMP.fontSizeMax = _PrevSummaryTMP.fontSizeMin = 1.2f;
                _PrevSummaryTMP.outlineColor = new(0, 0, 0, 100);
                _PrevSummaryTMP.outlineWidth = 0.15f;
                _PrevSummaryTMP.lineSpacing = -25f;
                _PrevSummaryTMP.characterSpacing = -5f;
                _PrevSummaryTMP.gameObject.SetActive(true);

                return _PrevSummaryTMP;
            }
        }
        public static string PrevSummaryText;

        public static void Show()
        {
            if (PrevSummaryText != null && Main.ShowLobbySummary.Value)
            {
                PrevSummaryTMP.text = Regex.Replace(
                    PrevSummaryText.Replace(Translator.GetString("RoleSummaryText"), "前の試合の役職一覧:"),
                    @"</?pos(=\d*%)?>", "");
                PrevSummaryTMP.gameObject.SetActive(true);
            }
            else
            {
                Hide();
            }
        }
        public static void Hide()
        {
            PrevSummaryTMP.gameObject.SetActive(false);
        }
    }
}
