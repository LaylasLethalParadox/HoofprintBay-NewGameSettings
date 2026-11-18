using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NewGameSettings;

// The new game screen on the ferry: a "Settings" button under Outfit opens a paper like the
// Outfit one holding the settings window's "Settings per Save Game" column.
internal static class FerrySettings
{
    private const string Actions = "PlayerCustomization/CanvasPlayer/ActionsRight";
    private const string Clothes = "PlayerCustomization/CanvasPlayer/Wardrobe/Clothes";
    private const string ButtonName = "ActionSaveGameSettings";

    private static GameObject panel;
    private static GameObject actionsPanel;

    // Unity's null check is false once the ferry scene is gone.
    internal static bool IsOnFerry => panel != null;

    internal static void Add()
    {
        var actions = GameObject.Find(Actions);
        var clothes = GameObject.Find(Clothes);
        if (actions == null || clothes == null)
            return; // not the ferry
        if (actions.transform.Find(ButtonName) != null)
            return;
        if (SettingsTemplate.Window == null)
        {
            Plugin.Logger.LogWarning("No copy of the settings window (the main menu wasn't seen); no Settings button.");
            return;
        }
        var style = actions.transform.Find("ActionStyle")?.GetComponent<RectTransform>();
        var dress = actions.transform.Find("ActionDress")?.GetComponent<RectTransform>();
        if (style == null || dress == null)
        {
            Plugin.Logger.LogWarning("Appearance/Outfit buttons not found on the ferry.");
            return;
        }

        actionsPanel = actions;
        panel = BuildPanel(clothes.GetComponent<RectTransform>());
        AddButton(style, dress);
    }

    private static void AddButton(RectTransform style, RectTransform dress)
    {
        var copy = Object.Instantiate(dress.gameObject, dress.parent, false).Cast<GameObject>();
        copy.name = ButtonName;
        foreach (var updater in copy.GetComponentsInChildren<LanguageUpdater>(true))
            Object.DestroyImmediate(updater);
        var text = copy.transform.Find("TextAction")?.GetComponent<TMP_Text>();
        if (text != null)
            text.text = Lang.gs("ui.settings.title");
        var icon = copy.transform.Find("IconAction")?.GetComponent<Image>();
        var board = FindSprite("ui-icons_board");
        if (icon != null && board != null)
            icon.sprite = board;

        var button = copy.GetComponent<Button>();
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(new Action(Open));

        // Three buttons where there were two: move Appearance and Outfit up so the third clears Continue.
        var step = dress.anchoredPosition - style.anchoredPosition;
        var lift = -step * 0.4f;
        style.anchoredPosition += lift;
        dress.anchoredPosition += lift;
        copy.GetComponent<RectTransform>().anchoredPosition = dress.anchoredPosition + step;
    }

