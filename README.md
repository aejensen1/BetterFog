BetterFog

To-Dos:
- Test ship leaving automatically in case that fog is not destroyed.
- Add wheel for fog presets
- Get vertical positioning for each landing that will set fog base.
- Tune fog settings
- Change fog based on weather?


-----------------------------------------
Good Config Settings:
## Settings file was created by plugin Remove fog v0.1.0.0
## Plugin GUID: grug.lethalcompany.fogremover

[Fog Settings]

## Controls the distance at which the fog starts to fade in (in meters).
# Setting type: Single
# Default value: 100
Mean Free Path = 9800

## Sets the height above ground where the fog starts.
# Setting type: Single
# Default value: 0
Base Height = -10000

## Sets the maximum height of the fog above the base height.
# Setting type: Single
# Default value: 10
Maximum Height = 20000

## Controls the red color component of the fog.
# Setting type: Single
# Default value: 1
Albedo Red = 67

## Controls the green color component of the fog.
# Setting type: Single
# Default value: 1
Albedo Green = 24

## Controls the blue color component of the fog.
# Setting type: Single
# Default value: 1
Albedo Blue = 24

## Controls the transparency of the fog.
# Setting type: Single
# Default value: 1
Albedo Alpha = 0.8

## Controls the directional anisotropy of the fog.
# Setting type: Single
# Default value: 0
Anisotropy = 0

## Controls the brightness of light probes affected by fog.
# Setting type: Single
# Default value: 1
Global Light Probe Dimmer = 1000

