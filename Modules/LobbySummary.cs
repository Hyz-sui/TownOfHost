using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfHost.Modules
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
        public static readonly DirectoryInfo HistoryDirectory = ModTempData.GetSubDirectory("History");
        public static string[] History { get; private set; }
        private static int currentIndex = 0;

        public static void Show()
        {
            Load();
            if (History?.Length > 0 && Main.ShowLobbySummary.Value)
            {
                currentIndex = 0;
                ChangePage(currentIndex);
                PrevSummaryTMP.gameObject.SetActive(true);
            }
            else
            {
                Hide();
            }
        }
        public static void Hide()
        {
            History = null;
            PrevSummaryTMP.gameObject.SetActive(false);
        }
        public static void Save(string text)
        {
            var path = $"{HistoryDirectory.FullName}/result_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_ffff}.txt";
            var formattedText = Regex.Replace(
                text.Replace(Translator.GetString("RoleSummaryText"), ""),
                @"</?pos(=\d*%)?>", "");
            File.WriteAllText(path, formattedText, Encoding.UTF8);
        }
        public static void Load()
        {
            var files = (
                from file in HistoryDirectory.GetFiles()
                let name = file.Name
                where name.StartsWith("result_")
                where name.EndsWith(".txt")
                orderby file.Name descending
                select file).ToArray();
            History = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                using var reader = new StreamReader(file.FullName);
                History[i] = reader.ReadToEnd();
            }
        }
        public static void Update()
        {
            if (!Main.ShowLobbySummary.Value || History == null || History.Length <= 0)
            {
                return;
            }

            if (PlayerControl.LocalPlayer?.CanMove == false)
            {
                return;
            }

            var mouseDelta = Input.mouseScrollDelta.y;
            if (mouseDelta > 0)
            {
                PreviousGame();
            }
            else if (mouseDelta < 0)
            {
                NextGame();
            }
        }
        public static void PreviousGame()
        {
            currentIndex++;
            if (currentIndex > History.Length - 1)
            {
                currentIndex = 0;
            }
            ChangePage(currentIndex);
        }
        public static void NextGame()
        {
            currentIndex--;
            if (currentIndex < 0)
            {
                currentIndex = History.Length - 1;
            }
            ChangePage(currentIndex);
        }
        private static void ChangePage(int index)
        {
            var gamesAgo = index + 1;
            var summaryBuilder = new StringBuilder(gamesAgo == 1 ? "前の試合の結果:" : $"{gamesAgo}試合前の結果:");
            PrevSummaryTMP.text = summaryBuilder
                .AppendLine(History[index])
                .AppendLine()
                .Append("マウススクロールで切り替え...")
                .AppendFormat(" ({0}/{1})", History.Length - index, History.Length)
                .ToString();
        }
    }
}
