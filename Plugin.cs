using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;

namespace NewGameSettings;

[BepInPlugin(Guid, Name, Version)]
public class Plugin : BasePlugin
{
    public const string Guid = "hoofprintbay.newgamesettings";
    public const string Name = "New Game Settings";
    public const string Version = "1.0.0";

    internal static ManualLogSource Logger;
    internal static ConfigEntry<bool> Enabled;

    public override void Load()
    {
        Logger = Log;
        Enabled = Config.Bind("General", "Enabled", true,
            "Adds a Settings button to the new game screen (on the ferry) for the settings per save game.");

        ClassInjector.RegisterTypeInIl2Cpp<EmptyHeadings>();
        var harmony = new Harmony(Guid);
        harmony.PatchAll(typeof(Patches));
        harmony.PatchAll(typeof(RiderColors));
        Log.LogInfo($"{Name} {Version} loaded.");
    }
}

[HarmonyPatch]
internal static class Patches
{
    // The main menu is the only place before the ferry that has a settings window to copy.
    [HarmonyPatch(typeof(UiMenu), nameof(UiMenu.Start))]
    [HarmonyPostfix]
    private static void AfterMenuStart(UiMenu __instance)
    {
        try
        {
            if (Plugin.Enabled.Value && __instance.uiNewGameDialog != null)
                SettingsTemplate.Take(__instance);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Copying the settings window failed: {e}");
        }
    }

    [HarmonyPatch(typeof(WardrobePlayer), nameof(WardrobePlayer.Start))]
    [HarmonyPostfix]
    private static void AfterWardrobeStart(WardrobePlayer __instance)
    {
        try
        {
            if (!Plugin.Enabled.Value)
                return;
            RiderColors.Wardrobe = __instance;
            FerrySettings.Add();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Adding the settings button failed: {e}");
        }
    }
}
