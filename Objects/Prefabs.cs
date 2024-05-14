using TMPro;
using UnityEngine;

namespace TownOfHost.Objects
{
    public static class Prefabs
    {
        private static TextMeshPro _simpleText;
        public static TextMeshPro SimpleText
        {
            get => _simpleText;
            set
            {
                if (_simpleText != null)
                {
                    return;
                }
                _simpleText = Object.Instantiate(value);
                _simpleText.text = "Simple Text Prefab";
                _simpleText.name = "SimpleTextPrefab";
                _simpleText.alignment = TextAlignmentOptions.Center;
                Object.DontDestroyOnLoad(_simpleText);
                _simpleText.gameObject.SetActive(false);
                if (_simpleText.GetComponent<AspectPosition>() is AspectPosition component)
                {
                    component.enabled = false;
                }
            }
        }
        private static PassiveButton _simpleButton;
        public static PassiveButton SimpleButton
        {
            get => _simpleButton;
            set
            {
                if (_simpleButton != null)
                {
                    return;
                }
                _simpleButton = Object.Instantiate(value);
                var label = _simpleButton.transform.Find("FontPlacer/Text_TMP").GetComponent<TextMeshPro>();
                _simpleButton.gameObject.SetActive(false);
                Object.DontDestroyOnLoad(_simpleButton);
                _simpleButton.name = "SimpleButtonPrefab";
                Object.Destroy(_simpleButton.GetComponent<AspectPosition>());
                label.DestroyTranslator();
                label.fontSize = label.fontSizeMax = label.fontSizeMin = 3.5f;
                label.enableWordWrapping = false;
                label.text = "Simple Button Prefab";
                var buttonCollider = _simpleButton.GetComponent<BoxCollider2D>();
                buttonCollider.offset = new(0f, 0f);
                _simpleButton.OnClick = new();
            }
        }
    }
}
