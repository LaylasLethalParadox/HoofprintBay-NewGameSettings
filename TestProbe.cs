#if DEBUG_PROBE
using UnityEngine;

namespace NewGameSettings;

// Test only (bridge call): give the custom hair scheme one plain colour and apply it the way the gradient editor does.
public static class TestProbe
{
    public static string SetHair(string hex)
    {
        if (!ColorUtility.TryParseHtmlString(hex, out var c))
            return "bad colour";
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(c, 0), new GradientColorKey(c, 1) },
            new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) });
        DataManager.i.data.saveGameSettings.customRiderHairColors = g;
        foreach (var ui in Resources.FindObjectsOfTypeAll<UiSettings>())
            if (ui.perSavegameSettings && ui.gameObject.activeInHierarchy)
            {
                ui.OnApplyHairGradient();
                return "applied; rider hair now " + ColorUtility.ToHtmlStringRGB(DataManager.i.data.player.GetHairColor(0.5f));
            }
        return "no open settings panel";
    }
}
#endif
