using BetterFog.Assets;
using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;

namespace BetterFog.Input
{
    internal class IngameKeybinds : LcInputActions
    {
        //internal static IngameKeybinds Instance = new IngameKeybinds();
        //internal static InputActionAsset GetAsset() => Instance.Asset;

        public InputAction nextPresetHotkey { get; set; }
        public InputAction nextModeHotkey { get; set; }
        public InputAction refreshPresetHotKey { get; set; }
        public InputAction weatherScalePresetHotKey { get; set; }
        public InputAction settingsHotKey { get; set; }

        private static IngameKeybinds _instance;
        public static IngameKeybinds Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new IngameKeybinds();
                return _instance;
            }
        }

        internal void InitializeKeybindings(string nextPresetHotkeyString, string nextModeHotkeyString, string refreshHotkeyString, string weatherScaleHotkeyString, string settingsHotkeyString)
        {
            // Optionally add a gamepad binding
            //nextPresetHotkey.AddBinding("<Gamepad>/leftStickPress");
            //refreshPresetHotKey.AddBinding("<Gamepad>/rightStickPress");

            // Next preset hotkey
            if (nextPresetHotkey != null && nextPresetHotkey.enabled)
            {
                nextPresetHotkey.Disable();
                nextPresetHotkey.performed -= ctx => BetterFog.NextPreset();
            }
            nextPresetHotkey = new InputAction("Next Fog Preset", binding: $"<Keyboard>/{nextPresetHotkeyString}");
            if (BetterFog.nextPresetHotKeyEnabled.Value)
            {
                // Enable the action
                nextPresetHotkey.Enable();
                nextPresetHotkey.performed += ctx => BetterFog.NextPreset();
            }
            else
                nextPresetHotkey.Disable();

            // Next mode hotkey
            if (nextModeHotkey != null && nextModeHotkey.enabled)
            {
                nextModeHotkey.Disable();
                nextModeHotkey.performed -= ctx => BetterFog.NextMode();
            }
            nextModeHotkey = new InputAction("Next Fog Preset", binding: $"<Keyboard>/{nextModeHotkeyString}");
            if (BetterFog.nextModeHotKeyEnabled.Value)
            {
                // Enable the action
                nextModeHotkey.Enable();

                // Subscribe to the performed event
                nextModeHotkey.performed += ctx => BetterFog.NextMode();
            }
            else
                nextModeHotkey.Disable();

            // Refresh preset hotkey
            if (refreshPresetHotKey != null && refreshPresetHotKey.enabled)
            {
                refreshPresetHotKey.Disable();
                refreshPresetHotKey.performed -= ctx => BetterFog.ApplyFogSettings();
            }
            refreshPresetHotKey = new InputAction("Refresh Fog Preset", binding: $"<Keyboard>/{refreshHotkeyString}");
            if (BetterFog.refreshHotKeyEnabled.Value)
            {
                // Enable the action
                refreshPresetHotKey.Enable();

                // Subscribe to the performed event
                refreshPresetHotKey.performed += ctx => BetterFog.ApplyFogSettings();
            }
            else
                refreshPresetHotKey.Disable();

            // Weather scale preset hotkey
            if (weatherScalePresetHotKey != null && weatherScalePresetHotKey.enabled)
            {
                weatherScalePresetHotKey.Disable();
                weatherScalePresetHotKey.performed -= ctx =>
                {
                    BetterFog.densityScaleEnabled = !BetterFog.densityScaleEnabled;
                    BetterFog.ApplyFogSettings();
                    if (FogSettingsManager.Instance.IsSettingsEnabled())
                    {
                        FogSettingsManager.Instance.UpdateSettings();
                    }
                };
            }
            weatherScalePresetHotKey = new InputAction("Weather Scale Preset", binding: $"<Keyboard>/{weatherScaleHotkeyString}");
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
                weatherScalePresetHotKey.Disable();

            // Settings hotkey
            if (settingsHotKey != null && settingsHotKey.enabled)
            {
                settingsHotKey.Disable();
                settingsHotKey.performed -= ctx => FogSettingsManager.Instance.ToggleSettings();
            }
            settingsHotKey = new InputAction("Settings", binding: $"<Keyboard>/{settingsHotkeyString}");
            if (BetterFog.settingsHotKeyEnabled.Value)
            {
                // Enable the action
                settingsHotKey.Enable();

                // Subscribe to the performed event
                settingsHotKey.performed += ctx => FogSettingsManager.Instance.ToggleSettings();
            }
            else
                settingsHotKey.Disable();
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
