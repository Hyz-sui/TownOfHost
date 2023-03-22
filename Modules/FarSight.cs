using System;

using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfHost.Modules
{
    public static class FarSight
    {
        public static ActionButton Button;
        private static AudioClip openSound;
        private static AudioClip closeSound;

        public static bool IsActive { get; private set; }
        public static bool ShouldShowButton =>
            Options.FarSight.GetBool() &&
            (!PlayerControl.LocalPlayer.Is(CustomRoleTypes.Impostor) || !Options.FarSightExcludeImpostors.GetBool());
        private static bool CanActivate =>
            DestroyableSingleton<HudManager>.Instance.UseButton.isActiveAndEnabled && PlayerControl.LocalPlayer.CanMove;

        public static void CreateButton(HudManager hudManager)
        {
            Button = Object.Instantiate(hudManager.UseButton, hudManager.UseButton.transform.parent);
            Button.graphic.sprite = hudManager.UseButton.fastUseSettings[ImageNames.CamsButton].Image;
            Button.SetCooldownFill(0.5f);

            var label = Button.buttonLabelText;
            label.outlineColor = new(140, 255, 255, 255);
            label.outlineWidth = 0.077f;
            label.faceColor = new(0, 0, 0, 255);

            var passiveButton = Button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener((Action)Toggle);

            var gameObject = Button.gameObject;
            gameObject.name = "FarSightButton";
            gameObject.SetActive(false);
        }
        public static void Update()
        {
            if (openSound == null)
            {
                openSound = ShipStatus.Instance?.EmergencyButton?.MinigamePrefab?.OpenSound;
            }
            if (closeSound == null)
            {
                closeSound = ShipStatus.Instance?.EmergencyButton?.MinigamePrefab?.CloseSound;
            }

            if (IsActive && !CanActivate)
            {
                DisableAbility();
            }

            if (CanActivate)
            {
                Button.SetEnabled();
            }
            else
            {
                Button.SetDisabled();
            }

            if (!Button.isActiveAndEnabled)
            {
                return;
            }

            Button.OverrideText("千里眼");

            if (!Button.CanInteract())
            {
                return;
            }

            Button.graphic.color = new(0f, 0.6f, 1f, 0.7f);

            // アビリティボタンは憑依で埋まってるのでキルのキー設定を使用
            if (KeyboardJoystick.player.GetButtonDown(8))
            {
                Toggle();
            }
        }
        public static void Toggle()
        {
            SoundManager.Instance.PlaySound(IsActive ? closeSound : openSound, false);
            Button.SetDisabled();
            if (IsActive)
            {
                DisableAbility();
            }
            else
            {
                EnableAbility();
            }
        }
        public static void EnableAbility()
        {
            if (IsActive || !CanActivate)
            {
                return;
            }

            Camera.main.orthographicSize = DestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize *= 7f;
            DestroyableSingleton<HudManager>.Instance.transform.localScale *= 7f;
            IsActive = true;
        }
        public static void DisableAbility()
        {
            if (!IsActive)
            {
                return;
            }

            Camera.main.orthographicSize = DestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize /= 7f;
            DestroyableSingleton<HudManager>.Instance.transform.localScale /= 7f;
            IsActive = false;
        }
    }
}
