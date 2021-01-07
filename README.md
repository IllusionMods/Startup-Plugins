# Startup Plugins
A collection of BepInEx plugins for Illusion games that skip game startup screens or let you directly start character maker with a special game shortcut. Currently supports HoneySelect, PlayHome, Koikatu/Koikatsu, AI-Syoujyo/AI-Shoujo and HoneySelect2.

## How to install
1. Install latest versions of [BepInEx 5.x](https://github.com/BepInEx/BepInEx) and [ModdingAPI](https://github.com/IllusionMods/IllusionModdingAPI).
2. Download latest release for your game from releases (KK = version for Koikatsu).
3. Extract the release zip inside your game directory. The BepInEx directory inside the archive should be merged with BepInEx folder in your game directory.

## Autostart (HS PH KK AI HS2)
Adds support for commandline arguments to start certain modes automatically during startup (mostly character maker, in some games the startup is much faster than normal). The available command line arguments may vary between games.

It also adds a config setting to load directly into character maker when starting the game normally (no need for a shortcut, hold `esc` when game is starting to cancel). The config settings are only available in AI, HS and HS2.

This feature was originally a part of TitleShortcuts [here](https://github.com/Keelhauled/KeelPlugins).

#### How to use
1. Install the plugin by following steps above.
2. Create a shortcut to main game .exe file (for example Koikatu.exe).
3. Go to properties of the shortcut.
4. Add one of the supported commands from below at the end of the "Target" field (for example `"D:\Games\KK\Koikatu.exe" -femalemaker`).

#### Supported commands
- `-maker` and `-femalemaker` - Start female character maker.
- `-malemaker` - Start male character maker (if the game has one).

## IntroBegone (KK)
Speeds up game startup by skipping intro screens. Can be disabled in plugin settings to get the default intro screens back.

## PH_MakerButton
Adds a "Character maker" button to main menu in PlayHome.
