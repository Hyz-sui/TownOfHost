using System;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfHost.Objects;

public sealed class FlatButton
{
    public FlatButton(Transform parent, string name, Vector3 localPosition, Color32 normalColor, Color32 hoverColor, Action action, string label, Vector2 scale)
    {
        Button = Object.Instantiate(buttonPrefab, parent);
        Label = Button.transform.Find("FontPlacer/Text_TMP").GetComponent<TextMeshPro>();
        NormalRenderer = Button.inactiveSprites.GetComponent<SpriteRenderer>();
        HoverRenderer = Button.activeSprites.GetComponent<SpriteRenderer>();
        buttonCollider = Button.GetComponent<BoxCollider2D>();

        var container = Label.transform.parent;
        Object.Destroy(Label.GetComponent<AspectPosition>());
        container.SetLocalX(0f);
        Label.transform.SetLocalX(0f);
        Label.horizontalAlignment = HorizontalAlignmentOptions.Center;

        NormalRenderer.color = normalColor;
        HoverRenderer.color = hoverColor;

        Button.name = name;
        Button.transform.localPosition = localPosition;
        Button.OnClick.AddListener(action);
        Label.text = label;
        Scale = scale;
        Button.gameObject.SetActive(true);
    }

    public PassiveButton Button { get; }
    public TextMeshPro Label { get; }
    public SpriteRenderer NormalRenderer { get; }
    public SpriteRenderer HoverRenderer { get; }
    private readonly BoxCollider2D buttonCollider;
    private Vector2 _scale;
    public Vector2 Scale
    {
        get => _scale;
        set => _scale = NormalRenderer.size = HoverRenderer.size = buttonCollider.size = value;
    }
    private float _fontSize;
    public float FontSize
    {
        get => _fontSize;
        set => _fontSize = Label.fontSize = Label.fontSizeMin = Label.fontSizeMax = value;
    }

    private static PassiveButton buttonPrefab = CreatePrefab();
    private static PassiveButton CreatePrefab()
    {
        if (Prefabs.SimpleButton == null)
        {
            throw new InvalidOperationException("SimpleButtonのプレファブが未設定");
        }

        var button = Object.Instantiate(Prefabs.SimpleButton);
        var label = button.transform.Find("FontPlacer/Text_TMP").GetComponent<TextMeshPro>();
        var normalRenderer = button.inactiveSprites.GetComponent<SpriteRenderer>();
        var hoverRenderer = button.activeSprites.GetComponent<SpriteRenderer>();

        Object.Destroy(button.GetComponent<AspectScaledAsset>());

        var texture = new Texture2D(normalRenderer.sprite.texture.width, normalRenderer.sprite.texture.height, TextureFormat.ARGB32, false);
        for (var x = 0; x < texture.width; x++)
        {
            for (var y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, Color.white);
            }
        }
        texture.Apply();
        var normalSprite = Sprite.Create(texture, new(0f, 0f, texture.width, texture.height), new(0.5f, 0.5f));
        var hoverSprite = Sprite.Create(texture, new(0f, 0f, texture.width, texture.height), new(0.5f, 0.5f));
        normalRenderer.sprite = normalSprite;
        hoverRenderer.sprite = hoverSprite;

        button.name = "FlatButtonPrefab";
        label.text = "Flat Button Prefab";
        button.gameObject.SetActive(false);
        Object.DontDestroyOnLoad(button);
        return button;
    }
}
