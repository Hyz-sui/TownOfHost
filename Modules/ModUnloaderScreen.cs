using System;
using HarmonyLib;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfHost;

public static class ModUnloaderScreen
{
    public static SpriteRenderer Popup { get; private set; }
    public static TextMeshPro WarnText { get; private set; }
    public static ToggleButtonBehaviour CancelButton { get; private set; }
    public static ToggleButtonBehaviour UnloadButton { get; private set; }

    public static void Init(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        Popup = Object.Instantiate(optionsMenuBehaviour.Background, ClientActionItem.CustomBackground.transform);
        Popup.name = "UnloadModPopup";
        Popup.transform.localPosition = new(0f, 0f, -8f);
        Popup.transform.localScale = new(0.7f, 0.7f, 1f);
        Popup.gameObject.SetActive(false);

        WarnText = Object.Instantiate(optionsMenuBehaviour.DisableMouseMovement.Text, Popup.transform);
        WarnText.name = "Warning";
        WarnText.transform.localPosition = new(0f, 0.3f, -1f);
        WarnText.text = Translator.GetString("UnloadWarning");
        WarnText.gameObject.SetActive(true);

        CancelButton = Object.Instantiate(optionsMenuBehaviour.DisableMouseMovement, Popup.transform);
        CancelButton.name = "Cancel";
        CancelButton.transform.localPosition = new(-0.8f, -0.7f, -2f);
        CancelButton.transform.localScale = new(0.7f, 0.7f, 1f);
        CancelButton.Text.text = Translator.GetString("Cancel");
        var cancelPassiveButton = CancelButton.GetComponent<PassiveButton>();
        cancelPassiveButton.OnClick = new();
        cancelPassiveButton.OnClick.AddListener((Action)Hide);
        CancelButton.gameObject.SetActive(true);

        UnloadButton = Object.Instantiate(optionsMenuBehaviour.DisableMouseMovement, Popup.transform);
        UnloadButton.name = "Unload";
        UnloadButton.transform.localPosition = new(0.8f, -0.7f, -2f);
        UnloadButton.transform.localScale = new(0.7f, 0.7f, 1f);
        UnloadButton.Text.text = Translator.GetString("Unload");
        var unloadPassiveButton = UnloadButton.GetComponent<PassiveButton>();
        unloadPassiveButton.OnClick = new();
        unloadPassiveButton.OnClick.AddListener(new Action(() =>
        {
            ClientActionItem.CustomBackground.gameObject.SetActive(false);
            ClientActionItem.ModOptionsButton.gameObject.SetActive(false);
            Logger.Info("ModをUnloadします", nameof(ModUnloaderScreen));
            Harmony.UnpatchAll();
            Main.Instance.Unload();
        }));
    }

    public static void Show()
    {
        if (Popup != null)
        {
            Popup.gameObject.SetActive(true);
        }
    }
    public static void Hide()
    {
        if (Popup != null)
        {
            Popup.gameObject.SetActive(false);
        }
    }
}
