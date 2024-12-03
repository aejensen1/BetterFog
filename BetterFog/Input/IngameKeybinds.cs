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
        public InputAction refreshPresetHotkey { get; set; }
        public InputAction weatherScalePresetHotkey { get; set; }
        public InputAction settingsHotkey { get; set; }
        public InputAction autoPresetModeHotkey { get; set; }

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

        internal void InitializeKeybindings(string nextPresetHotkeyString, string nextModeHotkeyString, string refreshHotkeyString, string weatherScaleHotkeyString, string settingsHotkeyString, string autoPresetModeHotkeyString)
        {
            // Optionally add a gamepad binding
            //nextPresetHotkey.AddBinding("<Gamepad>/leftStickPress");
            //refreshPresetHotkey.AddBinding("<Gamepad>/rightStickPress");

            // Next preset hotkey
            if (nextPresetHotkey != null && nextPresetHotkey.enabled)
            {
                nextPresetHotkey.Disable();
                nextPresetHotkey.performed -= ctx => BetterFog.NextPreset();
            }
            nextPresetHotkey = new InputAction("Next Fog Preset", binding: $"<Keyboard>/{nextPresetHotkeyString}");
            if (BetterFog.nextPresetHotkeyConfig.Value != "")
            {
                // Enable the action
                nextPresetHotkey.Enable();
                nextPresetHotkey.performed += ctx =>
                {
                    BetterFog.mls.LogInfo("Next preset hotkey pressed.");
                    BetterFog.NextPreset();
                };
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
            if (BetterFog.nextModeHotkeyConfig.Value != "")
            {
                // Enable the action
                nextModeHotkey.Enable();

                // Subscribe to the performed event
                nextModeHotkey.performed += ctx =>
                {
                    BetterFog.mls.LogInfo("Next mode hotkey pressed.");
                    BetterFog.NextMode();
                };
            }
            else
                nextModeHotkey.Disable();

            // autoPresetMode hotkey
            if (autoPresetModeHotkey != null && autoPresetModeHotkey.enabled)
            {
                autoPresetModeHotkey.Disable();
                autoPresetModeHotkey.performed -= ctx => BetterFog.ToggleAutoPresetMode();
            }
            autoPresetModeHotkey = new InputAction("Auto Preset Mode", binding: $"<Keyboard>/{autoPresetModeHotkeyString}");
            if (BetterFog.autoPresetModeHotkeyConfig.Value != "")
            {
                // Enable the action
                autoPresetModeHotkey.Enable();

                // Subscribe to the performed event
                autoPresetModeHotkey.performed += ctx =>
                {
                    BetterFog.mls.LogInfo("Auto preset mode hotkey pressed.");
                    BetterFog.ToggleAutoPresetMode();
                };
            }
            else
                autoPresetModeHotkey.Disable();

            // Refresh preset hotkey
            if (refreshPresetHotkey != null && refreshPresetHotkey.enabled)
            {
                refreshPresetHotkey.Disable();
                refreshPresetHotkey.performed -= ctx => BetterFog.ApplyFogSettings(false);
            }
            refreshPresetHotkey = new InputAction("Refresh Fog Preset", binding: $"<Keyboard>/{refreshHotkeyString}");
            if (BetterFog.refreshPresetHotkeyConfig.Value != "")
            {
                // Enable the action
                refreshPresetHotkey.Enable();

                // Subscribe to the performed event
                refreshPresetHotkey.performed += ctx =>
                {
                    BetterFog.mls.LogInfo("Refresh preset hotkey pressed.");
                    BetterFog.ApplyFogSettings(false);
                };
            }
            else
                refreshPresetHotkey.Disable();

            // Weather scale preset hotkey
            if (weatherScalePresetHotkey != null && weatherScalePresetHotkey.enabled)
            {
                weatherScalePresetHotkey.Disable();
                weatherScalePresetHotkey.performed -= ctx => BetterFog.ToggleWeatherScaling();
            }
            weatherScalePresetHotkey = new InputAction("Weather Scale Preset", binding: $"<Keyboard>/{weatherScaleHotkeyString}");
            if (BetterFog.weatherScaleHotkeyConfig.Value != "")
            {
                // Enable the action
                weatherScalePresetHotkey.Enable();
                //BetterFog.mls.LogInfo("Weather scaling hotkey enabled.");

                // Subscribe to the performed event
                weatherScalePresetHotkey.performed += ctx =>
                {
                    BetterFog.mls.LogInfo("Weather scaling hotkey pressed.");
                    BetterFog.ToggleWeatherScaling();
                };
            }
            else
                weatherScalePresetHotkey.Disable();

            // Settings hotkey
            if (settingsHotkey != null && settingsHotkey.enabled)
            {
                settingsHotkey.Disable();
                settingsHotkey.performed -= ctx => FogSettingsManager.Instance.ToggleSettings();
            }
            settingsHotkey = new InputAction("Settings", binding: $"<Keyboard>/{settingsHotkeyString}");
            if (BetterFog.settingsHotkeyConfig.Value != "")
            {
                // Enable the action
                settingsHotkey.Enable();
                BetterFog.settingsHotkeyEnabled = true;

                // Subscribe to the performed event
                settingsHotkey.performed += ctx =>
                {
                    BetterFog.mls.LogInfo("Settings hotkey pressed.");
                    FogSettingsManager.Instance.ToggleSettings();
                };
            }
            else
            {
                settingsHotkey.Disable();
                BetterFog.settingsHotkeyEnabled = false;
            }              
        }

        public static void DisableHotkeys()
        {
            BetterFog.hotkeysEnabled = false;
            Instance.nextPresetHotkey.Disable();
            Instance.refreshPresetHotkey.Disable();
            Instance.weatherScalePresetHotkey.Disable();
            Instance.settingsHotkey.Disable();
        }

        public static void EnableHotkeys()
        {
            BetterFog.hotkeysEnabled = true;
            Instance.nextPresetHotkey.Enable();
            Instance.refreshPresetHotkey.Enable();
            Instance.weatherScalePresetHotkey.Enable();
            Instance.settingsHotkey.Enable();
        }
    }
}
