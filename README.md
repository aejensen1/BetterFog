BetterFog
==============

![FogIcon](https://github.com/user-attachments/assets/0cb0bf4c-0675-4d7e-92c6-fb2b5742067c)

BetterFog is a Mod Plugin that can be used on the game Lethal Company. If you have played the game, you may know how foggy weather is detrimental and even downright unplayable in essentially any other moon than Experimentation and Titan. BetterFog mod solves that issue by creating a preset list of fog options that can have custom densities, colors, and lighting settings. With this, there are virtually thousands of variations of custom fog presets possible. Plus, there is a graphical user interface (GUI) available in the pause menu that allows for live modification of presets in game.

Prerequisites
==============
- [BepInEx](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/)
- [LethalCompanyInputUtils](https://thunderstore.io/c/lethal-company/p/Rune580/LethalCompany_InputUtils/)

Known Bugs
==============
- Fog settings sometimes stop working when running into new fog. Current workaround is to refresh your current preset in menu or with hotkey.

To-Dos
==============
- Add more automated refreshes to make settings always appear applied.
- Add granularity where fog does not affect everything (i.e. inside ship and facility).
- Add scale option to weather.

Instructions
==============
- If you are manually setting up the mod, place BetterFog.dll in "Program Files (x86)/Steam/steamapps/common/Lethal Company/BepInEx/plugins/BetterFog.dll". The fogsettingsgui file should also be included under "Program Files (x86)/Steam/steamapps/common/Lethal Company/BepInEx/plugins/Assets/FogAssetBundnle/fogsettingsgui".
- Manage presets in the config file. Any number of presets can be added, as long as there is at least one. Settings can be tweaked for fog density and color.
- Hotkey 'n' is used to switch between presets in-game. LeftStickPress should be the button for controller, but this has not been tested yet. Keybinds are adjustable in the config file!
- Open the pause menu to access GUI which allows for live modification of presets in lobbies. Note that these modified settings do not carry over if you restart the game; you must modify the config file presets to do this.

Examples
==============
Red Fog:
![RedFog2](https://github.com/user-attachments/assets/f33c9469-c990-4ece-bb7e-6afec565f6f0)

Vanilla Fog:
![VanillaFog2](https://github.com/user-attachments/assets/a0977f89-9f8e-4024-ba97-562edd65cb6f)

GUI:
![GUI](https://github.com/user-attachments/assets/d301e7af-fabd-4597-b489-52b8a32c238a)

Credits
==============
Thanks to [mrov](https://github.com/AndreyMrovol) for some code suggestions on finding fog objects.
