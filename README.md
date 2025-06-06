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

To-Dos
==============
- New modes

Screenshots
==============
| ![Settings](https://github.com/user-attachments/assets/a660d670-aeb2-434d-81c3-0a2cdc580dc0) | ![Vanilla](https://github.com/user-attachments/assets/51b7473d-bbb0-4962-8e13-1488ce4ca843) | ![DefaultFog](https://github.com/user-attachments/assets/ab9a4773-0d91-4751-8eb9-a023a60b7dc8) |
|:--:|:--:|:--:|
| **Settings GUI** | **Vanilla Fog** | **Better Fog** |

| ![NoFog](https://github.com/user-attachments/assets/c21bcba4-230a-4472-b84a-0bcb2f9ac622) | ![Red](https://github.com/user-attachments/assets/b7b4e8a2-bc73-42c7-ab85-c5fffe7ab5c8) | ![Orange](https://github.com/user-attachments/assets/00b685fc-45fd-460f-a2d4-7f817c85131f) |
|:--:|:--:|:--:|
| **No Fog** | **Red Fog** | **Orange Fog** |

| ![Green](https://github.com/user-attachments/assets/d2e3a6f7-188b-482f-a3db-9a18ff498ba3) | ![Blue](https://github.com/user-attachments/assets/4d7dc4fa-194b-42c6-8ab7-ca8ab6c2727a) | ![Pink](https://github.com/user-attachments/assets/7ac774cd-a64a-41a6-a60e-31fb12b60c7e) |
|:--:|:--:|:--:|
| **Green Fog** | **Blue Fog** | **Pink Fog** |

| ![Mist](https://github.com/user-attachments/assets/f5eb863c-4c69-422f-8020-199867f18224) | ![White](https://github.com/user-attachments/assets/92b7db3c-49b4-4258-bc0c-dec229bb935f) | ![GreenShip](https://github.com/user-attachments/assets/a80ee226-eafc-4acd-84d1-5cad5dc2a7ad) |
|:--:|:--:|:--:|
| **Misty Fog** | **White Fog** | **Green Ship** |

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
