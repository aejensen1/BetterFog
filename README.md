BetterFog
==============

![FogIcon](https://github.com/user-attachments/assets/0cb0bf4c-0675-4d7e-92c6-fb2b5742067c)

BetterFog is a Mod Plugin that can be used on the game Lethal Company. If you have played the game, you may know how foggy weather is detrimental and even downright unplayable in essentially any other moon than Experimentation and Titan. BetterFog mod solves that issue by creating a preset list of fog options that can have custom densities, colors, and lighting settings. With this, there are virtually thousands of variations of custom fog presets possible. Plus, there is a graphical user interface (GUI) available in the pause menu that allows for live modification of presets in game. Client-side.

The No Fog feature truly removes all fog. Unlike most other mods, this one removes clouds and broken pipe fog too when enabled!

Instructions
==============
- If you are manually setting up the mod, place BetterFog.dll in "Program Files (x86)/Steam/steamapps/common/Lethal Company/BepInEx/plugins/BetterFog.dll". The fogsettingsgui file should also be included under "Program Files (x86)/Steam/steamapps/common/Lethal Company/BepInEx/plugins/Assets/FogAssetBundnle/fogsettingsgui".
- Manage presets in the config file. Any number of presets can be added, as long as there is at least one. Settings can be tweaked for fog density and color.
- Hotkey 'n' is used to switch between presets in-game. LeftStickPress should be the button for controller, but this has not been tested yet. Keybinds are adjustable in the config file!
- Open the pause menu to access GUI which allows for live modification of presets in lobbies. Note that these modified settings do not carry over if you restart the game; you must modify the config file presets to do this.

What is "Weather Scale"?
==============

"Weather Scale" is essentially a toggle option that when set to true multiplies the fog density value to another value based on the moon and weather type. For example, Rend, being naturally more foggy even without weather, might have a multiplier of 0.3, while Offense with no fog may have a multiplier of 0.9. A larger value means more space between fog particles which also means thinner fog. These values can be changed in the config file. When weather scaling is disabled the fog stays static according to the preset it is on, or in other words the fog will not change based on weather or moon. 

"Weather Scaled Enabled by Default" in the config file is a convenience option to enable the Weather Scale on the booting up of the game. When disabled, Weather Scale will be disabled by default and must be enabled via the GUI or hotkey.

Default Values Reference
==============

For reference here are the default values that simulate vanilla fog. You should be able to see these in the config file descriptions as well:

[Fog Presets]

Preset 0 = PresetName=Default,Density=250,Red Hue=0.441,Green Hue=0.459,Blue Hue=0.5,No Fog=False

[Weather and Moon Density Scales]

MoonScales = 71 Gordion=1,41 Experimentation=0.95,220 Assurance=0.9,56 Vow=0.8,21 Offense=0.9,61 March=0.75,20 Adamance=0.75,85 Rend=0.285,7 Dine=0.325,8 Titan=0.285,68 Artifice=0.9,5 Embrion=0.85,44 Liquidation=0.85

WeatherScales = none=1,rainy=0.75,stormy=0.5,foggy=0.45,eclipsed=0.77,dust clouds=0.8,flooded=0.765

Prerequisites
==============
- [BepInEx](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/)
- [LethalCompanyInputUtils](https://thunderstore.io/c/lethal-company/p/Rune580/LethalCompany_InputUtils/)

Known Bugs
==============
- No known bugs

To-Dos
==============
- Future modes coming once most bugs are worked out.

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
