using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NewGameSettings;

// The ferry's rider and its skin/hair sliders take their colours when the scene starts, so a custom colour
// scheme set in our panel would only show on the next screen. Redraw them whenever the panel changes one.
[HarmonyPatch]
internal static class RiderColors
{
    [HarmonyPatch(typeof(UiSettings), nameof(UiSettings.ToggleCustomSkinColor))]
    [HarmonyPatch(typeof(UiSettings), nameof(UiSettings.OnApplySkinGradient))]
    [HarmonyPatch(typeof(UiSettings), nameof(UiSettings.ResetCustomSkinColor))]
    [HarmonyPatch(typeof(UiSettings), nameof(UiSettings.ToggleCustomHairColor))]
    [HarmonyPatch(typeof(UiSettings), nameof(UiSettings.OnApplyHairGradient))]
    [HarmonyPatch(typeof(UiSettings), nameof(UiSettings.ResetCustomHairColor))]
    [HarmonyPostfix]
    private static void AfterColorChange()
    {
        try
        {
            Refresh();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Updating the rider's colours failed: {e}");
        }
    }

    internal static WardrobePlayer Wardrobe;

    internal static void Refresh()
    {
        if (!FerrySettings.IsOnFerry || Wardrobe == null)
            return;
        var player = DataManager.i?.data?.player;
        if (player == null)
            return;

        // The player's own colour lookup already picks the custom scheme or the game's, so the slider
        // backgrounds are sampled from it.
        SetBackground(Wardrobe.sliderSkinColorBackground, t => player.GetSkinColor(t));
        SetBackground(Wardrobe.sliderHairColorBackground, t => player.GetHairColor(t));

        // The game's own slider handlers colour the rider (and the knobs) from the current scheme.
        if (Wardrobe.sliderSkinColor != null)
            Wardrobe.SetSkinColor(Wardrobe.sliderSkinColor.value);
        if (Wardrobe.sliderHairColor != null)
            Wardrobe.SetHairColor(Wardrobe.sliderHairColor.value);
    }

    private static void SetBackground(RawImage image, Func<float, Color> color)
    {
        if (image == null)
            return;
        var old = image.texture;
        var width = old != null && old.width > 1 ? old.width : 256;
        var height = old != null && old.height > 0 ? old.height : 1;
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = Made, wrapMode = TextureWrapMode.Clamp };
        var pixels = new Color[width * height];
        for (var x = 0; x < width; x++)
        {
            var c = color(x / (float)(width - 1));
            for (var y = 0; y < height; y++)
                pixels[y * width + x] = c;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        image.texture = tex;
        // Only drop textures we made; the first one may be the game's.
        if (old != null && old.name == Made)
            Object.Destroy(old);
    }

    private const string Made = "NewGameSettings gradient";
}
