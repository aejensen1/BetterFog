BetterFog
==============

<p align="center">
  <img src="https://github.com/user-attachments/assets/0cb0bf4c-0675-4d7e-92c6-fb2b5742067c" alt="FogIcon"/>
</p>

BetterFog is a Mod Plugin that can be used on the game Lethal Company. If you have played the game, you may know how foggy weather is detrimental and even downright unplayable in essentially any other moon than Experimentation and Titan. BetterFog mod solves that issue by creating a preset list of fog options that can have custom densities, colors, and lighting settings. With this, there are virtually thousands of variations of custom fog presets possible. Plus, there is a graphical user interface (GUI) available when pressing F1 on your keyboard that allows for live modification of presets in game. Client-side.

Why Use This Mod?
==============
- **Feature rich configurable settings** with changeable hotkeys
- **Not binary with No Fog and All Fog** - You can set the density and colors to any level!
- **Thousands of potential fog presets.** Set your game to fit your mood.
- **Fog density can adapt to different moons/weathers**, or just specified moons/weathers by blacklisting the others. The atmosphere can be augmented to your liking.
- **Reduces eye strain.** Are you in a dark room? Squinting to see through super thick fog two inches away from the screen? Darken the fog colors & decrease fog density to relieve some headache.
- **No fog not only removes all fog**, including ground fog clouds, smoke, and pipe fog (not including animations)
- **Vanilla mode available in settings.** No need to exit and disable the mod.
- **Client side** - you don't need the host or anyone else to have the mode for it to work (but they should still get it ;D)
- **Works on custom moons!** If it is too easy or hard to see on a custom moon, settings can be applied just as they are to vanilla moons.
- **Works on custom weathers!**
- **Good for modpacks**: you can lock in whatever settings you want by setting a default preset and disabling hotkeys in the config to add some style to your modpack.

Instructions
==============

- If you are manually setting up the mod, place BetterFog.dll in "Program Files (x86)/Steam/steamapps/common/Lethal Company/BepInEx/plugins/BetterFog.dll". The fogsettingsgui file should also be included under "Program Files (x86)/Steam/steamapps/common/Lethal Company/BepInEx/plugins/Assets/FogAssetBundnle/fogsettingsgui".
- Manage presets in the config file. Settings can be tweaked for fog density and color.
- Hotkey 'n' is used to switch between presets in-game. LeftStickPress should be the button for controller, but this has not been tested yet. Keybinds are adjustable in the config file!
- Press F1 on your keyboard (changeable in config) to access GUI which allows for live modification of presets in lobbies. Note that these modified settings do not carry over if you restart the game; you must modify the config file presets to do this.
- To use density scaling for custom moons you will need to add the full name of the moon and scale to the MoonScales list. Otherwise a warning log will appear indicating that the <full name of moon> was not found in records.
- "Auto Sync" Preset/Mode Settings: Automatically apply presets and modes to moons and weathers. 
  - On the left of = enter a moon and/or weather name, and on the right enter a single preset or mode name. 
  - Entering a preset name on the right automatically sets the mode to "Better Fog". 
  - To have a condition that requires both a moon and weather, enter "&" in between entries. This will override single entries if both moon and weather are present. 
  - If a preset name is the same as a mode name, the mode will be set to "Better Fog" and that preset will be applied. 
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

Prerequisites
==============
- [BepInEx](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/)
- [LethalCompanyInputUtils](https://thunderstore.io/c/lethal-company/p/Rune580/LethalCompany_InputUtils/)

Known Bugs
==============
- "LC Simplified Chinese Localization" By NarkiriFox in combination with this mod causes text to lose their texture and become unreadable. This affects a very small group of players. To mitigate, set "Enable Settings Hotkey" to false and graphics will not be loaded in. This also disabled in-game settings, but text displays correctly.
- Index out of bounds exception may occur when loading into a lobby. Fix is a work-in-progress. Please report issues if you run into them in the modding discord or github page.

To-Dos
==============
- New modes

Screenshots
==============
| ![![settings](https://github.com/user-attachments/assets/a225bc44-f4f2-4252-ad91-d0e00aac4e32) | ![DefaultFog](https://github.com/user-attachments/assets/6ddc9d3e-c16e-4a0d-868e-4025786e49bf) | ![HeavyFog](https://github.com/user-attachments/assets/f9273464-d866-47d8-b3d2-2b1733d23994) |
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
- mrov for WeatherRegistry. I used this for testing only, but it was very useful!
- Rune580 for LethalCompany InputUtils
- DarthFigo for a code suggestion on finding enemy fog to exclude.
- Everyone who helped bug test and suggest new features
