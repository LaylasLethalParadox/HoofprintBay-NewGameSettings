# New Game Settings for Horses of Hoofprint Bay

In [Horses of Hoofprint Bay](https://store.steampowered.com/app/3727090/) the settings per save game (colour schemes and difficulty) can only be changed once a game is running. New Game Settings adds a Settings button to the new game screen on the ferry, under Appearance and Outfit, so you can set them before the game starts.

The button opens a paper like the Outfit one with the game's own "Settings per Save Game" column:

- Custom colour schemes for the rider's skin, the rider's hair and horse colours. When you switch or edit a skin or hair scheme, the rider on the ferry changes at once.
- Receive money for quests, Money Gain, Costs, Prestige Rewards, Prestige Penalties and Competition Difficulty.

The game's own settings code reads and stores these values, so they end up in the new save as if you had set them in the settings menu later. Close the paper with its X or with Escape. You can still change everything in the game's settings menu afterwards.

## Install

You need BepInEx 6 bleeding edge build 793 for IL2CPP (Windows x64). BepInEx 5 and the older 6.0.0-pre builds can't load this game. [Mod Manager](https://github.com/LaylasLethalParadox/HoofprintBay-ModManager#install) explains how to install it; New Game Settings itself doesn't need Mod Manager.

1. Extract the New Game Settings zip into the game folder, so that you have `BepInEx\plugins\NewGameSettings\NewGameSettings.dll`.
2. Start the game and start a new game. The Settings button is on the ferry.

To uninstall, delete `BepInEx\plugins\NewGameSettings`. To switch the button off without uninstalling, set `Enabled = false` in `BepInEx\config\hoofprintbay.newgamesettings.cfg` (or untick it in the Mods menu if you have Mod Manager).

## Building

You need the .NET 6 SDK and the game with BepInEx installed and started at least once, because the project compiles against files BepInEx generates from the game. This repository contains no game files.

```
dotnet build -c Release -p:GameDir="C:\Path\To\HorsesOfHoofprintBay\\"
```

The build copies the DLL into the game's `BepInEx\plugins\NewGameSettings` folder, so close the game first. Add `-p:NoInstall=true` to only compile. `package.ps1` makes the release zip without installing it.

## License

GPL-3.0, see [LICENSE](LICENSE).

This is an unofficial fan project and isn't affiliated with or endorsed by the developers of Horses of Hoofprint Bay.
