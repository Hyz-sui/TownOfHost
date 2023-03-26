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
    }
}