    private static GameObject BuildPanel(RectTransform clothes)
    {
        var go = new GameObject("NewGameSettingsPanel");
        go.SetActive(false); // the settings code runs when it is opened, not while it is built
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(clothes.parent, false);
        rect.anchorMin = clothes.anchorMin;
        rect.anchorMax = clothes.anchorMax;
        rect.pivot = clothes.pivot;
        rect.sizeDelta = clothes.sizeDelta;
        // The cabinet slides in from the side; where it ends up open is x = 0 in its parent.
        rect.anchoredPosition = new Vector2(0, clothes.anchoredPosition.y);
        rect.localScale = clothes.localScale;

        foreach (var part in new[] { "Background", "TextHeading", "ButtonClose" })
        {
            var src = clothes.Find(part);
            if (src == null)
                continue;
            var copy = Object.Instantiate(src.gameObject, rect, false).Cast<GameObject>();
            copy.name = part;
            foreach (var updater in copy.GetComponentsInChildren<LanguageUpdater>(true))
                Object.DestroyImmediate(updater);
        }
        var heading = rect.Find("TextHeading")?.GetComponent<TMP_Text>();
        if (heading != null)
            heading.text = Lang.gs("ui.settings.perSaveGame");
        var close = rect.Find("ButtonClose")?.GetComponent<Button>();
        if (close != null)
        {
            close.onClick = new Button.ButtonClickedEvent();
            close.onClick.AddListener(new Action(Close));
        }

        // The settings window, cut down to its per-save column.
        var window = Object.Instantiate(SettingsTemplate.Window, rect, false).Cast<GameObject>();
        window.name = "SaveGameSettings";
        var ui = window.GetComponent<UiSettings>();
        ui.perSavegameSettings = true;
        var escape = window.GetComponent<CloseOnEscape>();
        if (escape != null)
            Object.DestroyImmediate(escape);
        var bg = window.GetComponent<Image>();
        if (bg != null)
            bg.enabled = false;
        var wrect = window.GetComponent<RectTransform>();
        wrect.anchorMin = Vector2.zero;
        wrect.anchorMax = Vector2.one;
        wrect.pivot = new Vector2(0.5f, 0.5f);
        wrect.offsetMin = Vector2.zero;
        wrect.offsetMax = Vector2.zero;
        wrect.localScale = Vector3.one;

        Transform perGame = null;
        for (var i = 0; i < window.transform.childCount; i++)
        {
            var child = window.transform.GetChild(i);
            if (child.name == "Scroll View PerGame")
                perGame = child;
            else if (child.gameObject.activeSelf)
                child.gameObject.SetActive(false); // title, close, global column
        }
        if (perGame != null)
        {
            var content = perGame.Find("Viewport/Content");
            if (content != null)
                go.AddComponent<EmptyHeadings>().Content = content;
            var scale = Layout(rect, perGame.GetComponent<RectTransform>());
            // Pop-ups (colour editor, colour picker) are sized for the big window too.
            for (var i = 0; i < window.transform.childCount; i++)
            {
                var child = window.transform.GetChild(i);
                if (child == perGame || child.gameObject.activeSelf)
                    continue;
                child.localScale = new Vector3(scale, scale, 1);
                var r = child.GetComponent<RectTransform>();
                if (r == null)
                    continue;
                // In the big window the colour editor sits left of centre and the colour picker right of it,
                // which here would be off the screen: centre the editor on the paper, the picker left of the paper.
                if (child.name == "GradientEditor")
                    r.anchoredPosition = new Vector2(0, r.anchoredPosition.y * scale);
                else if (child.name == "UiColorPicker")
                    r.anchoredPosition = new Vector2(
                        -(rect.rect.width + r.rect.width * scale) / 2 - 10, r.anchoredPosition.y * scale);
                else
                    r.anchoredPosition *= scale;
            }
        }
        else
            Plugin.Logger.LogWarning("The settings window has no 'Scroll View PerGame'.");
        window.SetActive(true); // shows only when the panel does

        // Escape closes the panel, like the game's own windows.
        var esc = go.AddComponent<CloseOnEscape>();
        esc.additionalActionOnClose = new UnityEvent();
        esc.additionalActionOnClose.AddListener(new Action(ShowActions));

        var size = rect.rect.size;
        Plugin.Logger.LogInfo($"Settings panel built ({size.x:0}x{size.y:0}).");
        return go;
    }

    // The column is wider than the paper, so it is scaled down to fit inside it.
    private static float Layout(RectTransform panelRect, RectTransform column)
    {
        const float sideMargin = 45f, topMargin = 110f, bottomMargin = 45f, wanted = 720f;
        var size = panelRect.rect.size;
        var width = size.x - 2 * sideMargin;
        var scale = Mathf.Min(1f, width / wanted);
        column.anchorMin = new Vector2(0, 0);
        column.anchorMax = new Vector2(0, 0);
        column.pivot = new Vector2(0, 0);
        column.localScale = new Vector3(scale, scale, 1);
        column.sizeDelta = new Vector2(width / scale, (size.y - topMargin - bottomMargin) / scale);
        column.anchoredPosition = new Vector2(sideMargin, bottomMargin);
        Plugin.Logger.LogInfo($"Per-save column scaled {scale:0.00} into {size.x:0}x{size.y:0}.");
        return scale;
    }

    private static void Open()
    {
        if (panel == null)
            return;
        panel.transform.SetAsLastSibling();
        panel.SetActive(true);
        // Like the Outfit cabinet: the paper with the name and buttons goes while it is open.
        if (actionsPanel != null)
            actionsPanel.SetActive(false);
    }

    private static void Close()
    {
        if (panel != null)
            panel.SetActive(false);
        ShowActions();
    }

    private static void ShowActions()
    {
        if (actionsPanel != null)
            actionsPanel.SetActive(true);
    }

    private static Sprite FindSprite(string name)
    {
        foreach (var s in Resources.FindObjectsOfTypeAll<Sprite>())
            if (s.name == name)
                return s;
        return null;
    }
}
