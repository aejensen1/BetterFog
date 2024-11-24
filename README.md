BetterFog
==============

<p align="center">
  <img src="https://github.com/user-attachments/assets/0cb0bf4c-0675-4d7e-92c6-fb2b5742067c" alt="FogIcon"/>
</p>

BetterFog is a Mod Plugin that can be used on the game Lethal Company. If you have played the game, you may know how foggy weather is detrimental and even downright unplayable in essentially any other moon than Experimentation and Titan. BetterFog mod solves that issue by creating a preset list of fog options that can have custom densities, colors, and lighting settings. With this, there are virtually thousands of variations of custom fog presets possible. Plus, there is a graphical user interface (GUI) available when pressing F1 on your keyboard that allows for live modification of presets in game. Client-side.

Why Use This Mod?
==============
- Extremely feature rich configurable settings with changeable hotkeys
- Not binary with No Fog and All Fog - You can set the density and colors to any level!
- Thousands of potential fog presets. Set your game to fit your mood.
- Fog density can adapt to different moons/weathers, or just specified moons/weathers by blacklisting the others. The atmosphere can be augmented to your liking.
- Reduces eye strain. Are you in a dark room? Squinting to see through super thick fog two inches away from the screen? Darken the fog colors & decrease fog density to relieve some headache.
- No fog not only removes ground fog but also clouds, smoke, and pipe fog (not including animations)
- If you get tired of BetterFog settings, just switch to Vanilla mode in-game. No need to exit and disable the mod.
- Client side - you don't need the host or anyone else to have the mode for it to work (but they should still get it ;D)
- Works on custom moons! If it is too easy or hard to see on a custom moon, settings can be applied just as they are to vanilla moons.
- Good for modpacks: you can lock in whatever settings you want by setting a default preset and disabling hotkeys in the config to add some style to your modpack.

Instructions
==============

- If you are manually setting up the mod, place BetterFog.dll in "Program Files (x86)/Steam/steamapps/common/Lethal Company/BepInEx/plugins/BetterFog.dll". The fogsettingsgui file should also be included under "Program Files (x86)/Steam/steamapps/common/Lethal Company/BepInEx/plugins/Assets/FogAssetBundnle/fogsettingsgui".
- Manage presets in the config file. Settings can be tweaked for fog density and color.
- Hotkey 'n' is used to switch between presets in-game. LeftStickPress should be the button for controller, but this has not been tested yet. Keybinds are adjustable in the config file!
- Press F1 on your keyboard (changeable in config) to access GUI which allows for live modification of presets in lobbies. Note that these modified settings do not carry over if you restart the game; you must modify the config file presets to do this.
- To use density scaling for custom moons you will need to add the full name of the moon and scale to the MoonScales list. Otherwise a warning log will appear indicating that the <full name of moon> was not found in records.
- "Auto Preset/Mode Settings: Automatically apply presets and modes to moons and weathers. 
  - On the left of = enter a moon and/or weather name, and on the right enter a single preset or mode name. 
  - Entering a preset name on the right automatically sets the mode to "Better Fog". 
  - To have a condition that requires both a moon and weather, enter "&" in between entries. This will override single entries if both moon and weather are present. 
  - If a preset name is the same as a mode name, the mode will be set to \"Better Fog\" and that preset will be set. 
  - Warning: If you create different conditions that conflict (such as none=mist,68 Artifice=No Fog and you land on Art with no weather), the leftmost condition will apply. For that reason, put double conditions with the most specific condition first, and single condition last.
  - Example: "7 Dine&eclipsed=Orange Fog,61 March=Light Fog,7 Dine=Heavy Fog,eclipsed=Red Fog,8 Titan=Heavy Fog,none=Mist,none&8 Titan=No Fog"
- Please report any bugs to me as they are found. I want to help!

What is "Weather Scale"?
==============

