using BetterFog.Assets;
using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;

namespace BetterFog.Input
{
    internal class IngameKeybinds : LcInputActions
    {
        internal static IngameKeybinds Instance = new IngameKeybinds();
        internal static InputActionAsset GetAsset() => Instance.Asset;

        public InputAction nextPresetHotkey { get; set; }
        public InputAction nextModeHotkey { get; set; }
        public InputAction refreshPresetHotKey { get; set; }
        public InputAction weatherScalePresetHotKey { get; set; }
        public InputAction settingsHotKey { get; set; }

        internal void InitializeKeybindings(string nextPresetHotkeyString, string nextModeHotkeyString, string refreshHotkeyString, string weatherScaleHotkeyString, string settingsHotkeyString)
        {
            // Set the hotkey dynamically
            nextPresetHotkey = new InputAction("Next Fog Preset", binding: $"<Keyboard>/{nextPresetHotkeyString}");
            nextModeHotkey = new InputAction("Next Fog Preset", binding: $"<Keyboard>/{nextModeHotkeyString}");
            refreshPresetHotKey = new InputAction("Refresh Fog Preset", binding: $"<Keyboard>/{refreshHotkeyString}");
            weatherScalePresetHotKey = new InputAction("Weather Scale Preset", binding: $"<Keyboard>/{weatherScaleHotkeyString}");
            settingsHotKey = new InputAction("Settings", binding: $"<Keyboard>/{settingsHotkeyString}");

            // Optionally add a gamepad binding
            //nextPresetHotkey.AddBinding("<Gamepad>/leftStickPress");
            //refreshPresetHotKey.AddBinding("<Gamepad>/rightStickPress");

            if (BetterFog.nextPresetHotKeyEnabled.Value)
            {
                // Enable the action
                nextPresetHotkey.Enable();

                // Subscribe to the performed event
                nextPresetHotkey.performed += ctx => BetterFog.NextPreset();
            }
            else
            {
                // Disable the action
                nextPresetHotkey.Disable();
            }

            if (BetterFog.nextModeHotKeyEnabled.Value)
            {
                // Enable the action
                nextModeHotkey.Enable();

                // Subscribe to the performed event
                nextModeHotkey.performed += ctx => BetterFog.NextMode();
            }
            else
            {
                // Disable the action
                nextModeHotkey.Disable();
            }

            if (BetterFog.refreshHotKeyEnabled.Value)
            {
                // Enable the action
                refreshPresetHotKey.Enable();

                // Subscribe to the performed event
                refreshPresetHotKey.performed += ctx => BetterFog.ApplyFogSettings();
            }
            else
            {
                // Disable the action
                refreshPresetHotKey.Disable();
            }

            if (BetterFog.weatherScaleHotKeyEnabled.Value)
            {
                // Enable the action
                weatherScalePresetHotKey.Enable();
                //BetterFog.mls.LogInfo("Weather scaling hotkey enabled.");

                // Subscribe to the performed event
                weatherScalePresetHotKey.performed += ctx =>
                {
                    BetterFog.densityScaleEnabled = !BetterFog.densityScaleEnabled;
                    //BetterFog.mls.LogInfo($"Density scaling is now {(BetterFog.isDensityScaleEnabled ? "enabled" : "disabled")}.");
                    BetterFog.ApplyFogSettings();
                    if (FogSettingsManager.Instance.IsSettingsEnabled())
                    {
                        FogSettingsManager.Instance.UpdateSettings();
                    }
                };
            }
            else
            {
                // Disable the action
                weatherScalePresetHotKey.Disable();
            }

            if (BetterFog.settingsHotKeyEnabled.Value)
            {
                // Enable the action
                settingsHotKey.Enable();

                // Subscribe to the performed event
                settingsHotKey.performed += ctx => FogSettingsManager.Instance.ToggleSettings();
            }
            else
            {
                // Disable the action
                settingsHotKey.Disable();
            }
        }

        public static void DisableHotkeys()
        {
            BetterFog.hotkeysEnabled = false;
            Instance.nextPresetHotkey.Disable();
            Instance.refreshPresetHotKey.Disable();
            Instance.weatherScalePresetHotKey.Disable();
            Instance.settingsHotKey.Disable();
        }

        public static void EnableHotkeys()
        {
            BetterFog.hotkeysEnabled = true;
            Instance.nextPresetHotkey.Enable();
            Instance.refreshPresetHotKey.Enable();
            Instance.weatherScalePresetHotKey.Enable();
            Instance.settingsHotKey.Enable();
        }
    }
}
