using UnityEngine;
using Object = UnityEngine.Object;

namespace NewGameSettings;

// An inactive copy of the main menu's settings window, kept across scenes. The ferry scene has no
// settings window of its own, so its settings panel is copied from this one.
internal static class SettingsTemplate
{
    internal static GameObject Window;

    internal static void Take(UiMenu menu)
    {
        if (Window != null)
            return;
        var top = menu.transform; // Transform.root is stripped
        while (top.parent != null)
            top = top.parent;
        var original = top.GetComponentInChildren<UiSettings>(true);
        if (original == null)
        {
            Plugin.Logger.LogWarning("No settings window in the main menu; the ferry gets no Settings button.");
            return;
        }
        var wasActive = original.gameObject.activeSelf;
        // Copied while inactive so the copy's Awake/OnEnable don't run here. Taken at menu start,
        // before other mods add their rows to the window when it opens.
        original.gameObject.SetActive(false);
        // No parent: Mod Manager looks for "a settings window with a parent" and must not find this one.
        Window = Object.Instantiate(original.gameObject).Cast<GameObject>();
        original.gameObject.SetActive(wasActive);
        Window.name = "NewGameSettings Template";
        Object.DontDestroyOnLoad(Window);
        Plugin.Logger.LogInfo("Copied the settings window for the ferry.");
    }
}