"Weather Scale" or "Density Scale" is essentially a toggle option that when set to true multiplies the fog density value to another value based on the moon and weather type. For example, Rend, being naturally more foggy even without weather, might have a multiplier of 0.3, while Offense with no fog may have a multiplier of 0.9. A larger value means more space between fog particles which also means thinner fog. These values can be changed in the config file. When weather scaling is disabled the fog stays static according to the preset it is on, or in other words the fog will not change based on weather or moon. 

"Weather Scaled Enabled by Default" in the config file is a convenience option to enable the Weather Scale on the booting up of the game. When disabled, Weather Scale will be disabled by default and must be enabled via the GUI or hotkey.

Notes
==============
- Starting at v3.3.0, you must disable the GUI keybind to disable the GUI. There is no longer a config option that states to disable the GUI.
- Starting at v3.3.3, WeatherRegistry by Mrov is a dependency to this mod. It should be also installed, or the mod may not work correctly.

Default Values Reference
==============

For reference here are the default values that *somewhat* simulate vanilla fog. You should be able to see these in the config file descriptions as well:

[Fog Presets]

Preset 0 = PresetName=Default,Density=250,Red Hue=0.441,Green Hue=0.459,Blue Hue=0.5,No Fog=False

[Weather and Moon Density Scales]

MoonScales = 71 Gordion=1,41 Experimentation=0.95,220 Assurance=0.9,56 Vow=0.8,21 Offense=0.9,61 March=0.75,20 Adamance=0.75,85 Rend=0.285,7 Dine=0.325,8 Titan=0.285,68 Artifice=0.9,5 Embrion=0.85,44 Liquidation=0.85

WeatherScales = none=1,rainy=0.75,stormy=0.5,foggy=0.45,eclipsed=0.77,dust clouds=0.8,flooded=0.765

Prerequisites
==============
- [BepInEx](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/)
- [LethalCompanyInputUtils](https://thunderstore.io/c/lethal-company/p/Rune580/LethalCompany_InputUtils/)
- [WeatherRegistry](https://thunderstore.io/c/lethal-company/p/mrov/WeatherRegistry/)

Known Bugs
==============
- "LC Simplified Chinese Localization" By NarkiriFox in combination with this mod causes text to lose their texture and become unreadable. This affects a very small group of players. Solution has not been found...

To-Dos
==============
- New modes

Screenshots
==============
| ![SettingsGUI](https://github.com/user-attachments/assets/f9df6c2e-1194-4332-b5f6-720833e2fad6) | ![DefaultFog](https://github.com/user-attachments/assets/6ddc9d3e-c16e-4a0d-868e-4025786e49bf) | ![HeavyFog](https://github.com/user-attachments/assets/f9273464-d866-47d8-b3d2-2b1733d23994) |
|:--:|:--:|:--:|
| **Settings GUI** | **Default Fog** | **Heavy Fog** |

| ![NoFog](https://github.com/user-attachments/assets/e400170a-85a7-4b59-8107-a056b8e70bc5) | ![VanillaFog](https://github.com/user-attachments/assets/56d8d5cd-0f69-4f17-9a25-cdc672e052e1) | ![RedFog](https://github.com/user-attachments/assets/e392e535-06e7-47e7-9917-b638ed620271) |
|:--:|:--:|:--:|
| **No Fog** | **Vanilla Fog** | **Red Fog** |

| ![OrangeFog](https://github.com/user-attachments/assets/7f39a444-2267-453a-957b-89817b42110a) | ![PinkFog](https://github.com/user-attachments/assets/27c0e56a-5fd6-4986-aa7d-470331aa5225) | ![BlueFog](https://github.com/user-attachments/assets/5ddfd892-753e-4cce-959d-a847dface9e6) |
|:--:|:--:|:--:|
| **Orange Fog** | **Pink Fog** | **Blue Fog** |

Questions?
==============
You may post questions in the Lethal Company Modding discord server: https://discord.com/channels/1168655651455639582/1280288943857733632

Credits
==============
- [mrov](https://github.com/AndreyMrovol) for some code suggestions on finding fog objects.
- mrov for WeatherRegistry
- Rune580 for LethalCompany InputUtils
- DarthFigo for a code suggestion on finding enemy fog to exclude.
