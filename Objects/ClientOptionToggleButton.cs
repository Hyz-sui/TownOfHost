using System;
using System.Linq;

using BepInEx.Configuration;
using UnityEngine;
using Object = UnityEngine.Object;

using TownOfHost.Modules;

namespace TownOfHost.Objects
{
    public class ClientOptionToggleButton
    {
        public ConfigEntry<bool> Config;
        public ToggleButtonBehaviour Behaviour;

        public static SpriteRenderer CustomBackground;
        private static int numOptions = 0;

        private ClientOptionToggleButton(
            string label,
            string objectName,
            ConfigEntry<bool> config,
            OptionsMenuBehaviour menuBehaviour,
            Action additionalOnClickEvent = null)
        {
            this.Config = config;

            var mouseButton = menuBehaviour.DisableMouseMovement;

            if (CustomBackground == null)
            {
                numOptions = 0;
                CustomBackground = Object.Instantiate(menuBehaviour.Background, menuBehaviour.transform);
                CustomBackground.name = "CustomBackground";
                CustomBackground.transform.localScale = new(0.9f, 0.9f, 1f);
                CustomBackground.transform.localPosition += Vector3.back * 8;
                CustomBackground.gameObject.SetActive(false);

                var closeButton = Object.Instantiate(mouseButton, CustomBackground.transform);
                closeButton.transform.localPosition = new(1.3f, -2.3f, -6f);
                closeButton.name = "Close";
                closeButton.Text.text = "閉じる";
                closeButton.Background.color = Palette.DisabledGrey;
                var closePassiveButton = closeButton.GetComponent<PassiveButton>();
                closePassiveButton.OnClick = new();
                closePassiveButton.OnClick.AddListener((Action)(() =>
                {
                    CustomBackground.gameObject.SetActive(false);
                }));

                var buttons = menuBehaviour.ControllerSelectable.ToArray().ToArray();
                var leaveButton = buttons.FirstOrDefault(element => element.name == "LeaveGameButton")?.GetComponent<PassiveButton>();
                var returnButton = buttons.FirstOrDefault(element => element.name == "ReturnToGameButton")?.GetComponent<PassiveButton>();
                var parent = leaveButton?.transform?.parent ?? menuBehaviour.Tabs.FirstOrDefault(tab => tab.name == "GeneralButton").Content.transform;
                var modOptionsButton = Object.Instantiate(mouseButton, parent);
                modOptionsButton.transform.localPosition = leaveButton?.transform?.localPosition ?? new(0f, -2.4f, 1f);
                modOptionsButton.name = "TOHHOptions";
                modOptionsButton.Text.text = $"{"TOH-H".Color(Main.ModColor)}の設定";
                modOptionsButton.Background.color = Palette.FromHex(0x00bfff);
                var modOptionsPassiveButton = modOptionsButton.GetComponent<PassiveButton>();
                modOptionsPassiveButton.OnClick = new();
                modOptionsPassiveButton.OnClick.AddListener((Action)(() =>
                {
                    CustomBackground.gameObject.SetActive(true);
                }));

                if (leaveButton != null)
                {
                    leaveButton.transform.localPosition = new(-1.35f, -2.411f, -1f);
                }
                if (returnButton != null)
                {
                    returnButton.transform.localPosition = new(1.35f, -2.411f, -1f);
                }
            }

            this.Behaviour = Object.Instantiate(mouseButton, CustomBackground.transform);
            this.Behaviour.transform.localPosition = new Vector3(
                numOptions % 2 == 0 ? -1.3f : 1.3f,
                2.2f - (0.5f * (numOptions / 2)),
                -6f);
            this.Behaviour.name = objectName;
            this.Behaviour.Text.text = label;
            var passiveButton = this.Behaviour.GetComponent<PassiveButton>();
            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener((Action)(() =>
            {
                config.Value = !config.Value;
                this.UpdateToggle();
                additionalOnClickEvent?.Invoke();
            }));
            this.UpdateToggle();
            numOptions++;
        }

        public static ClientOptionToggleButton Create(
            string label,
            string objectName,
            ConfigEntry<bool> config,
            OptionsMenuBehaviour menuBehaviour,
            Action additionalOnClickEvent = null)
        {
            return new ClientOptionToggleButton(label, objectName, config, menuBehaviour, additionalOnClickEvent);
        }

        public void UpdateToggle()
        {
            if (this.Behaviour == null)
            {
                return;
            }

            var color = this.Config.Value ? CustomPalette.EnabledGreen : CustomPalette.DisabledRed;
            this.Behaviour.Background.color = color;
            if (this.Behaviour.Rollover)
            {
                this.Behaviour.Rollover.ChangeOutColor(color);
            }
        }
    }
}
